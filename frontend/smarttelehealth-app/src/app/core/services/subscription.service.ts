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
   * Backend expects: [FromBody] string reason (raw string, not object)
   * Used in: Manage Subscription Page
   */
  cancelSubscription(id: string, reason: string): Observable<ApiResponse<any>> {
    // Backend expects raw string in body, not { reason: "..." } object
    return this.commonService.post(`Subscriptions/${id}/cancel`, reason);
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
   * Retry failed payment for a subscription
   * API: POST /api/Subscriptions/{subscriptionId}/retry-payment
   * Used in: Subscription Detail Page for PaymentFailed/Suspended subscriptions
   * Phase 2: Failed Payment Recovery
   */
  retryPayment(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${subscriptionId}/retry-payment`, {});
  }

  /**
   * Check if user is eligible to subscribe to a plan
   * API: GET /api/Subscriptions/eligibility/{planId}
   * Used in: Plan Browse Page before checkout
   * Phase 2: Duplicate Subscription Prevention
   */
  checkEligibility(planId: string): Observable<ApiResponse<{ isEligible: boolean; reason: string }>> {
    return this.commonService.get<{ isEligible: boolean; reason: string }>(`Subscriptions/eligibility/${planId}`);
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

  // ===== SCHEDULED PLAN CHANGES (NO PRORATION) =====

  /**
   * Schedule subscription upgrade to take effect at next billing cycle
   * API: POST /api/Subscriptions/{id}/schedule-upgrade
   * Used in: Plan Change Modal
   * Phase 4: Scheduled Plan Changes
   */
  scheduleUpgrade(id: string, newPlanId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/schedule-upgrade`, { newPlanId });
  }

  /**
   * Schedule subscription downgrade to take effect at next billing cycle
   * API: POST /api/Subscriptions/{id}/schedule-downgrade
   * Used in: Plan Change Modal
   * Phase 4: Scheduled Plan Changes
   */
  scheduleDowngrade(id: string, newPlanId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`Subscriptions/${id}/schedule-downgrade`, { newPlanId });
  }

  /**
   * Cancel scheduled plan change
   * API: DELETE /api/Subscriptions/{id}/scheduled-change
   * Used in: Subscription Detail (Cancel Change Button)
   * Phase 4: Scheduled Plan Changes
   */
  cancelScheduledPlanChange(id: string): Observable<ApiResponse<any>> {
    return this.commonService.delete(`Subscriptions/${id}/scheduled-change`);
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


