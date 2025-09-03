import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  SubscriptionDto, 
  CreateSubscriptionDto, 
  SubscriptionPlanDto, 
  CreateSubscriptionPlanDto, 
  UpdateSubscriptionPlanDto,
  MasterBillingCycle,
  MasterCurrency,
  MasterPrivilegeType,
  Privilege
} from '../models/subscription.models';
import { CommonService, ApiResponse, PaginatedResponse } from './common.service';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  constructor(private commonService: CommonService) {}

  // Subscription Plans CRUD
  getAllPlans(page: number = 1, pageSize: number = 20, searchTerm?: string, categoryId?: string, isActive?: boolean): Observable<ApiResponse<SubscriptionPlanDto[]>> {
    const params: any = { page, pageSize };
    if (searchTerm) params.searchTerm = searchTerm;
    if (categoryId) params.categoryId = categoryId;
    if (isActive !== undefined) params.isActive = isActive;

    return this.commonService.getWithAuth<SubscriptionPlanDto[]>('/webadmin/subscription-management/plans', params);
  }

  getPlanById(planId: string): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.commonService.getWithAuth<SubscriptionPlanDto>(`/webadmin/subscription-management/plans/${planId}`);
  }

  createPlan(planDto: CreateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.commonService.postWithAuth<SubscriptionPlanDto>('/webadmin/subscription-management/plans', planDto);
  }

  updatePlan(planId: string, planDto: UpdateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.commonService.putWithAuth<SubscriptionPlanDto>(`/webadmin/subscription-management/plans/${planId}`, planDto);
  }

  deletePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.deleteWithAuth<any>(`/webadmin/subscription-management/plans/${planId}`);
  }

  // User Subscriptions CRUD
  getAllSubscriptions(page: number = 1, pageSize: number = 20, searchTerm?: string, status?: string[], planId?: string[]): Observable<ApiResponse<SubscriptionDto[]>> {
    const params: any = { page, pageSize };
    if (searchTerm) params.searchTerm = searchTerm;
    if (status?.length) params.status = status;
    if (planId?.length) params.planId = planId;

    return this.commonService.getWithAuth<SubscriptionDto[]>('/api/Subscriptions/admin/user-subscriptions', params);
  }

  getSubscriptionById(subscriptionId: string): Observable<ApiResponse<SubscriptionDto>> {
    return this.commonService.getWithAuth<SubscriptionDto>(`/webadmin/subscription-management/subscriptions/${subscriptionId}`);
  }

  createSubscription(subscriptionDto: CreateSubscriptionDto): Observable<ApiResponse<SubscriptionDto>> {
    return this.commonService.postWithAuth<SubscriptionDto>('/api/subscriptions', subscriptionDto);
  }

  updateSubscription(subscriptionId: string, updateDto: any): Observable<ApiResponse<SubscriptionDto>> {
    return this.commonService.putWithAuth<SubscriptionDto>(`/api/subscriptions/${subscriptionId}`, updateDto);
  }

  cancelSubscription(subscriptionId: string, reason: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/Subscriptions/admin/${subscriptionId}/cancel`, { reason });
  }

  pauseSubscription(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/Subscriptions/admin/${subscriptionId}/pause`, {});
  }

  resumeSubscription(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/Subscriptions/admin/${subscriptionId}/resume`, {});
  }

  extendSubscription(subscriptionId: string, additionalDays: number): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/Subscriptions/admin/${subscriptionId}/extend`, { additionalDays });
  }

  // Categories
  getCategories(): Observable<ApiResponse<any[]>> {
    return this.commonService.getWithAuth<any[]>('/webadmin/subscription-management/categories');
  }

  // Analytics
  getSubscriptionAnalytics(startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.commonService.getWithAuth<any>('/webadmin/subscription-management/analytics', params);
  }

  // Master Data APIs
  getBillingCycles(): Observable<ApiResponse<MasterBillingCycle[]>> {
    return this.commonService.getWithAuth<MasterBillingCycle[]>('/api/MasterData/billing-cycles');
  }

  getCurrencies(): Observable<ApiResponse<MasterCurrency[]>> {
    return this.commonService.getWithAuth<MasterCurrency[]>('/api/MasterData/currencies');
  }

  getPrivilegeTypes(): Observable<ApiResponse<MasterPrivilegeType[]>> {
    return this.commonService.getWithAuth<MasterPrivilegeType[]>('/api/MasterData/privilege-types');
  }

  getPrivileges(): Observable<ApiResponse<Privilege[]>> {
    return this.commonService.getWithAuth<Privilege[]>('/api/Privileges');
  }

  // Plan Privilege Management
  getPlanPrivileges(planId: string): Observable<ApiResponse<any[]>> {
    return this.commonService.getWithAuth<any[]>(`/webadmin/subscription-management/plans/${planId}/privileges`);
  }

  assignPrivilegesToPlan(planId: string, privileges: any[]): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/webadmin/subscription-management/plans/${planId}/privileges`, privileges);
  }

  removePrivilegeFromPlan(planId: string, privilegeId: string): Observable<ApiResponse<any>> {
    return this.commonService.deleteWithAuth<any>(`/webadmin/subscription-management/plans/${planId}/privileges/${privilegeId}`);
  }

  updatePlanPrivilege(planId: string, privilegeId: string, privilegeDto: any): Observable<ApiResponse<any>> {
    return this.commonService.putWithAuth<any>(`/webadmin/subscription-management/plans/${planId}/privileges/${privilegeId}`, privilegeDto);
  }

  // Plan activation/deactivation
  activatePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/webadmin/subscription-management/plans/${planId}/activate`, {});
  }

  deactivatePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/webadmin/subscription-management/plans/${planId}/deactivate`, {});
  }
}
