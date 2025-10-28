import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import {
  SubscriptionPlanDto,
  CreateSubscriptionPlanDto,
  UpdateSubscriptionPlanDto,
  SubscriptionPlanFilterDto,
  PlanPrivilegeDto
} from '../models';

/**
 * Subscription Plan Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/SubscriptionPlans/active
 * - GET /api/SubscriptionPlans/{id}
 * - GET /api/SubscriptionPlans/category/{categoryId}
 * - POST /api/SubscriptionPlans/filter
 * - POST /api/SubscriptionPlans (Admin)
 * - PUT /api/SubscriptionPlans/{id} (Admin)
 * - POST /api/SubscriptionPlans/{id}/deactivate (Admin)
 * - POST /api/SubscriptionPlans/{id}/privileges (Admin)
 */
@Injectable({
  providedIn: 'root'
})
export class SubscriptionPlanService {
  constructor(private commonService: CommonService) {}

  /**
   * Get active subscription plans (Public)
   * API: GET /api/SubscriptionPlans/active
   * Used in: Marketing Plan List, User Plan Selection
   */
  getActivePlans(page: number = 1, pageSize: number = 10, searchTerm?: string, categoryId?: string): Observable<ApiResponse<SubscriptionPlanDto[]>> {
    const params: any = { page, pageSize };
    if (searchTerm) params.searchTerm = searchTerm;
    if (categoryId) params.categoryId = categoryId;
    
    return this.commonService.get<SubscriptionPlanDto[]>('SubscriptionPlans/active', params);
  }

  /**
   * Get plan by ID
   * API: GET /api/SubscriptionPlans/{planId}
   * Used in: Plan Detail Page, Checkout
   */
  getPlanById(planId: string): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.commonService.get<SubscriptionPlanDto>(`SubscriptionPlans/${planId}`);
  }

  /**
   * Get plans by category
   * API: GET /api/SubscriptionPlans/category/{categoryId}
   * Used in: Marketing Category Browse
   */
  getPlansByCategory(categoryId: string): Observable<ApiResponse<SubscriptionPlanDto[]>> {
    return this.commonService.get<SubscriptionPlanDto[]>(`SubscriptionPlans/category/${categoryId}`);
  }

  /**
   * Filter plans (Advanced filtering)
   * API: POST /api/SubscriptionPlans/filter
   * Used in: Admin Plan List with Filters
   */
  filterPlans(filter: SubscriptionPlanFilterDto): Observable<ApiResponse<SubscriptionPlanDto[]>> {
    return this.commonService.post<SubscriptionPlanDto[]>('SubscriptionPlans/filter', filter);
  }

  /**
   * Get all plans (Admin view - includes inactive)
   * API: GET /api/SubscriptionPlans/admin
   * Used in: Admin Plan Management
   */
  getAllPlansAdmin(page: number = 1, pageSize: number = 20): Observable<ApiResponse<SubscriptionPlanDto[]>> {
    return this.commonService.get<SubscriptionPlanDto[]>('SubscriptionPlans/admin', { page, pageSize });
  }

  /**
   * Create new plan (Admin Only)
   * API: POST /api/SubscriptionPlans/admin
   * Used in: Admin Create Plan Stepper Form
   */
  createPlan(dto: CreateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.commonService.post<SubscriptionPlanDto>('SubscriptionPlans/admin', dto);
  }

  /**
   * Update existing plan (Admin Only)
   * API: PUT /api/SubscriptionPlans/admin/{planId}
   * Used in: Admin Edit Plan Form
   */
  updatePlan(planId: string, dto: UpdateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.commonService.put<SubscriptionPlanDto>(`SubscriptionPlans/admin/${planId}`, dto);
  }

  /**
   * Deactivate plan (Admin Only)
   * API: POST /api/SubscriptionPlans/admin/{planId}/deactivate
   * Used in: Admin Plan List Actions
   */
  deactivatePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`SubscriptionPlans/admin/${planId}/deactivate`, {});
  }

  /**
   * Assign privileges to plan (Admin Only)
   * API: POST /api/SubscriptionPlans/admin/{planId}/privileges
   * Used in: Admin Plan Creation/Edit - Privilege Configuration
   */
  assignPrivilegesToPlan(planId: string, privileges: PlanPrivilegeDto[]): Observable<ApiResponse<any>> {
    return this.commonService.post(`SubscriptionPlans/admin/${planId}/privileges`, privileges);
  }

  /**
   * Remove privilege from plan (Admin Only)
   * API: DELETE /api/SubscriptionPlans/admin/{planId}/privileges/{privilegeId}
   * Used in: Admin Plan Edit
   */
  removePrivilegeFromPlan(planId: string, privilegeId: string): Observable<ApiResponse<any>> {
    return this.commonService.delete(`SubscriptionPlans/admin/${planId}/privileges/${privilegeId}`);
  }

  /**
   * Get effective price for a plan (centralized pricing calculation)
   * API: GET /api/SubscriptionPlans/{planId}/effective-price
   * Used in: Frontend pricing display to ensure consistency with backend
   */
  getEffectivePrice(planId: string): Observable<ApiResponse<{
    PlanId: string;
    BasePrice: number;
    EffectivePrice: number;
    DiscountPercentage?: number;
    BillingDiscountPercentage?: number;
    DiscountValidUntil?: string;
    CurrencyCode: string;
    CalculatedAt: string;
  }>> {
    return this.commonService.get(`SubscriptionPlans/${planId}/effective-price`);
  }

  /**
   * Reactivate plan (Admin Only)
   * API: POST /api/SubscriptionPlans/admin/{planId}/reactivate
   * Used in: Admin Plan List Actions
   */
  reactivatePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`SubscriptionPlans/admin/${planId}/reactivate`, {});
  }

  /**
   * Activate plan (Admin Only)
   * API: POST /api/SubscriptionPlans/admin/{planId}/activate
   * Used in: Admin Plan List Actions
   */
  activatePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.post(`SubscriptionPlans/admin/${planId}/activate`, {});
  }

  /**
   * Get plan privileges (Admin Only)
   * API: GET /api/SubscriptionPlans/admin/{planId}/privileges
   * Used in: Admin Plan Edit - Privilege Management
   */
  getPlanPrivileges(planId: string): Observable<ApiResponse<PlanPrivilegeDto[]>> {
    return this.commonService.get(`SubscriptionPlans/admin/${planId}/privileges`);
  }

  /**
   * Update plan privilege (Admin Only)
   * API: PUT /api/SubscriptionPlans/admin/{planId}/privileges/{privilegeId}
   * Used in: Admin Plan Edit - Privilege Configuration
   */
  updatePlanPrivilege(planId: string, privilegeId: string, privilege: PlanPrivilegeDto): Observable<ApiResponse<any>> {
    return this.commonService.put(`SubscriptionPlans/admin/${planId}/privileges/${privilegeId}`, privilege);
  }

  /**
   * Export plans (Admin Only)
   * API: GET /api/SubscriptionPlans/admin/export
   * Used in: Admin Plan Management - Export functionality
   */
  exportPlans(searchTerm?: string, categoryId?: string, isActive?: boolean, format: string = 'csv'): Observable<ApiResponse<any>> {
    const params: any = { format };
    if (searchTerm) params.searchTerm = searchTerm;
    if (categoryId) params.categoryId = categoryId;
    if (isActive !== undefined) params.isActive = isActive;
    
    return this.commonService.get('SubscriptionPlans/admin/export', params);
  }

  /**
   * Get plans for comparison (Admin Only)
   * API: GET /api/SubscriptionPlans/admin/compare/{categoryId}
   * Used in: Admin Plan Management - Comparison feature
   */
  getPlansForComparison(categoryId: string): Observable<ApiResponse<SubscriptionPlanDto[]>> {
    return this.commonService.get(`SubscriptionPlans/admin/compare/${categoryId}`);
  }
}

