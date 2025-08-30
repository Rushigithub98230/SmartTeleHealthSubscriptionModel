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
  PaginatedResponse 
} from '../models/subscription.models';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private readonly baseUrl = 'https://localhost:7001/api'; // Update with your API URL

  constructor(private http: HttpClient) {}

  // Subscription Plans CRUD
  getAllPlans(page: number = 1, pageSize: number = 20, searchTerm?: string, categoryId?: string, isActive?: boolean): Observable<ApiResponse<PaginatedResponse<SubscriptionPlanDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (categoryId) params = params.set('categoryId', categoryId);
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());

    return this.http.get<ApiResponse<PaginatedResponse<SubscriptionPlanDto>>>(`${this.baseUrl}/subscriptions/admin/plans`, { params });
  }

  getPlanById(planId: string): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.http.get<ApiResponse<SubscriptionPlanDto>>(`${this.baseUrl}/subscriptions/admin/plans/${planId}`);
  }

  createPlan(planDto: CreateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.http.post<ApiResponse<SubscriptionPlanDto>>(`${this.baseUrl}/subscriptions/admin/plans`, planDto);
  }

  updatePlan(planId: string, planDto: UpdateSubscriptionPlanDto): Observable<ApiResponse<SubscriptionPlanDto>> {
    return this.http.put<ApiResponse<SubscriptionPlanDto>>(`${this.baseUrl}/subscriptions/admin/plans/${planId}`, planDto);
  }

  deletePlan(planId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/plans/${planId}`);
  }

  // User Subscriptions CRUD
  getAllSubscriptions(page: number = 1, pageSize: number = 20, searchTerm?: string, status?: string[], planId?: string[]): Observable<ApiResponse<PaginatedResponse<SubscriptionDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (status?.length) {
      status.forEach(s => params = params.append('status', s));
    }
    if (planId?.length) {
      planId.forEach(p => params = params.append('planId', p));
    }

    return this.http.get<ApiResponse<PaginatedResponse<SubscriptionDto>>>(`${this.baseUrl}/subscriptions/admin/user-subscriptions`, { params });
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
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/cancel`, reason);
  }

  pauseSubscription(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/pause`, {});
  }

  resumeSubscription(subscriptionId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/resume`, {});
  }

  extendSubscription(subscriptionId: string, additionalDays: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/subscriptions/admin/${subscriptionId}/extend`, additionalDays);
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
}
