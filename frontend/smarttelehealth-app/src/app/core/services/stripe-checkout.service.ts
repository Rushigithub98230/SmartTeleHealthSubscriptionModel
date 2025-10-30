import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

export interface CheckoutSessionResponse {
  url: string;
  sessionId: string;
}

@Injectable({
  providedIn: 'root'
})
export class StripeCheckoutService {

  constructor(private commonService: CommonService) { }

  /**
   * Create Stripe checkout session using the production-ready endpoint
   * 
   * This endpoint automatically:
   * - Validates user eligibility (no active subscriptions)
   * - Prevents customer ID duplication (searches Stripe by email)
   * - Syncs customer ID to User table
   * - Constructs success/cancel URLs securely on backend
   * 
   * API: POST /api/stripe/create-checkout-session/{planId}
   * 
   * @param planId - The subscription plan ID to purchase
   * @returns Observable with checkout session URL
   */
  createCheckoutSession(planId: string): Observable<ApiResponse<CheckoutSessionResponse>> {
    return this.commonService.post<CheckoutSessionResponse>(`stripe/create-checkout-session/${planId}`, {});
  }

  /**
   * Redirect to Stripe checkout
   * @param url - The Stripe checkout session URL
   */
  redirectToCheckout(url: string): void {
    if (url) {
      window.location.href = url;
    } else {
      console.error('No checkout URL provided');
    }
  }
}
