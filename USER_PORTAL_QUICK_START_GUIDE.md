# User Portal - Quick Start Guide

## 📋 Overview

This guide provides a quick reference for implementing the User Portal. For detailed specifications, see `USER_PORTAL_COMPLETE_IMPLEMENTATION_BLUEPRINT.md`.

---

## 🎯 What We're Building

A complete User Portal where users can:
- ✅ Manage subscriptions (view, pause, resume, cancel)
- ✅ Manually pay for renewals when automatic payment fails
- ✅ Purchase additional privilege credits (pay-as-you-go)
- ✅ Track privilege usage with warnings
- ✅ View billing history and download invoices
- ✅ Manage payment methods (add, remove, set default)
- ✅ See upcoming billing previews

---

## ✅ Backend Status: COMPLETE

All required APIs are production-ready:

### Subscription APIs
- `GET /api/Subscriptions/user/{userId}` - List subscriptions
- `GET /api/Subscriptions/{id}` - Get details
- `POST /api/Subscriptions/{id}/pause` - Pause subscription
- `POST /api/Subscriptions/{id}/resume` - Resume subscription
- `POST /api/Subscriptions/{id}/cancel` - Cancel subscription
- `POST /api/Subscriptions/{id}/purchase-credits` - Buy additional credits
- `GET /api/Subscriptions/{id}/check-privilege/{name}` - Check availability

### Billing APIs
- `GET /api/Billing/records` - Get billing history with filters
- `GET /api/Billing/subscription/{id}` - Get subscription billing

### Payment APIs
- `GET /api/payments/payment-methods` - List payment methods
- `POST /api/payments/payment-methods` - Add payment method
- `PUT /api/payments/payment-methods/{id}/default` - Set default
- `DELETE /api/payments/payment-methods/{id}` - Remove method
- `POST /api/payments/process-payment` - Process payment (manual renewal)
- `POST /api/payments/retry-payment/{billingRecordId}` - Retry failed payment

### Invoice APIs
- `GET /api/Invoice/user/{userId}` - Get user invoices
- `GET /api/Invoice/{invoiceNumber}/download` - Download PDF

### Privilege APIs
- `GET /api/PrivilegeBasedBilling/usage-summary/{userId}` - Get usage summary

**Missing**: Only 1 new endpoint needed - Preview Next Bill (optional enhancement)

---

## 🚀 Implementation Phases

### Phase 1: Manual Renewal Payment (PRIORITY: HIGH)

**Why**: Users need a way to pay when automatic renewal fails

**Components to Build**:
1. `subscription-renewal-payment.component.ts` - Payment modal
2. Update `subscription-detail.component.ts` - Add "Pay Now" button
3. Update `dashboard.component.ts` - Add failed payment alert

**APIs Used**:
- `GET /api/Billing/subscription/{subscriptionId}` - Get pending bills
- `GET /api/payments/payment-methods` - Load payment methods
- `POST /api/payments/process-payment` - Process payment

**User Flow**:
1. User sees "Payment Failed" alert
2. Clicks "Pay Now" button
3. Modal opens with pending amount and payment methods
4. User selects payment method
5. User clicks "Pay Now"
6. Payment processes via Stripe
7. On success: Subscription reactivated, privileges reset
8. Show success message, close modal

**Key Code**:
```typescript
processRenewalPayment(): void {
  const request: ProcessPaymentRequestDto = {
    billingRecordId: this.pendingBilling.id,
    paymentMethodId: this.selectedPaymentMethodId
  };
  
  this.paymentService.processPayment(request).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        this.showSuccess('Payment successful! Your subscription is now active.');
        this.refreshSubscription();
        this.closeModal();
      }
    },
    error: (error) => {
      this.showError(error.error.message || 'Payment failed');
    }
  });
}
```

---

### Phase 2: Privilege Purchase (PRIORITY: HIGH)

**Why**: Users need to buy additional credits when they exhaust their limit

**Components to Build**:
1. `privilege-purchase-modal.component.ts` - Purchase modal
2. Update `privilege-usage.component.ts` - Add "Buy More" buttons, warnings

**APIs Used**:
- `GET /api/Subscriptions/{id}/check-privilege/{name}` - Check if exhausted
- `POST /api/Subscriptions/{id}/purchase-credits` - Buy credits
- `GET /api/payments/payment-methods` - Load payment methods

**User Flow**:
1. User exhausts privilege (e.g., used 5 of 5 Teleconsultations)
2. Warning appears: "You've used all your Teleconsultation credits"
3. User clicks "Buy More"
4. Modal shows: unit cost, quantity selector, total
5. User selects quantity (1-100)
6. User selects payment method
7. User clicks "Purchase"
8. Payment processes IMMEDIATELY via Stripe
9. If success: AllowedValue increases, user can continue
10. If failed: No credits added, show error

**Key Code**:
```typescript
purchaseCredits(): void {
  const dto: PurchaseAdditionalCreditsDto = {
    privilegeName: this.privilegeName,
    quantity: this.quantity,
    paymentMethodId: this.selectedPaymentMethodId
  };
  
  this.subscriptionService.purchaseAdditionalCredits(
    this.subscriptionId,
    dto
  ).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        const data = response.data;
        this.showSuccess(
          `Successfully purchased ${data.creditsAdded} credits for $${data.totalPaid}. ` +
          `New limit: ${data.newLimit}`
        );
        this.refreshPrivilegeUsage();
        this.closeModal();
      }
    },
    error: (error) => {
      this.showError('Purchase failed: ' + error.error.message);
    }
  });
}
```

---

### Phase 3: Dashboard Enhancement (PRIORITY: MEDIUM)

**Components to Update**:
1. `dashboard.component.ts` - Add widgets and alerts

**APIs Used**:
- `GET /api/Subscriptions/user/{userId}` - Get subscriptions
- `GET /api/PrivilegeBasedBilling/usage-summary/{userId}` - Get privilege usage
- `GET /api/Billing/records` - Get recent billing

**Widgets to Add**:
1. **Active Subscription Card**
   - Plan name, status badge
   - Next billing date and amount
   - Quick actions: Pause, Cancel, Manage

2. **Alerts Section**
   - Upcoming renewal (7 days before): "Your subscription renews in 6 days for $99.99"
   - Failed payment: "Payment failed. [Pay Now] to reactivate your subscription"
   - Privilege warnings: "You've used 90% of your Teleconsultation credits [Buy More]"

3. **Privilege Usage Overview**
   - Mini usage bars for each privilege
   - Color-coded: Green (<80%), Yellow (80-99%), Red (100%)
   - Link to full privilege page

4. **Recent Billing Activity**
   - Last 5 billing records
   - Date, amount, status
   - Download invoice buttons

---

### Phase 4: Billing History Enhancement (PRIORITY: MEDIUM)

**Components to Update**:
1. `billing-history.component.ts` - Add actions

**Features to Add**:
1. **Invoice Download**
   - Add "Download Invoice" button for paid records
   - Download PDF via API
   - Trigger browser download

2. **Pay Pending Bills**
   - Add "Pay Now" button for pending/failed records
   - Opens payment modal (Phase 1 component)

3. **Filters**
   - Status filter: All, Paid, Pending, Failed, Refunded
   - Type filter: All, Subscription, Overage, Consultation
   - Date range filter

4. **Pagination**
   - Page size: 10, 20, 50
   - Page navigation

---

### Phase 5: Payment Method Management (PRIORITY: LOW)

**Components to Update**:
1. `payment-methods.component.ts` - Add Stripe Elements

**Features to Add**:
1. **Add New Card**
   - Integrate Stripe.js
   - Create Stripe Elements card input
   - Create PaymentMethod via Stripe
   - Add to backend

2. **Card Warnings**
   - Show warning if card expires within 30 days
   - Highlight expired cards

3. **Set Default**
   - One-click set as default

4. **Remove Card**
   - Confirmation dialog
   - Remove via API

**Stripe Integration**:
```typescript
// stripe-client.service.ts
@Injectable({ providedIn: 'root' })
export class StripeClientService {
  private stripe: any;
  
  constructor() {
    this.stripe = (window as any).Stripe('pk_test_...');
  }
  
  async createPaymentMethod(cardElement: any): Promise<any> {
    const { paymentMethod, error } = await this.stripe.createPaymentMethod({
      type: 'card',
      card: cardElement
    });
    
    if (error) throw error;
    return paymentMethod;
  }
}
```

---

## 🔒 Security Checklist

### Frontend Validation
- ✅ Check user is logged in (auth guard)
- ✅ Validate payment method selected before processing payment
- ✅ Validate quantity range (1-100) for privilege purchase
- ✅ Check card expiry before using payment method
- ✅ Sanitize user input (cancellation reason)

### Backend Authorization (Already Implemented)
- ✅ All endpoints require authentication (`[Authorize]`)
- ✅ User can only access own subscriptions
- ✅ User can only pay own bills
- ✅ User can only manage own payment methods
- ✅ Admin can access all resources

---

## 🧪 Testing Strategy

### Unit Testing
- ✅ Test each component method
- ✅ Test API service methods
- ✅ Test validation functions
- ✅ Test error handling

### Integration Testing
- ✅ Test complete renewal payment flow
- ✅ Test complete privilege purchase flow
- ✅ Test subscription lifecycle (pause → resume → cancel)

### End-to-End Testing
1. **New User Journey**
   - Subscribe to plan
   - Use privileges
   - Reach limit
   - Purchase additional credits
   - Automatic renewal

2. **Failed Payment Journey**
   - See failed payment alert
   - Click "Pay Now"
   - Select payment method
   - Complete payment
   - Subscription reactivated

3. **Subscription Management Journey**
   - Pause subscription
   - Resume subscription
   - View billing history
   - Download invoices
   - Cancel subscription

---

## 📊 Implementation Metrics

### Time Estimates
- Phase 1 (Manual Renewal): 2-3 days
- Phase 2 (Privilege Purchase): 2-3 days
- Phase 3 (Dashboard): 2 days
- Phase 4 (Billing History): 1-2 days
- Phase 5 (Payment Methods): 2 days
- Testing & QA: 3 days

**Total**: ~12-15 days for complete implementation

### Components Count
- New Components: 3 (renewal payment, privilege purchase, add payment method)
- Enhanced Components: 6 (subscription detail, privilege usage, billing history, dashboard, subscription list, payment methods)
- Services: All exist, no new services needed
- API Endpoints: 14 existing, 1 new (optional)

---

## 🎓 Developer Notes

### Common Patterns

**Loading State**:
```typescript
loading = false;

loadData(): void {
  this.loading = true;
  this.service.getData().subscribe({
    next: (response) => {
      this.data = response.data;
      this.loading = false;
    },
    error: (error) => {
      this.error = error.message;
      this.loading = false;
    }
  });
}
```

**Error Handling**:
```typescript
handleError(error: any): void {
  let message = 'An error occurred';
  
  if (error.error && error.error.message) {
    message = error.error.message;
  } else if (error.status === 401) {
    message = 'Session expired. Please log in again.';
    this.router.navigate(['/auth/login']);
    return;
  } else if (error.status === 403) {
    message = 'Access denied';
  }
  
  this.showError(message);
}
```

**Modal Pattern**:
```typescript
showModal = false;

openModal(): void {
  this.loadModalData();
  this.showModal = true;
}

closeModal(): void {
  this.showModal = false;
  this.resetForm();
}

submitModal(): void {
  if (!this.validateForm()) return;
  
  this.processing = true;
  this.service.submitData(this.formData).subscribe({
    next: (response) => {
      this.showSuccess('Success!');
      this.refreshParentData();
      this.closeModal();
      this.processing = false;
    },
    error: (error) => {
      this.showError(error.message);
      this.processing = false;
    }
  });
}
```

---

## 📚 Documentation Structure

1. **USER_PORTAL_COMPLETE_IMPLEMENTATION_BLUEPRINT.md** (This file)
   - Detailed technical specification
   - Complete API documentation
   - Step-by-step user flows
   - Security considerations
   - Error handling patterns

2. **USER_PORTAL_QUICK_START_GUIDE.md** (Summary)
   - Quick reference
   - Implementation phases
   - Key code snippets
   - Testing strategy

3. **API Endpoint Reference** (in Blueprint)
   - Complete API list
   - Request/response examples
   - Auth requirements
   - Error responses

---

## 🚦 Getting Started

1. **Review the Blueprint**: Read `USER_PORTAL_COMPLETE_IMPLEMENTATION_BLUEPRINT.md` in full
2. **Start with Phase 1**: Build manual renewal payment
3. **Test Thoroughly**: Verify each feature before moving to next phase
4. **Follow Security Checklist**: Validate at both frontend and backend
5. **Implement Error Handling**: Use centralized error handler
6. **Add Loading States**: Show spinners for all async operations

---

## 🎉 Success Criteria

When complete, users should be able to:
- ✅ Manage entire subscription lifecycle without contacting support
- ✅ Recover from failed payments independently
- ✅ Purchase additional credits when needed
- ✅ Track usage with clear warnings
- ✅ View complete financial history
- ✅ Download invoices for records
- ✅ Manage payment methods securely

**The portal should be intuitive, fast, and require zero training.**

---

For detailed implementation guidance, refer to `USER_PORTAL_COMPLETE_IMPLEMENTATION_BLUEPRINT.md`.



