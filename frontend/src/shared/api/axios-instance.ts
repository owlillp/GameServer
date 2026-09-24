import { useSessionStore } from "@/src/shared/stores/session-store";
import axios, { AxiosHeaders, type InternalAxiosRequestConfig } from "axios";
import qs from "qs";
import { Envelope } from "./envelope";
import { EnvelopeError, ErrorType } from "./errors";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

declare module "axios" {
  export interface AxiosRequestConfig {
    // Не пытаться обновлять access-токен при 401 (login/register/refresh/session).
    skipAuthRefresh?: boolean;
    // Внутренний флаг: запрос уже повторяли после refresh.
    _retry?: boolean;
  }

  export interface InternalAxiosRequestConfig {
    skipAuthRefresh?: boolean;
    _retry?: boolean;
  }
}

const apiClientConfig = {
  baseURL: BASE_URL,
  // Без withCredentials axios не отправляет Identity-cookie и refresh-cookie.
  withCredentials: true,
  headers: { "Content-Type": "application/json" },
  paramsSerializer: (params: unknown) =>
    qs.stringify(params, { indices: false, allowDots: true }),
};

export const apiClient = axios.create(apiClientConfig);

// Отдельный клиент для refresh — без интерсептора, чтобы не было рекурсии.
const refreshClient = axios.create(apiClientConfig);

type JwtRefreshPayload = {
  accessToken: string;
  expiresAt: string;
};

// Дедупликация: параллельные 401 ждут один и тот же refresh.
let refreshPromise: Promise<string> | null = null;

// Если в сторе есть JWT — добавляем Authorization: Bearer.
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
  async (error) => {
    if (axios.isAxiosError(error)) {
      const config = error.config as InternalAxiosRequestConfig | undefined;

      // 401 по JWT-схеме -> один раз пробуем обновить токен и повторить запрос.
      if (config && shouldRefresh(error.response?.status, config)) {
        config._retry = true;
        try {
          const token = await getRefreshPromise();
          config.headers = AxiosHeaders.from(config.headers);
          config.headers.set("Authorization", `Bearer ${token}`);
          return apiClient(config);
        } catch (refreshError) {
          useSessionStore.getState().clear();
          return Promise.reject(refreshError);
        }
      }

      if (error.response?.data) {
        const envelope = error.response.data as Envelope;

        if (envelope?.isError && envelope.error) {
          return Promise.reject(new EnvelopeError(envelope.error));
        }
      }
    }

    return Promise.reject(error);
  },
);

function shouldRefresh(
  status: number | undefined,
  config: { skipAuthRefresh?: boolean; _retry?: boolean },
): boolean {
  return (
    status === 401 &&
    !config.skipAuthRefresh &&
    !config._retry &&
    useSessionStore.getState().scheme === "jwt"
  );
}

function getRefreshPromise(): Promise<string> {
  refreshPromise ??= performRefresh().finally(() => {
    refreshPromise = null;
  });
  return refreshPromise;
}

async function performRefresh(): Promise<string> {
  const response = await refreshClient.post<Envelope<JwtRefreshPayload>>(
    "/auth/jwt/refresh",
  );
  const envelope = response.data;

  if (envelope.isError || !envelope.result) {
    throw new EnvelopeError(envelope.error ?? { messages: [], type: ErrorType.FAILURE });
  }

  useSessionStore.getState().setJwt(envelope.result.accessToken);
  return envelope.result.accessToken;
}
