import type { FC } from "react";
import { Dialog, DialogTitle, DialogActions, Button } from "@mui/material";

interface ConfirmDialogProps {
  open: boolean;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmCreateAccountDialog: FC<ConfirmDialogProps> = ({
  open,
  message,
  onConfirm,
  onCancel,
}) => {
  return (
    <Dialog open={open} onClose={onCancel}>
      <DialogTitle>{message}</DialogTitle>
      <DialogActions>
        <Button onClick={onCancel} color="error" variant="contained">
          No
        </Button>
        <Button onClick={onConfirm} color="success" variant="contained">
          Yes
        </Button>
      </DialogActions>
    </Dialog>
  );
};
