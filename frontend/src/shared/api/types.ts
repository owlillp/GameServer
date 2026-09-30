export type PaginationRequest = {
  page: number;
  pageSize: number;
};

export type PagedResult<T> = {
  records: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

// Зеркалит Shared.SharedKernel.Responses.PaginationResponse<T> (items/...).
export type PaginationResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type InfinitePaginationRequest = {
  cursor?: Cursor | null;
  limit: number;
};

export type InfinitePagedResult<T> = {
  records: T[];
  nextCursor?: Cursor;
  hasNextPage: boolean;
};

export type Cursor = {
  id: string;
  value?: string;
};
