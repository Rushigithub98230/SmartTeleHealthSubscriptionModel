import { Component, Input, Output, EventEmitter, OnInit, AfterViewInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../../../../core/services';
import { StripeClientService } from '../../../../../core/services/stripe-client.service';

/**
 * Add Payment Method Modal Component
 * Allows users to add new credit cards using Stripe Elements
 * 
 * APIs Used:
 * - Stripe.js createPaymentMethod()
 * - POST /api/payments/payment-methods
 * 
 * Features:
 * - Stripe Elements card input (PCI compliant)
 * - Card validation
 * - Set as default option
 * - Error handling
 * 
 * Usage:
 * <app-add-payment-method-modal
 *   [(isOpen)]="showAddCardModal"
 *   (cardAdded)="onCardAdded()">
 * </app-add-payment-method-modal>
 */
@Component({
  selector: 'app-add-payment-method-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-payment-method-modal.component.html',
  styleUrls: ['./add-payment-method-modal.component.scss']
})
export class AddPaymentMethodModalComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();
  @Output() cardAdded = new EventEmitter<void>();

  cardElement: any;
  cardholderName: string = '';
  setAsDefault: boolean = false;
  
  processing = false;
  error: string | null = null;
  successMessage: string | null = null;
  
  cardErrors: string | null = null;
  isStripeReady = false;

  constructor(
    private stripeService: StripeClientService,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    this.isStripeReady = this.stripeService.isReady();
    
    if (!this.isStripeReady) {
      this.error = 'Stripe.js is not loaded. Please refresh the page and try again.';
      console.error('❌ Stripe.js not ready');
    } else {
      console.log('✅ Stripe.js ready');
    }
  }

  ngAfterViewInit(): void {
    // Initial mount if modal is already open
    if (this.isOpen && this.isStripeReady) {
      this.mountCardElement();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Watch for modal opening
    if (changes['isOpen'] && changes['isOpen'].currentValue === true) {
      if (this.isStripeReady) {
        // Modal just opened - mount card element
        setTimeout(() => {
          this.mountCardElement();
        }, 200);
      } else {
        this.error = 'Stripe.js is not loaded. Please refresh the page and try again.';
      }
    }
  }

  ngOnDestroy(): void {
    if (this.cardElement) {
      this.cardElement.unmount();
      this.cardElement = null;
    }
  }

  /**
   * Mount Stripe card element to DOM
   */
  mountCardElement(): void {
    // Unmount existing element if any
    if (this.cardElement) {
      try {
        this.cardElement.unmount();
        this.cardElement = null;
      } catch (err) {
        // Element might already be unmounted
        this.cardElement = null;
      }
    }

    // Small delay to ensure DOM is ready
    setTimeout(() => {
      try {
        // Check if container exists
        const cardElementContainer = document.getElementById('card-element');
        if (!cardElementContainer) {
          console.error('❌ Card element container not found in DOM');
          this.error = 'Payment form not ready. Please close and reopen the modal.';
          return;
        }

        console.log('🔄 Creating Stripe card element...');
        this.cardElement = this.stripeService.createCardElement();
        
        // Listen for card input changes
        this.cardElement.on('change', (event: any) => {
          if (event.error) {
            this.cardErrors = event.error.message;
          } else {
            this.cardErrors = null;
          }
        });
        
        // Mount to DOM
        this.cardElement.mount('#card-element');
        console.log('✅ Stripe card element mounted successfully');
        
        // Clear any previous errors
        this.error = null;
      } catch (err: any) {
        console.error('❌ Error mounting card element:', err);
        this.error = err.message || 'Failed to initialize card input. Please refresh and try again.';
      }
    }, 150);
  }

  /**
   * Validate form before submission
   */
  validateForm(): boolean {
    this.error = null;

    if (!this.cardholderName || this.cardholderName.trim().length === 0) {
      this.error = 'Please enter the cardholder name';
      return false;
    }

    if (this.cardholderName.trim().length < 2) {
      this.error = 'Cardholder name must be at least 2 characters';
      return false;
    }

    if (this.cardErrors) {
      this.error = this.cardErrors;
      return false;
    }

    return true;
  }

  /**
   * Add new card
   * 1. Create PaymentMethod with Stripe
   * 2. Send PaymentMethod ID to backend
   * 3. Optionally set as default
   */
  async addCard(): Promise<void> {
    if (!this.validateForm()) {
      return;
    }

    if (!this.cardElement) {
      this.error = 'Card information not entered';
      return;
    }

    this.processing = true;
    this.error = null;

    try {
      // Create PaymentMethod with Stripe
      const { paymentMethod, error } = await this.stripeService.createPaymentMethod(
        this.cardElement,
        {
          name: this.cardholderName.trim()
        }
      );

      if (error) {
        this.error = error.message || 'Failed to process card information';
        this.processing = false;
        return;
      }

      console.log('✅ PaymentMethod created:', paymentMethod.id);

      // Add to backend
      this.paymentService.addPaymentMethod(paymentMethod.id).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.successMessage = 'Card added successfully!';
            
            // If user wants to set as default, make second API call
            if (this.setAsDefault) {
              this.paymentService.setDefaultPaymentMethod(paymentMethod.id).subscribe({
                next: () => {
                  console.log('✅ Set as default payment method');
                },
                error: (err) => {
                  console.error('Failed to set as default:', err);
                  // Non-critical error, card is still added
                }
              });
            }
            
            // Close modal after 1.5 seconds
            setTimeout(() => {
              this.cardAdded.emit();
              this.close();
            }, 1500);
          } else {
            this.error = response.message || 'Failed to save card';
          }
          this.processing = false;
        },
        error: (err) => {
          console.error('Error adding payment method:', err);
          this.error = err.error?.message || 'Failed to save card. Please try again.';
          this.processing = false;
        }
      });
    } catch (err: any) {
      console.error('Unexpected error:', err);
      this.error = err.message || 'An unexpected error occurred';
      this.processing = false;
    }
  }

  /**
   * Close modal
   */
  close(): void {
    if (this.cardElement) {
      this.cardElement.unmount();
      this.cardElement = null;
    }
    
    this.isOpen = false;
    this.isOpenChange.emit(false);
    
    // Reset form
    this.cardholderName = '';
    this.setAsDefault = false;
    this.error = null;
    this.successMessage = null;
    this.cardErrors = null;
  }

  /**
   * Prevent modal close when clicking inside
   */
  stopPropagation(event: Event): void {
    event.stopPropagation();
  }
}

