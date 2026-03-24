import { Dialog, DialogTitle } from "@mui/material";
import { useState } from "react";
import type { AccountDto } from "../../../api/accountService";
import { TransferForm } from "./TransferForm";
import TransferProcessing from "./TransferProcessing";
import { TransferResult } from "./TransferResult";
import { useTransactionUpdates } from "../hooks/useTransactionUpdates";
import { TransactionStatusEnum } from "../../../api/transactionService";
import { useAccounts } from "../../../hooks/useAccounts";
import toast from "react-hot-toast";

interface Props {
  open: boolean;
  onClose: () => void;
  accounts: AccountDto[];
  fromAccountId: string; // активна картка зі слайдера
}

enum StepsEnum {
  Form = 1,
  Processing = 2,
  Result = 3,
}

export const TransferDialog = ({
  open,
  onClose,
  accounts,
  fromAccountId,
}: Props) => {
  const { updateAccountBalance } = useAccounts();

  const [step, setStep] = useState<StepsEnum>(StepsEnum.Form);
  const [transactionId, setTransactionId] = useState<string | null>(null);
  const [result, setResult] = useState<TransactionStatusEnum | null>(null);

  //signalR
  useTransactionUpdates(transactionId, (data) => {
    if (data.transactionStatus === TransactionStatusEnum.Accepted) {
      setResult(TransactionStatusEnum.Accepted);
      updateAccountBalance(data.fromAccountNumber, data.fromAccountBalance);
      updateAccountBalance(data.toAccountNumber, data.toAccountBalance);
      toast.success("The operation was successful!");
      setStep(StepsEnum.Result);
    }

    if (data.transactionStatus === TransactionStatusEnum.Rejected) {
      setResult(TransactionStatusEnum.Rejected);
      setStep(StepsEnum.Result);
      toast.error(data.details);
    }
  });

  const handleTransactionCreated = (txId: string) => {
    setTransactionId(txId);
    setStep(StepsEnum.Processing);
  };

  const handleClose = () => {
    setStep(StepsEnum.Form);
    setTransactionId(null);
    setResult(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>Transfer</DialogTitle>

      {step === StepsEnum.Form && (
        <TransferForm
          onClose={handleClose}
          accounts={accounts}
          fromAccountId={fromAccountId}
          onTransactionCreated={handleTransactionCreated}
        />
      )}
      {step === StepsEnum.Processing && <TransferProcessing />}
      {step === StepsEnum.Result && (
        <TransferResult result={result} onClose={handleClose} />
      )}
    </Dialog>
  );
};
