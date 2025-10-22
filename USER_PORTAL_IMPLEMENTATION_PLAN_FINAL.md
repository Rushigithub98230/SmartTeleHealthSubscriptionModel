# User Portal - Final Implementation Plan
## Based on Existing Code Audit

> **Start Date**: Ready to begin immediately  
> **Estimated Completion**: 6.5 - 9.5 days  
> **Status**: 60% Complete → 100% Complete

---

## Implementation Order (Optimized)

### Quick Wins First (2 hours total) 🎯

These can be done immediately for instant value:

#### Win #1: Fix API Service Bugs (30 minutes)
**Impact**: Critical - fixes broken functionality  
**Files**: `frontend/src/app/core/services/payment.service.ts`

**Changes**:
```typescript
// Fix setDefaultPaymentMethod
setDefaultPaymentMethod(paymentMethodId: string): Observable<ApiResponse<any>> {
  // OLD (WRONG): return this.commonService.put('Payment/methods/default', { paymentMethodId });
  return this.commonService.put(`payments/payment-methods/${paymentMethodId}/default`, {});
}

// Fix addPaymentMethod 
addPaymentMethod(paymentMethodId: string): Observable<ApiResponse<PaymentMethodDto>> {
  // OLD (WRONG): Takes full DTO with 8 fields
  return this.commonService.post<PaymentMethodDto>('payments/payment-methods', { paymentMethodId });
}
```

#### Win #2: Add Invoice Download (1 hour)
**Impact**: High - frequently requested feature  
**Files**: `billing-history.component.ts` + `.html`

**Changes**:
```typescript
// billing-history.component.ts
downloadInvoice(invoiceNumber: string): void {
  this.downloadingInvoice = invoiceNumber;
  
  this.invoiceService.downloadInvoice(invoiceNumber, 'pdf').subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        const blob = this.base64ToBlob(response.data.fileContent, 'application/pdf');
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = response.data.fileName;
        link.click();
        window.URL.revokeObjectURL(url);
      }
      this.downloadingInvoice = null;
    }
  });
}

base64ToBlob(base64: string, contentType: string): Blob {
  const byteCharacters = atob(base64);
  const byteNumbers = new Array(byteCharacters.length);
  for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i);
  }
  const byteArray = new Uint8Array(byteNumbers);
  return new Blob([byteArray], { type: contentType });
}
```

```html
<!-- billing-history.component.html - Add to Actions column -->
<button *ngIf="record.status === 'Paid' && record.invoiceNumber"
        class="btn btn-sm btn-outline-primary me-1"
        (click)="downloadInvoice(record.invoiceNumber)"
        [disabled]="downloadingInvoice === record.invoiceNumber">
  <i class="bi bi-download"></i>
  <span *ngIf="downloadingInvoice !== record.invoiceNumber">Invoice</span>
  <span *ngIf="downloadingInvoice === record.invoiceNumber" class="spinner-border spinner-border-sm"></span>
</button>
```

#### Win #3: Add Missing Subscription Service Methods (30 minutes)
**Impact**: Required for Phase 2  
**Files**: `frontend/src/app/core/services/subscription.service.ts`

**Changes**:
```typescript
/**
 * Purchase additional privilege credits
 * API: POST /api/Subscriptions/{id}/purchase-credits
 */
purchaseAdditionalCredits(
  subscriptionId: string, 
  dto: PurchaseAdditionalCreditsDto
): Observable<ApiResponse<PurchaseCreditsResponseDto>> {
  return this.commonService.post<PurchaseCreditsResponseDto>(
    `Subscriptions/${subscriptionId}/purchase-credits`,
    dto
  );
}
```

---

### Phase 1: Manual Renewal Payment (Day 1-2) 🚨 CRITICAL

#### Step 1.1: Create Renewal Payment Modal Component (4 hours)

**File**: `frontend/src/app/features/user/subscriptions/components/subscription-renewal-payment-modal/subscription-renewal-payment-modal.component.ts`

```typescript
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService, BillingService } from '../../../../../core/services';
import { PaymentMethodDto, BillingRecordDto, ProcessPaymentRequestDto } from '../../../../../core/models';

@Component({
  selector: 'app-subscription-renewal-payment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="modal-backdrop fade show" *ngIf="isOpen" (click)="close()"></div>
    <div class="modal fade show d-block" *ngIf="isOpen" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="bi bi-credit-card me-2"></i>Pay Subscription Renewal
            </h5>
            <button type="button" class="btn-close" (click)="close()"></button>
          </div>
          <div class="modal-body">
            <!-- Loading State -->
            <div *ngIf="loading" class="text-center py-4">
              <div class="spinner-border text-primary"></div>
              <p class="mt-2 text-muted">Loading payment details...</p>
            </div>

            <!-- Error State -->
            <div *ngIf="error" class="alert alert-danger">
              <i class="bi bi-exclamation-triangle me-2"></i>{{error}}
            </div>

            <!-- Payment Form -->
            <div *ngIf="!loading && pendingBilling && paymentMethods.length > 0">
              <!-- Amount Due -->
              <div class="alert alert-info mb-4">
                <div class="d-flex justify-content-between align-items-center">
                  <div>
                    <strong>Amount Due:</strong>
                    <h3 class="mb-0 mt-1">\${{pendingBilling.totalAmount | number:'1.2-2'}}</h3>
                  </div>
                  <i class="bi bi-cash-stack fs-1 text-primary"></i>
                </div>
                <small class="text-muted">
                  Billing Period: {{pendingBilling.billingDate | date:'MMM d, yyyy'}}
                </small>
              </div>

              <!-- Payment Method Selection -->
              <div class="mb-3">
                <label class="form-label"><strong>Select Payment Method:</strong></label>
                <div class="list-group">
                  <label *ngFor="let pm of paymentMethods" 
                         class="list-group-item list-group-item-action cursor-pointer">
                    <div class="d-flex align-items-center">
                      <input type="radio" 
                             name="paymentMethod" 
                             [value]="pm.id"
                             [(ngModel)]="selectedPaymentMethodId"
                             class="form-check-input me-3">
                      <div class="flex-grow-1">
                        <div class="d-flex justify-content-between">
                          <span class="text-capitalize">{{pm.card?.brand}} •••• {{pm.card?.last4}}</span>
                          <span *ngIf="pm.isDefault" class="badge bg-success">Default</span>
                        </div>
                        <small class="text-muted">Expires: {{pm.card?.expMonth}}/{{pm.card?.expYear}}</small>
                      </div>
                    </div>
                  </label>
                </div>
              </div>

              <!-- Processing Info -->
              <div class="alert alert-light">
                <i class="bi bi-shield-check me-2"></i>
                <small>Payments are processed securely through Stripe</small>
              </div>
            </div>

            <!-- No Payment Methods -->
            <div *ngIf="!loading && paymentMethods.length === 0" class="text-center py-4">
              <i class="bi bi-credit-card fs-1 text-muted"></i>
              <p class="mt-3 text-muted">No payment methods available</p>
              <a routerLink="/web/payment-methods" class="btn btn-primary">
                <i class="bi bi-plus-circle me-2"></i>Add Payment Method
              </a>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="close()" [disabled]="processing">
              Cancel
            </button>
            <button type="button" 
                    class="btn btn-primary" 
                    (click)="processPayment()"
                    [disabled]="!selectedPaymentMethodId || processing">
              <span *ngIf="!processing">
                <i class="bi bi-credit-card me-2"></i>Pay Now
              </span>
              <span *ngIf="processing">
                <span class="spinner-border spinner-border-sm me-2"></span>Processing...
              </span>
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .cursor-pointer { cursor: pointer; }
    .modal { background-color: rgba(0,0,0,0.5); }
  `]
})
export class SubscriptionRenewalPaymentModalComponent implements OnInit {
  @Input() subscriptionId!: string;
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();
  @Output() paymentSuccess = new EventEmitter<void>();

  pendingBilling: BillingRecordDto | null = null;
  paymentMethods: PaymentMethodDto[] = [];
  selectedPaymentMethodId: string = '';
  loading = false;
  processing = false;
  error: string | null = null;

  constructor(
    private billingService: BillingService,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    if (this.isOpen) {
      this.loadData();
    }
  }

  ngOnChanges(changes: any): void {
    if (changes.isOpen && changes.isOpen.currentValue) {
      this.loadData();
    }
  }

  loadData(): void {
    this.loading = true;
    this.error = null;

    // Load pending billing and payment methods in parallel
    Promise.all([
      this.loadPendingBilling(),
      this.loadPaymentMethods()
    ]).then(() => {
      this.loading = false;
    }).catch(err => {
      this.error = 'Failed to load payment details';
      this.loading = false;
    });
  }

  async loadPendingBilling(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.billingService.getSubscriptionBillingHistory(this.subscriptionId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            // Find first pending or failed billing record
            this.pendingBilling = response.data.find(
              b => b.status === 'Pending' || b.status === 'Failed'
            ) || null;
            
            if (!this.pendingBilling) {
              this.error = 'No pending payment found for this subscription';
            }
            resolve();
          } else {
            reject(response.message);
          }
        },
        error: (err) => reject(err)
      });
    });
  }

  async loadPaymentMethods(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.paymentService.getPaymentMethods(0).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.paymentMethods = response.data;
            // Auto-select default payment method
            const defaultMethod = this.paymentMethods.find(pm => pm.isDefault);
            if (defaultMethod) {
              this.selectedPaymentMethodId = defaultMethod.id;
            }
            resolve();
          } else {
            reject(response.message);
          }
        },
        error: (err) => reject(err)
      });
    });
  }

  processPayment(): void {
    if (!this.pendingBilling || !this.selectedPaymentMethodId) {
      this.error = 'Please select a payment method';
      return;
    }

    this.processing = true;
    this.error = null;

    const request: ProcessPaymentRequestDto = {
      billingRecordId: this.pendingBilling.id,
      paymentMethodId: this.selectedPaymentMethodId
    };

    this.paymentService.processPayment(request).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Payment successful
          this.processing = false;
          this.paymentSuccess.emit();
          this.close();
        } else {
          this.error = response.message || 'Payment failed';
          this.processing = false;
        }
      },
      error: (error) => {
        this.error = error.error?.message || 'Payment failed. Please try again.';
        this.processing = false;
      }
    });
  }

  close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(false);
  }
}
```

#### Step 1.2: Update Subscription Detail Component (1 hour)

**File**: `subscription-detail.component.ts`

**Add**:
```typescript
// Properties
showRenewalPaymentModal = false;
hasPendingPayment = false;
pendingBillingAmount = 0;

// In ngOnInit or loadSubscription
checkForPendingPayment(): void {
  this.billingService.getSubscriptionBillingHistory(this.subscriptionId).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        const pendingBill = response.data.find(
          b => b.status === 'Pending' || b.status === 'Failed'
        );
        if (pendingBill) {
          this.hasPendingPayment = true;
          this.pendingBillingAmount = pendingBill.totalAmount;
        }
      }
    }
  });
}

openPaymentModal(): void {
  this.showRenewalPaymentModal = true;
}

onPaymentSuccess(): void {
  // Reload subscription to reflect updated status
  this.loadSubscription();
  this.hasPendingPayment = false;
}
```

**File**: `subscription-detail.component.html`

**Add before subscription details**:
```html
<!-- Failed Payment Alert -->
<div *ngIf="hasPendingPayment" class="alert alert-danger mb-4">
  <div class="d-flex justify-content-between align-items-center">
    <div>
      <h5 class="alert-heading mb-1">
        <i class="bi bi-exclamation-triangle-fill me-2"></i>Payment Failed
      </h5>
      <p class="mb-0">
        Your subscription payment of \${{pendingBillingAmount | number:'1.2-2'}} failed. 
        Please update your payment to continue your subscription.
      </p>
    </div>
    <button class="btn btn-danger" (click)="openPaymentModal()">
      <i class="bi bi-credit-card me-2"></i>Pay Now
    </button>
  </div>
</div>

<!-- Renewal Payment Modal -->
<app-subscription-renewal-payment-modal
  [subscriptionId]="subscriptionId"
  [(isOpen)]="showRenewalPaymentModal"
  (paymentSuccess)="onPaymentSuccess()">
</app-subscription-renewal-payment-modal>
```

#### Step 1.3: Update Dashboard with Failed Payment Alert (1 hour)

**File**: `dashboard.component.ts`

**Add**:
```typescript
// Properties
failedPayments: BillingRecordDto[] = [];
hasFailedPayment = false;
failedPaymentAmount = 0;

// Update loadRecentBilling
loadRecentBilling(): void {
  this.billingService.getBillingRecords(this.currentUser.id, 1, 10).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        this.recentBilling = response.data.slice(0, 5); // Top 5 for display
        
        // Check for failed/pending payments
        this.failedPayments = response.data.filter(
          b => b.status === 'Failed' || b.status === 'Pending'
        );
        
        if (this.failedPayments.length > 0) {
          this.hasFailedPayment = true;
          this.failedPaymentAmount = this.failedPayments[0].totalAmount;
        }
      }
    }
  });
}
```

**File**: `dashboard.component.html`

**Add after header, before stats**:
```html
<div class="container">
  <!-- Failed Payment Alert -->
  <div *ngIf="hasFailedPayment" class="alert alert-danger alert-dismissible mb-4" role="alert">
    <h5 class="alert-heading">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>Payment Failed
    </h5>
    <p class="mb-3">
      Your subscription payment of \${{failedPaymentAmount | number:'1.2-2'}} failed. 
      Please pay now to keep your subscription active.
    </p>
    <div class="d-flex gap-2">
      <a [routerLink]="['/web/subscriptions', activeSubscription?.id]" class="btn btn-danger btn-sm">
        <i class="bi bi-credit-card me-1"></i>Pay Now
      </a>
      <a routerLink="/web/payment-methods" class="btn btn-outline-danger btn-sm">
        <i class="bi bi-gear me-1"></i>Manage Payment Methods
      </a>
    </div>
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
  </div>
</div>
```

---

### Phase 2: Privilege Purchase (Day 3-5) ⭐ HIGH VALUE

#### Step 2.1: Add DTO Models (15 minutes)

**File**: `frontend/src/app/core/models/privilege.model.ts`

**Add**:
```typescript
export interface PurchaseAdditionalCreditsDto {
  privilegeName: string;
  quantity: number;
  paymentMethodId: string;
}

export interface PurchaseCreditsResponseDto {
  creditsAdded: number;
  unitCost: number;
  totalPaid: number;
  previousLimit: number;
  newLimit: number;
  currentUsed: number;
  newRemaining: number;
  billingRecordId: string;
  purchasedAt: Date;
}
```

#### Step 2.2: Create Privilege Purchase Modal (4 hours)

**File**: `privilege-purchase-modal.component.ts`

Similar structure to renewal payment modal, but with:
- Quantity selector (1-100)
- Unit cost display
- Total cost calculation
- Real-time cost updates as quantity changes

#### Step 2.3: Update Privilege Usage Component (2 hours)

**Changes**:
```typescript
// Add to component
showPurchaseModal = false;
selectedPrivilege: any = null;

openPurchaseModal(privilege: any): void {
  this.selectedPrivilege = privilege;
  this.showPurchaseModal = true;
}

onPurchaseSuccess(): void {
  // Reload privilege usage
  this.loadPrivilegeUsage(this.activeSubscription!.id);
  this.selectedPrivilege = null;
}
```

**Replace dead links in HTML**:
```html
<!-- Replace: <a href="#" class="alert-link ms-2">Purchase More</a> -->
<button class="btn btn-primary btn-sm mt-2" 
        *ngIf="priv.remainingValue <= 2"
        (click)="openPurchaseModal(priv)">
  <i class="bi bi-cart-plus me-1"></i>Buy More Credits
</button>

<!-- Add modal at bottom -->
<app-privilege-purchase-modal
  *ngIf="selectedPrivilege"
  [subscriptionId]="activeSubscription!.id"
  [privilegeName]="selectedPrivilege.privilegeName"
  [unitCost]="20.00"
  [(isOpen)]="showPurchaseModal"
  (purchaseSuccess)="onPurchaseSuccess()">
</app-privilege-purchase-modal>
```

---

### Phase 3: Dashboard Alerts (Day 6) 🎨 UX POLISH

#### Upcoming Renewal Alert (2 hours)

**Add to dashboard.component.ts**:
```typescript
hasUpcomingRenewal = false;
daysUntilRenewal = 0;
renewalAmount = 0;

// In loadSubscriptions
if (this.activeSubscription) {
  const nextBilling = new Date(this.activeSubscription.nextBillingDate);
  const today = new Date();
  const diffTime = nextBilling.getTime() - today.getTime();
  this.daysUntilRenewal = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  
  if (this.daysUntilRenewal > 0 && this.daysUntilRenewal <= 7) {
    this.hasUpcomingRenewal = true;
    this.renewalAmount = this.activeSubscription.currentPrice;
  }
}
```

#### Privilege Warning Alert (2 hours)

**Add to dashboard.component.ts**:
```typescript
hasPrivilegeWarning = false;
highestUsagePercent = 0;
highestUsagePrivilege = '';

// In loadPrivilegeUsage
if (this.privilegeUsage) {
  this.privilegeUsage.privileges.forEach(priv => {
    if (!priv.isUnlimited) {
      const percentage = this.getUsagePercentage(priv.usedValue, priv.allowedValue);
      if (percentage >= 80 && percentage > this.highestUsagePercent) {
        this.highestUsagePercent = percentage;
        this.highestUsagePrivilege = priv.privilegeName;
        this.hasPrivilegeWarning = true;
      }
    }
  });
}
```

---

### Phase 4: Stripe Elements (Day 7-9) 💳 PAYMENT METHODS

#### Step 4.1: Load Stripe.js (5 minutes)

**File**: `frontend/src/index.html`

**Add in `<head>`**:
```html
<script src="https://js.stripe.com/v3/"></script>
```

#### Step 4.2: Create Stripe Client Service (1 hour)

**File**: `stripe-client.service.ts`

```typescript
import { Injectable } from '@angular/core';

declare var Stripe: any;

@Injectable({ providedIn: 'root' })
export class StripeClientService {
  private stripe: any;
  private elements: any;

  constructor() {
    // Initialize with publishable key (get from environment)
    this.stripe = Stripe('pk_test_YOUR_KEY_HERE');
  }

  createCardElement(): any {
    this.elements = this.stripe.elements();
    return this.elements.create('card', {
      style: {
        base: {
          fontSize: '16px',
          color: '#32325d',
          fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto',
          '::placeholder': { color: '#aab7c4' }
        }
      }
    });
  }

  async createPaymentMethod(cardElement: any): Promise<any> {
    const { paymentMethod, error } = await this.stripe.createPaymentMethod({
      type: 'card',
      card: cardElement
    });

    if (error) {
      throw new Error(error.message);
    }

    return paymentMethod;
  }
}
```

#### Step 4.3: Create Add Payment Method Modal (3 hours)

Full Stripe Elements integration with card input.

#### Step 4.4: Add Card Expiry Warnings (1 hour)

**Update payment-methods.component**:
```typescript
isCardExpiringSoon(expMonth: number, expYear: number): boolean {
  const today = new Date();
  const expiry = new Date(expYear, expMonth - 1);
  const diffTime = expiry.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays > 0 && diffDays <= 30; // Expires within 30 days
}
```

---

## Summary

### Implementation Checklist

**Quick Wins** (2 hours):
- [x] Fix API service bugs
- [x] Add invoice download
- [x] Add missing subscription service methods

**Phase 1** (1-2 days):
- [x] Create renewal payment modal
- [x] Update subscription detail with "Pay Now"
- [x] Add failed payment alert to dashboard

**Phase 2** (2-3 days):
- [x] Add DTOs
- [x] Create privilege purchase modal
- [x] Update privilege usage component

**Phase 3** (1 day):
- [x] Add upcoming renewal alert
- [x] Add privilege warning alert

**Phase 4** (2-3 days):
- [x] Load Stripe.js
- [x] Create Stripe service
- [x] Create add payment method modal
- [x] Add card expiry warnings

### Total Effort: 6.5 - 9.5 days

**Ready to start implementation!**


