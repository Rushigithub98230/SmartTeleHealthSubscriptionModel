import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonService } from '../../../../core/services/common.service';

/**
 * Subscription Success Component
 * Handles successful subscription creation from Stripe checkout
 * 
 * Route: /web/subscriptions/success
 * Query Params: session_id (from Stripe)
 */
@Component({
  selector: 'app-subscription-success',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-8 col-lg-6">
          <div class="card shadow-sm">
            <div class="card-body text-center p-5">
              <!-- Success Icon -->
              <div class="mb-4">
                <i class="bi bi-check-circle-fill text-success" style="font-size: 4rem;"></i>
              </div>
              
              <!-- Success Message -->
              <h2 class="text-success mb-3" *ngIf="verified">Subscription Created Successfully!</h2>
              <h2 class="text-primary mb-3" *ngIf="!verified && !error">Verifying Your Subscription...</h2>
              <p class="text-muted mb-4" *ngIf="verified">
                Your subscription has been activated and your payment method has been saved for future use.
              </p>
              <p class="text-muted mb-4" *ngIf="!verified && !error">
                Please wait while we verify your subscription and billing information...
              </p>
              
              <!-- Verification Status -->
              <div *ngIf="verified && subscriptionId" class="alert alert-success mb-4">
                <i class="bi bi-check-circle me-2"></i>
                <strong>Verification Complete:</strong> Subscription {{subscriptionId}} is active
                <br>
                <small *ngIf="billingRecordCount > 0">{{billingRecordCount}} billing record(s) created successfully</small>
              </div>
              
              <!-- Payment Method Info -->
              <div *ngIf="verified" class="alert alert-success mb-4">
                <i class="bi bi-credit-card me-2"></i>
                <strong>Payment Method Saved:</strong> Your card details have been securely saved to your account.
                <br>
                <small>You can now use this payment method for future purchases and subscriptions.</small>
              </div>
              
              <!-- Session Info (if available) -->
              <div *ngIf="sessionId" class="alert alert-info mb-4">
                <i class="bi bi-info-circle me-2"></i>
                <strong>Session ID:</strong> {{sessionId}}
                <br>
                <small>Keep this for your records</small>
              </div>
              
              <!-- Loading State -->
              <div *ngIf="loading" class="mb-4">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2 text-muted">Verifying your subscription...</p>
              </div>
              
              <!-- Error State -->
              <div *ngIf="error" class="alert alert-danger mb-4">
                <i class="bi bi-exclamation-triangle me-2"></i>
                {{error}}
              </div>
              
              <!-- Action Buttons -->
              <div class="d-grid gap-2 d-md-flex justify-content-md-center">
                <button class="btn btn-primary btn-lg" (click)="goToSubscriptions()">
                  <i class="bi bi-list-ul me-2"></i> View My Subscriptions
                </button>
                <button class="btn btn-outline-primary btn-lg" (click)="goToPaymentMethods()">
                  <i class="bi bi-credit-card me-2"></i> Manage Payment Methods
                </button>
                <button class="btn btn-outline-secondary btn-lg" (click)="goToDashboard()">
                  <i class="bi bi-house me-2"></i> Go to Dashboard
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      border: none;
      border-radius: 15px;
    }
    
    .bi-check-circle-fill {
      animation: pulse 2s infinite;
    }
    
    @keyframes pulse {
      0% { transform: scale(1); }
      50% { transform: scale(1.05); }
      100% { transform: scale(1); }
    }
  `]
})
export class SubscriptionSuccessComponent implements OnInit, OnDestroy {
  sessionId: string | null = null;
  loading = true;
  error: string | null = null;
  verified = false;
  verificationStatus: string = 'pending';
  subscriptionId: string | null = null;
  billingRecordCount: number = 0;
  private maxRetries = 30; // Maximum number of polling attempts (30 * 2s = 60s max wait)
  private retryCount = 0;
  private pollInterval: any = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private commonService: CommonService
  ) {}

  ngOnInit(): void {
    console.log('🎯 [SUBSCRIPTION-SUCCESS] Component initialized');
    
    // Get session ID from query params
    this.sessionId = this.route.snapshot.queryParams['session_id'];
    console.log('🔗 [SUBSCRIPTION-SUCCESS] Session ID from URL:', this.sessionId);
    
    if (this.sessionId) {
      console.log('✅ [SUBSCRIPTION-SUCCESS] Session ID found - starting verification process');
      this.verifySubscription();
    } else {
      console.log('⚠️ [SUBSCRIPTION-SUCCESS] No session ID found in URL');
      this.loading = false;
      this.error = 'No session ID provided. Please contact support if you believe this is an error.';
    }
  }

  /**
   * Verify subscription creation and payment method saving
   * This calls APIs to verify the subscription was created and payment method was saved
   */
  private verifySubscription(): void {
    console.log('🔍 [SUBSCRIPTION-SUCCESS] Starting subscription verification for session:', this.sessionId);
    
    if (!this.sessionId) {
      console.error('❌ [SUBSCRIPTION-SUCCESS] No session ID provided');
      this.loading = false;
      this.error = 'No session ID provided. Please contact support if you believe this is an error.';
      return;
    }

    // Verify subscription was created
    this.verifySubscriptionCreation();
    
    // Verify payment method was saved
    this.verifyPaymentMethodSaved();
  }

  /**
   * Verify subscription was created successfully
   * Polls the verification endpoint until subscription and billing record are confirmed
   */
  private verifySubscriptionCreation(): void {
    console.log('📋 [SUBSCRIPTION-SUCCESS] Verifying subscription creation for session:', this.sessionId);
    
    if (!this.sessionId) {
      console.error('❌ [SUBSCRIPTION-SUCCESS] No session ID provided');
      this.loading = false;
      this.error = 'No session ID provided. Please contact support if you believe this is an error.';
      return;
    }

    // Start polling the verification endpoint
    this.pollVerification();
  }

  /**
   * Polls the verification endpoint until subscription and billing record are confirmed
   */
  private pollVerification(): void {
    if (this.retryCount >= this.maxRetries) {
      console.error('❌ [SUBSCRIPTION-SUCCESS] Max retries reached - verification failed');
      this.loading = false;
      this.error = 'Subscription verification timed out. Please check your subscriptions or contact support.';
      return;
    }

    this.retryCount++;
    console.log(`🔍 [SUBSCRIPTION-SUCCESS] Polling verification attempt ${this.retryCount}/${this.maxRetries}`);

    this.commonService.get<any>(`Stripe/verify-session/${this.sessionId}`).subscribe({
      next: (response) => {
        console.log('📥 [SUBSCRIPTION-SUCCESS] Verification response:', response);
        
        if (response.statusCode === 200 && response.data?.verified === true) {
          // Verification successful!
          console.log('✅ [SUBSCRIPTION-SUCCESS] Verification successful!');
          this.verified = true;
          this.verificationStatus = 'verified';
          this.subscriptionId = response.data.subscriptionId;
          this.billingRecordCount = response.data.billingRecordCount || 0;
          this.loading = false;
          
          // Clear any polling interval
          if (this.pollInterval) {
            clearInterval(this.pollInterval);
            this.pollInterval = null;
          }
        } else if (response.statusCode === 202) {
          // Still processing - continue polling
          console.log(`⏳ [SUBSCRIPTION-SUCCESS] Verification pending (${response.data?.reason || 'processing'})`);
          this.verificationStatus = 'pending';
          
          // Continue polling after 2 seconds
          setTimeout(() => {
            this.pollVerification();
          }, 2000);
        } else {
          // Error occurred
          console.error('❌ [SUBSCRIPTION-SUCCESS] Verification error:', response.message);
          this.loading = false;
          this.error = response.message || 'Failed to verify subscription. Please check your subscriptions or contact support.';
          
          // Clear polling interval
          if (this.pollInterval) {
            clearInterval(this.pollInterval);
            this.pollInterval = null;
          }
        }
      },
      error: (err) => {
        console.error('❌ [SUBSCRIPTION-SUCCESS] Verification API error:', err);
        
        // If it's a 404 or server error, continue polling (might be temporary)
        if (this.retryCount < this.maxRetries && (err.status === 404 || err.status >= 500)) {
          console.log(`⏳ [SUBSCRIPTION-SUCCESS] Retrying after error (attempt ${this.retryCount}/${this.maxRetries})`);
          setTimeout(() => {
            this.pollVerification();
          }, 2000);
        } else {
          // Max retries or client error - stop polling
          this.loading = false;
          this.error = err.error?.message || 'Failed to verify subscription. Please check your subscriptions or contact support.';
          
          // Clear polling interval
          if (this.pollInterval) {
            clearInterval(this.pollInterval);
            this.pollInterval = null;
          }
        }
      }
    });
  }

  /**
   * Cleanup polling on component destroy
   */
  ngOnDestroy(): void {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
      this.pollInterval = null;
    }
  }

  /**
   * Verify payment method was saved to user's profile
   * Note: Payment method is automatically saved by Stripe during checkout
   * This is just for logging - actual verification is handled by verifySubscriptionCreation
   */
  private verifyPaymentMethodSaved(): void {
    console.log('💳 [SUBSCRIPTION-SUCCESS] Payment method is automatically saved by Stripe during checkout');
    // Payment method verification is handled as part of subscription verification
    // No separate API call needed
  }

  goToSubscriptions(): void {
    this.router.navigate(['/web/subscriptions']);
  }

  goToPaymentMethods(): void {
    this.router.navigate(['/web/payment-methods']);
  }

  goToDashboard(): void {
    this.router.navigate(['/web/dashboard']);
  }
}
