import {
  Typography,
  Button,
  DialogContent,
  DialogActions,
} from "@mui/material";
import { TransactionStatusEnum } from "../../../api/transactionService";

interface Props {
  result: TransactionStatusEnum | null;
  onClose: () => void;
}

export const TransferResult = ({ result, onClose }: Props) => {
  if (!result) return null;

  return (
    <>
      <DialogContent>
        <Typography
          variant="h6"
          align="center"
          color={result === TransactionStatusEnum.Accepted ? "green" : "error"}
        >
          {result === TransactionStatusEnum.Accepted
            ? "The operation was successful!"
            : "The operation was rejected!"}
        </Typography>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2, justifyContent: "center" }}>
        <Button onClick={onClose} variant="contained">
          Close
        </Button>
      </DialogActions>
    </>
  );
};
