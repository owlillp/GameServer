import { Player } from "./types";
import { queryOptions } from "@tanstack/react-query";
import { PagedResult, PaginationRequest } from "@/src/shared/api/types";

// Реальная интеграция с бэкендом (раскомментировать, когда API будет готов).
// import { apiClient } from "@/src/shared/api/axios-instance";
// import { Envelope } from "@/src/shared/api/envelope";

export type GetPlayersRequest = {
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  pagination?: PaginationRequest;
};

export type CreatePlayerRequest = {
  nickname: string;
  email: string;
};

// --- MOCK ДАННЫЕ ---------------------------------------------------------
// Временная заглушка вместо запросов к бэкенду.

const MOCK_DELAY = 400;

const mockPlayers: Player[] = [
  {
    id: "1",
    nickname: "Neo",
    email: "neo@matrix.io",
    level: 42,
    isActive: true,
    createdAt: "2026-01-12T10:30:00.000Z",
  },
  {
    id: "2",
    nickname: "Trinity",
    email: "trinity@matrix.io",
    level: 38,
    isActive: true,
    createdAt: "2026-02-03T14:05:00.000Z",
  },
  {
    id: "3",
    nickname: "Morpheus",
    email: "morpheus@matrix.io",
    level: 50,
    isActive: false,
    createdAt: "2025-11-27T08:15:00.000Z",
  },
  {
    id: "4",
    nickname: "Cypher",
    email: "cypher@matrix.io",
    level: 21,
    isActive: false,
    createdAt: "2026-03-18T19:40:00.000Z",
  },
];

function delay(ms = MOCK_DELAY) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

// --- API -----------------------------------------------------------------

export const playersApi = {
  getPlayers: async (
    request: GetPlayersRequest,
  ): Promise<PagedResult<Player> | null> => {
    // Реальная реализация:
    // const response = await apiClient.get<Envelope<PagedResult<Player>>>(
    //   "/Players",
    //   { params: request },
    // );
    // return response.data.result;

    await delay();

    const page = request.pagination?.page ?? 1;
    const pageSize = request.pagination?.pageSize ?? 20;

    let records = [...mockPlayers];

    if (request.search) {
      const search = request.search.toLowerCase();
      records = records.filter(
        (player) =>
          player.nickname.toLowerCase().includes(search) ||
          player.email.toLowerCase().includes(search),
      );
    }

    if (request.isActive !== undefined) {
      records = records.filter(
        (player) => player.isActive === request.isActive,
      );
    }

    const totalCount = records.length;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const start = (page - 1) * pageSize;

    return {
      records: records.slice(start, start + pageSize),
      totalCount,
      page,
      pageSize,
      totalPages,
    };
  },

  createPlayer: async (request: CreatePlayerRequest) => {
    // Реальная реализация:
    // const response = await apiClient.post<Envelope<string>>(
    //   "/Players",
    //   request,
    // );
    // return response.data;

    await delay();

    const newPlayer: Player = {
      id: crypto.randomUUID(),
      nickname: request.nickname,
      email: request.email,
      level: 1,
      isActive: true,
      createdAt: new Date().toISOString(),
    };

    mockPlayers.unshift(newPlayer);

    return { isSuccess: true, isFailure: false };
  },

  deletePlayer: async (playerId: string) => {
    // Реальная реализация:
    // const response = await apiClient.delete<Envelope>(`/Players/${playerId}`);
    // return response.data;

    await delay();

    const index = mockPlayers.findIndex((player) => player.id === playerId);
    if (index !== -1) mockPlayers.splice(index, 1);

    return { isSuccess: true, isFailure: false };
  },
};

export const playersQueryOptions = {
  baseKey: "players",

  getPlayersOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () =>
        playersApi.getPlayers({
          pagination: { page, pageSize },
          sortBy: "nickname",
          sortDirection: "asc",
        }),
      queryKey: [playersQueryOptions.baseKey, { page, pageSize }],
    });
  },
};
