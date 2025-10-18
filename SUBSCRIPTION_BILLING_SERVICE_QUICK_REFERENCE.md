# SubscriptionBillingService - Quick Reference Guide

**Version:** 1.0  
**Status:** ✅ Production Ready  
**Total Methods:** 51  
**Lines of Code:** 2,255

---

## 🎯 What is SubscriptionBillingService?

The **single, comprehensive service** for all billing operations in the Smart Telehealth Subscription Management system. It consolidates functionality from the former `BillingService` and `PrivilegeBasedBillingService`.

---

## 🔑 Key Capabilities

### 1. **Subscription Plan Pricing** (Client Workflow Step 1)
```csharp
await _subscriptionBillingService.CalculatePlanBasePriceAsync(calculateDto, tokenModel);
```
- Calculates base price from privileges: `Σ(Value × UnitCost) + Commission`
- **Fixed**: Uses `Value` field correctly (not `DailyLimit`)

### 2. **Subscription Billing** (Client Workflow Step 2)
```csharp
await _subscriptionBillingService.CreateSubscriptionBillingAsync(subscription, amount, description, dueDate, tokenModel);
```
- Creates initial subscription billing record
- Sets up payment tracking

### 3. **Privilege Usage Tracking** (Client Workflow Steps 3-4)
```csharp
await _subscriptionBillingService.ProcessPrivilegeUsageAsync(usageDto, tokenModel);
```
- Tracks usage against limits
- Calculates overage charges automatically
- Supports daily, weekly, monthly limits

### 4. **Overage Billing** (Client Workflow Step 4)
```csharp
await _subscriptionBillingService.CreateOverageBillingAsync(subscription, privilegeName, amount, tokenModel);
```
- Applies extra charges: `(Used - Limit) × UnitCost`
- Real-time or deferred billing support

### 5. **Payment Processing** (Client Workflow Step 5)
```csharp
await _subscriptionBillingService.ProcessPaymentAsync(billingRecordId, tokenModel);
await _subscriptionBillingService.ProcessRefundAsync(billingRecordId, amount, tokenModel);
await _subscriptionBillingService.RetryPaymentAsync(billingRecordId, tokenModel);
```
- Complete payment lifecycle management
- Refund support
- Failed payment retry logic

### 6. **Subscription Renewal** (Client Workflow Step 6)
```csharp
await _subscriptionBillingService.ProcessSubscriptionRenewalAsync(subscriptionId, tokenModel);
```
- Resets privilege usage
- Carries over pending overage charges
- Updates next billing date

---

## 📊 All Available Methods (51 Total)

### Privilege-Based Billing (4 methods)
| Method | Description |
|--------|-------------|
| `CalculatePlanBasePriceAsync` | Calculates plan base price from privileges |
| `ProcessPrivilegeUsageAsync` | Tracks usage and calculates overage |
| `ProcessSubscriptionRenewalAsync` | Renews subscription, resets usage |
| `GetPrivilegeUsageSummaryAsync` | Gets usage summary with overage details |

### Billing Record Factory Methods (4 methods)
| Method | Description |
|--------|-------------|
| `CreateSubscriptionBillingAsync` | Creates subscription billing record |
| `CreateOverageBillingAsync` | Creates overage billing record |
| `CreateConsultationBillingAsync` | Creates consultation billing record |
| `CreateMedicationBillingAsync` | Creates medication billing record |

### Core Billing Management (8 methods)
| Method | Description |
|--------|-------------|
| `CreateBillingRecordAsync` | Creates generic billing record |
| `GetBillingRecordAsync` | Retrieves specific billing record |
| `GetUserBillingHistoryAsync` | Gets user's billing history |
| `GetSubscriptionBillingHistoryAsync` | Gets subscription billing history |
| `GetBillingRecordsWithFilteringAsync` | Advanced filtering with pagination |
| `GetAllBillingRecordsAsync` | Legacy method for all records |
| `GetOverdueBillingRecordsAsync` | Gets overdue records |
| `GetPendingPaymentsAsync` | Gets pending payments |

### Payment Processing (7 methods)
| Method | Description |
|--------|-------------|
| `ProcessPaymentAsync` | Processes payment for billing record |
| `ProcessRefundAsync` | Processes refund (2 overloads) |
| `RetryFailedPaymentAsync` | Retries failed payment |
| `RetryPaymentAsync` | Retries payment processing |
| `ProcessPartialPaymentAsync` | Processes partial payment |
| `UpdatePaymentMethodAsync` | Updates payment method |

### Calculations (5 methods)
| Method | Description |
|--------|-------------|
| `CalculateTotalAmountAsync` | Calculates total (base + tax + shipping) |
| `CalculateTaxAmountAsync` | Calculates tax based on state |
| `CalculateShippingAmountAsync` | Calculates shipping cost |
| `IsPaymentOverdueAsync` | Checks if payment is overdue |
| `CalculateDueDateAsync` | Calculates due date with grace period |

### Enhanced Billing (5 methods)
| Method | Description |
|--------|-------------|
| `CreateUpfrontPaymentAsync` | Creates upfront payment |
| `ProcessBundlePaymentAsync` | Processes bundle payment |
| `CreateRecurringBillingAsync` | Sets up recurring billing |
| `ProcessRecurringPaymentAsync` | Processes recurring payment |
| `CancelRecurringBillingAsync` | Cancels recurring billing |

### Date Calculations (2 methods)
| Method | Description |
|--------|-------------|
| `CalculateNextBillingDate` | Calculates next billing date from cycle |
| `CalculateNextBillingDateForSubscriptionAsync` | Calculates next billing for subscription |

### Billing Adjustments (4 methods)
| Method | Description |
|--------|-------------|
| `ApplyBillingAdjustmentAsync` | Applies adjustment to billing record |
| `GetBillingAdjustmentsAsync` | Gets all adjustments for record |
| `ReverseBillingAdjustmentAsync` | Reverses an adjustment |
| `GetTotalAdjustmentAmountAsync` | Calculates total adjustments |

### Analytics & Reporting (8 methods)
| Method | Description |
|--------|-------------|
| `GetPaymentHistoryAsync` | Gets payment history (2 overloads) |
| `GetPaymentAnalyticsAsync` | Gets payment analytics (2 overloads) |
| `GetBillingAnalyticsAsync` | Gets billing analytics |
| `GetBillingSummaryAsync` | Gets billing summary for user |
| `GetRevenueSummaryAsync` | Gets revenue summary |

### Invoicing (5 methods)
| Method | Description |
|--------|-------------|
| `CreateInvoiceAsync` | Creates invoice |
| `GenerateInvoiceAsync` | Generates invoice from billing record |
| `GenerateInvoicePdfAsync` | Generates PDF invoice |
| `GetInvoiceAsync` | Retrieves invoice by number |
| `UpdateInvoiceStatusAsync` | Updates invoice status |

### Reporting & Export (3 methods)
| Method | Description |
|--------|-------------|
| `GenerateBillingReportAsync` | Generates billing report |
| `ExportBillingRecordsAsync` | Exports billing records |
| `ExportRevenueAsync` | Exports revenue data |

### Billing Cycle Management (4 methods)
| Method | Description |
|--------|-------------|
| `CreateBillingCycleAsync` | Creates new billing cycle |
| `ProcessBillingCycleAsync` | Processes billing cycle |
| `GetBillingCycleRecordsAsync` | Gets cycle records |
| `GetPaymentScheduleAsync` | Gets payment schedule |

---

## 💉 Dependency Injection

### How to Use:
```csharp
public class MyController : ControllerBase
{
    private readonly ISubscriptionBillingService _billingService;

    public MyController(ISubscriptionBillingService billingService)
    {
        _billingService = billingService;
    }

    // Use any of the 51 methods
}
```

### All Dependencies (12 total):
- `IUnitOfWork` - Transaction management
- `IBillingRepository` - Billing data access
- `ISubscriptionRepository` - Subscription data
- `ISubscriptionPlanRepository` - Plan data
- `IUserSubscriptionPrivilegeUsageRepository` - Usage tracking
- `IPrivilegeRepository` - Privilege definitions
- `IUserRepository` - User information
- `IPaymentService` - Payment operations (SRP delegation)
- `IStripeService` - Stripe integration
- `INotificationService` - Email notifications
- `IMapper` - Entity-DTO mapping
- `ILogger<SubscriptionBillingService>` - Logging

---

## 🔍 Common Use Cases

### Calculate Plan Price
```csharp
var dto = new CalculatePlanPriceDto
{
    PlanId = planId,
    AdminCommissionPercentage = 10
};
var result = await _billingService.CalculatePlanBasePriceAsync(dto, tokenModel);
```

### Track Privilege Usage
```csharp
var usageDto = new ProcessPrivilegeUsageDto
{
    UserId = userId,
    PrivilegeId = privilegeId,
    UsageCount = 1
};
var result = await _billingService.ProcessPrivilegeUsageAsync(usageDto, tokenModel);
```

### Process Payment
```csharp
var result = await _billingService.ProcessPaymentAsync(billingRecordId, tokenModel);
```

### Renew Subscription
```csharp
var result = await _billingService.ProcessSubscriptionRenewalAsync(subscriptionId, tokenModel);
```

### Get Billing History
```csharp
var history = await _billingService.GetUserBillingHistoryAsync(userId, tokenModel);
```

---

## ⚠️ Important Notes

### SRP Delegation
Many methods correctly delegate to `PaymentService`:
- Invoice operations
- Payment analytics
- Recurring billing setup
- Payment schedule retrieval

**This is intentional and correct!** It follows the Single Responsibility Principle.

### Transaction Management
Methods using `IUnitOfWork` for data integrity:
- `ProcessPrivilegeUsageAsync`
- `ProcessSubscriptionRenewalAsync`

Always ensure proper transaction handling for these operations.

### Error Handling
All methods include:
- Try-catch blocks
- Detailed logging
- User-friendly error messages
- Proper HTTP status codes

---

## 🎯 Client Workflow Integration

This service **fully supports** the client's subscription management workflow:

1. **Admin creates plan** → `CalculatePlanBasePriceAsync`
2. **User subscribes** → `CreateSubscriptionBillingAsync`
3. **User uses privileges** → `ProcessPrivilegeUsageAsync`
4. **Usage exceeds limit** → `CreateOverageBillingAsync`
5. **Payment processed** → `ProcessPaymentAsync`
6. **Subscription renews** → `ProcessSubscriptionRenewalAsync`

---

## 📞 Support & Documentation

For detailed implementation details, see:
- `BILLING_CONSOLIDATION_COMPLETE.md` - Complete migration report
- `ISubscriptionBillingService.cs` - Interface with XML documentation
- `SubscriptionBillingService.cs` - Full implementation (2,255 lines)

---

**Last Updated:** Thursday, October 16, 2025  
**Status:** ✅ PRODUCTION READY

