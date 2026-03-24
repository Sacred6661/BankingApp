import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "../app/store";
import {
  createAccount,
  fetchAccounts,
  updateAccountBalance,
} from "../features/account/accountsSlice";

export const useAccounts = () => {
  const dispatch = useDispatch<AppDispatch>();

  const { accountsList, loading, error } = useSelector(
    (state: RootState) => state.accounts,
  );

  return {
    accountsList,
    loading,
    error,
    createAccount: () => dispatch(createAccount()),
    fetchAccounts: () => dispatch(fetchAccounts()),
    updateAccountBalance: (accountId: string, balance: number) =>
      dispatch(updateAccountBalance({ accountId, balance })),
  };
};
