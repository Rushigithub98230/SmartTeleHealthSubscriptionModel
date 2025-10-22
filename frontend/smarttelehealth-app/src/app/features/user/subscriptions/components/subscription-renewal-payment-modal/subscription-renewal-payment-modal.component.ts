import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PaymentService, BillingService, AuthService } from '../../../../../core/services';
import { PaymentMethodDto, BillingRecordDto, ProcessPaymentRequestDto, UserDto } from '../../../../../core/models';

/**
 * Subscription Renewal Payment Modal Component
 * Allows users to manually pay for failed subscription renewals
 * 
 * APIs Used:
 * - GET /api/Billing/subscription/{subscriptionId}
 * - GET /api/payments/payment-methods
 * - POST /api/payments/process-payment
 * 
 * Usage:
 * <app-subscription-renewal-payment-modal
 *   [subscriptionId]="subscriptionId"
 *   [(isOpen)]="showPaymentModal"
 *   (paymentSuccess)="onPaymentSuccess()">
 * </app-subscription-renewal-payment-modal>
 */
@Component({
  selector: 'app-subscription-renewal-payment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './subscription-renewal-payment-modal.component.html',
  styleUrls: ['./subscription-renewal-payment-modal.component.scss']
})
export class SubscriptionRenewalPaymentModalComponent implements OnInit, OnChanges {
  @Input() subscriptionId!: string;
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();
  @Output() paymentSuccess = new EventEmitter<void>();

  currentUser: UserDto | null = null;
  pendingBilling: BillingRecordDto | null = null;
  paymentMethods: PaymentMethodDto[] = [];
  selectedPaymentMethodId: string = '';
  
  loading = false;
  processing = false;
  error: string | null = null;
  successMessage: string | null = null;

  constructor(
    private billingService: BillingService,
    private paymentService: PaymentService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.isOpen && this.subscriptionId) {
      this.loadData();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && changes['isOpen'].currentValue && this.subscriptionId) {
      this.loadData();
    }
  }

  /**
   * Load pending billing and payment methods
   */
  loadData(): void {
    this.loading = true;
    this.error = null;
    this.successMessage = null;

    // Load both in parallel
    Promise.all([
      this.loadPendingBilling(),
      this.loadPaymentMethods()
    ]).then(() => {
      this.loading = false;
    }).catch(err => {
      this.error = err || 'Failed to load payment details';
      this.loading = false;
    });
  }

  /**
   * Load pending/failed billing record for subscription
   * API: GET /api/Billing/subscription/{subscriptionId}
   */
  async loadPendingBilling(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.billingService.getSubscriptionBillingHistory(this.subscriptionId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            // Find first pending or failed billing record (most recent)
            this.pendingBilling = response.data.find(
              b => b.status === 'Pending' || b.status === 'Failed'
            ) || null;
            
            if (!this.pendingBilling) {
              reject('No pending payment found for this subscription');
            } else {
              resolve();
            }
          } else {
            reject(response.message || 'Failed to load billing records');
          }
        },
        error: (err) => {
          reject(err.error?.message || 'Failed to load billing records');
        }
      });
    });
  }

  /**
   * Load user's payment methods
   * API: GET /api/payments/payment-methods
   */
  async loadPaymentMethods(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (!this.currentUser) {
        reject('User not authenticated');
        return;
      }

      this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.paymentMethods = response.data;
            
            // Auto-select default payment method
            const defaultMethod = this.paymentMethods.find(pm => pm.isDefault);
            if (defaultMethod) {
              this.selectedPaymentMethodId = defaultMethod.id;
            } else if (this.paymentMethods.length > 0) {
              // If no default, select first method
              this.selectedPaymentMethodId = this.paymentMethods[0].id;
            }
            
            resolve();
          } else {
            reject(response.message || 'Failed to load payment methods');
          }
        },
        error: (err) => {
          reject(err.error?.message || 'Failed to load payment methods');
        }
      });
    });
  }

  /**
   * Process renewal payment
   * API: POST /api/payments/process-payment
   */
  processPayment(): void {
    if (!this.pendingBilling || !this.selectedPaymentMethodId) {
      this.error = 'Please select a payment method';
      return;
    }

    // Validate payment method selected
    if (!this.validatePaymentMethod()) {
      return;
    }

    this.processing = true;
    this.error = null;

    const request: ProcessPaymentRequestDto = {
      billingRecordId: this.pendingBilling.id,
      paymentMethodId: this.selectedPaymentMethodId
    };

    console.log('Processing renewal payment:', request);

    this.paymentService.processPayment(request).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Payment successful
          this.successMessage = 'Payment successful! Your subscription has been renewed.';
          this.processing = false;
          
          // Emit success event
          this.paymentSuccess.emit();
          
          // Close modal after 2 seconds
          setTimeout(() => {
            this.close();
          }, 2000);
        } else {
          this.error = response.message || 'Payment failed. Please try again.';
          this.processing = false;
        }
      },
      error: (error) => {
        console.error('Payment error:', error);
        
        // Parse error message
        let errorMessage = 'Payment failed. Please try again.';
        
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.status === 400) {
          errorMessage = 'Payment declined. Please check your card details or try a different payment method.';
        } else if (error.status === 403) {
          errorMessage = 'Access denied. You can only pay for your own subscriptions.';
        }
        
        this.error = errorMessage;
        this.processing = false;
      }
    });
  }

  /**
   * Validate selected payment method
   */
  validatePaymentMethod(): boolean {
    if (!this.selectedPaymentMethodId) {
      this.error = 'Please select a payment method';
      return false;
    }

    const method = this.paymentMethods.find(pm => pm.id === this.selectedPaymentMethodId);
    if (!method) {
      this.error = 'Invalid payment method';
      return false;
    }

    // Check if card is expired
    if (method.card) {
      const now = new Date();
      const expiry = new Date(method.card.expYear, method.card.expMonth - 1);
      if (expiry < now) {
        this.error = 'Selected card has expired. Please select a different payment method.';
        return false;
      }
    }

    return true;
  }

  /**
   * Get selected payment method
   */
  getSelectedPaymentMethod(): PaymentMethodDto | undefined {
    return this.paymentMethods.find(pm => pm.id === this.selectedPaymentMethodId);
  }

  /**
   * Close modal
   */
  close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(false);
    this.error = null;
    this.successMessage = null;
  }

  /**
   * Prevent modal close when clicking inside
   */
  stopPropagation(event: Event): void {
    event.stopPropagation();
  }
}


