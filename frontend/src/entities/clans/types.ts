import type { PaginationResponse } from "@/src/shared/api/types";

// Зеркалит контракты ClanService.Contracts (camelCase).

export type ClanSummary = {
  id: string;
  name: string;
  tag: string;
  leaderId: string;
  memberCount: number;
  createdAt: string;
};

export type ClanMember = {
  userId: string;
  name: string | null;
  userName: string | null;
  email: string | null;
  joinedAt: string;
};

export type ClanDetails = {
  id: string;
  name: string;
  tag: string;
  description: string | null;
  leaderId: string;
  createdAt: string;
  updatedAt: string;
  members: ClanMember[];
};

export type ClansRequest = {
  page: number;
  pageSize: number;
  search: string;
};

export type CreateClanRequest = {
  name: string;
  tag: string;
  description?: string | null;
};

export type ClansPage = PaginationResponse<ClanSummary>;
