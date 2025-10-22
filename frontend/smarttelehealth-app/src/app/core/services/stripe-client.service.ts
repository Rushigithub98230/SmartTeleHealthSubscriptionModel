import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * Stripe Client Service
 * Handles client-side Stripe.js integration for secure card collection
 * 
 * Features:
 * - Create Stripe Elements (card input)
 * - Create PaymentMethod from card
 * - Secure PCI-compliant card handling
 * 
 * Configuration Strategy:
 * 1. Primary: Uses key from environment.ts (synced with backend appsettings.json)
 * 2. Alternative: Can fetch from backend /api/Config/stripe-public (via ConfigService)
 * 
 * Current Implementation:
 * - Using environment.ts for simplicity (same key as backend)
 * - Both frontend and backend use the same Stripe account keys
 * - Keys are managed in:
 *   - Development: frontend/src/environments/environment.ts
 *   - Production: frontend/src/environments/environment.prod.ts
 *   - Backend: appsettings.json (StripeSettings:PublishableKey)
 * 
 * Usage:
 * 1. Load Stripe.js in index.html ✅
 * 2. Inject this service
 * 3. Create card element
 * 4. Mount to DOM
 * 5. Create PaymentMethod on submit
 */

declare var Stripe: any;

@Injectable({
  providedIn: 'root'
})
export class StripeClientService {
  private stripe: any;
  private isStripeLoaded = false;

  constructor() {
    this.initializeStripe();
  }

  /**
   * Initialize Stripe instance with publishable key from environment
   * 
   * Configuration Sources (in order of precedence):
   * 1. environment.ts / environment.prod.ts (currently used)
   * 2. Can optionally fetch from backend /api/Config/stripe-public
   * 
   * The publishable key is safe to embed in frontend code as it only
   * allows creating PaymentMethods and tokens, not charging customers.
   * 
   * Note: Requires Stripe.js loaded in index.html
   */
  private initializeStripe(): void {
    if (typeof Stripe !== 'undefined') {
      // Get publishable key from environment configuration
      const publishableKey = environment.stripePublishableKey;
      
      if (!publishableKey || publishableKey === 'pk_test_51234567890' || publishableKey === 'pk_live_YOUR_LIVE_KEY') {
        console.error('⚠️ Stripe publishable key not configured!');
        console.error('📝 Please update the key in:');
        console.error('   - Development: frontend/src/environments/environment.ts');
        console.error('   - Production: frontend/src/environments/environment.prod.ts');
        console.error('💡 Get your key from: https://dashboard.stripe.com/test/apikeys');
        return;
      }
      
      this.stripe = Stripe(publishableKey);
      this.isStripeLoaded = true;
      
      const keyType = publishableKey.startsWith('pk_live') ? 'LIVE' : 'TEST';
      console.log(`✅ Stripe initialized successfully (${keyType} mode)`);
      console.log('🔑 Using publishable key:', publishableKey.substring(0, 20) + '...');
    } else {
      console.error('❌ Stripe.js not loaded!');
      console.error('📝 Add this to frontend/smarttelehealth-app/src/index.html:');
      console.error('   <script src="https://js.stripe.com/v3/"></script>');
    }
  }

  /**
   * Check if Stripe is loaded
   */
  isReady(): boolean {
    return this.isStripeLoaded;
  }

  /**
   * Create Stripe Elements card input
   * Returns card element ready to mount to DOM
   */
  createCardElement(): any {
    if (!this.stripe) {
      throw new Error('Stripe not initialized');
    }

    const elements = this.stripe.elements();
    
    const cardElement = elements.create('card', {
      style: {
        base: {
          fontSize: '16px',
          color: '#32325d',
          fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
          fontSmoothing: 'antialiased',
          '::placeholder': {
            color: '#aab7c4'
          }
        },
        invalid: {
          color: '#fa755a',
          iconColor: '#fa755a'
        }
      },
      hidePostalCode: false // Show ZIP code field
    });

    return cardElement;
  }

  /**
   * Create PaymentMethod from card element
   * Returns {paymentMethod, error}
   * 
   * @param cardElement - Stripe card element
   * @param billingDetails - Optional billing details (name, email, address)
   */
  async createPaymentMethod(
    cardElement: any,
    billingDetails?: {
      name?: string;
      email?: string;
      phone?: string;
      address?: {
        line1?: string;
        city?: string;
        state?: string;
        postal_code?: string;
        country?: string;
      }
    }
  ): Promise<{paymentMethod?: any, error?: any}> {
    if (!this.stripe) {
      return { error: { message: 'Stripe not initialized' } };
    }

    try {
      const result = await this.stripe.createPaymentMethod({
        type: 'card',
        card: cardElement,
        billing_details: billingDetails
      });

      return result;
    } catch (error) {
      console.error('Error creating PaymentMethod:', error);
      return { error: { message: 'Failed to create payment method' } };
    }
  }

  /**
   * Confirm payment (for immediate charges)
   * Used when processing payments that require 3D Secure
   */
  async confirmCardPayment(clientSecret: string, cardElement: any): Promise<any> {
    if (!this.stripe) {
      throw new Error('Stripe not initialized');
    }

    return await this.stripe.confirmCardPayment(clientSecret, {
      payment_method: {
        card: cardElement
      }
    });
  }

  /**
   * Handle card action (for 3D Secure)
   */
  async handleCardAction(clientSecret: string): Promise<any> {
    if (!this.stripe) {
      throw new Error('Stripe not initialized');
    }

    return await this.stripe.handleCardAction(clientSecret);
  }
}

