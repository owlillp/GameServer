import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope, unwrapEnvelope } from "@/src/shared/api/envelope";
import { queryOptions } from "@tanstack/react-query";
import type {
  JwtLoginResponse,
  JwtSessionStatusResponse,
  LoginRequest,
  LoginResponse,
  ProfileResponse,
  RegisterRequest,
  RegisterResponse,
} from "./types";

export const authApi = {
  // === cookie-схема (Identity.Application) ===
  // skipAuthRefresh — при 401 не пытаемся обновлять JWT (неверные креды/другая схема).
  register: async (request: RegisterRequest): Promise<RegisterResponse> => {
    const response = await apiClient.post<Envelope<RegisterResponse>>(
      "/auth/register",
      request,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  login: async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<Envelope<LoginResponse>>(
      "/auth/login",
      request,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  // === jwt-схема (Bearer + HttpOnly refresh-cookie) ===
  jwtRegister: async (request: RegisterRequest): Promise<RegisterResponse> => {
    const response = await apiClient.post<Envelope<RegisterResponse>>(
      "/auth/jwt/register",
      request,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  jwtLogin: async (request: LoginRequest): Promise<JwtLoginResponse> => {
    const response = await apiClient.post<Envelope<JwtLoginResponse>>(
      "/auth/jwt/login",
      request,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  // Меняет access-токен по HttpOnly refresh-cookie (сервер также ротирует cookie).
  jwtRefresh: async (): Promise<JwtLoginResponse> => {
    const response = await apiClient.post<Envelope<JwtLoginResponse>>(
      "/auth/jwt/refresh",
      undefined,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  // Отзывает refresh-сессию и удаляет cookie. Ответ без result — не разворачиваем.
  jwtLogout: async (): Promise<void> => {
    await apiClient.post<Envelope>("/auth/jwt/logout", undefined, {
      skipAuthRefresh: true,
    });
  },

  jwtSession: async (signal?: AbortSignal): Promise<JwtSessionStatusResponse> => {
    const response = await apiClient.get<Envelope<JwtSessionStatusResponse>>(
      "/auth/jwt/session",
      { signal, skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  // === профиль (одна ручка, обе схемы) ===
  // Здесь skipAuthRefresh НЕ ставим: на 401 интерсептор обновит токен и повторит.
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
  jwtSession: () => [...authQueryKeys.all, "jwt-session"] as const,
};

// retry: false — ретраить 401 бессмысленно без действий пользователя.
export const profileQueryOptions = () =>
  queryOptions({
    queryKey: authQueryKeys.profile(),
    queryFn: ({ signal }) => authApi.profile(signal),
    retry: false,
    staleTime: 0,
  });

export const jwtSessionQueryOptions = () =>
  queryOptions({
    queryKey: authQueryKeys.jwtSession(),
    queryFn: ({ signal }) => authApi.jwtSession(signal),
    retry: false,
    staleTime: 0,
  });
