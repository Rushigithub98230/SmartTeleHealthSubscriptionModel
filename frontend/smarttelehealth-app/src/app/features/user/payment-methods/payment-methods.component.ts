import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PaymentService, AuthService } from '../../../core/services';
import { PaymentMethodDto, UserDto } from '../../../core/models';
import { AddPaymentMethodModalComponent } from './components/add-payment-method-modal/add-payment-method-modal.component';

/**
 * Payment Methods Management Component
 * Allows users to view, add, set as default, and delete payment methods
 * 
 * APIs Used:
 * - GET /api/Payment/payment-methods
 * - POST /api/Payment/payment-methods
 * - PUT /api/Payment/payment-methods/{id}/default
 * - DELETE /api/Payment/payment-methods/{id}
 * 
 * Route: /web/payment-methods
 * Access: Authenticated users only
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
  
  // Modal state
  showAddCardModal = false;

  constructor(
    private paymentService: PaymentService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('🎯 [PAYMENT-METHODS] Component initialized');
    this.currentUser = this.authService.getCurrentUser();
    
    console.log('👤 [PAYMENT-METHODS] Current user:', {
      id: this.currentUser?.id,
      email: this.currentUser?.email,
      name: this.currentUser?.fullName
    });
    
    if (this.currentUser) {
      this.loadPaymentMethods();
    } else {
      console.error('❌ [PAYMENT-METHODS] No current user found');
    }
  }

  /**
   * Load user's payment methods
   * API: GET /api/Payment/payment-methods
   */
  loadPaymentMethods(): void {
    if (!this.currentUser) {
      console.error('❌ [PAYMENT-METHODS] No current user - cannot load payment methods');
      return;
    }

    console.log('💳 [PAYMENT-METHODS] Loading payment methods for user:', this.currentUser.id);
    this.loading = true;
    this.error = null;

    this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
      next: (response) => {
        console.log('✅ [PAYMENT-METHODS] Payment methods loaded:', {
          statusCode: response.statusCode,
          methodCount: response.data?.length || 0,
          methods: response.data
        });
        
        if (response.statusCode === 200) {
          this.paymentMethods = response.data;
        } else {
          this.error = response.message || 'Failed to load payment methods';
          console.error('❌ [PAYMENT-METHODS] Failed to load payment methods:', response.message);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ [PAYMENT-METHODS] Error loading payment methods:', error);
        this.error = error.message || 'An error occurred';
        this.loading = false;
      }
    });
  }

  /**
   * Open add card modal
   */
  openAddCardModal(): void {
    this.showAddCardModal = true;
  }

  /**
   * Close add card modal
   */
  closeAddCardModal(): void {
    this.showAddCardModal = false;
  }

  /**
   * Handle successful card addition
   */
  onCardAdded(): void {
    this.closeAddCardModal();
    this.loadPaymentMethods(); // Reload to show new card
  }

  /**
   * Set payment method as default
   * API: PUT /api/Payment/payment-methods/{id}/default
   */
  setDefaultMethod(paymentMethodId: string): void {
    console.log('⭐ [PAYMENT-METHODS] Setting payment method as default:', paymentMethodId);
    this.actionLoading = true;

    this.paymentService.setDefaultPaymentMethod(paymentMethodId).subscribe({
      next: (response) => {
        console.log('✅ [PAYMENT-METHODS] Default payment method set:', {
          statusCode: response.statusCode,
          paymentMethodId: paymentMethodId
        });
        
        if (response.statusCode === 200) {
          // Update local state
          this.paymentMethods.forEach(pm => {
            pm.isDefault = pm.id === paymentMethodId;
          });
          console.log('🎯 [PAYMENT-METHODS] Local state updated - payment method set as default');
        } else {
          this.error = response.message || 'Failed to set default payment method';
          console.error('❌ [PAYMENT-METHODS] Failed to set default payment method:', response.message);
        }
        this.actionLoading = false;
      },
      error: (error) => {
        console.error('❌ [PAYMENT-METHODS] Error setting default payment method:', error);
        this.error = error.message || 'An error occurred';
        this.actionLoading = false;
      }
    });
  }

  /**
   * Delete payment method
   * API: DELETE /api/Payment/payment-methods/{id}
   */
  deleteMethod(paymentMethodId: string): void {
    if (!confirm('Are you sure you want to remove this payment method?')) {
      return;
    }

    this.actionLoading = true;

    this.paymentService.removePaymentMethod(paymentMethodId).subscribe({
      next: (response: any) => {
        console.log('✅ [PAYMENT-METHODS] Payment method removed:', {
          statusCode: response.statusCode,
          paymentMethodId: paymentMethodId
        });
        
        if (response.statusCode === 200) {
          // Remove from local state
          this.paymentMethods = this.paymentMethods.filter(pm => pm.id !== paymentMethodId);
          console.log('🎯 [PAYMENT-METHODS] Local state updated - payment method removed');
        } else {
          this.error = response.message || 'Failed to remove payment method';
          console.error('❌ [PAYMENT-METHODS] Failed to remove payment method:', response.message);
        }
        this.actionLoading = false;
      },
      error: (error: any) => {
        console.error('❌ [PAYMENT-METHODS] Error removing payment method:', error);
        this.error = error.message || 'An error occurred';
        this.actionLoading = false;
      }
    });
  }

  /**
   * Check if card is expired
   */
  isCardExpired(expMonth: number, expYear: number): boolean {
    if (!expMonth || !expYear) return false;
    
    const now = new Date();
    const expiryDate = new Date(expYear, expMonth - 1); // Month is 0-indexed
    
    return expiryDate < now;
  }

  /**
   * Check if card is expiring soon (within 30 days)
   */
  isCardExpiringSoon(expMonth: number, expYear: number): boolean {
    if (!expMonth || !expYear) return false;
    
    const now = new Date();
    const expiryDate = new Date(expYear, expMonth - 1);
    const thirtyDaysFromNow = new Date(now.getTime() + (30 * 24 * 60 * 60 * 1000));
    
    return expiryDate <= thirtyDaysFromNow && !this.isCardExpired(expMonth, expYear);
  }

  /**
   * Get days until card expiry
   */
  getDaysUntilExpiry(expMonth: number, expYear: number): number {
    if (!expMonth || !expYear) return 0;
    
    const now = new Date();
    const expiryDate = new Date(expYear, expMonth - 1);
    const diffTime = expiryDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return Math.max(0, diffDays);
  }

  /**
   * Get card brand icon class
   */
  getCardBrandIcon(brand: string | undefined): string {
    if (!brand) {
      return 'bi bi-credit-card text-muted';
    }
    
    const brandIcons: { [key: string]: string } = {
      'visa': 'bi bi-credit-card text-primary',
      'mastercard': 'bi bi-credit-card text-warning',
      'amex': 'bi bi-credit-card text-info',
      'discover': 'bi bi-credit-card text-success',
      'jcb': 'bi bi-credit-card text-secondary',
      'diners': 'bi bi-credit-card text-dark',
      'unionpay': 'bi bi-credit-card text-primary'
    };
    
    return brandIcons[brand.toLowerCase()] || 'bi bi-credit-card text-muted';
  }

  /**
   * Clear error message
   */
  clearError(): void {
    this.error = null;
  }
}
