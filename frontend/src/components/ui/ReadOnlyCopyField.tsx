import React from "react";
import { TextField, IconButton, InputAdornment } from "@mui/material";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";

interface ReadOnlyCopyFieldProps {
  value: string;
  label?: string;
  variant?: "outlined" | "filled" | "standard";
  fullWidth?: boolean;
}

const ReadOnlyCopyField: React.FC<ReadOnlyCopyFieldProps> = ({
  value,
  label = "",
  variant = "outlined",
  fullWidth = true,
}) => {
  const handleCopy = () => {
    navigator.clipboard.writeText(value);
  };

  return (
    <TextField
      label={label}
      value={value}
      variant={variant}
      fullWidth={fullWidth}
      InputProps={{
        readOnly: true,
        endAdornment: (
          <InputAdornment position="end">
            <IconButton onClick={handleCopy} edge="end">
              <ContentCopyIcon />
            </IconButton>
          </InputAdornment>
        ),
        sx: {
          color: "text.primary",
        },
      }}
    />
  );
};

export default ReadOnlyCopyField;
