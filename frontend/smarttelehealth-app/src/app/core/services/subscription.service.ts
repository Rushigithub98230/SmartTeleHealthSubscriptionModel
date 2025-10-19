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
}


