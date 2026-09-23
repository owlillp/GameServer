import { useSessionStore } from "@/src/shared/stores/session-store";
import axios, { AxiosHeaders } from "axios";
import qs from "qs";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./errors";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export const apiClient = axios.create({
  baseURL: BASE_URL,
  // Без withCredentials axios не отправляет Identity-cookie на запросы —
  // cookie-схема логина тогда не работает (GET /auth/profile вернёт 401).
  withCredentials: true,
  headers: { "Content-Type": "application/json" },
  paramsSerializer: (params) =>
    qs.stringify(params, { indices: false, allowDots: true }),
});

// Если в сторе лежит JWT (схема "jwt") — добавляем Authorization: Bearer.
// Cookie-схема header не ставит: срабатывает cookie, отправленная withCredentials.
apiClient.interceptors.request.use((config) => {
  const token = useSessionStore.getState().accessToken;

  if (token) {
    config.headers = AxiosHeaders.from(config.headers);
    config.headers.set("Authorization", `Bearer ${token}`);
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => {
    const envelope = response.data as Envelope;

    if (envelope?.isError && envelope.error) {
      return Promise.reject(new EnvelopeError(envelope.error));
    }

    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as Envelope;

      if (envelope?.isError && envelope.error) {
        return Promise.reject(new EnvelopeError(envelope.error));
      }
    }

    return Promise.reject(error);
  },
);
