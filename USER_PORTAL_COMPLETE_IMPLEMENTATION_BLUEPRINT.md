# User Portal - Complete Implementation Blueprint
## Production-Ready Technical Specification

> **Purpose**: Detailed, actionable blueprint for building a complete User Portal that allows users to manage their entire subscription lifecycle end-to-end.

---

## Table of Contents
1. [API Endpoint Verification](#api-endpoint-verification)
2. [Request/Response Specifications](#requestresponse-specifications)
3. [Frontend-Backend Flow Mapping](#frontend-backend-flow-mapping)
4. [Step-by-Step User Flows](#step-by-step-user-flows)
5. [Security & Validation](#security--validation)
6. [Error Handling](#error-handling)
7. [Implementation Checklist](#implementation-checklist)

---

## API Endpoint Verification

### Authentication
- **All Endpoints Require**: `Authorization: Bearer <token>` header
- **Token Source**: localStorage (`authToken`)
- **User Context**: Extracted from JWT token (`tokenModel.UserID`)

### 1. Subscription Management APIs

#### 1.1 Get User Subscriptions
```
GET /api/Subscriptions/user/{userId}
```
**Auth**: Required (user can only access own subscriptions, admin can access any)
**URL**: https://api.domain.com/api/Subscriptions/user/123
**Headers**:
```json
{
  "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs...",
  "Content-Type": "application/json"
}
```
**Response 200**:
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": 123,
      "subscriptionPlanId": "plan-guid",
      "planName": "Basic Plan",
      "status": "Active",
      "startDate": "2024-01-01T00:00:00Z",
      "nextBillingDate": "2024-02-01T00:00:00Z",
      "lastBillingDate": "2024-01-01T00:00:00Z",
      "price": 99.99,
      "currency": "USD",
      "billingCycle": "Monthly",
      "autoRenew": true,
      "stripeSubscriptionId": "sub_xxx",
      "stripeCustomerId": "cus_xxx"
    }
  ],
  "message": "Subscriptions retrieved successfully",
  "statusCode": 200
}
```
**Response 403** (Access Denied):
```json
{
  "data": {},
  "message": "Access denied",
  "statusCode": 403
}
```

#### 1.2 Get Subscription Details
```
GET /api/Subscriptions/{id}
```
**Auth**: Required
**URL**: https://api.domain.com/api/Subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6
**Response 200**: (Same structure as single subscription above, with additional fields)
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": 123,
    ...existing fields...,
    "privileges": [
      {
        "privilegeName": "Teleconsultation",
        "usedValue": 3,
        "allowedValue": 5,
        "remainingValue": 2,
        "isUnlimited": false,
        "isExhausted": false
      }
    ]
  },
  "message": "Subscription retrieved successfully",
  "statusCode": 200
}
```

#### 1.3 Pause Subscription
```
POST /api/Subscriptions/{id}/pause
```
**Auth**: Required
**URL**: https://api.domain.com/api/Subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6/pause
**Request Body**: None
**Response 200**:
```json
{
  "data": {
    "subscriptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Paused",
    "pausedAt": "2024-01-15T10:30:00Z"
  },
  "message": "Subscription paused successfully",
  "statusCode": 200
}
```

#### 1.4 Resume Subscription
```
POST /api/Subscriptions/{id}/resume
```
**Auth**: Required
**Request Body**: None
**Response 200**: (Similar to pause response)

#### 1.5 Cancel Subscription
```
POST /api/Subscriptions/{id}/cancel
```
**Auth**: Required
**Request Body**:
```json
{
  "reason": "User provided cancellation reason"
}
```
**Content-Type**: application/json
**Response 200**:
```json
{
  "data": {
    "subscriptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Cancelled",
    "cancelledAt": "2024-01-15T10:30:00Z",
    "refundAmount": 0.00
  },
  "message": "Subscription cancelled successfully",
  "statusCode": 200
}
```

#### 1.6 Purchase Additional Credits
```
POST /api/Subscriptions/{id}/purchase-credits
```
**Auth**: Required
**URL**: https://api.domain.com/api/Subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6/purchase-credits
**Request Body**:
```json
{
  "privilegeName": "Teleconsultation",
  "quantity": 2,
  "paymentMethodId": "pm_1234567890"
}
```
**Response 200**:
```json
{
  "data": {
    "creditsAdded": 2,
    "unitCost": 20.00,
    "totalPaid": 40.00,
    "previousLimit": 5,
    "newLimit": 7,
    "currentUsed": 5,
    "newRemaining": 2,
    "billingRecordId": "billing-guid",
    "purchasedAt": "2024-01-15T10:30:00Z"
  },
  "message": "Successfully purchased 2 additional Teleconsultation credits for $40.00",
  "statusCode": 200
}
```
**Response 400** (Payment Failed):
```json
{
  "data": { "paymentFailed": true },
  "message": "Payment failed: Insufficient funds. Credits not added.",
  "statusCode": 400
}
```

#### 1.7 Check Privilege Availability
```
GET /api/Subscriptions/{id}/check-privilege/{privilegeName}?requestedAmount=1
```
**Auth**: Required
**URL**: https://api.domain.com/api/Subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6/check-privilege/Teleconsultation?requestedAmount=1
**Response 200** (Available):
```json
{
  "data": {
    "available": true,
    "remaining": 2,
    "requested": 1,
    "message": "You have 2 Teleconsultation credits remaining."
  },
  "statusCode": 200
}
```
**Response 402** (Limit Exceeded - Payment Required):
```json
{
  "data": {
    "available": false,
    "limitExceeded": true,
    "remaining": 0,
    "requested": 1,
    "shortfall": 1,
    "unitCost": 20.00,
    "requiredPayment": 20.00,
    "message": "You've used all your included Teleconsultation credits. Purchase 1 additional credit for $20.00 to continue.",
    "purchaseDetails": {
      "privilegeName": "Teleconsultation",
      "quantity": 1,
      "totalCost": 20.00
    }
  },
  "statusCode": 402
}
```

### 2. Billing Management APIs

#### 2.1 Get Billing Records
```
GET /api/Billing/records?userId={userId}&page=1&pageSize=10&status[]=Paid&status[]=Pending
```
**Auth**: Required
**URL**: https://api.domain.com/api/Billing/records?userId=123&page=1&pageSize=10
**Query Parameters**:
- `page` (int, default: 1)
- `pageSize` (int, default: 10)
- `searchTerm` (string, optional)
- `status` (string[], optional): Paid, Pending, Failed, Refunded
- `type` (string[], optional): Subscription, Overage, Consultation
- `userId` (int[], optional)
- `subscriptionId` (guid[], optional)
- `startDate` (datetime, optional)
- `endDate` (datetime, optional)
- `sortBy` (string, optional)
- `sortOrder` (string, optional): asc, desc

**Response 200**:
```json
{
  "data": [
    {
      "id": "billing-guid",
      "userId": 123,
      "subscriptionId": "sub-guid",
      "amount": 99.99,
      "totalAmount": 99.99,
      "description": "Monthly subscription payment",
      "dueDate": "2024-02-01T00:00:00Z",
      "status": "Paid",
      "type": "Subscription",
      "paidAt": "2024-02-01T10:30:00Z",
      "paymentIntentId": "pi_xxx",
      "subscriptionName": "Basic Plan",
      "userName": "John Doe",
      "userEmail": "john@example.com",
      "invoiceNumber": "INV-2024-001",
      "stripeInvoiceId": "in_xxx",
      "createdDate": "2024-01-01T00:00:00Z"
    }
  ],
  "meta": {
    "currentPage": 1,
    "pageSize": 10,
    "totalRecords": 25,
    "totalPages": 3
  },
  "message": "Billing records retrieved successfully",
  "statusCode": 200
}
```

#### 2.2 Get Subscription Billing History
```
GET /api/Billing/subscription/{subscriptionId}
```
**Auth**: Required
**Response**: Same structure as Get Billing Records

### 3. Payment Management APIs

#### 3.1 Get Payment Methods
```
GET /api/payments/payment-methods
```
**Auth**: Required
**Response 200**:
```json
{
  "data": [
    {
      "id": "pm_1234567890",
      "customerId": "cus_xxx",
      "type": "card",
      "card": {
        "brand": "visa",
        "last4": "4242",
        "expMonth": 12,
        "expYear": 2025
      },
      "isDefault": true,
      "createdDate": "2024-01-01T00:00:00Z"
    }
  ],
  "message": "Payment methods retrieved successfully",
  "statusCode": 200
}
```

#### 3.2 Add Payment Method
```
POST /api/payments/payment-methods
```
**Auth**: Required
**Request Body**:
```json
{
  "paymentMethodId": "pm_1234567890"
}
```
**Response 200**:
```json
{
  "data": {
    "id": "pm_1234567890",
    "type": "card",
    "card": {
      "brand": "visa",
      "last4": "4242",
      "expMonth": 12,
      "expYear": 2025
    },
    "isDefault": false
  },
  "message": "Payment method added successfully",
  "statusCode": 200
}
```

#### 3.3 Set Default Payment Method
```
PUT /api/payments/payment-methods/{paymentMethodId}/default
```
**Auth**: Required
**URL**: https://api.domain.com/api/payments/payment-methods/pm_1234567890/default
**Request Body**: None
**Response 200**:
```json
{
  "data": true,
  "message": "Default payment method updated",
  "statusCode": 200
}
```

#### 3.4 Remove Payment Method
```
DELETE /api/payments/payment-methods/{paymentMethodId}
```
**Auth**: Required
**Response 200**:
```json
{
  "data": true,
  "message": "Payment method removed",
  "statusCode": 200
}
```

#### 3.5 Process Payment (Manual Renewal)
```
POST /api/payments/process-payment
```
**Auth**: Required
**Request Body**:
```json
{
  "billingRecordId": "billing-guid",
  "paymentMethodId": "pm_1234567890"
}
```
**Response 200**:
```json
{
  "data": {
    "billingRecordId": "billing-guid",
    "paymentIntentId": "pi_xxx",
    "amount": 99.99,
    "status": "succeeded",
    "paidAt": "2024-01-15T10:30:00Z"
  },
  "message": "Payment processed successfully",
  "statusCode": 200
}
```
**Response 400** (Payment Failed):
```json
{
  "data": {
    "billingRecordId": "billing-guid",
    "status": "failed",
    "failureReason": "Insufficient funds"
  },
  "message": "Payment failed: Insufficient funds",
  "statusCode": 400
}
```

#### 3.6 Retry Payment
```
POST /api/payments/retry-payment/{billingRecordId}
```
**Auth**: Required
**Request Body**: None (uses default payment method)
**Response**: Same as Process Payment

### 4. Invoice Management APIs

#### 4.1 Get User Invoices
```
GET /api/Invoice/user/{userId}?page=1&pageSize=20
```
**Auth**: Required
**Response 200**:
```json
{
  "data": [
    {
      "invoiceNumber": "INV-2024-001",
      "billingRecordId": "billing-guid",
      "userId": 123,
      "amount": 99.99,
      "status": "Paid",
      "issuedDate": "2024-01-01T00:00:00Z",
      "dueDate": "2024-01-15T00:00:00Z",
      "paidDate": "2024-01-02T10:30:00Z"
    }
  ],
  "meta": {
    "currentPage": 1,
    "pageSize": 20,
    "totalRecords": 5
  },
  "statusCode": 200
}
```

#### 4.2 Download Invoice
```
GET /api/Invoice/{invoiceNumber}/download?format=pdf
```
**Auth**: Required
**Response 200**:
```json
{
  "data": {
    "fileContent": "base64-encoded-pdf-string",
    "fileName": "INV-2024-001.pdf",
    "contentType": "application/pdf"
  },
  "message": "Invoice downloaded successfully",
  "statusCode": 200
}
```

### 5. Privilege Management APIs

#### 5.1 Get Privilege Usage Summary
```
GET /api/PrivilegeBasedBilling/usage-summary/{userId}
```
**Auth**: Required
**Response 200**:
```json
{
  "data": {
    "userId": 123,
    "subscriptionId": "sub-guid",
    "privileges": [
      {
        "privilegeName": "Teleconsultation",
        "usedValue": 3,
        "allowedValue": 5,
        "remainingValue": 2,
        "isUnlimited": false,
        "isExhausted": false,
        "usagePercentage": 60,
        "overage": 0,
        "overageCost": 0.00,
        "periodStart": "2024-01-01T00:00:00Z",
        "periodEnd": "2024-02-01T00:00:00Z"
      }
    ]
  },
  "statusCode": 200
}
```

---

## Frontend-Backend Flow Mapping

### Flow 1: View Subscription List

**Screen**: `subscription-list.component.ts`
**Route**: `/web/subscriptions`

**API Calls**:
1. `GET /api/Subscriptions/user/{userId}` - Load all user subscriptions

**Frontend Logic**:
```typescript
loadSubscriptions(): void {
  this.loading = true;
  this.subscriptionService.getUserSubscriptions(this.currentUser.id).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        this.subscriptions = response.data;
        this.categorizeSubscriptions(); // Group by status
      }
      this.loading = false;
    },
    error: (error) => {
      this.error = error.message;
      this.loading = false;
    }
  });
}
```

**Backend Flow**:
1. `SubscriptionsController.GetUserSubscriptions(userId)`
2. Validate: User can only access own subscriptions (or is admin)
3. `SubscriptionService.GetUserSubscriptionsAsync(userId, tokenModel)`
4. `SubscriptionRepository.GetByUserIdAsync(userId)`
5. Return list with plan details

**UI Updates**:
- Show loading spinner while fetching
- Display subscriptions grouped by status (Active, Paused, Cancelled)
- Show "No subscriptions" message if empty
- Display status badges with colors
- Show next billing date for active subscriptions

---

### Flow 2: Manual Renewal Payment

**Screen**: `subscription-renewal-payment.component.ts` (NEW)
**Trigger**: User clicks "Pay Now" button on failed payment alert

**Step-by-Step User Flow**:

1. **User Action**: Click "Pay Now" button
   - **Validation**: Check if subscription has pending billing record

2. **Frontend Opens Modal**:
   - Load pending billing record
   - Load saved payment methods
   - Display:
     - Amount due
     - Billing period
     - Payment method selector
     - "Add New Card" button

3. **API Call 1**: Get Pending Billing
   ```typescript
   loadPendingBilling(subscriptionId: string): void {
     this.billingService.getSubscriptionBillingHistory(subscriptionId).subscribe({
       next: (response) => {
         if (response.statusCode === 200) {
           // Find pending/failed billing record
           this.pendingBilling = response.data.find(
             b => b.status === 'Pending' || b.status === 'Failed'
           );
           if (!this.pendingBilling) {
             this.error = 'No pending payment found';
             return;
           }
         }
       }
     });
   }
   ```
   - **Backend**: `BillingController.GetSubscriptionBillingHistory(subscriptionId)`
   - **Response**: List of billing records for subscription

4. **API Call 2**: Load Payment Methods
   ```typescript
   loadPaymentMethods(): void {
     this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
       next: (response) => {
         if (response.statusCode === 200) {
           this.paymentMethods = response.data;
           // Auto-select default method
           this.selectedPaymentMethodId = this.paymentMethods.find(pm => pm.isDefault)?.id;
         }
       }
     });
   }
   ```
   - **Backend**: `PaymentController.GetPaymentMethods()`
   - **Response**: List of saved payment methods

5. **User Selects Payment Method** (or adds new card)

6. **User Clicks "Pay Now"**:
   - **Frontend Validation**:
     ```typescript
     // Check payment method selected
     if (!this.selectedPaymentMethodId) {
       this.error = 'Please select a payment method';
       return;
     }
     ```

7. **API Call 3**: Process Payment
   ```typescript
   processRenewalPayment(): void {
     this.processing = true;
     
     const request: ProcessPaymentRequestDto = {
       billingRecordId: this.pendingBilling.id,
       paymentMethodId: this.selectedPaymentMethodId
     };
     
     this.paymentService.processPayment(request).subscribe({
       next: (response) => {
         if (response.statusCode === 200) {
           // Payment successful
           this.showSuccessMessage();
           this.refreshSubscription();
           this.closeModal();
         }
         this.processing = false;
       },
       error: (error) => {
         this.error = error.message;
         this.processing = false;
       }
     });
   }
   ```
   - **Backend Flow**:
     1. `PaymentController.ProcessPayment(request)`
     2. Validate billing record exists and belongs to user
     3. Validate billing record status is Pending/Failed
     4. `SubscriptionBillingService.ProcessPaymentAsync()`
     5. Create Stripe PaymentIntent
     6. Confirm payment with Stripe
     7. **Transaction Start**
     8. Update billing record status to "Paid"
     9. Create SubscriptionPayment record
     10. Update subscription billing dates
     11. Reset privilege usage for new billing period
     12. **Transaction Commit**
     13. Send payment confirmation email
     14. Return success response

8. **UI Updates**:
   - Show success notification
   - Refresh subscription details
   - Close payment modal
   - Update subscription status badge (Pending → Active)
   - Show updated next billing date

**Error Handling**:
- Payment declined: Show Stripe error message, allow retry
- Insufficient funds: Show message, suggest adding new card
- Card expired: Show message, prompt to update card
- Network error: Show generic error, allow retry

---

### Flow 3: Purchase Additional Privilege Credits

**Screen**: `privilege-purchase-modal.component.ts` (NEW)
**Trigger**: User clicks "Buy More" button on privilege card

**Step-by-Step Flow**:

1. **User Action**: Click "Buy More" on Teleconsultation privilege
   - **Current State**: UsedValue: 5, AllowedValue: 5, Remaining: 0

2. **Frontend Opens Modal**:
   - Display:
     - Privilege name (Teleconsultation)
     - Current usage (5 of 5 used)
     - Unit cost ($20 per credit)
     - Quantity selector (default: 1)
     - Total cost calculation (quantity × unit cost)
     - Payment method selector
     - "Purchase" button

3. **API Call 1**: Check Privilege Availability (Pre-check)
   ```typescript
   checkAvailability(): void {
     this.privilegeService.checkAvailability(
       this.subscriptionId,
       this.privilegeName,
       1
     ).subscribe({
       next: (response) => {
         if (response.statusCode === 402) {
           // Limit exceeded, show purchase UI
           this.unitCost = response.data.unitCost;
           this.showPurchaseForm = true;
         } else if (response.statusCode === 200) {
           // Still have credits, show info
           this.message = response.data.message;
         }
       }
     });
   }
   ```
   - **Backend**: `SubscriptionsController.CheckPrivilegeAvailability()`
   - **Response**: Availability status with purchase details

4. **User Adjusts Quantity**:
   ```typescript
   updateQuantity(quantity: number): void {
     if (quantity < 1 || quantity > 100) return;
     this.quantity = quantity;
     this.totalCost = this.quantity * this.unitCost;
   }
   ```
   - **Validation**: 1-100 credits per purchase

5. **API Call 2**: Load Payment Methods
   - Same as manual renewal flow

6. **User Clicks "Purchase"**:
   - **Frontend Validation**:
     ```typescript
     validatePurchase(): boolean {
       if (this.quantity < 1 || this.quantity > 100) {
         this.error = 'Quantity must be between 1 and 100';
         return false;
       }
       if (!this.selectedPaymentMethodId) {
         this.error = 'Please select a payment method';
         return false;
       }
       return true;
     }
     ```

7. **API Call 3**: Purchase Credits
   ```typescript
   purchaseCredits(): void {
     if (!this.validatePurchase()) return;
     
     this.purchasing = true;
     
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
           this.showSuccessMessage(response.data);
           this.refreshPrivilegeUsage();
           this.closeModal();
         }
         this.purchasing = false;
       },
       error: (error) => {
         this.error = error.message;
         this.purchasing = false;
       }
     });
   }
   ```
   
   - **Backend Flow**:
     1. `SubscriptionsController.PurchaseAdditionalCredits(subscriptionId, dto)`
     2. Validate subscription exists and is active
     3. Validate user owns subscription
     4. Validate privilege exists in plan
     5. `PrivilegeService.PurchaseAdditionalCreditsAsync()`
     6. Calculate total cost: quantity × unitCost
     7. **Transaction Start**
     8. Create billing record (type: Overage)
     9. Process payment via Stripe IMMEDIATELY (upfront)
     10. If payment succeeds:
         - Update UserSubscriptionPrivilegeUsage.AllowedValue += quantity
         - Mark billing record as Paid
         - **Transaction Commit**
         - Send confirmation email
         - Return success with new limits
     11. If payment fails:
         - **Transaction Rollback**
         - Return 400 error (no credits added)

8. **Success Response**:
   ```json
   {
     "creditsAdded": 2,
     "unitCost": 20.00,
     "totalPaid": 40.00,
     "previousLimit": 5,
     "newLimit": 7,
     "currentUsed": 5,
     "newRemaining": 2
   }
   ```

9. **UI Updates**:
   - Show success message: "Successfully purchased 2 credits for $40"
   - Update privilege card:
     - AllowedValue: 5 → 7
     - Remaining: 0 → 2
     - Usage bar: 100% → 71%
   - Show green "Available" badge
   - Close modal after 2 seconds

**Error Handling**:
- Payment declined: Show error, don't add credits, allow retry
- Invalid privilege: Show error message
- Subscription inactive: Show error, disable purchase
- Network error: Show retry option

---

### Flow 4: Privilege Usage Dashboard

**Screen**: `privilege-usage.component.ts`
**Route**: `/web/privileges`

**Step-by-Step Flow**:

1. **Page Load**:
   - **API Call 1**: Get User Subscriptions
     ```typescript
     ngOnInit(): void {
       this.currentUser = this.authService.getCurrentUser();
       this.loadData();
     }
     
     loadData(): void {
       this.loading = true;
       
       // First, get active subscription
       this.subscriptionService.getUserSubscriptions(this.currentUser.id).subscribe({
         next: (response) => {
           if (response.statusCode === 200) {
             this.activeSubscription = response.data.find(
               s => s.status === 'Active' || s.status === 'TrialActive'
             );
             
             if (this.activeSubscription) {
               this.loadPrivilegeUsage(this.activeSubscription.id);
             } else {
               this.error = 'No active subscription found';
               this.loading = false;
             }
           }
         }
       });
     }
     ```

   - **API Call 2**: Get Privilege Usage Summary
     ```typescript
     loadPrivilegeUsage(subscriptionId: string): void {
       this.privilegeService.getUsageSummary(subscriptionId).subscribe({
         next: (response) => {
           if (response.statusCode === 200) {
             this.privilegeUsage = response.data;
             this.checkOverageWarnings(); // Check for 80%, 90%, 100% usage
           }
           this.loading = false;
         }
       });
     }
     ```

2. **Display Privilege Cards**:
   ```typescript
   <div *ngFor="let priv of privilegeUsage.privileges" class="privilege-card">
     <div class="privilege-header">
       <h4>{{ priv.privilegeName }}</h4>
       <span [class]="getStatusBadgeClass(priv)">
         {{ getStatusText(priv) }}
       </span>
     </div>
     
     <div class="usage-info">
       <p>{{ priv.usedValue }} of {{ priv.allowedValue }} used</p>
       <div class="progress">
         <div class="progress-bar" 
              [class]="getProgressBarClass(getUsagePercentage(priv))"
              [style.width.%]="getUsagePercentage(priv)">
         </div>
       </div>
       <small>{{ priv.remainingValue }} remaining</small>
     </div>
     
     <!-- Show warning at 80%+ usage -->
     <div *ngIf="getUsagePercentage(priv) >= 80" class="alert alert-warning">
       <i class="bi bi-exclamation-triangle"></i>
       You've used {{ getUsagePercentage(priv) }}% of your {{ priv.privilegeName }} credits
     </div>
     
     <!-- Show Buy More button when exhausted or near limit -->
     <button *ngIf="priv.remainingValue <= 2" 
             class="btn btn-primary btn-sm"
             (click)="openPurchaseModal(priv)">
       <i class="bi bi-cart-plus"></i> Buy More
     </button>
   </div>
   ```

3. **Warning System**:
   ```typescript
   checkOverageWarnings(): void {
     this.privilegeUsage.privileges.forEach(priv => {
       const percentage = this.getUsagePercentage(priv.usedValue, priv.allowedValue);
       
       if (percentage >= 100) {
         this.showWarning(priv.privilegeName, 'exhausted');
       } else if (percentage >= 90) {
         this.showWarning(priv.privilegeName, '90%');
       } else if (percentage >= 80) {
         this.showWarning(priv.privilegeName, '80%');
       }
     });
   }
   
   getProgressBarClass(percentage: number): string {
     if (percentage < 50) return 'bg-success';      // Green
     if (percentage < 80) return 'bg-warning';      // Yellow
     return 'bg-danger';                             // Red
   }
   ```

4. **User Clicks "Buy More"**:
   - Open privilege purchase modal (Flow 3)
   - Pass privilege details (name, unit cost, current usage)

5. **After Purchase**:
   - Refresh privilege usage automatically
   - Update UI without full page reload
   - Show toast notification with new limits

**UI States**:
- Loading: Show skeleton loaders
- No active subscription: Show message with "Browse Plans" button
- Active subscription with privileges: Show privilege cards
- Exhausted privilege: Show red badge, "Buy More" button prominent
- Near limit (80-99%): Show yellow badge, warning message
- Healthy usage (<80%): Show green badge

---

### Flow 5: Billing History with Invoice Download

**Screen**: `billing-history.component.ts`
**Route**: `/web/billing`

**Step-by-Step Flow**:

1. **Page Load**:
   ```typescript
   ngOnInit(): void {
     this.currentUser = this.authService.getCurrentUser();
     this.loadBillingRecords();
   }
   
   loadBillingRecords(): void {
     this.loading = true;
     
     const filters: Partial<BillingFilterDto> = {};
     
     if (this.selectedStatus && this.selectedStatus !== 'All') {
       filters.status = [this.selectedStatus];
     }
     
     if (this.selectedType && this.selectedType !== 'All') {
       filters.type = [this.selectedType];
     }
     
     this.billingService.getBillingRecords(
       this.currentUser.id,
       this.currentPage,
       this.pageSize,
       filters
     ).subscribe({
       next: (response) => {
         if (response.statusCode === 200) {
           this.billingRecords = response.data;
           this.totalRecords = response.meta.totalRecords;
           this.totalPages = response.meta.totalPages;
         }
         this.loading = false;
       }
     });
   }
   ```

2. **Display Billing Table**:
   ```html
   <table class="table">
     <thead>
       <tr>
         <th>Date</th>
         <th>Description</th>
         <th>Type</th>
         <th>Amount</th>
         <th>Status</th>
         <th>Actions</th>
       </tr>
     </thead>
     <tbody>
       <tr *ngFor="let billing of billingRecords">
         <td>{{ billing.billingDate | date:'short' }}</td>
         <td>{{ billing.description }}</td>
         <td>
           <span [class]="getTypeBadgeClass(billing.type)">
             {{ billing.type }}
           </span>
         </td>
         <td>{{ billing.totalAmount | currency }}</td>
         <td>
           <span [class]="getStatusBadgeClass(billing.status)">
             {{ billing.status }}
           </span>
         </td>
         <td>
           <!-- Download invoice if paid -->
           <button *ngIf="billing.status === 'Paid' && billing.invoiceNumber"
                   class="btn btn-sm btn-outline-primary"
                   (click)="downloadInvoice(billing.invoiceNumber)">
             <i class="bi bi-download"></i> Invoice
           </button>
           
           <!-- Pay now if pending/failed -->
           <button *ngIf="billing.status === 'Pending' || billing.status === 'Failed'"
                   class="btn btn-sm btn-primary"
                   (click)="payNow(billing)">
             <i class="bi bi-credit-card"></i> Pay Now
           </button>
           
           <!-- View details -->
           <button class="btn btn-sm btn-outline-secondary"
                   [routerLink]="['/web/billing', billing.id]">
             <i class="bi bi-eye"></i> Details
           </button>
         </td>
       </tr>
     </tbody>
   </table>
   ```

3. **Download Invoice**:
   ```typescript
   downloadInvoice(invoiceNumber: string): void {
     this.downloadingInvoice = invoiceNumber;
     
     this.invoiceService.downloadInvoice(invoiceNumber, 'pdf').subscribe({
       next: (response) => {
         if (response.statusCode === 200) {
           // Convert base64 to blob and trigger download
           const blob = this.base64ToBlob(
             response.data.fileContent,
             'application/pdf'
           );
           const url = window.URL.createObjectURL(blob);
           const link = document.createElement('a');
           link.href = url;
           link.download = response.data.fileName;
           link.click();
           window.URL.revokeObjectURL(url);
           
           this.showSuccessToast('Invoice downloaded successfully');
         }
         this.downloadingInvoice = null;
       },
       error: (error) => {
         this.showErrorToast('Failed to download invoice');
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

4. **Pay Pending Bill**:
   ```typescript
   payNow(billing: BillingRecordDto): void {
     // Open manual payment modal (Flow 2)
     this.selectedBilling = billing;
     this.showPaymentModal = true;
   }
   ```

5. **Filter Billing Records**:
   ```typescript
   applyFilters(): void {
     this.currentPage = 1; // Reset to first page
     this.loadBillingRecords();
   }
   ```

**Backend Flow for Download Invoice**:
1. `InvoiceController.DownloadInvoice(invoiceNumber, format)`
2. Validate: User owns the invoice (or is admin)
3. `InvoiceService.DownloadInvoiceAsync()`
4. Retrieve invoice from database
5. Generate PDF using PDF library
6. Convert to base64
7. Return file data with content type

---

## Security & Validation

### Frontend Validation

#### Before API Calls:
```typescript
// Subscription actions
validateSubscriptionAction(action: string): boolean {
  if (!this.subscription) {
    this.showError('Subscription not found');
    return false;
  }
  
  switch (action) {
    case 'pause':
      if (this.subscription.status !== 'Active') {
        this.showError('Only active subscriptions can be paused');
        return false;
      }
      break;
    case 'resume':
      if (this.subscription.status !== 'Paused') {
        this.showError('Only paused subscriptions can be resumed');
        return false;
      }
      break;
    case 'cancel':
      if (this.subscription.status === 'Cancelled') {
        this.showError('Subscription is already cancelled');
        return false;
      }
      break;
  }
  
  return true;
}

// Payment validation
validatePaymentMethod(paymentMethodId: string): boolean {
  if (!paymentMethodId) {
    this.showError('Please select a payment method');
    return false;
  }
  
  const method = this.paymentMethods.find(pm => pm.id === paymentMethodId);
  if (!method) {
    this.showError('Invalid payment method');
    return false;
  }
  
  // Check if card is expired
  if (method.card) {
    const now = new Date();
    const expiry = new Date(method.card.expYear, method.card.expMonth - 1);
    if (expiry < now) {
      this.showError('Selected card has expired. Please update your payment method.');
      return false;
    }
  }
  
  return true;
}

// Purchase credits validation
validateCreditPurchase(quantity: number): boolean {
  if (quantity < 1) {
    this.showError('Quantity must be at least 1');
    return false;
  }
  
  if (quantity > 100) {
    this.showError('Maximum 100 credits per purchase');
    return false;
  }
  
  return true;
}
```

### Backend Authorization

#### User Access Validation:
```csharp
// SubscriptionLifecycleService.cs
private async Task<bool> HasAccessToSubscription(int userId, Guid subscriptionId)
{
    var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
    return subscription != null && subscription.UserId == userId;
}

// Usage in every subscription operation:
if (tokenModel.RoleID != (int)RoleId.Admin && 
    !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
{
    return new JsonModel 
    { 
        data = new object(),
        Message = "Access denied. You don't have permission to access this subscription.",
        StatusCode = 403 
    };
}
```

#### Payment Method Validation:
```csharp
// PaymentController.cs
[HttpPost("payment-methods")]
public async Task<JsonModel> AddPaymentMethod([FromBody] AddPaymentMethodDto request)
{
    var token = GetToken(HttpContext);
    
    // Validate payment method with Stripe
    var validationResult = await _stripeService.ValidatePaymentMethodAsync(
        request.PaymentMethodId, 
        token
    );
    
    if (!validationResult)
    {
        return new JsonModel 
        { 
            data = new object(), 
            Message = "Invalid payment method. Please check your card details.",
            StatusCode = 400 
        };
    }
    
    // Continue with adding payment method...
}
```

#### Billing Record Access:
```csharp
// PaymentController.ProcessPayment
var billingRecord = await _billingService.GetBillingRecordAsync(
    request.BillingRecordId, 
    token
);

if (billingRecord.StatusCode != 200 || billingRecord.data == null)
{
    return new JsonModel 
    { 
        data = new object(), 
        Message = "Billing record not found",
        StatusCode = 404 
    };
}

var record = (BillingRecordDto)billingRecord.data;
if (record.UserId != token.UserID)
{
    return new JsonModel 
    { 
        data = new object(), 
        Message = "Access denied. You can only pay your own bills.",
        StatusCode = 403 
    };
}
```

### Data Sanitization

#### Frontend:
```typescript
// Sanitize user input for cancellation reason
sanitizeCancellationReason(reason: string): string {
  // Remove HTML tags
  reason = reason.replace(/<[^>]*>/g, '');
  
  // Trim whitespace
  reason = reason.trim();
  
  // Limit length
  if (reason.length > 500) {
    reason = reason.substring(0, 500);
  }
  
  return reason;
}
```

#### Backend:
```csharp
// All DTOs use validation attributes
public class PurchaseAdditionalCreditsDto
{
    [Required(ErrorMessage = "Privilege name is required")]
    [MaxLength(100)]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Invalid privilege name")]
    public string PrivilegeName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}
```

---

## Error Handling

### Error Response Format

All API errors follow this structure:
```json
{
  "data": {},
  "message": "Human-readable error message",
  "statusCode": 400
}
```

### Frontend Error Handling Pattern

```typescript
// Centralized error handler
handleApiError(error: any): void {
  let message = 'An unexpected error occurred';
  
  if (error.error && error.error.message) {
    // API returned error message
    message = error.error.message;
  } else if (error.status === 0) {
    // Network error
    message = 'Unable to connect to server. Please check your internet connection.';
  } else if (error.status === 401) {
    // Unauthorized
    message = 'Session expired. Please log in again.';
    this.router.navigate(['/auth/login']);
    return;
  } else if (error.status === 403) {
    // Forbidden
    message = 'Access denied. You don\'t have permission for this action.';
  } else if (error.status === 404) {
    // Not found
    message = 'Requested resource not found.';
  } else if (error.status >= 500) {
    // Server error
    message = 'Server error. Please try again later.';
  }
  
  this.showErrorToast(message);
  console.error('API Error:', error);
}
```

### Specific Error Scenarios

#### Payment Declined:
```typescript
// Frontend
processPayment(): void {
  this.paymentService.processPayment(request).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        this.showSuccess('Payment successful');
      }
    },
    error: (error) => {
      if (error.status === 400) {
        // Payment failed
        const failureReason = error.error.data?.failureReason || error.error.message;
        
        if (failureReason.includes('insufficient_funds')) {
          this.showError(
            'Payment declined due to insufficient funds. Please try a different card or add funds to your account.'
          );
        } else if (failureReason.includes('card_declined')) {
          this.showError(
            'Your card was declined. Please contact your bank or try a different card.'
          );
        } else if (failureReason.includes('expired_card')) {
          this.showError(
            'Your card has expired. Please update your payment method.'
          );
        } else {
          this.showError('Payment failed: ' + failureReason);
        }
      } else {
        this.handleApiError(error);
      }
    }
  });
}
```

#### Privilege Exhausted:
```typescript
// Frontend
checkPrivilegeBeforeUse(privilegeName: string): void {
  this.privilegeService.checkAvailability(
    this.subscriptionId,
    privilegeName,
    1
  ).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        // Privilege available, proceed
        this.proceedWithAction();
      } else if (response.statusCode === 402) {
        // Payment required
        this.showPurchaseModal(response.data);
      } else if (response.statusCode === 403) {
        // Privilege disabled in plan
        this.showError('This feature is not included in your plan. Please upgrade.');
      }
    },
    error: (error) => {
      this.handleApiError(error);
    }
  });
}
```

#### Subscription Not Found:
```typescript
// Frontend
loadSubscription(subscriptionId: string): void {
  this.subscriptionService.getSubscriptionById(subscriptionId).subscribe({
    next: (response) => {
      if (response.statusCode === 200) {
        this.subscription = response.data;
      } else if (response.statusCode === 404) {
        this.showError('Subscription not found');
        this.router.navigate(['/web/subscriptions']);
      } else if (response.statusCode === 403) {
        this.showError('Access denied');
        this.router.navigate(['/web/dashboard']);
      }
    },
    error: (error) => {
      this.handleApiError(error);
      this.router.navigate(['/web/subscriptions']);
    }
  });
}
```

---

## Implementation Checklist

### Phase 1: Manual Renewal Payment (HIGH PRIORITY)

#### Backend (Already Complete ✅)
- [x] `POST /api/payments/process-payment` endpoint exists
- [x] Validates billing record belongs to user
- [x] Processes payment via Stripe
- [x] Updates billing dates and resets privileges
- [x] Sends confirmation email

#### Frontend (To Build)
- [ ] Create `subscription-renewal-payment.component.ts`
  - [ ] Modal UI with payment form
  - [ ] Load pending billing records
  - [ ] Load saved payment methods
  - [ ] Payment method selector
  - [ ] Amount display
  - [ ] Process payment on submit
  - [ ] Handle success/error states
  - [ ] Refresh subscription after payment

- [ ] Update `subscription-detail.component.html`
  - [ ] Add "Pay Now" button for failed payments
  - [ ] Add payment failure alert banner
  - [ ] Show next billing date prominently

- [ ] Update `subscription-list.component.html`
  - [ ] Add payment failed badge
  - [ ] Add "Pay Now" quick action button

- [ ] Update `dashboard.component.ts`
  - [ ] Add failed payment alert widget
  - [ ] Show pending payment amount
  - [ ] Link to payment page

#### Testing
- [ ] Test with active subscription (should show "Up to date")
- [ ] Test with pending billing record (should show "Pay Now")
- [ ] Test with failed payment (should show alert)
- [ ] Test payment success flow
- [ ] Test payment decline scenarios
- [ ] Test privilege reset after payment
- [ ] Test billing date update after payment

---

### Phase 2: Privilege Purchase (HIGH PRIORITY)

#### Backend (Already Complete ✅)
- [x] `POST /api/Subscriptions/{id}/purchase-credits` endpoint exists
- [x] Validates subscription and privilege
- [x] Processes immediate payment
- [x] Updates AllowedValue on success
- [x] Rolls back on payment failure
- [x] Creates billing record (type: Overage)
- [x] Returns detailed purchase summary

#### Frontend (To Build)
- [ ] Create `privilege-purchase-modal.component.ts`
  - [ ] Modal UI with purchase form
  - [ ] Display privilege name, unit cost
  - [ ] Quantity selector (1-100)
  - [ ] Total cost calculation
  - [ ] Load payment methods
  - [ ] Payment method selector
  - [ ] Purchase button with loading state
  - [ ] Handle success response
  - [ ] Handle payment failure
  - [ ] Show purchase confirmation

- [ ] Update `privilege-usage.component.ts`
  - [ ] Add "Buy More" buttons to privilege cards
  - [ ] Show warning at 80%, 90%, 100% usage
  - [ ] Progress bar color coding
  - [ ] Call purchase modal
  - [ ] Refresh usage after purchase

- [ ] Update `privilege-usage.component.html`
  - [ ] Privilege cards with usage bars
  - [ ] Warning messages
  - [ ] "Buy More" buttons
  - [ ] Remaining count display

#### Testing
- [ ] Test privilege purchase flow end-to-end
- [ ] Test with exhausted privilege (0 remaining)
- [ ] Test with near-exhausted privilege (1-2 remaining)
- [ ] Test quantity validation (1-100)
- [ ] Test payment success
- [ ] Test payment decline (no credits added)
- [ ] Test AllowedValue update
- [ ] Test billing record creation

---

### Phase 3: Dashboard Enhancement (MEDIUM PRIORITY)

#### Backend (Already Complete ✅)
- [x] All required APIs exist

#### Frontend (To Build)
- [ ] Update `dashboard.component.ts`
  - [ ] Load user subscriptions
  - [ ] Load privilege usage summary
  - [ ] Load recent billing records
  - [ ] Check for upcoming renewals (7 days)
  - [ ] Check for failed payments
  - [ ] Check for privilege warnings

- [ ] Update `dashboard.component.html`
  - [ ] Active subscription card
    - [ ] Plan name, status
    - [ ] Next billing date
    - [ ] Amount
    - [ ] Quick actions (Pause, Cancel)
  - [ ] Upcoming renewal alert (7 days before)
  - [ ] Failed payment alert (with Pay Now button)
  - [ ] Privilege usage overview
    - [ ] Mini usage bars for each privilege
    - [ ] Warning badges at 80%+
    - [ ] Link to full privileges page
  - [ ] Recent billing activity (last 5)
    - [ ] Date, amount, status
    - [ ] Link to full history
  - [ ] Quick action buttons
    - [ ] Manage Subscription
    - [ ] View Billing
    - [ ] Manage Privileges
    - [ ] Payment Methods

#### Testing
- [ ] Test with active subscription
- [ ] Test with upcoming renewal (6 days)
- [ ] Test with failed payment
- [ ] Test with privilege at 85% usage
- [ ] Test with no active subscription
- [ ] Test quick action buttons

---

### Phase 4: Billing History Enhancement (MEDIUM PRIORITY)

#### Backend (Needs One New Endpoint)
- [x] `GET /api/Billing/records` exists
- [x] `GET /api/Invoice/{invoiceNumber}/download` exists
- [ ] NEW: `GET /api/Billing/subscription/{subscriptionId}/preview-next-bill`
  - [ ] Calculate base subscription amount
  - [ ] Calculate projected overage (current usage - allowed)
  - [ ] Return estimated total and breakdown

#### Frontend (To Build)
- [ ] Update `billing-history.component.ts`
  - [ ] Add invoice download functionality
  - [ ] Add "Pay Now" button for pending bills
  - [ ] Add refund status display
  - [ ] Implement filters (status, type, date range)

- [ ] Update `billing-history.component.html`
  - [ ] Invoice download buttons (PDF icon)
  - [ ] Pay Now buttons for pending
  - [ ] Refund badges and status
  - [ ] Filter controls
  - [ ] Pagination

- [ ] Create `billing-detail.component.ts`
  - [ ] Load billing record details
  - [ ] Show payment information
  - [ ] Show refund history
  - [ ] Download invoice button

- [ ] Create `upcoming-billing-preview.component.ts` (NEW)
  - [ ] Load next bill estimate
  - [ ] Show base amount
  - [ ] Show projected overage
  - [ ] Show total estimate
  - [ ] Link to manage privileges (reduce usage)

#### Testing
- [ ] Test invoice download (PDF)
- [ ] Test billing filters
- [ ] Test pagination
- [ ] Test pay pending bill
- [ ] Test refund status display
- [ ] Test upcoming billing preview

---

### Phase 5: Payment Method Management (LOW PRIORITY)

#### Backend (Already Complete ✅)
- [x] All payment method APIs exist

#### Frontend (Enhancement)
- [ ] Update `payment-methods.component.ts`
  - [ ] Integrate Stripe Elements for card collection
  - [ ] Add new card with Stripe.js
  - [ ] Show card expiry warnings
  - [ ] Implement remove card with confirmation

- [ ] Update `payment-methods.component.html`
  - [ ] Payment method list (cards)
  - [ ] Default badge
  - [ ] Expiry warnings (expires < 30 days)
  - [ ] "Add New Card" button
  - [ ] Set default buttons
  - [ ] Remove buttons

- [ ] Create `add-payment-method-modal.component.ts` (NEW)
  - [ ] Stripe Elements card input
  - [ ] Create Stripe PaymentMethod
  - [ ] Call backend to add method
  - [ ] Set as default option

#### Stripe Integration
- [ ] Load Stripe.js in index.html
  ```html
  <script src="https://js.stripe.com/v3/"></script>
  ```

- [ ] Create Stripe service
  ```typescript
  @Injectable({ providedIn: 'root' })
  export class StripeClientService {
    private stripe: any;
    private elements: any;
    private cardElement: any;
    
    constructor() {
      this.stripe = (window as any).Stripe('pk_test_...');
    }
    
    createCardElement(): any {
      this.elements = this.stripe.elements();
      this.cardElement = this.elements.create('card');
      return this.cardElement;
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

#### Testing
- [ ] Test add new card with Stripe Elements
- [ ] Test set default payment method
- [ ] Test remove payment method
- [ ] Test card expiry warning
- [ ] Test with no payment methods
- [ ] Test validation (expired cards, invalid cards)

---

### Phase 6: Subscription Actions (MEDIUM PRIORITY)

#### Backend (Already Complete ✅)
- [x] All subscription lifecycle APIs exist

#### Frontend (Enhancement)
- [ ] Update `subscription-detail.component.html`
  - [ ] Comprehensive action panel
  - [ ] Pause button (only for Active)
  - [ ] Resume button (only for Paused)
  - [ ] Cancel button (with confirmation)
  - [ ] Upgrade button
  - [ ] View Billing History button

- [ ] Create confirmation modals
  - [ ] Pause confirmation
  - [ ] Cancel confirmation (with reason input)
  - [ ] Resume confirmation

- [ ] Add upgrade flow
  - [ ] Navigate to plan selection
  - [ ] Pass current plan ID
  - [ ] Show upgrade options
  - [ ] Calculate prorated amount
  - [ ] Process upgrade payment

#### Testing
- [ ] Test pause (Active → Paused)
- [ ] Test resume (Paused → Active)
- [ ] Test cancel with reason
- [ ] Test action buttons show/hide based on status
- [ ] Test confirmations work correctly

---

### Phase 7: Testing & Quality Assurance

#### Integration Testing
- [ ] Test complete renewal payment flow
- [ ] Test complete privilege purchase flow
- [ ] Test subscription lifecycle (create → pause → resume → cancel)
- [ ] Test payment failure recovery
- [ ] Test privilege exhaustion → purchase → use

#### End-to-End User Scenarios
- [ ] New user subscribes to plan
  - [ ] Create subscription
  - [ ] Use privileges
  - [ ] Approach limit (see warnings)
  - [ ] Purchase additional credits
  - [ ] Automatic renewal
- [ ] User with failed payment
  - [ ] See failed payment alert
  - [ ] Click "Pay Now"
  - [ ] Select payment method
  - [ ] Complete payment
  - [ ] Subscription reactivated
- [ ] User manages subscription
  - [ ] Pause subscription
  - [ ] Resume subscription
  - [ ] View billing history
  - [ ] Download invoices
  - [ ] Update payment methods

#### Security Testing
- [ ] Test user cannot access other users' subscriptions
- [ ] Test user cannot pay other users' bills
- [ ] Test expired token handling
- [ ] Test SQL injection prevention (input sanitization)
- [ ] Test XSS prevention (output encoding)

#### Performance Testing
- [ ] Test with large billing history (100+ records)
- [ ] Test invoice download with large PDFs
- [ ] Test privilege usage with many privileges
- [ ] Test pagination performance

---

## Missing API Endpoints (To Be Created)

### 1. Preview Next Billing
```csharp
// BillingController.cs
[HttpGet("subscription/{subscriptionId}/preview-next-bill")]
public async Task<JsonModel> PreviewNextBill(Guid subscriptionId)
{
    var token = GetToken(HttpContext);
    return await _billingService.PreviewNextBillAsync(subscriptionId, token);
}
```

```csharp
// ISubscriptionBillingService.cs
Task<JsonModel> PreviewNextBillAsync(Guid subscriptionId, TokenModel tokenModel);
```

```csharp
// SubscriptionBillingService.cs
public async Task<JsonModel> PreviewNextBillAsync(Guid subscriptionId, TokenModel tokenModel)
{
    try
    {
        // Get subscription
        var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
        if (subscription == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
        
        // Validate access
        if (tokenModel.RoleID != (int)RoleId.Admin && subscription.UserId != tokenModel.UserID)
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        
        // Get base amount
        var plan = subscription.SubscriptionPlan;
        var baseAmount = plan.Price;
        
        // Calculate projected overage
        var usages = await _privilegeUsageRepository.GetBySubscriptionIdAsync(subscriptionId);
        decimal projectedOverage = 0;
        
        foreach (var usage in usages)
        {
            if (usage.UsedValue > usage.AllowedValue)
            {
                var planPrivilege = plan.PlanPrivileges.FirstOrDefault(pp => pp.Id == usage.SubscriptionPlanPrivilegeId);
                if (planPrivilege != null)
                {
                    var overageAmount = (usage.UsedValue - usage.AllowedValue) * planPrivilege.OverageCost;
                    projectedOverage += overageAmount;
                }
            }
        }
        
        var result = new
        {
            baseAmount,
            projectedOverage,
            estimatedTotal = baseAmount + projectedOverage,
            nextBillingDate = subscription.NextBillingDate,
            breakdown = usages.Where(u => u.UsedValue > u.AllowedValue).Select(u => new
            {
                privilegeName = u.SubscriptionPlanPrivilege?.Privilege?.Name ?? "Unknown",
                overageUnits = u.UsedValue - u.AllowedValue,
                unitCost = u.SubscriptionPlanPrivilege?.OverageCost ?? 0,
                overageAmount = (u.UsedValue - u.AllowedValue) * (u.SubscriptionPlanPrivilege?.OverageCost ?? 0)
            }).ToList()
        };
        
        return new JsonModel { data = result, Message = "Next bill preview calculated", StatusCode = 200 };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error previewing next bill for subscription {SubId}", subscriptionId);
        return new JsonModel { data = new object(), Message = "Error calculating preview", StatusCode = 500 };
    }
}
```

---

## Summary

This blueprint provides:

1. ✅ **Complete API endpoint verification** with exact URLs, methods, and auth requirements
2. ✅ **Detailed request/response structures** for every endpoint
3. ✅ **Precise frontend-backend mappings** showing which screens call which APIs
4. ✅ **Step-by-step user flows** with code examples for each feature
5. ✅ **Security considerations** with validation at both frontend and backend
6. ✅ **Error handling patterns** with specific scenarios
7. ✅ **Implementation checklist** for systematic development

**Ready to implement**: Follow the checklist phase by phase, starting with Phase 1 (Manual Renewal Payment), which is the highest priority feature for allowing users to manage failed payments.

**All backend APIs are production-ready**. The focus is on building robust, user-friendly frontend components that integrate seamlessly with the existing backend.



