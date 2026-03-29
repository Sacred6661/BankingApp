import { Dialog, DialogTitle } from "@mui/material";
import { useState } from "react";
import type { AccountDto } from "../../../api/accountService";
import { useTransactionUpdates } from "../hooks/useTransactionUpdates";
import { TransactionStatusEnum } from "../../../api/transactionService";
import { useAccounts } from "../../../hooks/useAccounts";
import { DepositWithdrawForm } from "./DepositWithdrawForm";
import { TransactionTypeEnum } from "../../../api/transactionService";
import TransferProcessing from "./TransferProcessing";
import { TransferResult } from "./TransferResult";
import toast from "react-hot-toast";

interface Props {
  open: boolean;
  onClose: () => void;
  accounts: AccountDto[];
  fromAccountId: string;
  transactionType: TransactionTypeEnum;
}

enum StepsEnum {
  Form = 1,
  Processing = 2,
  Result = 3,
}

export const DepositWithdrawDialog = ({
  open,
  onClose,
  accounts,
  fromAccountId,
  transactionType,
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

  const dialogTitle =
    transactionType === TransactionTypeEnum.Deposit ? "Deposit" : "Withdraw";

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{dialogTitle}</DialogTitle>

      {step === StepsEnum.Form && (
        <DepositWithdrawForm
          onClose={handleClose}
          accounts={accounts}
          transactionType={transactionType}
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
