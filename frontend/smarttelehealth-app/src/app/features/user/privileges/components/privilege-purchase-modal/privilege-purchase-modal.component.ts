import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PaymentService, SubscriptionService, AuthService } from '../../../../../core/services';
import { PaymentMethodDto, UserDto } from '../../../../../core/models';

/**
 * Privilege Purchase Modal Component
 * Allows users to purchase additional privilege credits with immediate payment
 * 
 * APIs Used:
 * - GET /api/payments/payment-methods
 * - POST /api/Subscriptions/{id}/purchase-credits
 * 
 * Usage:
 * <app-privilege-purchase-modal
 *   [subscriptionId]="subscriptionId"
 *   [privilegeName]="privilegeName"
 *   [unitCost]="20.00"
 *   [currentUsed]="5"
 *   [currentAllowed]="5"
 *   [(isOpen)]="showModal"
 *   (purchaseSuccess)="onPurchaseSuccess()">
 * </app-privilege-purchase-modal>
 */
@Component({
  selector: 'app-privilege-purchase-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './privilege-purchase-modal.component.html',
  styleUrls: ['./privilege-purchase-modal.component.scss']
})
export class PrivilegePurchaseModalComponent implements OnInit, OnChanges {
  @Input() subscriptionId!: string;
  @Input() privilegeName!: string;
  @Input() unitCost: number = 20.00;
  @Input() currentUsed: number = 0;
  @Input() currentAllowed: number = 0;
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();
  @Output() purchaseSuccess = new EventEmitter<any>();

  currentUser: UserDto | null = null;
  paymentMethods: PaymentMethodDto[] = [];
  selectedPaymentMethodId: string = '';
  quantity: number = 1;
  totalCost: number = 0;
  
  loading = false;
  purchasing = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Constants
  readonly MIN_QUANTITY = 1;
  readonly MAX_QUANTITY = 100;

  constructor(
    private paymentService: PaymentService,
    private subscriptionService: SubscriptionService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.calculateTotalCost();
    
    if (this.isOpen) {
      this.loadPaymentMethods();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && changes['isOpen'].currentValue) {
      this.loadPaymentMethods();
      this.calculateTotalCost();
    }
    
    if (changes['unitCost'] || changes['quantity']) {
      this.calculateTotalCost();
    }
  }

  /**
   * Load user's payment methods
   * API: GET /api/payments/payment-methods
   */
  loadPaymentMethods(): void {
    if (!this.currentUser) return;

    this.loading = true;
    this.error = null;

    this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.paymentMethods = response.data;
          
          // Auto-select default payment method
          const defaultMethod = this.paymentMethods.find(pm => pm.isDefault);
          if (defaultMethod) {
            this.selectedPaymentMethodId = defaultMethod.id;
          } else if (this.paymentMethods.length > 0) {
            this.selectedPaymentMethodId = this.paymentMethods[0].id;
          }
        } else {
          this.error = response.message || 'Failed to load payment methods';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading payment methods:', error);
        this.error = error.error?.message || 'Failed to load payment methods';
        this.loading = false;
      }
    });
  }

  /**
   * Calculate total cost based on quantity and unit cost
   */
  calculateTotalCost(): void {
    this.totalCost = this.quantity * this.unitCost;
  }

  /**
   * Update quantity and recalculate total
   */
  updateQuantity(newQuantity: number): void {
    if (newQuantity < this.MIN_QUANTITY) {
      this.quantity = this.MIN_QUANTITY;
    } else if (newQuantity > this.MAX_QUANTITY) {
      this.quantity = this.MAX_QUANTITY;
    } else {
      this.quantity = newQuantity;
    }
    
    this.calculateTotalCost();
  }

  /**
   * Increment quantity
   */
  incrementQuantity(): void {
    if (this.quantity < this.MAX_QUANTITY) {
      this.quantity++;
      this.calculateTotalCost();
    }
  }

  /**
   * Decrement quantity
   */
  decrementQuantity(): void {
    if (this.quantity > this.MIN_QUANTITY) {
      this.quantity--;
      this.calculateTotalCost();
    }
  }

  /**
   * Validate purchase before submission
   */
  validatePurchase(): boolean {
    if (this.quantity < this.MIN_QUANTITY || this.quantity > this.MAX_QUANTITY) {
      this.error = `Quantity must be between ${this.MIN_QUANTITY} and ${this.MAX_QUANTITY}`;
      return false;
    }

    if (!this.selectedPaymentMethodId) {
      this.error = 'Please select a payment method';
      return false;
    }

    // Validate payment method
    const method = this.paymentMethods.find(pm => pm.id === this.selectedPaymentMethodId);
    if (!method) {
      this.error = 'Invalid payment method selected';
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
   * Purchase additional credits
   * API: POST /api/Subscriptions/{id}/purchase-credits
   */
  purchaseCredits(): void {
    if (!this.validatePurchase()) {
      return;
    }

    this.purchasing = true;
    this.error = null;

    const dto = {
      privilegeName: this.privilegeName,
      quantity: this.quantity,
      paymentMethodId: this.selectedPaymentMethodId
    };

    console.log('Purchasing credits:', dto);

    this.subscriptionService.purchaseAdditionalCredits(this.subscriptionId, dto).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          const data = response.data;
          
          this.successMessage = `Successfully purchased ${data.creditsAdded} credit${data.creditsAdded > 1 ? 's' : ''} for $${data.totalPaid.toFixed(2)}`;
          this.purchasing = false;
          
          // Emit success event with purchase details
          this.purchaseSuccess.emit(data);
          
          // Close modal after 2 seconds
          setTimeout(() => {
            this.close();
          }, 2000);
        } else {
          this.error = response.message || 'Purchase failed';
          this.purchasing = false;
        }
      },
      error: (error) => {
        console.error('Purchase error:', error);
        
        // Parse error message
        let errorMessage = 'Purchase failed. Please try again.';
        
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.status === 400) {
          errorMessage = 'Payment declined. Please check your card details or try a different payment method.';
        } else if (error.status === 403) {
          errorMessage = 'Access denied. You can only purchase credits for your own subscription.';
        } else if (error.status === 404) {
          errorMessage = 'Subscription or privilege not found.';
        }
        
        this.error = errorMessage;
        this.purchasing = false;
      }
    });
  }

  /**
   * Get selected payment method details
   */
  getSelectedPaymentMethod(): PaymentMethodDto | undefined {
    return this.paymentMethods.find(pm => pm.id === this.selectedPaymentMethodId);
  }

  /**
   * Calculate new limit after purchase
   */
  getNewLimit(): number {
    return this.currentAllowed + this.quantity;
  }

  /**
   * Calculate remaining credits after purchase
   */
  getNewRemaining(): number {
    return (this.currentAllowed + this.quantity) - this.currentUsed;
  }

  /**
   * Close modal
   */
  close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(false);
    this.error = null;
    this.successMessage = null;
    this.quantity = 1;
    this.calculateTotalCost();
  }

  /**
   * Prevent modal close when clicking inside
   */
  stopPropagation(event: Event): void {
    event.stopPropagation();
  }
}


