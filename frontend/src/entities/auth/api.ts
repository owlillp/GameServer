import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope, unwrapEnvelope } from "@/src/shared/api/envelope";
import { queryOptions } from "@tanstack/react-query";
import type {
  JwtLoginResponse,
  LoginRequest,
  LoginResponse,
  ProfileResponse,
  RegisterRequest,
  RegisterResponse,
} from "./types";

export const authApi = {
  // === cookie-схема (Identity.Application) ===
  register: async (request: RegisterRequest): Promise<RegisterResponse> => {
    const response = await apiClient.post<Envelope<RegisterResponse>>(
      "/auth/register",
      request,
    );

    return unwrapEnvelope(response.data);
  },

  login: async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<Envelope<LoginResponse>>(
      "/auth/login",
      request,
    );

    return unwrapEnvelope(response.data);
  },

  // === jwt-схема (Bearer) ===
  jwtRegister: async (request: RegisterRequest): Promise<RegisterResponse> => {
    const response = await apiClient.post<Envelope<RegisterResponse>>(
      "/auth/jwt/register",
      request,
    );

    return unwrapEnvelope(response.data);
  },

  jwtLogin: async (request: LoginRequest): Promise<JwtLoginResponse> => {
    const response = await apiClient.post<Envelope<JwtLoginResponse>>(
      "/auth/jwt/login",
      request,
    );

    return unwrapEnvelope(response.data);
  },

  // === профиль (одна ручка, обе схемы) ===
  // Сервер определяет схему по тому, что валидно: cookie из withCredentials
  // или Bearer из request-интерсептора.
  profile: async (signal?: AbortSignal): Promise<ProfileResponse> => {
    const response = await apiClient.get<Envelope<ProfileResponse>>(
      "/auth/profile",
      { signal },
    );

    return unwrapEnvelope(response.data);
  },
};

export const authQueryKeys = {
  all: ["auth"] as const,
  profile: () => [...authQueryKeys.all, "profile"] as const,
};

// retry: false — ретраить 401 бессмысленно без действий пользователя.
export const profileQueryOptions = () =>
  queryOptions({
    queryKey: authQueryKeys.profile(),
    queryFn: ({ signal }) => authApi.profile(signal),
    retry: false,
    staleTime: 0,
  });
