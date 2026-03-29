import axios, { AxiosError } from "axios";
import { connection } from "../services/signalr/connection";

const API_URL = "https://localhost:7023";

const axiosClient = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

let isRedirecting = false;

axiosClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401 && !isRedirecting) {
      isRedirecting = true;

      await connection.stop(); // 🔥 ключова штука

      window.location.href = "/login";
    }

    return Promise.reject(error);
  },
);

export default axiosClient;
