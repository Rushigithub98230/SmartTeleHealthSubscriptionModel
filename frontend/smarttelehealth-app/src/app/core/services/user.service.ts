import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import { UserDto } from '../models';
import { environment } from '../../../environments/environment';

/**
 * User Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/Users/{id}
 * - GET /api/Users
 * - GET /api/Users/{userId}/analytics
 * - POST /api/Users
 * - PUT /api/Users/{id}
 * - DELETE /api/Users/{id}
 */
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly baseUrl = environment.apiUrl;

  constructor(
    private commonService: CommonService,
    private http: HttpClient
  ) {}

  /**
   * Get user by ID
   * API: GET /api/Users/{id}
   * Used in: Admin User Detail, Profile Pages
   */
  getUserById(userId: number): Observable<ApiResponse<UserDto>> {
    return this.commonService.get<UserDto>(`Users/${userId}`);
  }

  /**
   * Get all users with filtering (Admin only)
   * API: GET /api/Users
   * Used in: Admin User List
   */
  getAllUsers(params?: {
    searchTerm?: string;
    role?: string;
    isActive?: boolean | null;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortOrder?: string;
  }): Observable<ApiResponse<UserDto[]>> {
    const queryParams: any = {};
    
    if (params) {
      if (params.searchTerm) queryParams.searchText = params.searchTerm;
      if (params.role) queryParams.role = params.role;
      if (params.isActive !== undefined && params.isActive !== null) {
        queryParams.isActive = params.isActive;
      }
      if (params.page) queryParams.page = params.page;
      if (params.pageSize) queryParams.pageSize = params.pageSize;
      // sortBy and sortOrder may be added to backend in future
    }

    return this.commonService.get<UserDto[]>('Users', queryParams);
  }

  /**
   * Get comprehensive user analytics (Admin only)
   * API: GET /api/Users/{userId}/analytics
   * Used in: Admin User Detail - Analytics Tab
   */
  getUserAnalytics(
    userId: number,
    startDate?: Date,
    endDate?: Date
  ): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.commonService.get<any>(`Users/${userId}/analytics`, params);
  }

  /**
   * Create new user (Admin only)
   * API: POST /api/Users
   * Used in: Admin User Management
   */
  createUser(userData: any): Observable<ApiResponse<UserDto>> {
    return this.commonService.post<UserDto>('Users', userData);
  }

  /**
   * Update user (Admin only)
   * API: PUT /api/Users/{userId}
   * Used in: Admin User Management, Profile Edit
   */
  updateUser(userId: number, userData: any): Observable<ApiResponse<UserDto>> {
    return this.commonService.put<UserDto>(`Users/${userId}`, userData);
  }

  /**
   * Delete user (Admin only)
   * API: DELETE /api/Users/{userId}
   * Used in: Admin User Management
   */
  deleteUser(userId: number): Observable<ApiResponse<any>> {
    return this.commonService.delete(`Users/${userId}`);
  }

  /**
   * Get user stats
   * API: GET /api/Users/{userId}/stats
   * Used in: User Dashboard
   */
  getUserStats(userId: number): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`Users/${userId}/stats`);
  }

  /**
   * Export user analytics to Excel or CSV
   * API: GET /api/Users/{userId}/export-analytics
   * Used in: Admin User Detail - Analytics Tab
   */
  exportUserAnalytics(
    userId: number,
    format: 'excel' | 'csv',
    startDate?: Date,
    endDate?: Date
  ): Observable<Blob> {
    let params: any = { format };
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get(`${this.baseUrl}/Users/${userId}/export-analytics`, {
      params,
      responseType: 'blob'
    });
  }
}

