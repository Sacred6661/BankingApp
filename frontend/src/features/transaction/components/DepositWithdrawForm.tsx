import {
  DialogContent,
  DialogActions,
  Button,
  Stack,
  TextField,
  MenuItem,
  Typography,
} from "@mui/material";
import { useState, useEffect } from "react";
import type { AccountDto } from "../../../api/accountService";
import type {
  DepositRequest,
  WithdrawRequest,
} from "../../../api/transactionService";
import { transactionService } from "../../../api/transactionService";
import { TransactionTypeEnum } from "../../../api/transactionService";

interface Props {
  onClose: () => void;
  accounts: AccountDto[];
  transactionType: TransactionTypeEnum;
  fromAccountId: string;
  onTransactionCreated: (transactionId: string) => void;
}

export const DepositWithdrawForm = ({
  onClose,
  accounts,
  transactionType,
  fromAccountId,
  onTransactionCreated,
}: Props) => {
  const [accountId, setAccountId] = useState(fromAccountId);
  const [amount, setAmount] = useState("");
  const [loading, setLoading] = useState(false);

  const account = accounts.find((a) => a.id === accountId);

  useEffect(() => {
    setAccountId(fromAccountId);
  }, [fromAccountId]);

  const handleSubmit = async () => {
    try {
      setLoading(true);

      if (transactionType === TransactionTypeEnum.Deposit) {
        const payload: DepositRequest = {
          accountNumber: accountId,
          amount: Number(amount),
        };

        const res = await transactionService.deposit(payload);
        onTransactionCreated(res.transactionId);
      } else if (transactionType === TransactionTypeEnum.Withdraw) {
        const payload: WithdrawRequest = {
          accountNumber: accountId,
          amount: Number(amount),
        };

        const res = await transactionService.withdraw(payload);
        onTransactionCreated(res.transactionId);
      }
    } catch (error) {
      console.error("Operation failed", error);
    } finally {
      setLoading(false);
    }
  };

  const submitText =
    transactionType === TransactionTypeEnum.Deposit ? "Deposit" : "Withdraw";

  return (
    <>
      <DialogContent>
        <Stack spacing={3} mt={1}>
          <Stack spacing={1}>
            <Typography variant="overline">Account number</Typography>
            <TextField
              select
              fullWidth
              value={accountId}
              onChange={(e) => setAccountId(e.target.value)}
            >
              {accounts.map((acc) => (
                <MenuItem key={acc.id} value={acc.id}>
                  {acc.id}
                </MenuItem>
              ))}
            </TextField>

            {account && (
              <Typography variant="body2" sx={{ opacity: 0.7 }}>
                Available balance:{" "}
                <strong>
                  {account.balance.toLocaleString("uk-UA", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2,
                  })}{" "}
                  UAH
                </strong>
              </Typography>
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
          disabled={!amount || !account}
        >
          {loading ? "Sending..." : submitText}
        </Button>
      </DialogActions>
    </>
  );
};
