import axios, { AxiosError } from "axios";

const API_URL = "https://localhost:7023";

const axiosClient = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

// Response interseptor
axiosClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    // 401 skip
    return Promise.reject(error);
  }
);

export default axiosClient;
