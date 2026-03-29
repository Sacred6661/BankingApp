import { useEffect } from "react";
import {
  onTransactionUpdated,
  offTransactionUpdated,
} from "../../../services/signalr/transactionHub";
//import { startConnection } from "../../../services/signalr/connection";
import {
  TransactionStatusEnum,
  TransactionTypeEnum,
} from "../../../api/transactionService";

export interface TransactionUpdateDto {
  transactionId: string;
  transactionType: TransactionTypeEnum;
  fromAccountNumber: string;
  fromAccountBalance: number;
  toAccountNumber: string;
  toAccountBalance: number;
  amount: number;
  transactionStatus: TransactionStatusEnum;
  details: string;
}

export const useTransactionUpdates = (
  transactionId: string | null,
  onUpdate: (data: TransactionUpdateDto) => void,
) => {
  useEffect(() => {
    if (!transactionId) return;

    const setup = async () => {
      onTransactionUpdated(onUpdate);
    };

    setup();

    return () => {
      offTransactionUpdated(onUpdate);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [transactionId]);
};
