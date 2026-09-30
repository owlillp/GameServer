// Зеркалит контракты AuthService.Contracts.Admin (camelCase).

export type AdminUser = {
  id: string;
  email: string | null;
  userName: string | null;
  displayName: string | null;
  roles: string[];
  isLockedOut: boolean;
  createdAt: string;
};

export type AdminStats = {
  totalUsers: number;
  adminCount: number;
  moderatorCount: number;
  lockedOutCount: number;
};

export type AdminUsersRequest = {
  page: number;
  pageSize: number;
  search: string;
};

// Зеркалит Shared.SharedKernel.Responses.PaginationResponse<T>.
export type PaginationResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
