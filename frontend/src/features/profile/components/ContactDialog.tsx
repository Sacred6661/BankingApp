// ContactDialog.tsx
import React, { useEffect, useMemo } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
} from "@mui/material";
import { useFormik } from "formik";
import * as Yup from "yup";
import type {
  ProfileContactDto,
  ContactTypeDto,
} from "../../../api/profileService";

interface ContactDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (contact: ProfileContactDto) => void;
  contact?: ProfileContactDto; // to edit
  existingContacts: ProfileContactDto[];
  contactTypes: ContactTypeDto[];
}

const ContactDialog: React.FC<ContactDialogProps> = ({
  open,
  onClose,
  onSave,
  contact,
  existingContacts,
  contactTypes,
}) => {
  const buildContactDto = (values: typeof formik.values): ProfileContactDto => {
    const existing = existingContacts.find(
      (c) => c.contactTypeId === Number(values.contactTypeId)
    );
    return {
      id: existing?.id || 0,
      isActive: true,
      contactTypeId: Number(values.contactTypeId),
      contactTypeName:
        contactTypes.find((t) => t.id === Number(values.contactTypeId))
          ?.typeName || "",
      value: values.value,
    };
  };

  // локальна форма
  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      contactTypeId: contact?.contactTypeId || 0,
      value: contact?.value || "",
    },
    validationSchema: Yup.object({
      contactTypeId: Yup.number().required("Required").min(1, "Select type"),
      value: Yup.string().required("Required"),
    }),
    onSubmit: (values) => {
      onSave(buildContactDto(values));
      onClose();
    },
  });

  useEffect(() => {
    if (open) {
      if (!contact) {
        // if Add - reset the form
        formik.resetForm({
          values: {
            contactTypeId: 0,
            value: "",
          },
        });
      } else {
        // if edit - try to get existed info
        formik.resetForm({
          values: {
            contactTypeId: contact.contactTypeId,
            value: contact.value || "",
          },
        });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, contact]);

  useEffect(() => {
    if (!formik.values.contactTypeId) return;

    const contactTypeId = Number(formik.values.contactTypeId);

    // ooking for an existing contact for this type
    const existing = existingContacts.find(
      (c) => c.contactTypeId === contactTypeId
    );

    // if we edit a contact, its value will already be in contact
    if (contact && contact.contactTypeId === contactTypeId) {
      formik.setFieldValue("value", contact.value || "");
    } else if (existing) {
      // get any contact of this type
      formik.setFieldValue("value", existing.value || "");
    } else {
      formik.setFieldValue("value", "");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formik.values.contactTypeId, existingContacts, contact]);

  // for memorize
  const options = useMemo(
    () => contactTypes.filter((t) => t.id !== 1), // exlude "primary email"
    [contactTypes]
  );

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{contact ? "Edit Contact" : "Add Contact"}</DialogTitle>
      <form onSubmit={formik.handleSubmit}>
        <DialogContent>
          <TextField
            select
            label="Contact Type"
            fullWidth
            SelectProps={{ native: true }}
            margin="normal"
            name="contactTypeId"
            value={formik.values.contactTypeId}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={
              !!formik.touched.contactTypeId && !!formik.errors.contactTypeId
            }
            helperText={
              formik.touched.contactTypeId && formik.errors.contactTypeId
            }
          >
            <option value={0}>Select type</option>
            {options.map((t) => (
              <option key={t.id} value={t.id}>
                {t.typeName}
              </option>
            ))}
          </TextField>

          <TextField
            label="Value"
            fullWidth
            margin="normal"
            name="value"
            value={formik.values.value}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={!!formik.touched.value && !!formik.errors.value}
            helperText={formik.touched.value && formik.errors.value}
          />
        </DialogContent>

        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained">
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default ContactDialog;
