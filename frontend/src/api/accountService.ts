import axiosClient from "./axiosClient";

export interface CreateAccountRequest {
  userId?: string | null;
  initialBalance?: number;
}

export interface AccountDto {
  id: string;
  userId: string;
  balance: number;
  isActive: boolean;
  createdAt: string;
}

export const accountService = {
  createAccount: async (data: CreateAccountRequest): Promise<AccountDto> => {
    const response = await axiosClient.post<AccountDto>("/accounts", data);
    return response.data;
  },
  getAccountById: async (accountId: string): Promise<AccountDto> => {
    const response = await axiosClient.get<AccountDto>(
      `/accounts/${accountId}`,
    );
    return response.data;
  },
  getAllAccounts: async (
    everyoneAccounts: boolean = false,
  ): Promise<AccountDto[]> => {
    const response = await axiosClient.get<AccountDto[]>("/accounts", {
      params: { getAllAccounts: everyoneAccounts },
    });
    return response.data;
  },
};
