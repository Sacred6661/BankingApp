import axiosClient from "./axiosClient";

export interface LoginRequest {
    email: string;
    password: string;
  }

  export interface RefreshTokenRequest {
    refreshToken: string;
  }
  
  export interface LoginResponse {
    access_token: string;
    expires_in: number;
    token_type: string;
    refresh_token: string;
  }

  export interface MeResponse {
    id: string;
    email: string;
    roles: string[];
  }

  export interface RegisterRequest {
    email: string;
    password: string;
  }
  
  export const authService = {
    login: async (data: LoginRequest): Promise<LoginResponse> => {
      const response = await axiosClient.post<LoginResponse>("/auth/login", data);
      return response.data;
    },
  
    refreshToken: async (data: RefreshTokenRequest): Promise<LoginResponse> => {
      const response = await axiosClient.post<LoginResponse>("/auth/refresh-token", data);
      return response.data;
    },

    register: async (data: RegisterRequest): Promise<LoginResponse> => {
      const response = await axiosClient.post<LoginResponse>("/auth/register", data);
      return response.data;
    },

    me: async (): Promise<MeResponse> => {
      try {
        const response = await axiosClient.get<MeResponse>("/auth/me");
        return response.data;
      } catch (err) {
        // if 401 — user is unauthorized
        throw err;
      }
    }
  };
