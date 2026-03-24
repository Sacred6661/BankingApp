// features/accounts/accountsSlice.ts
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import { accountService } from "../../api/accountService";
import type { AccountDto } from "../../api/accountService";

interface AccountsState {
  accountsList: AccountDto[];
  loading: boolean;
  error: string | null;
}

const initialState: AccountsState = {
  accountsList: [],
  loading: false,
  error: null,
};

export const createAccount = createAsyncThunk(
  "accounts/createAccount",
  async () => {
    return await accountService.createAccount({});
  },
);

// thunk для підвантаження рахунків
export const fetchAccounts = createAsyncThunk(
  "accounts/fetchAccounts",
  async () => {
    return await accountService.getAllAccounts(); // твій API
  },
);

const accountsSlice = createSlice({
  name: "accounts",
  initialState,
  reducers: {
    updateAccountBalance: (
      state,
      action: PayloadAction<{ accountId: string; balance: number }>,
    ) => {
      const account = state.accountsList.find(
        (a) => a.id === action.payload.accountId,
      );
      if (account) {
        account.balance = action.payload.balance;
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchAccounts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        fetchAccounts.fulfilled,
        (state, action: PayloadAction<AccountDto[]>) => {
          state.loading = false;
          state.accountsList = action.payload.sort((a, b) => {
            const aTime = Date.parse(a.createdAt);
            const bTime = Date.parse(b.createdAt);
            return bTime - aTime;
          });
        },
      )
      .addCase(fetchAccounts.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      .addCase(createAccount.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        createAccount.fulfilled,
        (state, action: PayloadAction<AccountDto>) => {
          state.loading = false;
          state.accountsList.push(action.payload);
          state.accountsList.sort((a, b) => {
            const aTime = Date.parse(a.createdAt);
            const bTime = Date.parse(b.createdAt);
            return bTime - aTime;
          });
        },
      )
      .addCase(createAccount.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { updateAccountBalance } = accountsSlice.actions;
export default accountsSlice.reducer;
