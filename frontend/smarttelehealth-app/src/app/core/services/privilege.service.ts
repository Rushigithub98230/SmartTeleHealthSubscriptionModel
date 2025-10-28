import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import {
  PrivilegeDto,
  PrivilegeUsageDto,
  PrivilegeUsageSummary,
  PrivilegeAvailability,
  UsePrivilegeDto,
  PrivilegeUsageResult,
  PrivilegeUsageHistory
} from '../models';

/**
 * Privilege Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/Privileges
 * - GET /api/Privileges/availability
 * - POST /api/Privileges/use
 * - GET /api/Privileges/usage/{subscriptionId}
 * - GET /api/Privileges/history
 */
@Injectable({
  providedIn: 'root'
})
export class PrivilegeService {
  constructor(private commonService: CommonService) {}

  /**
   * Get all active privileges
   * API: GET /api/SubscriptionPlans/admin/privileges?isActive=true
   * Used in: Admin Plan Create/Edit (Step 2 - Privilege Selection)
   */
  getActivePrivileges(): Observable<ApiResponse<PrivilegeDto[]>> {
    return this.commonService.get<PrivilegeDto[]>('SubscriptionPlans/admin/privileges', { isActive: true });
  }

  /**
   * Check privilege availability (before using)
   * API: GET /api/Subscriptions/{id}/check-privilege/{privilegeName}?requestedAmount={amount}
   * Used in: Before any privilege consumption, Overage detection
   * FIXED: Updated to use correct backend endpoint path
   */
  checkAvailability(
    subscriptionId: string,
    privilegeName: string,
    amount: number = 1
  ): Observable<ApiResponse<PrivilegeAvailability>> {
    return this.commonService.get<PrivilegeAvailability>(
      `Subscriptions/${subscriptionId}/check-privilege/${privilegeName}`,
      { requestedAmount: amount }
    );
  }

  /**
   * Use/consume privilege
   * API: POST /api/Subscriptions/user/privileges/use
   * Used in: After availability check passes
   * FIXED: Updated to use correct backend endpoint path
   */
  usePrivilege(dto: UsePrivilegeDto): Observable<ApiResponse<PrivilegeUsageResult>> {
    return this.commonService.post<PrivilegeUsageResult>('Subscriptions/user/privileges/use', dto);
  }

  /**
   * Get usage summary for subscription
   * API: GET /api/SubscriptionPlans/admin/privileges/usage-summary?subscriptionId={id}
   * Used in: User Dashboard, Privilege Usage Page
   * FIXED: Updated to use correct backend endpoint path
   */
  getUsageSummary(subscriptionId: string): Observable<ApiResponse<PrivilegeUsageSummary>> {
    return this.commonService.get<PrivilegeUsageSummary>(
      'SubscriptionPlans/privileges/usage-summary',
      { subscriptionId: subscriptionId }
    );
  }

  /**
   * Get usage history
   * API: GET /api/SubscriptionPlans/admin/privileges/usage-history?subscriptionId={id}
   * Used in: Usage History Page
   * FIXED: Updated to use correct backend endpoint path
   */
  getUsageHistory(
    subscriptionId: string,
    page: number = 1,
    pageSize: number = 20
  ): Observable<ApiResponse<PrivilegeUsageHistory[]>> {
    return this.commonService.get<PrivilegeUsageHistory[]>(
      'SubscriptionPlans/admin/privileges/usage-history',
      {
        subscriptionId,
        page,
        pageSize
      }
    );
  }

  /**
   * Get privilege usage summary for a user (Admin Only)
   * API: GET /api/PrivilegeBasedBilling/usage-summary/{userId}
   * Used in: Admin User Detail - Privileges Tab
   */
  getPrivilegeUsageSummary(userId: number): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`PrivilegeBasedBilling/usage-summary/${userId}`);
  }

  /**
   * Create privilege (Admin Only)
   * API: POST /api/SubscriptionPlans/admin/privileges
   * Used in: Admin Privilege Management
   */
  createPrivilege(dto: any): Observable<ApiResponse<PrivilegeDto>> {
    return this.commonService.post<PrivilegeDto>('SubscriptionPlans/admin/privileges', dto);
  }

  /**
   * Update privilege (Admin Only)
   * API: PUT /api/SubscriptionPlans/admin/privileges/{id}
   * Used in: Admin Privilege Edit
   */
  updatePrivilege(id: string, dto: any): Observable<ApiResponse<PrivilegeDto>> {
    return this.commonService.put<PrivilegeDto>(`SubscriptionPlans/admin/privileges/${id}`, dto);
  }
}

