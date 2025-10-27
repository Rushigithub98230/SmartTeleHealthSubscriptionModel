import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

export interface CheckoutSessionRequest {
  planId: string;
  successUrl: string;
  cancelUrl: string;
}

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
   * Create Stripe checkout session
   * API: POST /api/stripe/create-checkout-session
   */
  createCheckoutSession(request: CheckoutSessionRequest): Observable<ApiResponse<CheckoutSessionResponse>> {
    return this.commonService.post<CheckoutSessionResponse>('stripe/create-checkout-session', request);
  }

  /**
   * Redirect to Stripe checkout
   */
  redirectToCheckout(url: string): void {
    if (url) {
      window.location.href = url;
    } else {
      console.error('No checkout URL provided');
    }
  }
}
