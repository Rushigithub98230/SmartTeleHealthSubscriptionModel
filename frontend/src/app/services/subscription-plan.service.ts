import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import { environment } from '../../environments/environment';

export interface CreatePlanRequest {
  name: string;
  description: string;
  basePrice: number;
  billingCycle: 'monthly' | 'quarterly' | 'annual';
  privileges: Array<{
    name: string;
    limit: number;
    unitCost: number;
  }>;
  isActive: boolean;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  basePrice: number;
  billingCycle: string;
  privileges: any[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionPlanService {
  constructor(private commonService: CommonService) {}

  /**
   * Create a new subscription plan
   */
  createPlan(planData: CreatePlanRequest): Observable<ApiResponse<SubscriptionPlan>> {
    return this.commonService.postWithAuth<SubscriptionPlan>('/api/subscription-plans/admin', planData);
  }

  /**
   * Get all subscription plans
   */
  getAllPlans(): Observable<ApiResponse<SubscriptionPlan[]>> {
    return this.commonService.getWithAuth<SubscriptionPlan[]>('/api/subscription-plans/admin');
  }

  /**
   * Get plan by ID
   */
  getPlanById(planId: string): Observable<ApiResponse<SubscriptionPlan>> {
    return this.commonService.getWithAuth<SubscriptionPlan>(`/api/subscription-plans/admin/${planId}`);
  }

  /**
   * Update subscription plan
   */
  updatePlan(planId: string, planData: Partial<CreatePlanRequest>): Observable<ApiResponse<SubscriptionPlan>> {
    return this.commonService.putWithAuth<SubscriptionPlan>(`/api/subscription-plans/admin/${planId}`, planData);
  }

  /**
   * Delete subscription plan
   */
  deletePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.deleteWithAuth(`/api/subscription-plans/admin/${planId}`);
  }

  /**
   * Activate plan
   */
  activatePlan(planId: string): Observable<ApiResponse<SubscriptionPlan>> {
    return this.commonService.postWithAuth<SubscriptionPlan>(`/api/subscription-plans/${planId}/activate`, {});
  }

  /**
   * Deactivate plan
   */
  deactivatePlan(planId: string): Observable<ApiResponse<SubscriptionPlan>> {
    return this.commonService.postWithAuth<SubscriptionPlan>(`/api/subscription-plans/admin/${planId}/deactivate`, {});
  }
}
