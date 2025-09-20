import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { CommonService, ApiResponse } from './common.service';

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  shortDescription?: string;
  price: number;
  discountedPrice?: number;
  discountValidUntil?: string;
  billingCycleId: string;
  currencyId: string;
  categoryId: string;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  features?: string;
  terms?: string;
  effectiveDate?: string;
  expirationDate?: string;
  effectivePrice: number;
  hasActiveDiscount: boolean;
  isCurrentlyAvailable: boolean;
  createdDate: string;
  updatedDate?: string;
  // Plan features and limits
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  maxConcurrentUsers: number;
  gracePeriodDays: number;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  displayOrder: number;
  features?: string;
  consultationDescription?: string;
  basePrice: number;
  consultationFee: number;
  consultationDurationMinutes: number;
  requiresHealthAssessment: boolean;
  allowsMedicationDelivery: boolean;
  allowsFollowUpMessaging: boolean;
  allowsOneTimeConsultation: boolean;
  oneTimeConsultationFee: number;
  oneTimeConsultationDurationMinutes: number;
  isMostPopular: boolean;
  isTrending: boolean;
  subscriptionPlans?: SubscriptionPlan[];
}

export interface BillingCycle {
  id: string;
  name: string;
  durationInMonths: number;
  isActive: boolean;
  description?: string;
  displayOrder: number;
}

export interface CheckoutSessionRequest {
  planId: string;
  billingCycleId: string;
  successUrl: string;
  cancelUrl: string;
  questionnaireResponses?: { [key: string]: any };
  categoryId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private readonly apiUrl = `${environment.apiUrl}/api`;
  private plansSubject = new BehaviorSubject<SubscriptionPlan[]>([]);
  private categoriesSubject = new BehaviorSubject<Category[]>([]);

  public plans$ = this.plansSubject.asObservable();
  public categories$ = this.categoriesSubject.asObservable();

  constructor(private http: HttpClient, private commonService: CommonService) {}

  getActivePlans(): Observable<SubscriptionPlan[]> {
    const url = `/api/SubscriptionPlans/active`;
    console.log('Fetching plans from:', url);
    
    return this.commonService.getWithAuth<SubscriptionPlan[]>(url).pipe(
      tap(response => console.log('Plans API response:', response)),
      map(response => {
        if (response.statusCode === 200) {
          const plans = response.data.map((plan: any) => ({
            ...plan,
            popular: plan.isMostPopular,
            trending: plan.isTrending
          }));
          this.plansSubject.next(plans);
          return plans;
        }
        throw new Error(response.message || 'Failed to fetch plans');
      }),
      tap(plans => console.log('Processed plans:', plans))
    );
  }

  getCategories(): Observable<Category[]> {
    const url = `/api/Categories`;
    console.log('Fetching categories from:', url);
    
    return this.commonService.getWithAuth<Category[]>(url).pipe(
      tap(response => console.log('Categories API response:', response)),
      map(response => {
        if (response.statusCode === 200) {
          this.categoriesSubject.next(response.data);
          return response.data;
        }
        throw new Error(response.message || 'Failed to fetch categories');
      }),
      tap(categories => console.log('Processed categories:', categories))
    );
  }

  createCheckoutSession(request: CheckoutSessionRequest): Observable<{url: string}> {
    return this.commonService.postWithAuth<{url: string}>('/api/stripe/create-checkout-session', request).pipe(
      map(response => {
        if (response.statusCode === 200) {
          return { url: response.data.url };
        }
        throw new Error(response.message || 'Failed to create checkout session');
      })
    );
  }

  formatPrice(price: number, currency: string = 'USD'): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency
    }).format(price);
  }

  // Plan Management Methods for Admin Portal
  getAllPlans(page: number = 1, pageSize: number = 10, searchTerm?: string, categoryId?: string, isActive?: boolean): Observable<any> {
    const params: any = { page, pageSize };
    if (searchTerm) params.searchTerm = searchTerm;
    if (categoryId) params.categoryId = categoryId;
    if (isActive !== undefined) params.isActive = isActive;

    return this.commonService.getWithAuth<any>('/api/SubscriptionPlans/admin', params);
  }

  getSubscriptionHistory(subscriptionId: string): Observable<any> {
    return this.commonService.getWithAuth<any>(`/api/Subscriptions/${subscriptionId}/analytics`);
  }

  createPlan(planData: any): Observable<any> {
    return this.commonService.postWithAuth<any>('/api/SubscriptionPlans/admin', planData);
  }

  updatePlan(planId: string, planData: any): Observable<any> {
    return this.commonService.putWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}`, planData);
  }

  // RECOMMENDED: Use deactivatePlan instead of deletePlan for better data integrity
  deactivatePlan(planId: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/deactivate`, {});
  }

  reactivatePlan(planId: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/reactivate`, {});
  }

  activatePlan(planId: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/activate`, {});
  }

  // DEPRECATED: Use deactivatePlan instead for better data integrity and business continuity
  deletePlan(planId: string): Observable<any> {
    console.warn('deletePlan is deprecated. Use deactivatePlan instead for better data integrity.');
    return this.deactivatePlan(planId);
  }

  // Subscription Management Methods
  getAllSubscriptions(page: number = 1, pageSize: number = 10, searchTerm?: string, status?: string): Observable<any> {
    const params: any = { page, pageSize };
    if (searchTerm) params.searchTerm = searchTerm;
    if (status) params.status = status;

    return this.commonService.getWithAuth<any>('/api/admin/subscriptions', params);
  }

  upgradeSubscription(subscriptionId: string, newPlanId: string, paymentMethodId?: string): Observable<any> {
    const upgradeData: any = { newPlanId };
    if (paymentMethodId) {
      upgradeData.paymentMethodId = paymentMethodId;
    }
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/upgrade`, upgradeData);
  }

  downgradeSubscription(subscriptionId: string, newPlanId: string, paymentMethodId?: string): Observable<any> {
    const downgradeData: any = { newPlanId };
    if (paymentMethodId) {
      downgradeData.paymentMethodId = paymentMethodId;
    }
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/downgrade`, downgradeData);
  }

  extendSubscription(subscriptionId: string, additionalDays: number): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/extend`, additionalDays);
  }

  reactivateSubscription(subscriptionId: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/reactivate`, {});
  }

  getBillingHistory(subscriptionId: string): Observable<any> {
    return this.commonService.getWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/billing-history`);
  }

  getPrivilegeUsage(subscriptionId: string): Observable<any> {
    return this.commonService.getWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/privilege-usage`);
  }

  pauseSubscription(subscriptionId: string, reason?: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/pause`, reason || '');
  }

  resumeSubscription(subscriptionId: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/resume`, {});
  }

  cancelSubscription(subscriptionId: string, reason: string): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}/cancel`, reason);
  }

  // Additional Plan Management Methods
  getPlanById(planId: string): Observable<any> {
    return this.commonService.getWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}`);
  }

  getPlanPrivileges(planId: string): Observable<any> {
    return this.commonService.getWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/privileges`);
  }

  assignPrivilegesToPlan(planId: string, privileges: any[]): Observable<any> {
    return this.commonService.postWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/privileges`, privileges);
  }

  removePrivilegeFromPlan(planId: string, privilegeId: string): Observable<any> {
    return this.commonService.deleteWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/privileges/${privilegeId}`);
  }

  updatePlanPrivilege(planId: string, privilegeId: string, privilege: any): Observable<any> {
    return this.commonService.putWithAuth<any>(`/api/SubscriptionPlans/admin/${planId}/privileges/${privilegeId}`, privilege);
  }

  // Additional admin methods for subscription management
  updateSubscription(subscriptionId: string, updateData: any): Observable<any> {
    return this.commonService.putWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}`, updateData);
  }

  performBulkAction(actions: any[]): Observable<any> {
    return this.commonService.postWithAuth<any>('/api/admin/subscriptions/bulk-action', actions);
  }

  getSubscriptionDetails(subscriptionId: string): Observable<any> {
    return this.commonService.getWithAuth<any>(`/api/admin/subscriptions/${subscriptionId}`);
  }
}