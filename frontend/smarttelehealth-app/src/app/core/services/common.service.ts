import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

/**
 * Standard API Response structure from backend
 */
export interface ApiResponse<T = any> {
  statusCode: number;
  message: string;
  data: T;
  meta?: PaginationMeta;
}

/**
 * Pagination metadata from backend
 */
export interface PaginationMeta {
  totalRecords: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * CENTRALIZED COMMON SERVICE
 * 
 * This is the ONLY service in the application that directly uses HttpClient.
 * All other services (SubscriptionService, BillingService, etc.) must call
 * methods from this service instead of using HttpClient directly.
 * 
 * Benefits:
 * - Centralized error handling
 * - Consistent request/response handling
 * - Easy to add global interceptors
 * - Single point for API configuration
 */
@Injectable({
  providedIn: 'root'
})
export class CommonService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {
    console.log('[CommonService] Initialized with base URL:', this.baseUrl);
  }

  /**
   * HTTP GET request
   * @param endpoint - API endpoint (without base URL)
   * @param params - Query parameters as object
   * @returns Observable of ApiResponse
   */
  get<T>(endpoint: string, params?: any): Observable<ApiResponse<T>> {
    const url = `${this.baseUrl}/${endpoint}`;
    const options = {
      params: this.buildHttpParams(params)
    };

    return this.http.get<ApiResponse<T>>(url, options).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * HTTP POST request
   * @param endpoint - API endpoint (without base URL)
   * @param body - Request body
   * @param params - Optional query parameters
   * @returns Observable of ApiResponse
   */
  post<T>(endpoint: string, body: any, params?: any): Observable<ApiResponse<T>> {
    const url = `${this.baseUrl}/${endpoint}`;
    const options = {
      params: this.buildHttpParams(params)
    };

    return this.http.post<ApiResponse<T>>(url, body, options).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * HTTP PUT request
   * @param endpoint - API endpoint (without base URL)
   * @param body - Request body
   * @param params - Optional query parameters
   * @returns Observable of ApiResponse
   */
  put<T>(endpoint: string, body: any, params?: any): Observable<ApiResponse<T>> {
    const url = `${this.baseUrl}/${endpoint}`;
    const options = {
      params: this.buildHttpParams(params)
    };

    return this.http.put<ApiResponse<T>>(url, body, options).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * HTTP DELETE request
   * @param endpoint - API endpoint (without base URL)
   * @param params - Optional query parameters
   * @returns Observable of ApiResponse
   */
  delete<T>(endpoint: string, params?: any): Observable<ApiResponse<T>> {
    const url = `${this.baseUrl}/${endpoint}`;
    const options = {
      params: this.buildHttpParams(params)
    };

    return this.http.delete<ApiResponse<T>>(url, options).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * HTTP PATCH request
   * @param endpoint - API endpoint (without base URL)
   * @param body - Request body
   * @param params - Optional query parameters
   * @returns Observable of ApiResponse
   */
  patch<T>(endpoint: string, body: any, params?: any): Observable<ApiResponse<T>> {
    const url = `${this.baseUrl}/${endpoint}`;
    const options = {
      params: this.buildHttpParams(params)
    };

    return this.http.patch<ApiResponse<T>>(url, body, options).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Helper method to build HttpParams from object
   * Handles null/undefined values
   */
  private buildHttpParams(params: any): HttpParams {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        if (value !== null && value !== undefined) {
          // Handle arrays
          if (Array.isArray(value)) {
            value.forEach(v => {
              httpParams = httpParams.append(key, v.toString());
            });
          } else {
            httpParams = httpParams.set(key, value.toString());
          }
        }
      });
    }
    
    return httpParams;
  }

  /**
   * Centralized error handling
   * All HTTP errors are processed here
   */
  private handleError(error: any): Observable<never> {
    console.error('[CommonService] API Error:', error);
    
    let errorMessage = 'An unexpected error occurred';
    let statusCode = error.status || 500;

    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    } else if (typeof error.error === 'string') {
      errorMessage = error.error;
    }

    // Handle specific error codes
    if (statusCode === 401) {
      errorMessage = 'Unauthorized. Please login again.';
    } else if (statusCode === 403) {
      errorMessage = 'Access forbidden. Insufficient permissions.';
    } else if (statusCode === 404) {
      errorMessage = 'Resource not found.';
    } else if (statusCode === 500) {
      errorMessage = 'Server error. Please try again later.';
    }

    return throwError(() => ({
      statusCode,
      message: errorMessage,
      originalError: error
    }));
  }
}


