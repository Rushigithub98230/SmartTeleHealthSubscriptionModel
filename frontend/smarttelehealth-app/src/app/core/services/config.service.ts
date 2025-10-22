import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { CommonService, ApiResponse } from './common.service';
import { environment } from '../../../environments/environment';

/**
 * Configuration Service
 * Fetches public configuration from backend and provides fallback to environment
 * 
 * APIs Used:
 * - GET /api/Config/stripe-public - Get Stripe publishable key
 * - GET /api/Config/frontend - Get all frontend configuration
 * 
 * Usage:
 * 1. Call loadStripeConfig() on app initialization
 * 2. Use getStripePublishableKey() to get the key
 */

export interface StripeConfig {
  publishableKey: string;
  currency: string;
  locale: string;
}

export interface FrontendConfig {
  stripe: StripeConfig;
  features: {
    chatEnabled: boolean;
    videoEnabled: boolean;
    appointmentsEnabled: boolean;
    subscriptionsEnabled: boolean;
  };
  limits: {
    maxFileUploadSizeMB: number;
    maxMessageLength: number;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private stripeConfig: StripeConfig | null = null;
  private frontendConfig: FrontendConfig | null = null;

  constructor(private commonService: CommonService) {}

  /**
   * Load Stripe configuration from backend
   * Falls back to environment.ts if backend call fails
   * 
   * API: GET /api/Config/stripe-public
   */
  loadStripeConfig(): Observable<StripeConfig> {
    return this.commonService.get<StripeConfig>('Config/stripe-public').pipe(
      map(response => {
        if (response.statusCode === 200 && response.data) {
          this.stripeConfig = response.data;
          console.log('✅ Loaded Stripe config from backend:', this.stripeConfig.publishableKey.substring(0, 20) + '...');
          return response.data;
        } else {
          throw new Error('Failed to load Stripe config from backend');
        }
      }),
      catchError(error => {
        console.warn('⚠️ Failed to load Stripe config from backend, using environment fallback');
        console.warn('Error:', error);
        
        // Fallback to environment configuration
        const fallbackConfig: StripeConfig = {
          publishableKey: environment.stripePublishableKey,
          currency: 'usd',
          locale: 'en'
        };
        
        this.stripeConfig = fallbackConfig;
        return of(fallbackConfig);
      })
    );
  }

  /**
   * Load complete frontend configuration from backend
   * 
   * API: GET /api/Config/frontend
   */
  loadFrontendConfig(): Observable<FrontendConfig> {
    return this.commonService.get<FrontendConfig>('Config/frontend').pipe(
      map(response => {
        if (response.statusCode === 200 && response.data) {
          this.frontendConfig = response.data;
          console.log('✅ Loaded frontend config from backend');
          return response.data;
        } else {
          throw new Error('Failed to load frontend config from backend');
        }
      }),
      catchError(error => {
        console.warn('⚠️ Failed to load frontend config from backend, using defaults');
        
        // Fallback to default configuration
        const fallbackConfig: FrontendConfig = {
          stripe: {
            publishableKey: environment.stripePublishableKey,
            currency: 'usd',
            locale: 'en'
          },
          features: {
            chatEnabled: true,
            videoEnabled: true,
            appointmentsEnabled: true,
            subscriptionsEnabled: true
          },
          limits: {
            maxFileUploadSizeMB: 10,
            maxMessageLength: 1000
          }
        };
        
        this.frontendConfig = fallbackConfig;
        return of(fallbackConfig);
      })
    );
  }

  /**
   * Get Stripe publishable key
   * Returns cached value or environment fallback
   */
  getStripePublishableKey(): string {
    return this.stripeConfig?.publishableKey || environment.stripePublishableKey;
  }

  /**
   * Get Stripe configuration
   */
  getStripeConfig(): StripeConfig | null {
    return this.stripeConfig;
  }

  /**
   * Get frontend configuration
   */
  getFrontendConfig(): FrontendConfig | null {
    return this.frontendConfig;
  }

  /**
   * Check if a feature is enabled
   */
  isFeatureEnabled(feature: keyof FrontendConfig['features']): boolean {
    return this.frontendConfig?.features[feature] ?? true;
  }
}


