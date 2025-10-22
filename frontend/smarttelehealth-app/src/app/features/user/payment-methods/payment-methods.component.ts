import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PaymentService, AuthService } from '../../../core/services';
import { PaymentMethodDto, UserDto } from '../../../core/models';
import { AddPaymentMethodModalComponent } from './components/add-payment-method-modal/add-payment-method-modal.component';

/**
 * Payment Methods Component
 * Manage user's saved payment methods
 * 
 * APIs Used:
 * - GET /api/Payment/methods
 * - POST /api/Payment/methods
 * - PUT /api/Payment/methods/default
 * - DELETE /api/Payment/methods/{id}
 * 
 * Route: /web/payment-methods
 * Access: Authenticated users
 */
@Component({
  selector: 'app-payment-methods',
  standalone: true,
  imports: [CommonModule, RouterLink, AddPaymentMethodModalComponent],
  templateUrl: './payment-methods.component.html',
  styleUrls: ['./payment-methods.component.scss']
})
export class PaymentMethodsComponent implements OnInit {
  currentUser: UserDto | null = null;
  paymentMethods: PaymentMethodDto[] = [];
  loading = false;
  actionLoading = false;
  error: string | null = null;
  
  // Add card modal
  showAddCardModal = false;

  constructor(
    private authService: AuthService,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.loadPaymentMethods();
    }
  }

  /**
   * Load user's payment methods
   * API: GET /api/Payment/methods
   */
  loadPaymentMethods(): void {
    if (!this.currentUser) return;

    this.loading = true;
    this.error = null;

    this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.paymentMethods = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load payment methods';
        this.loading = false;
      }
    });
  }

  /**
   * Set default payment method
   * API: PUT /api/Payment/methods/default
   */
  setDefaultMethod(paymentMethodId: string): void {
    this.actionLoading = true;

    this.paymentService.setDefaultPaymentMethod(paymentMethodId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Reload payment methods to reflect change
          this.loadPaymentMethods();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to set default payment method');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Delete payment method
   * API: DELETE /api/Payment/methods/{id}
   */
  deleteMethod(paymentMethodId: string): void {
    if (!confirm('Are you sure you want to remove this payment method?')) {
      return;
    }

    this.actionLoading = true;

    this.paymentService.deletePaymentMethod(paymentMethodId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Reload payment methods
          this.loadPaymentMethods();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to delete payment method');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Get card brand icon class
   */
  getCardBrandIcon(brand?: string): string {
    const brandMap: { [key: string]: string } = {
      'visa': 'bi-credit-card',
      'mastercard': 'bi-credit-card-2-front',
      'amex': 'bi-credit-card',
      'discover': 'bi-credit-card'
    };
    return brandMap[brand?.toLowerCase() || ''] || 'bi-credit-card';
  }

  /**
   * Check if card is expired
   */
  isCardExpired(expMonth: number, expYear: number): boolean {
    const today = new Date();
    const expDate = new Date(expYear, expMonth - 1);
    return expDate < today;
  }

  /**
   * Check if card expires soon (within 30 days)
   */
  isCardExpiringSoon(expMonth: number, expYear: number): boolean {
    const today = new Date();
    const expiry = new Date(expYear, expMonth - 1);
    const diffTime = expiry.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 && diffDays <= 30;
  }

  /**
   * Get days until card expires
   */
  getDaysUntilExpiry(expMonth: number, expYear: number): number {
    const today = new Date();
    const expiry = new Date(expYear, expMonth - 1);
    const diffTime = expiry.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  /**
   * Open add card modal
   */
  openAddCardModal(): void {
    this.showAddCardModal = true;
  }

  /**
   * Handle card added successfully
   */
  onCardAdded(): void {
    // Reload payment methods to show new card
    this.loadPaymentMethods();
  }
}


