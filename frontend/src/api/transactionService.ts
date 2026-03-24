import axiosClient from "./axiosClient";

export interface DepositRequest {
  accountNumber: string;
  amount: number;
}

export interface WithdrawRequest {
  accountNumber: string;
  amount: number;
}

export interface TransferRequest {
  fromAccountNumber: string;
  toAccountNumber: string;
  amount: number;
}

export enum TransactionTypeEnum {
  Deposit = 1,
  Withdrawal = 2,
  Transfer = 3,
}

export enum TransactionStatusEnum {
  Pending = 1,
  Accepted = 2,
  Rejected = 3,
}

export interface TransactionDto {
  transactionId: string;
  transactionType: TransactionTypeEnum;
  fromAccount: string;
  toAccount?: string | null;
  amount: number;
  timestamp: string;
  performedBy?: string | null;
  transactionStatus: TransactionStatusEnum;
}

export const transactionService = {
  deposit: async (data: DepositRequest): Promise<TransactionDto> => {
    const response = await axiosClient.post<TransactionDto>(
      "/transactions/deposit",
      data,
    );
    return response.data;
  },
  withdraw: async (data: WithdrawRequest): Promise<TransactionDto> => {
    const response = await axiosClient.post<TransactionDto>(
      "/transactions/withdraw",
      data,
    );
    return response.data;
  },
  transfer: async (data: TransferRequest): Promise<TransactionDto> => {
    const response = await axiosClient.post<TransactionDto>(
      "/transactions/transfer",
      data,
    );
    return response.data;
  },
};
