import { apiClient } from "@/src/shared/api/axios-instance";
import {
  assertEnvelopeSuccess,
  Envelope,
  unwrapEnvelope,
} from "@/src/shared/api/envelope";
import { keepPreviousData, queryOptions } from "@tanstack/react-query";
import type {
  ClanDetails,
  ClansPage,
  ClansRequest,
  ClanSummary,
  CreateClanRequest,
} from "./types";

// Все методы кланов требуют access-токен AuthService (Bearer ставит интерсептор).
export const clansApi = {
  getClans: async (
    request: ClansRequest,
    signal?: AbortSignal,
  ): Promise<ClansPage> => {
    const response = await apiClient.get<Envelope<ClansPage>>("/clans", {
      params: {
        page: request.page,
        pageSize: request.pageSize,
        search: request.search || undefined,
      },
      signal,
    });

    return unwrapEnvelope(response.data);
  },

  getClan: async (
    clanId: string,
    signal?: AbortSignal,
  ): Promise<ClanDetails> => {
    const response = await apiClient.get<Envelope<ClanDetails>>(
      `/clans/${clanId}`,
      { signal },
    );

    return unwrapEnvelope(response.data);
  },

  createClan: async (request: CreateClanRequest): Promise<ClanSummary> => {
    const response = await apiClient.post<Envelope<ClanSummary>>(
      "/clans",
      request,
    );

    return unwrapEnvelope(response.data);
  },

  joinClan: async (clanId: string): Promise<void> => {
    const response = await apiClient.post<Envelope>(`/clans/${clanId}/join`);
    assertEnvelopeSuccess(response.data);
  },

  leaveClan: async (clanId: string): Promise<void> => {
    const response = await apiClient.post<Envelope>(`/clans/${clanId}/leave`);
    assertEnvelopeSuccess(response.data);
  },

  deleteClan: async (clanId: string): Promise<void> => {
    const response = await apiClient.delete<Envelope>(`/clans/${clanId}`);
    assertEnvelopeSuccess(response.data);
  },

  // Только Admin (permission clans.admin).
  kickMember: async (clanId: string, userId: string): Promise<void> => {
    const response = await apiClient.post<Envelope>(
      `/clans/${clanId}/members/${userId}/kick`,
    );
    assertEnvelopeSuccess(response.data);
  },
};

export const clansQueryKeys = {
  all: ["clans"] as const,
  list: (request: ClansRequest) =>
    [...clansQueryKeys.all, "list", request] as const,
  details: (clanId: string) =>
    [...clansQueryKeys.all, "details", clanId] as const,
};

export const clansListQueryOptions = (request: ClansRequest) =>
  queryOptions({
    queryKey: clansQueryKeys.list(request),
    queryFn: ({ signal }) => clansApi.getClans(request, signal),
    placeholderData: keepPreviousData,
    staleTime: 10_000,
  });

export const clanDetailsQueryOptions = (clanId: string) =>
  queryOptions({
    queryKey: clansQueryKeys.details(clanId),
    queryFn: ({ signal }) => clansApi.getClan(clanId, signal),
    staleTime: 5_000,
  });
