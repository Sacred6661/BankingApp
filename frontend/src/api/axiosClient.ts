import axios, { AxiosError } from "axios";
import type { InternalAxiosRequestConfig } from "axios";

import { connection } from "../services/signalr/connection";

const API_URL = "https://localhost:7023";

const axiosClient = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

// To deny multiple redirecting
let isRedirecting = false;
// Save URL, to not rediredt, if already on login
let hasRedirected = false;

axiosClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // If not redirecting, not send new requests
    if (isRedirecting) {
      return Promise.reject(new Error("Redirecting to login..."));
    }
    return config;
  },
  (error) => Promise.reject(error),
);

axiosClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    // 403 sometimes indicates that authorization is required too
    const isUnauthorized =
      error.response?.status === 401 || error.response?.status === 403;

    // Do not redirecting if
    // - redirecting already
    // - already on login page
    // - this is not auth error
    if (!isUnauthorized || isRedirecting || hasRedirected) {
      return Promise.reject(error);
    }

    // Check if we're already logged in
    const currentPath = window.location.pathname;
    if (currentPath === "/login" || currentPath === "/register") {
      return Promise.reject(error);
    }

    isRedirecting = true;
    hasRedirected = true;

    try {
      await connection.stop();
    } catch (err) {
      console.error("Error stopping SignalR connection:", err);
    }

    // Clear local storage (optional)
    // localStorage.removeItem(‘token’);
    // sessionStorage.clear();

    // redirect
    window.location.href = "/login";

    // Return the rejected token to stop the chain
    return Promise.reject(error);
  },
);

// Function to reset the redirect state (call after a successful login)
export const resetRedirectState = () => {
  isRedirecting = false;
  hasRedirected = false;
};

export default axiosClient;
