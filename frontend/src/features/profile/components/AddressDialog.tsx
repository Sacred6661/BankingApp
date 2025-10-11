import React, { useEffect, useMemo } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
} from "@mui/material";
import { useFormik } from "formik";
import * as Yup from "yup";
import type {
  ProfileAddressDto,
  ContactTypeDto,
} from "../../../api/profileService";
import { VirtualizedAutocomplete } from "../../../components/ui/VirtualizedAutocomplete";

interface AddressDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (address: ProfileAddressDto) => void;
  address?: ProfileAddressDto;
  existingAddresses: ProfileAddressDto[];
  addressTypes: ContactTypeDto[];
  countries: { id: number; name: string }[];
}

const AddressDialog: React.FC<AddressDialogProps> = ({
  open,
  onClose,
  onSave,
  address,
  existingAddresses,
  addressTypes,
  countries,
}) => {
  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      addressTypeId: address?.addressTypeId || 0,
      addressLine: address?.addressLine || "",
      city: address?.city || "",
      zipCode: address?.zipCode || "",
      countryId: address?.countryId || 0,
    },
    validationSchema: Yup.object({
      addressTypeId: Yup.number().min(1, "Select type").required("Required"),
      addressLine: Yup.string().required("Required"),
      city: Yup.string().required("Required"),
      zipCode: Yup.string().required("Required"),
      countryId: Yup.number().min(1, "Select country").required("Required"),
    }),
    onSubmit: (values) => {
      const existing = existingAddresses.find(
        (a) => a.addressTypeId === values.addressTypeId
      );
      onSave({
        id: existing?.id || 0,
        isActive: true,
        addressTypeId: values.addressTypeId,
        addessTypeName:
          addressTypes.find((t) => t.id === values.addressTypeId)?.typeName ||
          "",
        addressLine: values.addressLine,
        city: values.city,
        zipCode: values.zipCode,
        countryId: values.countryId,
        countryName: countries.find((c) => c.id === values.countryId)?.name,
      });
      onClose();
    },
  });

  const typeOptions = useMemo(() => addressTypes, [addressTypes]);

  const countryOptions = countries;

  useEffect(() => {
    if (open && !address) {
      formik.resetForm();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, address]);

  useEffect(() => {
    if (!formik.values.addressTypeId) return;

    const addressTypeId = Number(formik.values.addressTypeId);

    const existing = existingAddresses.find(
      (a) => a.addressTypeId === addressTypeId
    );

    const newValues = {
      addressTypeId,
      addressLine: "",
      city: "",
      zipCode: "",
      countryId: 0,
    };

    if (address && address.addressTypeId === addressTypeId) {
      newValues.addressLine = address.addressLine || "";
      newValues.city = address.city || "";
      newValues.zipCode = address.zipCode || "";
      newValues.countryId = address.countryId || 0;
    } else if (existing) {
      newValues.addressLine = existing.addressLine || "";
      newValues.city = existing.city || "";
      newValues.zipCode = existing.zipCode || "";
      newValues.countryId = existing.countryId || 0;
    }

    formik.setValues((prev) => ({ ...prev, ...newValues }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formik.values.addressTypeId, existingAddresses, address]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{address ? "Edit Address" : "Add Address"}</DialogTitle>
      <form onSubmit={formik.handleSubmit}>
        <DialogContent>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <TextField
                select
                label="Address Type"
                fullWidth
                SelectProps={{ native: true }}
                name="addressTypeId"
                value={formik.values.addressTypeId}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={
                  !!formik.touched.addressTypeId &&
                  !!formik.errors.addressTypeId
                }
                helperText={
                  formik.touched.addressTypeId && formik.errors.addressTypeId
                }
              >
                <option value={0}>Select type</option>
                {typeOptions.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.typeName}
                  </option>
                ))}
              </TextField>
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                label="Address Line"
                fullWidth
                name="addressLine"
                value={formik.values.addressLine}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={
                  !!formik.touched.addressLine && !!formik.errors.addressLine
                }
                helperText={
                  formik.touched.addressLine && formik.errors.addressLine
                }
              />
            </Grid>

            <Grid size={{ xs: 6 }}>
              <TextField
                label="City"
                fullWidth
                name="city"
                value={formik.values.city}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={!!formik.touched.city && !!formik.errors.city}
                helperText={formik.touched.city && formik.errors.city}
              />
            </Grid>

            <Grid size={{ xs: 6 }}>
              <TextField
                label="Zip Code"
                fullWidth
                name="zipCode"
                value={formik.values.zipCode}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={!!formik.touched.zipCode && !!formik.errors.zipCode}
                helperText={formik.touched.zipCode && formik.errors.zipCode}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <VirtualizedAutocomplete
                value={
                  countryOptions.find(
                    (c) => c.id === formik.values.countryId
                  ) || null
                }
                options={countryOptions}
                getOptionLabel={(c) => c.name}
                onChange={(val) => formik.setFieldValue("countryId", val?.id)}
                label="Country"
                error={!!formik.errors.countryId}
                helperText={formik.errors.countryId}
              />
            </Grid>
          </Grid>
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

export default AddressDialog;
