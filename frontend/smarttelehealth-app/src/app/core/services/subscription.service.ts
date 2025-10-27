import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import {
  SubscriptionDto,
  CreateSubscriptionDto,
  UpgradeSubscriptionDto,
  PauseSubscriptionDto
} from '../models';

/**
 * Subscription Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - POST /api/Subscriptions
 * - GET /api/Subscriptions/user/{userId}
 * - GET /api/Subscriptions/{id}
 * - POST /api/Subscriptions/{id}/cancel
 * - POST /api/Subscriptions/{id}/pause
 * - POST /api/Subscriptions/{id}/resume
 * - POST /api/Subscriptions/{id}/upgrade
 */
@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  constructor(private commonService: CommonService) {}

  /**
   * Create new subscription
   * API: POST /api/Subscriptions
   * Used in: User Checkout Page
   */
  createSubscription(dto: CreateSubscriptionDto): Observable<ApiResponse<SubscriptionDto>> {
    return this.commonService.post<SubscriptionDto>('Subscriptions', dto);
  }

  /**
   * Get user's subscriptions
   * API: GET /api/Subscriptions/user/{userId}
   * Used in: User Dashboard, My Subscriptions Page
   */
  getUserSubscriptions(userId: number): Observable<ApiResponse<SubscriptionDto[]>> {
    return this.commonService.get<SubscriptionDto[]>(`Subscriptions/user/${userId}`);
  }

  /**
   * Get subscription by ID
   * API: GET /api/Subscriptions/{id}
   * Used in: Subscription Detail Page
   */
  getSubscriptionById(id: string): Observable<ApiResponse<SubscriptionDto>> {
    return this.commonService.get<SubscriptionDto>(`Subscriptions/${id}`);
  }

  /**
   * Cancel subscription
   * API: POST /api/Subscriptions/{id}/cancel
   * Used in: Manage Subscription Page
   */
  cancelSubscription(id: string, reason: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/cancel`, { reason });
  }

  /**
   * Pause subscription
   * API: POST /api/Subscriptions/{id}/pause
   * Used in: Manage Subscription Page
   */
  pauseSubscription(id: string, dto?: PauseSubscriptionDto): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/pause`, dto || {});
  }

  /**
   * Resume paused subscription
   * API: POST /api/Subscriptions/{id}/resume
   * Used in: Manage Subscription Page
   */
  resumeSubscription(id: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/resume`, {});
  }

  /**
   * Upgrade subscription
   * API: POST /api/Subscriptions/{id}/upgrade
   * Used in: Upgrade Plan Flow
   */
  upgradeSubscription(id: string, dto: UpgradeSubscriptionDto): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/upgrade`, dto);
  }

  /**
   * Purchase additional privilege credits
   * API: POST /api/Subscriptions/{id}/purchase-credits
   * Used in: Privilege Purchase Modal
   * Phase 2: Privilege Management
   */
  purchaseAdditionalCredits(id: string, dto: any): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/purchase-credits`, dto);
  }

  // ===== ADMIN SUBSCRIPTION METHODS =====

  /**
   * Cancel subscription (Admin Only)
   * API: POST /api/admin/subscriptions/{id}/cancel
   * Backend: CancelUserSubscription(string id, [FromBody] string? reason = null)
   * Used in: Admin Subscription Management
   */
  cancelAdminSubscription(id: string, reason: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`admin/subscriptions/${id}/cancel`, reason);
  }

  /**
   * Pause subscription (Admin Only)  
   * API: POST /api/admin/subscriptions/{id}/pause
   * Backend: PauseUserSubscription(string id, [FromBody] string? reason = null)
   * Note: Backend service doesn't use reason, but controller accepts it
   * Used in: Admin Subscription Management
   */
  pauseAdminSubscription(id: string, reason: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`admin/subscriptions/${id}/pause`, reason);
  }

  /**
   * Resume subscription (Admin Only)
   * API: POST /api/admin/subscriptions/{id}/resume  
   * Backend: ResumeUserSubscription(string id)
   * Used in: Admin Subscription Management
   */
  resumeAdminSubscription(id: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`admin/subscriptions/${id}/resume`, {});
  }

  /**
   * Extend subscription (Admin Only)
   * API: POST /api/admin/subscriptions/{id}/extend
   * Backend: ExtendUserSubscription(string id, [FromBody] int additionalDays)
   * Used in: Admin Subscription Management
   */
  extendAdminSubscription(id: string, days: number): Observable<ApiResponse<any>> {
    return this.commonService.post(`admin/subscriptions/${id}/extend`, days);
  }

  /**
   * Upgrade subscription (Admin Only)
   * API: POST /api/admin/subscriptions/{id}/upgrade
   * Backend: UpgradeUserSubscription(string id, [FromBody] string newPlanId)
   * Used in: Admin Subscription Management
   */
  upgradeAdminSubscription(id: string, newPlanId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`admin/subscriptions/${id}/upgrade`, newPlanId);
  }

  /**
   * Downgrade subscription (Admin Only)
   * API: POST /api/admin/subscriptions/{id}/downgrade
   * Backend: DowngradeUserSubscription(string id, [FromBody] string newPlanId)
   * Used in: Admin Subscription Management
   */
  downgradeAdminSubscription(id: string, newPlanId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`admin/subscriptions/${id}/downgrade`, newPlanId);
  }
}


