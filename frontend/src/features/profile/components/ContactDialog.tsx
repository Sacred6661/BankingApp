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
  ProfileContactDto,
  ContactTypeDto,
} from "../../../api/profileService";
import { PhoneInput } from "react-international-phone";
import "react-international-phone/style.css";

interface ContactDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (contact: ProfileContactDto) => void;
  contact?: ProfileContactDto;
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
  const filteredTypes = useMemo(() => {
    if (contact?.contactTypeId) {
      const editingType = contactTypes.find(
        (t) => t.id === contact.contactTypeId
      );
      if (editingType?.code === "PRIMARY_EMAIL") return [editingType];
    }

    return contactTypes.filter((t) => t.code !== "PRIMARY_EMAIL");
  }, [contact, contactTypes]);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      contactTypeId: contact ? contact.contactTypeId : 0,
      value: contact?.value || "",
    },
    validationSchema: Yup.object({
      contactTypeId: Yup.number().min(1, "Select type").required("Required"),
      value: Yup.string().required("Required"),
    }),
    onSubmit: (values) => {
      const type = contactTypes.find((t) => t.id === values.contactTypeId);
      onSave({
        id: contact?.id || 0,
        isActive: true,
        contactTypeId: values.contactTypeId,
        contactTypeName: type?.typeName || "",
        value: values.value,
      });
      onClose();
    },
  });

  // 🟢 Reset форми при Add Contact
  useEffect(() => {
    if (open && !contact) {
      formik.resetForm({ values: { contactTypeId: 0, value: "" } });
    }
  }, [open, contact]);

  const selectedType = useMemo(
    () =>
      filteredTypes.find((t) => t.id === formik.values.contactTypeId) ||
      contactTypes.find((t) => t.id === formik.values.contactTypeId),
    [filteredTypes, contactTypes, formik.values.contactTypeId]
  );

  // 🟢 Оновлення валідації при зміні типу
  useEffect(() => {
    if (!selectedType) return;

    const regex = selectedType.regexPattern;
    let validation = Yup.string().required("Required");

    if (regex) {
      validation = validation.matches(
        new RegExp(regex),
        `Invalid ${selectedType.typeName}`
      );
    }

    formik.setFormikState((prev) => ({
      ...prev,
      validationSchema: Yup.object({
        contactTypeId: Yup.number().min(1, "Select type").required("Required"),
        value: validation,
      }),
    }));
  }, [selectedType]);

  // 🟢 Підтягування шаблонів
  useEffect(() => {
    if (!formik.values.contactTypeId) return;

    if (contact) {
      formik.setFieldValue("value", contact.value || "");
      return;
    }

    const timeout = setTimeout(() => {
      const type = contactTypes.find(
        (t) => t.id === formik.values.contactTypeId
      );
      const template = existingContacts.find(
        (c) => c.contactTypeId === formik.values.contactTypeId && c.isActive
      );

      if (template && template.value) {
        formik.setFieldValue("value", template.value);
      } else if (type?.code === "PHONE") {
        formik.setFieldValue("value", "+380");
      } else {
        formik.setFieldValue("value", "");
      }
    }, 0);

    return () => clearTimeout(timeout);
  }, [formik.values.contactTypeId]);

  // 🟢 Рендер поля
  const renderValueField = () => {
    if (!selectedType) return null;

    switch (selectedType.code) {
      case "PRIMARY_EMAIL":
      case "EMAIL":
        return (
          <TextField
            label="Email"
            fullWidth
            type="email"
            name="value"
            value={formik.values.value}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={!!formik.touched.value && !!formik.errors.value}
            helperText={formik.touched.value && formik.errors.value}
            disabled={selectedType.code === "PRIMARY_EMAIL"}
          />
        );

      case "PHONE":
        return (
          <PhoneInput
            key={selectedType.id}
            defaultCountry="ua"
            value={formik.values.value}
            onChange={(phone: string) => formik.setFieldValue("value", phone)}
            inputStyle={{
              width: "100%",
              fontSize: "1rem",
              padding: "10px 12px",
              borderColor:
                formik.touched.value && formik.errors.value ? "red" : undefined,
            }}
          />
        );

      case "TELEGRAM":
        return (
          <TextField
            label="Telegram Username"
            fullWidth
            name="value"
            value={formik.values.value}
            onChange={(e) => {
              let val = e.target.value;
              if (!val.startsWith("@")) val = "@" + val;
              formik.setFieldValue("value", val);
            }}
            onBlur={formik.handleBlur}
            error={!!formik.touched.value && !!formik.errors.value}
            helperText={formik.touched.value && formik.errors.value}
          />
        );

      default:
        return (
          <TextField
            label="Value"
            fullWidth
            name="value"
            value={formik.values.value}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={!!formik.touched.value && !!formik.errors.value}
            helperText={formik.touched.value && formik.errors.value}
          />
        );
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="xs"
      fullWidth
      sx={{
        "& .MuiDialog-paper": { fontSize: "1rem" },
        "& .MuiInputBase-input": { fontSize: "1rem" },
        "& .MuiInputLabel-root": { fontSize: "0.95rem" },
        "& .MuiButton-root": { fontSize: "0.95rem" },
      }}
    >
      <DialogTitle>{contact ? "Edit Contact" : "Add Contact"}</DialogTitle>
      <form onSubmit={formik.handleSubmit}>
        <DialogContent>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <TextField
                select
                label="Contact Type"
                fullWidth
                SelectProps={{ native: true }}
                name="contactTypeId"
                value={formik.values.contactTypeId}
                onChange={(e) =>
                  formik.setFieldValue(
                    "contactTypeId",
                    Number((e.target as HTMLInputElement).value)
                  )
                }
                onBlur={formik.handleBlur}
                error={
                  !!formik.touched.contactTypeId &&
                  !!formik.errors.contactTypeId
                }
                helperText={
                  formik.touched.contactTypeId && formik.errors.contactTypeId
                }
                disabled={!!contact && selectedType?.code === "PRIMARY_EMAIL"}
              >
                <option value={0}>Select type</option>
                {filteredTypes.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.typeName}
                  </option>
                ))}
              </TextField>
            </Grid>

            <Grid size={{ xs: 12 }}>{renderValueField()}</Grid>
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

export default ContactDialog;
