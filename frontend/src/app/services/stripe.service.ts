import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

export interface CheckoutSessionRequest {
  successUrl: string;
  cancelUrl: string;
  priceId?: string;
  planId?: string;
  userId?: number;
}

export interface CheckoutSessionResponse {
  url: string;
  sessionId: string;
}

export interface StripeCustomer {
  id: string;
  email: string;
  name?: string;
  phone?: string;
  address?: any;
  created: number;
}

export interface StripePrice {
  id: string;
  product: string;
  unit_amount: number;
  currency: string;
  recurring?: {
    interval: string;
    interval_count: number;
  };
  active: boolean;
}

export interface StripeProduct {
  id: string;
  name: string;
  description?: string;
  active: boolean;
  metadata?: any;
}

@Injectable({
  providedIn: 'root'
})
export class StripeService {
  constructor(private commonService: CommonService) {}

  /**
   * Test Stripe connection
   */
  testConnection(): Observable<ApiResponse<any>> {
    return this.commonService.getWithAuth<any>('/api/stripe/test-connection');
  }

  /**
   * Create a checkout session for subscription
   */
  createCheckoutSession(request: CheckoutSessionRequest): Observable<ApiResponse<CheckoutSessionResponse>> {
    return this.commonService.postWithAuth<CheckoutSessionResponse>('/api/stripe/create-checkout-session', request);
  }

  /**
   * Get Stripe customers
   */
  getCustomers(): Observable<ApiResponse<StripeCustomer[]>> {
    return this.commonService.getWithAuth<StripeCustomer[]>('/api/stripe/customers');
  }

  /**
   * Get Stripe products
   */
  getProducts(): Observable<ApiResponse<StripeProduct[]>> {
    return this.commonService.getWithAuth<StripeProduct[]>('/api/stripe/products');
  }

  /**
   * Get Stripe prices
   */
  getPrices(): Observable<ApiResponse<StripePrice[]>> {
    return this.commonService.getWithAuth<StripePrice[]>('/api/stripe/prices');
  }

  /**
   * Create a Stripe product
   */
  createProduct(product: Partial<StripeProduct>): Observable<ApiResponse<StripeProduct>> {
    return this.commonService.postWithAuth<StripeProduct>('/api/stripe/products', product);
  }

  /**
   * Create a Stripe price
   */
  createPrice(price: Partial<StripePrice>): Observable<ApiResponse<StripePrice>> {
    return this.commonService.postWithAuth<StripePrice>('/api/stripe/prices', price);
  }

  /**
   * Sync subscription plans with Stripe (Note: This endpoint may not be available in backend)
   */
  syncPlansWithStripe(): Observable<ApiResponse<any>> {
    // For now, we'll use a placeholder endpoint that may need to be implemented in backend
    return this.commonService.postWithAuth<any>('/api/stripe/sync-plans', {});
  }

  /**
   * Get subscription by Stripe subscription ID
   */
  getSubscriptionByStripeId(stripeSubscriptionId: string): Observable<ApiResponse<any>> {
    return this.commonService.getWithAuth<any>(`/api/stripe/subscriptions/${stripeSubscriptionId}`);
  }

  /**
   * Cancel Stripe subscription
   */
  cancelStripeSubscription(stripeSubscriptionId: string, reason?: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/stripe/subscriptions/${stripeSubscriptionId}/cancel`, { reason });
  }

  /**
   * Pause Stripe subscription
   */
  pauseStripeSubscription(stripeSubscriptionId: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/stripe/subscriptions/${stripeSubscriptionId}/pause`, {});
  }

  /**
   * Resume Stripe subscription
   */
  resumeStripeSubscription(stripeSubscriptionId: string): Observable<ApiResponse<any>> {
    return this.commonService.postWithAuth<any>(`/api/stripe/subscriptions/${stripeSubscriptionId}/resume`, {});
  }
}
