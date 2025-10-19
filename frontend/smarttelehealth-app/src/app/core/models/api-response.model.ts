/**
 * Standard API Response structure from backend
 * Matches JsonModel from backend/SmartTelehealth.Application/DTOs/JsonModel.cs
 */
export interface ApiResponse<T = any> {
  statusCode: number;
  message: string;
  data: T;
  meta?: PaginationMeta;
}

/**
 * Pagination metadata from backend Meta class
 */
export interface PaginationMeta {
  totalRecords: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  defaultPageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Generic paginated response
 */
export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}


