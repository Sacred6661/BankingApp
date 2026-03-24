import {
  DialogContent,
  DialogActions,
  Button,
  Stack,
  TextField,
  RadioGroup,
  FormControlLabel,
  Radio,
  MenuItem,
  Typography,
} from "@mui/material";
import { useState, useEffect } from "react";
import type { AccountDto } from "../../../api/accountService";
import type { TransferRequest } from "../../../api/transactionService";
import { transactionService } from "../../../api/transactionService";

interface Props {
  onClose: () => void;
  accounts: AccountDto[];
  fromAccountId: string;
  onTransactionCreated: (transactionId: string) => void;
}

export const TransferForm = ({
  onClose,
  accounts,
  fromAccountId,
  onTransactionCreated,
}: Props) => {
  const [mode, setMode] = useState<"my" | "other">("my");
  const [from, setFrom] = useState(fromAccountId);
  const [to, setTo] = useState("");
  const [amount, setAmount] = useState("");
  const [loading, setLoading] = useState(false);

  const fromAccount = accounts.find((a) => a.id === from);

  useEffect(() => {
    setFrom(fromAccountId);
  }, [fromAccountId]);

  const availableToAccounts = accounts.filter((a) => a.id !== from);

  const handleSubmit = async () => {
    try {
      setLoading(true);

      const payload: TransferRequest = {
        fromAccountNumber: from,
        toAccountNumber: to,
        amount: Number(amount),
      };

      const res = await transactionService.transfer(payload);

      onTransactionCreated(res.transactionId);
    } catch (error) {
      console.error("Transfer failed", error);
    } finally {
      setLoading(false);
    }

    // тут буде реальний API
    //onClose();
  };

  return (
    <>
      <DialogContent>
        <Stack spacing={3} mt={1}>
          {/* MODE */}
          <RadioGroup
            row
            value={mode}
            onChange={(e) => setMode(e.target.value as "my" | "other")}
          >
            <FormControlLabel value="my" control={<Radio />} label="My" />
            <FormControlLabel value="other" control={<Radio />} label="Other" />
          </RadioGroup>

          {/* FROM */}
          <Stack spacing={1}>
            <Typography variant="overline">From</Typography>
            <TextField
              select
              fullWidth
              value={from}
              onChange={(e) => setFrom(e.target.value)}
            >
              {accounts.map((acc) => (
                <MenuItem key={acc.id} value={acc.id}>
                  {acc.id}
                </MenuItem>
              ))}
            </TextField>

            {fromAccount && (
              <Typography variant="body2" sx={{ opacity: 0.7 }}>
                Available balance:{" "}
                <strong>
                  {fromAccount.balance.toLocaleString("uk-UA", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2,
                  })}{" "}
                  UAH
                </strong>
              </Typography>
            )}
          </Stack>

          {/* TO */}
          <Stack spacing={1}>
            <Typography variant="overline">To</Typography>

            {mode === "my" ? (
              <TextField
                select
                fullWidth
                value={to}
                onChange={(e) => setTo(e.target.value)}
              >
                {availableToAccounts.map((acc) => (
                  <MenuItem key={acc.id} value={acc.id}>
                    {acc.id}
                  </MenuItem>
                ))}
              </TextField>
            ) : (
              <TextField
                fullWidth
                placeholder="Enter card number"
                value={to}
                onChange={(e) => setTo(e.target.value)}
              />
            )}
          </Stack>

          {/* AMOUNT */}
          <Stack spacing={1}>
            <Typography variant="overline">Amount</Typography>
            <TextField
              type="number"
              fullWidth
              inputProps={{ min: 0, step: 0.01 }}
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
            />
          </Stack>
        </Stack>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={!amount || !from || !to}
        >
          {loading ? "Sending..." : "Transfer"}
        </Button>
      </DialogActions>
    </>
  );
};
