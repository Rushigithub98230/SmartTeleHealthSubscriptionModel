import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

/**
 * System Settings Interface
 */
export interface SystemSettingsDto {
  id: string;
  defaultAdminCommissionPercent: number;
  defaultPriceChangeNoticeDays: number;
  maxFailedPaymentAttempts: number;
  lastUpdated: string;
  isActive: boolean;
  createdDate: string;
}

/**
 * Update System Settings DTO
 */
export interface UpdateSystemSettingsDto {
  defaultAdminCommissionPercent?: number;
  defaultPriceChangeNoticeDays?: number;
  maxFailedPaymentAttempts?: number;
}

/**
 * System Settings Service
 * Manages global system configuration values
 * 
 * API Endpoints Used:
 * - GET /api/admin/SystemSettings
 * - PUT /api/admin/SystemSettings
 * - POST /api/admin/SystemSettings/reset
 * - GET /api/admin/SystemSettings/history
 */
@Injectable({
  providedIn: 'root'
})
export class SystemSettingsService {
  constructor(private commonService: CommonService) {}

  /**
   * Get current system settings
   * API: GET /api/admin/SystemSettings
   * Used in: Plan creation form default value patching
   */
  getSettings(): Observable<ApiResponse<SystemSettingsDto>> {
    return this.commonService.get<SystemSettingsDto>('admin/SystemSettings');
  }

  /**
   * Update system settings
   * API: PUT /api/admin/SystemSettings
   * Used in: Admin settings management
   */
  updateSettings(updateDto: UpdateSystemSettingsDto): Observable<ApiResponse<SystemSettingsDto>> {
    return this.commonService.put<SystemSettingsDto>('admin/SystemSettings', updateDto);
  }

  /**
   * Reset system settings to defaults
   * API: POST /api/admin/SystemSettings/reset
   * Used in: Admin settings management
   */
  resetToDefaults(): Observable<ApiResponse<SystemSettingsDto>> {
    return this.commonService.post<SystemSettingsDto>('admin/SystemSettings/reset', {});
  }

  /**
   * Get settings change history
   * API: GET /api/admin/SystemSettings/history
   * Used in: Admin audit trail
   */
  getSettingsHistory(): Observable<ApiResponse<any[]>> {
    return this.commonService.get<any[]>('admin/SystemSettings/history');
  }
}
