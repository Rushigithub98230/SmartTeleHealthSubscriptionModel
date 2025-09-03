import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface ApiResponse<T = any> {
  data: T;
  message: string;
  statusCode: number;
  meta?: {
    totalRecords: number;
    pageSize: number;
    currentPage: number;
    totalPages: number;
  };
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class CommonService {
  private readonly baseUrl = 'http://localhost:61376'; // Updated to match backend port

  constructor(private http: HttpClient) {}

  /**
   * Get request with optional query parameters
   */
  getWithAuth<T>(endpoint: string, params?: any): Observable<ApiResponse<T>> {
    const httpParams = this.buildHttpParams(params);
    const headers = this.getAuthHeaders();
    
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, { 
      headers, 
      params: httpParams 
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Post request with body data
   */
  postWithAuth<T>(endpoint: string, body: any, params?: any): Observable<ApiResponse<T>> {
    const httpParams = this.buildHttpParams(params);
    const headers = this.getAuthHeaders();
    
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, body, { 
      headers, 
      params: httpParams 
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Put request with body data
   */
  putWithAuth<T>(endpoint: string, body: any, params?: any): Observable<ApiResponse<T>> {
    const httpParams = this.buildHttpParams(params);
    const headers = this.getAuthHeaders();
    
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, body, { 
      headers, 
      params: httpParams 
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete request
   */
  deleteWithAuth<T>(endpoint: string, params?: any): Observable<ApiResponse<T>> {
    const httpParams = this.buildHttpParams(params);
    const headers = this.getAuthHeaders();
    
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, { 
      headers, 
      params: httpParams 
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Build HTTP parameters from object
   */
  private buildHttpParams(params: any): HttpParams {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        if (value !== null && value !== undefined && value !== '') {
          if (Array.isArray(value)) {
            value.forEach(item => {
              httpParams = httpParams.append(key, item.toString());
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
   * Get authentication headers
   */
  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('adminToken') || localStorage.getItem('token');
    
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }

  /**
   * Handle HTTP errors
   */
  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Client Error: ${error.error.message}`;
    } else {
      // Server-side error
      if (error.error && error.error.message) {
        errorMessage = error.error.message;
      } else if (error.status === 0) {
        errorMessage = 'Unable to connect to server. Please check your connection.';
      } else if (error.status === 401) {
        errorMessage = 'Unauthorized. Please log in again.';
        // Optionally redirect to login
        localStorage.removeItem('adminToken');
        localStorage.removeItem('token');
      } else if (error.status === 403) {
        errorMessage = 'Access denied. You do not have permission to perform this action.';
      } else if (error.status === 404) {
        errorMessage = 'Resource not found.';
      } else if (error.status === 500) {
        errorMessage = 'Internal server error. Please try again later.';
      } else {
        errorMessage = `Server Error: ${error.status} - ${error.statusText}`;
      }
    }

    console.error('HTTP Error:', error);
    return throwError(() => new Error(errorMessage));
  };
}
