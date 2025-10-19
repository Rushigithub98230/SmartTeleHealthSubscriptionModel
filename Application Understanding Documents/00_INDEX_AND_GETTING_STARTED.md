# 📚 SmartTelehealth Subscription Management
## Developer Documentation - Index & Getting Started

**Version:** 1.0  
**Date:** October 17, 2025  
**Target Audience:** New Developers

---

> **✨ CURRENT IMPLEMENTATION** | Updated October 18, 2025
> 
> **System Version:** Solution A - Billing Cycle-Based Privilege Scaling
> 
> **Key Features:**
> - ✅ Multiple billing cycles (Monthly, Quarterly, Annual)
> - ✅ Dynamic privilege scaling: `Math.Ceiling(monthlyLimit × monthsInCycle)`
> - ✅ Smart price calculation: `monthlyPrice × (days/30) - discount`
> - ✅ Payment-triggered privilege resets (not time-based)

---

## 🎯 Purpose of This Documentation

Welcome to the **SmartTelehealth Subscription Management System**! This documentation set is designed to help you, as a new developer, understand our complete subscription management flow from the ground up.

### What You'll Learn

✅ How subscription plans are created and managed  
✅ How users subscribe and their subscriptions are tracked  
✅ How billing works (base subscription + overage charges)  
✅ How privilege usage is tracked and limits enforced  
✅ How Stripe integration works (payment processing)  
✅ How automated renewals and background jobs function  
✅ How all services interact with each other  

---

## 📖 Documentation Structure

### **5 Core Guides + 1 Complete Flow Document**

| Guide | Focus | New Developer Priority |
|-------|-------|------------------------|
| **00 - Index** (This File) | Navigation & Overview | Read First ⭐ |
| **01 - Subscription Plans** | How plans are created/managed | Read Second |
| **02 - User Subscriptions** | How users subscribe & lifecycle | Read Third |
| **03 - Billing & Payments** | How billing works | Read Fourth |
| **04 - Privilege Management** | How usage is tracked | Read Fifth |
| **05 - Stripe Integration** | How Stripe works | Read Sixth |
| **06 - Complete Flow** | End-to-end scenarios | Read Last (Summary) |

---

## 🚀 Quick Start Guide

### For First-Time Readers

**Step 1: Start Here** (5 minutes)
- Read this index file completely
- Understand the big picture
- Know what each guide covers

**Step 2: Learn the Foundation** (30 minutes)
- Read Guide 01: Subscription Plans
- Understand how plans are structured
- Know the difference between PrivilegeBaseCost and UnitCost

**Step 3: Understand User Flow** (30 minutes)
- Read Guide 02: User Subscriptions
- Learn subscription states
- Understand privilege initialization

**Step 4: Master Billing** (45 minutes)
- Read Guide 03: Billing & Payments
- Learn billing types
- Understand overage handling

**Step 5: Track Usage** (30 minutes)
- Read Guide 04: Privilege Management
- Learn how counters work
- Understand usage history

**Step 6: Stripe Deep Dive** (45 minutes)
- Read Guide 05: Stripe Integration
- Learn webhook processing
- Understand synchronization

**Step 7: See It All Together** (60 minutes)
- Read Guide 06: Complete Flow
- See all pieces working together
- Review all scenarios

**Total Time:** ~4 hours for complete understanding

---

## 🎨 Visual Learning Path

```
START HERE
    ↓
[INDEX] ← You are here
    ↓
    ├─→ [01 PLANS] → Learn how admins create plans
    │        ↓
    ├─→ [02 LIFECYCLE] → Learn how users subscribe
    │        ↓
    ├─→ [03 BILLING] → Learn how billing works
    │        ↓
    ├─→ [04 PRIVILEGES] → Learn how usage is tracked
    │        ↓
    └─→ [05 STRIPE] → Learn how Stripe integrates
             ↓
    [06 COMPLETE FLOW] → See everything together
             ↓
    ✅ READY TO CODE!
```

---

## 🏗️ System Architecture at a Glance

### High-Level Components

```
┌────────────────────┐
│   ADMIN PORTAL     │  → Creates plans, manages subscriptions
└─────────┬──────────┘
          │
          ↓
┌────────────────────┐
│   USER PORTAL      │  → Subscribes, uses services
└─────────┬──────────┘
          │
          ↓
┌────────────────────────────────────────────────┐
│           BACKEND API (.NET 8)                  │
│                                                 │
│  ┌───────────────────────────────────────┐    │
│  │ Controllers (HTTP Endpoints)           │    │
│  │  - SubscriptionPlansController         │    │
│  │  - SubscriptionsController             │    │
│  │  - BillingController                   │    │
│  │  - StripeWebhookController             │    │
│  └────────────┬──────────────────────────┘    │
│               │                                 │
│  ┌────────────▼──────────────────────────┐    │
│  │ Services (Business Logic)              │    │
│  │  - SubscriptionPlanService             │    │
│  │  - SubscriptionLifecycleService        │    │
│  │  - SubscriptionBillingService          │    │
│  │  - PrivilegeService                    │    │
│  │  - StripeService                       │    │
│  └────────────┬──────────────────────────┘    │
│               │                                 │
│  ┌────────────▼──────────────────────────┐    │
│  │ Repositories (Data Access)             │    │
│  │  - SubscriptionRepository              │    │
│  │  - BillingRepository                   │    │
│  │  - PrivilegeUsageRepository            │    │
│  └────────────┬──────────────────────────┘    │
└───────────────┼─────────────────────────────────┘
                │
    ┌───────────┴──────────┐
    │                      │
    ↓                      ↓
┌─────────────┐    ┌──────────────┐
│ YOUR DB     │    │   STRIPE     │
│ (SQL Server)│←──→│   (Cloud)    │
└─────────────┘    └──────────────┘
   Webhooks
```

### Core Services at a Glance

| Service | What It Does | Guide Reference |
|---------|--------------|-----------------|
| **SubscriptionPlanService** | Manages plans (create, update, delete) | Guide 01 |
| **SubscriptionLifecycleService** | Manages subscription states | Guide 02 |
| **SubscriptionBillingService** | Creates billing records, handles overage | Guide 03 |
| **PrivilegeService** | Tracks usage, enforces limits | Guide 04 |
| **PaymentService** | Processes payments, refunds | Guide 03 |
| **StripeService** | All Stripe API calls | Guide 05 |
| **AutomatedBillingService** | Automated renewals, retries | Guide 03 |

---

## 🔑 Key Concepts for New Developers

### Concept 1: Two Databases

We maintain data in **TWO places**:

1. **Your Database** (SQL Server)
   - Complete business data
   - Privilege usage tracking
   - Audit trails
   - Analytics data

2. **Stripe Database** (Stripe's Cloud)
   - Payment processing
   - Recurring billing automation
   - Customer payment methods
   - Transaction history

**Why Two?**
- Performance: Fast local queries
- Control: Complex privilege logic locally
- Reliability: System works if Stripe is temporarily down
- Compliance: Complete audit trail for healthcare regulations

### Concept 2: Synchronization

```
YOUR DATABASE ←──────── SYNC ──────────→ STRIPE

PUSH (You → Stripe):
  - When admin creates plan → Create Stripe product
  - When user subscribes → Create Stripe subscription
  - When you need payment → Create Stripe payment intent

PULL (Stripe → You):
  - When payment succeeds → Webhook updates your DB
  - When payment fails → Webhook updates your DB
  - When renewal occurs → Webhook updates your DB
```

### Concept 3: Privilege-Based Model

```
PRIVILEGE = A service user can access
  Examples: "Teleconsultation", "Medication Refill"

PLAN = A bundle of privileges
  Example: 5 consultations + 3 medications = $275/month

USAGE = Tracking how many user has consumed
  Example: Used 3 of 5 consultations, 2 remaining

OVERAGE = When user exceeds plan limits
  Example: User wants 6th consultation, must pay $25 upfront
```

### Concept 4: Transaction Safety

**We use Unit of Work pattern:**

```csharp
await _unitOfWork.BeginTransactionAsync();

try
{
    // Multiple database operations
    await _repo1.CreateAsync(...);
    await _repo2.UpdateAsync(...);
    await _repo3.CreateAsync(...);
    
    // ALL or NOTHING
    await _unitOfWork.CommitTransactionAsync();
}
catch (Exception ex)
{
    // Rollback everything if any operation fails
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

**Why?** Ensures data consistency. If Stripe API fails, your database doesn't get corrupted with partial data.

---

## 📊 Database Tables Quick Reference

### Core Tables

| Table | Purpose | Key Fields |
|-------|---------|------------|
| **Subscriptions** | User subscriptions | UserId, PlanId, Status, StripeSubscriptionId |
| **SubscriptionPlans** | Plan definitions | Name, Price, StripeProductId |
| **SubscriptionPlanPrivileges** | Plan-privilege config | Value, PrivilegeBaseCost, UnitCost |
| **UserSubscriptionPrivilegeUsage** | Usage tracking | AllocatedLimit, UsedValue, AllowedValue |
| **BillingRecords** | All billing events | Amount, Type, Status, StripeInvoiceId |
| **SubscriptionPayments** | Payment records | Amount, Status, TransactionId |
| **PrivilegeUsageHistory** | Usage audit trail | UsageType, Cost, Quantity |
| **SubscriptionStatusHistory** | Status changes | FromStatus, ToStatus, Reason |

**Full schemas in respective guides.**

---

## 🔄 Complete Flow Overview

### The Journey of a Subscription (30-Second Overview)

```
1. ADMIN CREATES PLAN
   ├─ Defines "Basic Health" with 5 consultations
   ├─ Sets price: Auto-calculated = $275
   └─ Stripe product created: prod_ABC123

2. USER SUBSCRIBES
   ├─ Selects "Basic Health"
   ├─ Stripe customer created: cus_XYZ789
   ├─ Subscription created in both systems
   └─ Privileges initialized: 5 consultations available

3. USER USES SERVICES
   ├─ Books consultation → Counter decrements: 5→4→3→2→1→0
   └─ Each use tracked in history

4. USER EXCEEDS LIMITS (OVERAGE)
   ├─ Tries 6th consultation → BLOCKED
   ├─ System requires $25 upfront payment
   ├─ User pays → Credit added → Service allowed
   └─ Marked in history as "Overage" with cost

5. MONTHLY RENEWAL
   ├─ Stripe auto-charges $275
   ├─ Webhook updates your DB
   ├─ Privilege counters RESET to 5
   └─ Cycle continues
```

---

## 🎓 Learning Roadmap

### Week 1: Foundation
- **Day 1-2**: Read all guides (Guides 01-05)
- **Day 3**: Study database schema
- **Day 4**: Review service architecture
- **Day 5**: Read complete flow (Guide 06)

### Week 2: Hands-On
- **Day 1**: Set up dev environment
- **Day 2**: Create a test subscription plan
- **Day 3**: Test subscription creation flow
- **Day 4**: Test privilege usage
- **Day 5**: Test overage handling

### Week 3: Advanced
- **Day 1**: Study Stripe webhook handling
- **Day 2**: Test payment failures and retries
- **Day 3**: Study automated billing jobs
- **Day 4**: Review error handling patterns
- **Day 5**: Code review with team

---

## 🔧 Development Environment Setup

### Required Tools
- Visual Studio 2022 or VS Code
- .NET 8 SDK
- SQL Server (local or remote)
- Stripe account (test mode)
- Postman (for API testing)

### Configuration

**appsettings.json:**
```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_...",  // Your Stripe secret key
    "PublishableKey": "pk_test_...",  // Your Stripe publishable key
    "WebhookSecret": "whsec_..."  // Webhook signing secret
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=SmartTelehealth;..."
  }
}
```

### Stripe Dashboard Setup

1. Create Stripe account (test mode)
2. Get API keys from Dashboard → Developers → API keys
3. Create webhook endpoint:
   - URL: `https://your-api.com/api/webhooks/stripe`
   - Events: Select all (we handle 51 types)
   - Get webhook secret
4. Test with Stripe CLI (optional)

---

## 🧭 Navigation Guide

### By Topic

| I Want To... | Go To |
|--------------|-------|
| Understand how plans are created | Guide 01, Section 5.1 |
| Learn about subscription states | Guide 02, Section 2 |
| Know how billing works | Guide 03, Section 5-7 |
| Understand overage charges | Guide 03, Section 6 & Guide 04, Section 6 |
| Learn privilege tracking | Guide 04, Section 5 |
| Understand Stripe webhooks | Guide 05, Section 5 |
| See complete end-to-end flow | Guide 06 |

### By Service

| Service | Primary Guide | Also Mentioned In |
|---------|---------------|-------------------|
| SubscriptionPlanService | Guide 01 | Guide 06 |
| SubscriptionLifecycleService | Guide 02 | Guide 03, 06 |
| SubscriptionBillingService | Guide 03 | Guide 02, 04, 06 |
| PrivilegeService | Guide 04 | Guide 02, 03, 06 |
| StripeService | Guide 05 | Guide 01, 02, 03, 06 |
| PaymentService | Guide 03 | Guide 05, 06 |

### By Scenario

| Scenario | Primary Guide | Section |
|----------|---------------|---------|
| Admin creates plan | Guide 01 | 5.1 |
| User subscribes | Guide 02 | 5.1 |
| User uses privilege | Guide 04 | 5.1 |
| User exceeds limit | Guide 04 | 6 & Guide 03 | 6 |
| Monthly renewal | Guide 03 | 7 |
| Payment fails | Guide 03 | 8 |
| Webhook processing | Guide 05 | 5 |

---

## 🎯 Key Concepts Summary

### The Big Picture

Our subscription management system is built on **5 core pillars**:

1. **Plans** - What we offer (products)
2. **Subscriptions** - What users buy (active plans)
3. **Billing** - How we charge users (money in)
4. **Privileges** - What users can do (usage tracking)
5. **Stripe** - How we process payments (payment provider)

### How They Connect

```
ADMIN creates PLAN (Guide 01)
    ↓
    Contains PRIVILEGES with limits (Guide 04)
    ↓
USER subscribes to PLAN (Guide 02)
    ↓
    Creates SUBSCRIPTION (Guide 02)
    ↓
    Initializes PRIVILEGE USAGE tracking (Guide 04)
    ↓
    Creates BILLING RECORD (Guide 03)
    ↓
    Processes PAYMENT via STRIPE (Guide 05)
    ↓
    Subscription becomes ACTIVE (Guide 02)
    ↓
USER uses PRIVILEGES (Guide 04)
    ↓
    Counters DECREMENT (5→4→3→2→1→0)
    ↓
    Usage tracked in HISTORY (Guide 04)
    ↓
USER exceeds limits (OVERAGE) (Guide 04)
    ↓
    BLOCKS usage (Guide 04, Section 6)
    ↓
    Requires UPFRONT PAYMENT (Guide 03, Section 6)
    ↓
    User pays → Credit added → Service allowed
    ↓
MONTHLY RENEWAL (Guide 03, Section 7)
    ↓
    STRIPE auto-charges (Guide 05)
    ↓
    WEBHOOK updates database (Guide 05, Section 5)
    ↓
    PRIVILEGES reset (Guide 04, Section 8.3)
    ↓
    Cycle continues...
```

---

## 📁 File Structure

### Backend Code Locations

```
SmartTeleHealthSubscriptionModel/
│
├── backend/
│   ├── SmartTelehealth.API/
│   │   └── Controllers/
│   │       ├── SubscriptionPlansController.cs  ← Guide 01
│   │       ├── SubscriptionsController.cs      ← Guide 02
│   │       ├── BillingController.cs            ← Guide 03
│   │       └── StripeWebhookController.cs      ← Guide 05
│   │
│   ├── SmartTelehealth.Application/
│   │   ├── Services/
│   │   │   ├── SubscriptionPlanService.cs      ← Guide 01
│   │   │   ├── SubscriptionLifecycleService.cs ← Guide 02
│   │   │   ├── SubscriptionBillingService.cs   ← Guide 03
│   │   │   ├── PrivilegeService.cs             ← Guide 04
│   │   │   └── PaymentService.cs               ← Guide 03
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── ISubscriptionPlanService.cs
│   │   │   ├── ISubscriptionLifecycleService.cs
│   │   │   └── ISubscriptionBillingService.cs
│   │   │
│   │   └── DTOs/
│   │       ├── CreateSubscriptionPlanDto.cs
│   │       ├── CreateSubscriptionDto.cs
│   │       └── BillingRecordDto.cs
│   │
│   ├── SmartTelehealth.Infrastructure/
│   │   ├── Services/
│   │   │   └── StripeService.cs                ← Guide 05
│   │   │
│   │   └── Repositories/
│   │       ├── SubscriptionRepository.cs
│   │       ├── BillingRepository.cs
│   │       └── PrivilegeUsageRepository.cs
│   │
│   └── SmartTelehealth.Core/
│       └── Entities/
│           ├── Subscription.cs                  ← Guide 02
│           ├── SubscriptionPlan.cs              ← Guide 01
│           ├── BillingRecord.cs                 ← Guide 03
│           └── UserSubscriptionPrivilegeUsage.cs ← Guide 04
```

---

## 💡 Common Questions for New Developers

### Q1: Why do we have both PrivilegeBaseCost and UnitCost?

**A:** Two different purposes:
- **PrivilegeBaseCost ($20)**: Used to calculate the monthly plan price
  - Example: 5 consultations × $20 = $100 (part of $275 plan price)
- **UnitCost ($25)**: Used to charge for overage when user exceeds limits
  - Example: 6th consultation costs $25 (extra charge)

**Why higher for overage?** Discourages abuse, encourages users to buy appropriate plan.

### Q2: When is a Stripe customer created?

**A:** When user **first subscribes** to any plan.
- Created once per user
- Stored in `Users.StripeCustomerId`
- Reused for all future subscriptions

### Q3: What's the difference between BillingRecord and SubscriptionPayment?

**A:** Different purposes:
- **BillingRecord**: The invoice/bill (what user owes)
- **SubscriptionPayment**: The actual payment transaction (money received)

**Example:**
```
BillingRecord (bill_001):
  Amount: $275
  Status: Paid
  InvoiceNumber: INV-2025-001

SubscriptionPayment (pay_001):
  BillingRecordId: bill_001 (links to above)
  TransactionId: pi_DEF456 (Stripe transaction)
  Amount: $275
  PaymentDate: 2025-10-17
```

### Q4: Why do we reset privileges on renewal?

**A:** Because subscriptions are **period-based** and tied to billing cycles:

**For Monthly Billing:**
- User pays $150/month for 30 days of service
- Gets 10 consultations for that month
- Next month: New payment ($150) = Fresh 10 consultations

**For Annual Billing:**
- User pays $1,530/year for 365 days of service
- Gets 122 consultations for the ENTIRE YEAR (10 × 12.17 months, rounded)
- Next year: New payment ($1,530) = Fresh 122 consultations

**Key Points:**
- ✅ Monthly billing → Reset every 30 days
- ✅ Quarterly billing → Reset every 90 days
- ✅ Annual billing → Reset every 365 days
- ✅ Reset happens when payment succeeds, not on arbitrary dates
- ✅ Privileges scale to billing cycle: `Math.Ceiling(monthlyLimit × (billingCycleDays / 30))`

**Without reset:** User could accumulate unlimited consultations, defeating the purpose of limits.

### Q5: What happens if webhook fails?

**A:** Stripe has built-in retry mechanism:
1. Stripe sends webhook → Your system is down → No response
2. Stripe waits 5 seconds, retries
3. If still fails, waits 1 hour, retries
4. Continues retrying for up to 3 days
5. Your idempotency check prevents duplicate processing when retries succeed

---

## 🎯 Important Patterns to Remember

### Pattern 1: Always Validate Before Action

```csharp
// DON'T do this:
await _privilegeService.UsePrivilegeAsync(userId, privilegeId);

// DO this:
var availabilityCheck = await _privilegeService
    .CheckPrivilegeAvailabilityAsync(userId, privilegeId);

if (availabilityCheck.StatusCode == 200)
{
    // Has credits, proceed
    await _privilegeService.UsePrivilegeAsync(userId, privilegeId);
}
else if (availabilityCheck.StatusCode == 402)
{
    // Needs to pay for overage
    return "Payment required";
}
```

### Pattern 2: Always Use Transactions for Multi-Step Operations

```csharp
// ✅ CORRECT
await _unitOfWork.BeginTransactionAsync();
try
{
    await _repo1.CreateAsync(...);
    await _repo2.UpdateAsync(...);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}

// ❌ WRONG (no transaction)
await _repo1.CreateAsync(...);
await _repo2.UpdateAsync(...);
// If 2nd operation fails, 1st is saved (inconsistent state!)
```

### Pattern 3: Always Sync with Stripe

```csharp
// ✅ CORRECT
var plan = await _planRepo.CreateAsync(planEntity);  // Create in DB
var stripeProductId = await _stripeService.CreateProductAsync(...);  // Create in Stripe
plan.StripeProductId = stripeProductId;  // Link them
await _planRepo.UpdateAsync(plan);  // Save link

// ❌ WRONG (no Stripe sync)
var plan = await _planRepo.CreateAsync(planEntity);
// Plan exists in DB but not in Stripe → Payment will fail!
```

### Pattern 4: Always Record in History

```csharp
// After using a privilege:
await _privilegeService.UsePrivilegeAsync(...);

// AND record in history:
await _privilegeUsageHistoryRepository.CreateAsync(
    new PrivilegeUsageHistory { ... }
);

// Why? Audit trail, billing verification, analytics, compliance
```

---

## 🚦 Status Codes Quick Reference

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful operation |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Validation failed, invalid input |
| 401 | Unauthorized | No/invalid authentication token |
| 402 | Payment Required | Insufficient credits, need to pay |
| 403 | Forbidden | Authenticated but not authorized (non-admin) |
| 404 | Not Found | Resource doesn't exist |
| 500 | Server Error | Exception occurred |

---

## 📝 Glossary

| Term | Definition |
|------|------------|
| **Privilege** | A service/feature user can access (e.g., "Teleconsultation") |
| **Plan** | A package of privileges sold as a subscription |
| **Subscription** | User's active plan purchase |
| **Billing Record** | Invoice/bill for payment |
| **Overage** | Usage beyond plan limits (requires extra payment) |
| **Upfront Payment** | Paying immediately before service (for overage) |
| **Webhook** | HTTP callback from Stripe when events occur |
| **Idempotency** | Ensuring operation runs only once (prevent duplicates) |
| **Unit of Work** | Pattern for managing transactions |
| **DTO** | Data Transfer Object (for API communication) |
| **Stripe Customer** | User record in Stripe's system |
| **Stripe Product** | Plan record in Stripe's system |
| **Stripe Subscription** | Recurring payment in Stripe's system |

---

## 📞 Getting Help

### Debugging Tips

1. **Check Logs**: All services log extensively
2. **Check Stripe Dashboard**: See real-time events
3. **Check Database**: Query tables to see current state
4. **Use Postman**: Test API endpoints manually
5. **Review Webhook Logs**: See what Stripe sent

### Common Issues

| Issue | Likely Cause | Solution |
|-------|--------------|----------|
| "Plan not found" | Invalid planId | Check SubscriptionPlans table |
| "Payment failed" | Card declined | Check Stripe dashboard |
| "Insufficient credits" | Used all privileges | User needs to pay for overage |
| "Access denied" | Non-admin trying admin action | Check role authorization |
| "Duplicate subscription" | User already has active | Check existing subscriptions |

---

## ✅ You're Ready!

After reading this index and the 5 core guides, you'll have a complete understanding of:

✅ How the system works end-to-end  
✅ How services interact  
✅ How data flows through the system  
✅ How Stripe integration works  
✅ How to debug issues  
✅ How to make changes safely  

**Now proceed to Guide 01 to start your learning journey!**

---

**Next:** [01 - Subscription Plan Management Guide](./01_SUBSCRIPTION_PLAN_MANAGEMENT_GUIDE.md)

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025



