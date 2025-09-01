import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  SubscriptionDto, 
  CreateSubscriptionDto, 
  SubscriptionPlanDto, 
  CreateSubscriptionPlanDto, 
  UpdateSubscriptionPlanDto,
  ApiResponse,
  PaginatedResponse,
  MasterBillingCycle,
  MasterCurrency,
  MasterPrivilegeType,
  Privilege
} from '../models/subscription.models';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private readonly baseUrl = 'http://localhost:61376/api'; // Updated to match backend URL

  constructor(private http: HttpClient) {}

  // Subscription Plans CRUD
  getAllPlans(page: number = 1, pageSize: number = 20, searchTerm?: string, categoryId?: string, isActive?: boolean): Observable<ApiResponse<PaginatedResponse<SubscriptionPlanDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (categoryId) params = params.set('categoryId', categoryId);
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());

    return this.http.get<ApiResponse<PaginatedResponse<SubscriptionPlanDto>>>(`${this.baseUrl}/SubscriptionPlans`, { params });
  }

  getPlanById(planId: string): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.http.get<ApiResponse<SubscriptionPlanDto>>(`${this.baseUrl}/SubscriptionPlans/${planId}`);
  }

  createPlan(planDto: CreateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.http.post<ApiResponse<SubscriptionPlanDto>>(`${this.baseUrl}/SubscriptionPlans`, planDto);
  }

  updatePlan(planId: string, planDto: UpdateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.http.put<ApiResponse<SubscriptionPlanDto>>(`${this.baseUrl}/SubscriptionPlans/${planId}`, planDto);
  }

  deletePlan(planId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/SubscriptionPlans/${planId}`);
  }

  // User Subscriptions CRUD
  getAllSubscriptions(page: number = 1, pageSize: number = 20, searchTerm?: string, status?: string[], planId?: string[]): Observable<ApiResponse<PaginatedResponse<SubscriptionDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    // Backend expects status and planId as single string, not array
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (status?.length) params = params.set('status', status[0]);
    if (planId?.length) params = params.set('planId', planId[0]);

    // Endpoint for admin subscription listing
    return this.http.get<ApiResponse<PaginatedResponse<SubscriptionDto>>>(`${this.baseUrl}/admin/AdminSubscription`, { params });
  }

  getSubscriptionById(subscriptionId: string): Observable<ApiResponse<SubscriptionDto>> {
    return this.http.get<ApiResponse<SubscriptionDto>>(`${this.baseUrl}/admin/subscription/${subscriptionId}`);
  }

  createSubscription(subscriptionDto: CreateSubscriptionDto): Observable<ApiResponse<SubscriptionDto>> {
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.baseUrl}/subscriptions`, subscriptionDto);
  }

  updateSubscription(subscriptionId: string, updateDto: any): Observable<ApiResponse<SubscriptionDto>> {
    return this.http.put<ApiResponse<SubscriptionDto>>(`${this.baseUrl}/subscriptions/${subscriptionId}`, updateDto);
  }

  cancelSubscription(subscriptionId: string, reason: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/cancel`, { reason });
  }

  pauseSubscription(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/pause`, {});
  }

  resumeSubscription(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/resume`, {});
  }

  extendSubscription(subscriptionId: string, additionalDays: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/extend`, { additionalDays });
  }

  // Categories
  getCategories(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.baseUrl}/subscriptions/admin/categories`);
  }

  // Analytics
  getSubscriptionAnalytics(startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());

    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/admin/subscription/analytics`, { params });
  }

  // Master Data APIs
  getBillingCycles(): Observable<ApiResponse<MasterBillingCycle[]>> {
    return this.http.get<ApiResponse<MasterBillingCycle[]>>(`${this.baseUrl}/MasterData/billing-cycles`);
  }

  getCurrencies(): Observable<ApiResponse<MasterCurrency[]>> {
    return this.http.get<ApiResponse<MasterCurrency[]>>(`${this.baseUrl}/MasterData/currencies`);
  }

  getPrivilegeTypes(): Observable<ApiResponse<MasterPrivilegeType[]>> {
    return this.http.get<ApiResponse<MasterPrivilegeType[]>>(`${this.baseUrl}/MasterData/privilege-types`);
  }

  getPrivileges(): Observable<ApiResponse<Privilege[]>> {
    return this.http.get<ApiResponse<Privilege[]>>(`${this.baseUrl}/Privileges`);
  }
}
