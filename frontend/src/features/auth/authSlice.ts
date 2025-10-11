import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { authService } from "../../api/authService";
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  MeResponse,
} from "../../api/authService";

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
  user: MeResponse | null;
}

const initialState: AuthState = {
  token: null,
  refreshToken: null,
  isAuthenticated: false,
  loading: false,
  error: null,
  user: null,
};

// login
export const login = createAsyncThunk<LoginResponse, LoginRequest>(
  "auth/login",
  async (credentials, thunkAPI) => {
    try {
      return await authService.login(credentials);
    } catch (err: any) {
      return thunkAPI.rejectWithValue(err.response?.data || "Login failed");
    }
  }
);

// register
export const register = createAsyncThunk<LoginResponse, RegisterRequest>(
  "auth/register",
  async (data, thunkAPI) => {
    try {
      return await authService.register(data);
    } catch (err: any) {
      return thunkAPI.rejectWithValue(err.response?.data || "Register failed");
    }
  }
);

// 🔹 fetchMe
export const fetchMe = createAsyncThunk<MeResponse>(
  "auth/me",
  async (_, thunkAPI) => {
    try {
      return await authService.me();
    } catch (err: any) {
      return thunkAPI.rejectWithValue(err.response?.data || "Unauthorized");
    }
  }
);

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    logout: (state) => {
      state.token = null;
      state.refreshToken = null;
      state.isAuthenticated = false;
      state.error = null;
      state.user = null;
    },
  },
  extraReducers: (builder) => {
    // login
    builder
      .addCase(login.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.loading = false;
        state.token = action.payload.access_token;
        state.refreshToken = action.payload.refresh_token;
        state.isAuthenticated = true;
      })
      .addCase(login.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      });

    // register
    builder
      .addCase(register.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.loading = false;
        state.token = action.payload.access_token;
        state.refreshToken = action.payload.refresh_token;
        state.isAuthenticated = true;
      })
      .addCase(register.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      });

    // fetchMe
    builder
      .addCase(fetchMe.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchMe.fulfilled, (state, action) => {
        state.loading = false;
        state.isAuthenticated = true;
        state.user = action.payload;
      })
      .addCase(fetchMe.rejected, (state) => {
        state.loading = false;
        state.isAuthenticated = false;
        state.user = null;
      });
  },
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;
