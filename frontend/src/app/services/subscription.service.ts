import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

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

  constructor(private http: HttpClient) {}

  getActivePlans(): Observable<SubscriptionPlan[]> {
    const url = `${this.apiUrl}/SubscriptionPlans/active`;
    console.log('Fetching plans from:', url);
    
    return this.http.get<{data: SubscriptionPlan[], statusCode: number, message: string}>(url).pipe(
      tap(response => console.log('Plans API response:', response)),
      map(response => {
        if (response.statusCode === 200) {
          const plans = response.data.map(plan => ({
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
    const url = `${this.apiUrl}/Categories`;
    console.log('Fetching categories from:', url);
    
    return this.http.get<{data: Category[], statusCode: number, message: string}>(url).pipe(
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
    return this.http.post<{data: {url: string}, statusCode: number, message: string}>(
      `${this.apiUrl}/stripe/create-checkout-session`,
      request
    ).pipe(
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

    return this.http.get<any>(`${this.apiUrl}/SubscriptionPlans/admin/paged`, { params });
  }

  getSubscriptionHistory(subscriptionId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Subscriptions/${subscriptionId}/analytics`);
  }

  createPlan(planData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/SubscriptionPlans/admin`, planData);
  }

  updatePlan(planId: string, planData: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/SubscriptionPlans/${planId}`, planData);
  }

  deletePlan(planId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/SubscriptionPlans/${planId}`);
  }

  activatePlan(planId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/SubscriptionPlans/${planId}/activate`, {});
  }

  deactivatePlan(planId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/SubscriptionPlans/${planId}/deactivate`, {});
  }

  // Subscription Management Methods
  getAllSubscriptions(page: number = 1, pageSize: number = 10, searchTerm?: string, status?: string): Observable<any> {
    const params: any = { page, pageSize };
    if (searchTerm) params.searchTerm = searchTerm;
    if (status) params.status = status;

    return this.http.get<any>(`${this.apiUrl}/Subscriptions/admin/user-subscriptions`, { params });
  }

  upgradeSubscription(subscriptionId: string, newPlanId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/upgrade`, { newPlanId });
  }

  downgradeSubscription(subscriptionId: string, newPlanId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/downgrade`, { newPlanId });
  }

  extendSubscription(subscriptionId: string, additionalDays: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/extend`, additionalDays);
  }

  reactivateSubscription(subscriptionId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/reactivate`, {});
  }

  getBillingHistory(subscriptionId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Subscriptions/${subscriptionId}/billing-history`);
  }

  getPrivilegeUsage(subscriptionId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Subscriptions/${subscriptionId}/usage-statistics`);
  }

  pauseSubscription(subscriptionId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/pause`, {});
  }

  resumeSubscription(subscriptionId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/resume`, {});
  }

  cancelSubscription(subscriptionId: string, reason: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/admin/${subscriptionId}/cancel`, reason);
  }
}