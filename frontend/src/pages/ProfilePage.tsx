import { useState } from "react";
import { Box, Tabs, Tab, Typography } from "@mui/material";
import { useFormik } from "formik";
import * as Yup from "yup";
import toast from "react-hot-toast";
import { useProfile } from "../hooks/useProfile";
import { useAuth } from "../hooks/useAuth";
import { useDictionaries } from "../hooks/useDictionaries";
import ConfirmDeleteDialog from "../components/ui/ConfirmDeleteDialog";
import LoadingOverlay from "../components/ui/LoadingOverlay";
import { getErrorMessage } from "../utils/errorHelpers";

import MainTab from "../features/profile/components/MainTab";
import ContactsTab from "../features/profile/components/ContactsTab";
import AddressesTab from "../features/profile/components/AddressesTab";
import ContactDialog from "../features/profile/components/ContactDialog";
import AddressDialog from "../features/profile/components/AddressDialog";
import MainSkeleton from "../features/profile/components/MainSkeleton";

import type {
  ProfileDto,
  ProfileContactDto,
  ProfileAddressDto,
} from "../api/profileService";

export default function ProfilePage() {
  const { user } = useAuth();
  const {
    profile,
    loading,
    error,
    addOrUpdateContact,
    addOrUpdateAddress,
    updateProfile,
    deleteContact,
    deleteAddress,
  } = useProfile();
  const {
    languages,
    timezones,
    contactTypes,
    addressTypes,
    countries,
    loading: dictLoading,
    error: dictError,
  } = useDictionaries();

  const [tab, setTab] = useState(0);
  const [deleteDialog, setDeleteDialog] = useState<{
    open: boolean;
    type?: "contact" | "address";
    id?: number;
  }>({ open: false });
  const [contactDialog, setContactDialog] = useState<{
    open: boolean;
    contact?: ProfileContactDto;
  }>({ open: false });
  const [addressDialog, setAddressDialog] = useState<{
    open: boolean;
    address?: ProfileAddressDto;
  }>({ open: false });

  const [defaultProfile, setDefaultProfile] = useState<ProfileDto>({
    userId: "",
    firstName: "",
    lastName: "",
    avatarUrl: "",
    email: "",
    birthday: undefined,
    contacts: [],
    addresses: [],
    settings: {
      userId: user?.id || "",
      languageId: 1,
      languageName: "",
      timezoneId: 2,
      timezoneName: "",
      notificationsEnabled: false,
    },
  });

  const formik = useFormik<ProfileDto>({
    initialValues: profile || defaultProfile,
    enableReinitialize: true,
    validationSchema: Yup.object({
      firstName: Yup.string().required("Required"),
      lastName: Yup.string().required("Required"),
      birthday: Yup.date().nullable(),
      settings: Yup.object({
        languageId: Yup.number().required("Required"),
        timezoneId: Yup.number().required("Required"),
      }),
    }),
    onSubmit: async (values) => {
      await updateProfile({
        ...values,
        birthday: values.birthday
          ? new Date(values.birthday).toISOString()
          : undefined,
      });

      toast.success("Profile saved");
    },
  });

  // --- CRUD для контактів ---
  const handleSaveContact = async (contact: ProfileContactDto) => {
    await addOrUpdateContact(contact);
    toast.success("Contact added");
  };

  const handleDeleteContact = async (id: number) => {
    await deleteContact(id);
    toast.success("Contact deleted");
  };

  // --- CRUD для адрес ---
  const handleSaveAddress = async (address: ProfileAddressDto) => {
    await addOrUpdateAddress(address);
    toast.success("Address added");
  };

  const handleDeleteAddress = async (id: number) => {
    await deleteAddress(id);
    toast.success("Address deleted");
  };

  if (error || dictError)
    return (
      <Typography color="error">
        {getErrorMessage(error || dictError)}
      </Typography>
    );

  const isLoading = loading || dictLoading;

  return (
    <Box>
      <Tabs value={tab} onChange={(_, v) => setTab(v)} variant="fullWidth">
        <Tab label="Main" />
        <Tab label="Contacts" />
        <Tab label="Addresses" />
      </Tabs>
      <form onSubmit={formik.handleSubmit}>
        {tab === 0 &&
          (isLoading ? (
            <MainSkeleton />
          ) : (
            <MainTab
              formik={formik}
              languages={languages}
              timezones={timezones}
            />
          ))}
        {tab === 1 && (
          <ContactsTab
            contacts={profile?.contacts ?? []}
            onAdd={() => setContactDialog({ open: true })}
            onEdit={(contact) => setContactDialog({ open: true, contact })}
            onDelete={(id) =>
              setDeleteDialog({ open: true, type: "contact", id })
            }
          />
        )}
        {tab === 2 && (
          <AddressesTab
            addresses={profile?.addresses ?? []}
            onAdd={() => setAddressDialog({ open: true })}
            onEdit={(address) => setAddressDialog({ open: true, address })}
            onDelete={(id) =>
              setDeleteDialog({ open: true, type: "address", id })
            }
          />
        )}
      </form>

      <ConfirmDeleteDialog
        open={deleteDialog.open}
        onClose={() => setDeleteDialog({ open: false })}
        onConfirm={() => {
          if (deleteDialog.type === "contact" && deleteDialog.id)
            handleDeleteContact(deleteDialog.id);
          if (deleteDialog.type === "address" && deleteDialog.id)
            handleDeleteAddress(deleteDialog.id);
          setDeleteDialog({ open: false });
        }}
        title="Confirm deletion"
        description="Are you sure?"
      />
      <ContactDialog
        open={contactDialog.open}
        contact={contactDialog.contact}
        onClose={() => setContactDialog({ open: false })}
        contactTypes={contactTypes}
        existingContacts={profile?.contacts ?? []}
        onSave={handleSaveContact}
      />
      <AddressDialog
        open={addressDialog.open}
        address={addressDialog.address}
        onClose={() => setAddressDialog({ open: false })}
        onSave={handleSaveAddress}
        existingAddresses={profile?.addresses ?? []}
        addressTypes={addressTypes}
        countries={countries}
      />

      <LoadingOverlay loading={loading || dictLoading} />
    </Box>
  );
}
