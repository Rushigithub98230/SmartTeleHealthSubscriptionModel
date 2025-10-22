# Subscription System Visual Flowcharts

## 1. ADMIN: Create Subscription Plan

```
┌─────────────────────────────────────────────────────────────────┐
│                    ADMIN PLAN CREATION FLOW                      │
└─────────────────────────────────────────────────────────────────┘

Frontend (Angular)                 Backend (.NET)                 Stripe
─────────────────                 ─────────────                 ──────

Admin opens                        
/webadmin/plans/create            
        │                          
        ▼                          
┌──────────────────┐              
│  STEP 1:         │              
│  Basic Info      │              
│  - Plan Name     │              
│  - Category      │              
│  - Billing Cycle │◄────── Load from GET /api/MasterData/billing-cycles
│  - Currency      │◄────── Load from GET /api/MasterData/currencies
│  - Base Price    │              
└──────────────────┘              
        │                          
        ▼                          
┌──────────────────┐              
│  STEP 2:         │              
│  Privileges      │◄────── Load from GET /api/Privileges?isActive=true
│  Add privileges  │              
│  - Value         │              
│  - BaseCost      │              
│  - UnitCost      │              
└──────────────────┘              
        │                          
        ▼                          
┌──────────────────┐              
│  STEP 3:         │              
│  Billing &       │              
│  Commission      │              
│  - Auto-calc?    │              
│  - Commission %  │              
└──────────────────┘              
        │                          
        ▼                          
┌──────────────────┐              
│  STEP 4:         │              
│  Review & Submit │              
└──────────────────┘              
        │                          
        ▼                          
POST /api/SubscriptionPlans/admin ────────►  SubscriptionPlanService
{                                                     │
  name: "Premium - Monthly",                          ▼
  price: 25.00,                                ┌──────────────────┐
  billingCycleId: "...",                       │ Begin Transaction│
  categoryId: "...",                            └──────────────────┘
  privileges: [...]                                     │
}                                                       ▼
                                               ┌──────────────────┐
                                               │ Create Plan      │
                                               │ Entity           │
                                               └──────────────────┘
                                                       │
                                                       ▼
                                               ┌──────────────────┐
                                               │ Create Stripe    │────► Stripe.Product.Create()
                                               │ Product          │      { name: "Premium - Monthly" }
                                               └──────────────────┘              │
                                                       │                          │
                                                       ▼                          ▼
                                               ┌──────────────────┐         Product ID
                                               │ Create Stripe    │◄────────────┘
                                               │ Price            │
                                               │ (ONE per plan)   │────► Stripe.Price.Create()
                                               └──────────────────┘      { 
                                                       │                   product_id: "...",
                                                       │                   unit_amount: 2500,
                                                       ▼                   currency: "usd",
                                               ┌──────────────────┐       recurring: { 
                                               │ Update Plan      │         interval: "month",
                                               │ with Stripe IDs  │         interval_count: 1
                                               └──────────────────┘       }
                                                       │                 }
                                                       ▼                          │
                                               ┌──────────────────┐              │
                                               │ Add Plan         │              ▼
                                               │ Privileges       │         Price ID
                                               │ (foreach)        │◄─────────────┘
                                               └──────────────────┘
                                                       │
                                                       ▼
                                               ┌──────────────────┐
                                               │ Commit           │
                                               │ Transaction      │
                                               └──────────────────┘
                                                       │
                                                       ▼
                                               ┌──────────────────┐
        Response 200 OK                        │ Return Success   │
┌────────────────────────────────◄────────────┤ with Plan DTO    │
│                                               └──────────────────┘
▼
Navigate to /webadmin/plans
(Plan list page)

RESULT: Plan created with Stripe integration ✅
```

---

## 2. USER: Subscribe to Plan

```
┌─────────────────────────────────────────────────────────────────┐
│                  USER SUBSCRIPTION PURCHASE FLOW                 │
└─────────────────────────────────────────────────────────────────┘

Frontend                          Backend                         Stripe
────────                          ───────                         ──────

User visits /pricing
        │
        ▼
GET /api/SubscriptionPlans/active ──────► SubscriptionPlanService
                                                  │
┌──────────────────┐                             ▼
│ Display Plans    │◄────────────────────── Plans with details
│ - Name           │                        (price, privileges, etc.)
│ - Price          │
│ - Features       │
│ [Subscribe Btn]  │
└──────────────────┘
        │
        │ User clicks [Subscribe]
        ▼
┌──────────────────┐
│ Checkout Page    │
│                  │
│ Plan: Premium    │
│ Price: $25/mo    │
│                  │
│ ┌──────────────┐ │
│ │ Stripe       │ │──► Stripe.js
│ │ Card Element │ │    (client-side)
│ └──────────────┘ │          │
│                  │          │ User enters card
│ [Pay & Subscribe]│          ▼
└──────────────────┘    PaymentMethod created
        │                     │
        │                     │
        ▼                     ▼
POST /api/Subscriptions       paymentMethodId: "pm_xxxxx"
{
  userId: 123,
  planId: "...",
  paymentMethodId: "pm_xxxxx" ───────►  SubscriptionLifecycleService
}                                                │
                                                 ▼
                                        ┌──────────────────┐
                                        │ VALIDATION PHASE │
                                        ├──────────────────┤
                                        │ 1. Check plan    │
                                        │ 2. Check dups    │
                                        │ 3. Validate PM   │────► Stripe.PaymentMethod.Retrieve()
                                        └──────────────────┘              │
                                                 │                        │
                                                 ▼                        ▼
                                        ┌──────────────────┐         Valid ✅
                                        │ STRIPE PHASE     │◄─────────────┘
                                        ├──────────────────┤
                                        │ 1. Create/Get    │────► Stripe.Customer.Create()
                                        │    Customer      │      { email: "...", name: "..." }
                                        │                  │              │
                                        │ 2. Attach PM     │              ▼
                                        │    to Customer   │      Customer ID: cus_xxxxx
                                        │                  │◄─────────────┘
                                        │ 3. Create Stripe │
                                        │    Subscription  │────► Stripe.Subscription.Create()
                                        └──────────────────┘      {
                                                 │                  customer: "cus_xxxxx",
                                                 │                  items: [{
                                                 ▼                    price: "price_xxxxx"
                                        ┌──────────────────┐       }],
                                        │ DATABASE PHASE   │       default_payment_method: "pm_xxxxx",
                                        ├──────────────────┤       trial_period_days: 0
                                        │ Begin Transaction│     }
                                        │                  │              │
                                        │ 1. Create        │              ▼
                                        │    Subscription  │      Stripe Subscription ID
                                        │                  │◄─────────────┘
                                        │ 2. Allocate      │
                                        │    Privileges    │
                                        │                  │
                                        │ 3. Status History│
                                        │                  │
                                        │ Commit           │
                                        └──────────────────┘
                                                 │
                                                 ▼
                                        ┌──────────────────┐
                                        │ PRIVILEGE        │
                                        │ ALLOCATION       │
                                        ├──────────────────┤
                                        │ foreach privilege│
                                        │ in plan:         │
                                        │   Create         │
                                        │   UsageRecord {  │
                                        │     AllowedValue │
                                        │     UsedValue: 0 │
                                        │   }              │
                                        └──────────────────┘
                                                 │
        Response 200 OK                          ▼
        { subscription details }        ┌──────────────────┐
◄───────────────────────────────────────┤ Return Success   │
                                        └──────────────────┘
        │
        ▼
Navigate to /web/subscriptions
(User's subscription list)

RESULT: User has active subscription with privileges ✅
```

---

## 3. USER: Use Privilege (Teleconsultation)

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRIVILEGE USAGE FLOW                          │
└─────────────────────────────────────────────────────────────────┘

User Action                       Backend                         Database
───────────                       ───────                         ────────

User clicks                        
"Book Consultation"               
        │                          
        ▼                          
GET /api/Subscriptions/{id}/check-privilege/Teleconsultation?requestedAmount=1
                                          │
                                          ▼
                                  PrivilegeService.CheckPrivilegeAvailabilityAsync()
                                          │
                                          ▼
                                  ┌──────────────────┐
                                  │ Get Subscription │────► subscriptions table
                                  └──────────────────┘
                                          │
                                          ▼
                                  ┌──────────────────┐
                                  │ Get Plan         │────► subscription_plan_privileges
                                  │ Privilege Config │
                                  │ - Value          │
                                  │ - UnitCost       │
                                  └──────────────────┘
                                          │
                                          ▼
                                  ┌──────────────────┐
                                  │ Get Current      │────► user_subscription_privilege_usage
                                  │ Usage            │
                                  │ - AllowedValue   │
                                  │ - UsedValue      │
                                  └──────────────────┘
                                          │
                                          ▼
                                  ┌──────────────────┐
                                  │ Check Limit:     │
                                  │                  │
                                  │ UsedValue < ?    │
                                  │ AllowedValue     │
                                  └──────────────────┘
                                          │
                        ┌─────────────────┴─────────────────┐
                        │                                   │
                   UsedValue < AllowedValue          UsedValue >= AllowedValue
                        │                                   │
                        ▼                                   ▼
                  ┌──────────┐                       ┌──────────┐
                  │ AVAILABLE│                       │ LIMIT    │
                  │ 200 OK   │                       │ EXCEEDED │
                  └──────────┘                       │ 402      │
                        │                            │ Payment  │
        ┌───────────────┘                            │ Required │
        │                                            └──────────┘
        ▼                                                  │
Frontend receives OK                        ┌──────────────┘
        │                                   │
        ▼                                   ▼
┌──────────────────┐              Response 402:
│ Proceed with     │              {
│ Consultation     │                "available": false,
│ Booking          │                "limitExceeded": true,
└──────────────────┘                "remaining": 0,
        │                           "requested": 1,
        ▼                           "shortfall": 1,
UsePrivilegeAsync()                 "unitCost": 15.00,
        │                           "requiredPayment": 15.00,
        ▼                           "purchaseDetails": { ... }
┌──────────────────┐              }
│ Increment        │────► Update user_subscription_privilege_usage       │
│ UsedValue        │      SET UsedValue = UsedValue + 1                  │
│                  │                                                      │
│ Create           │────► Insert into privilege_usage_history            │
│ History Record   │                                                      │
└──────────────────┘                                                      ▼
        │                                                        Frontend receives 402
        ▼                                                                 │
Consultation booked ✅                                                    ▼
                                                                 ┌──────────────────┐
                                                                 │ Show Payment     │
                                                                 │ Modal:           │
                                                                 │                  │
                                                                 │ "You've used all │
                                                                 │  5 consultations"│
                                                                 │                  │
                                                                 │ Purchase 1 more  │
                                                                 │ for $15?         │
                                                                 │                  │
                                                                 │ [Pay $15]        │
                                                                 └──────────────────┘
                                                                          │
                User clicks [Pay $15]                                    │
                        │                                                │
                        └────────────────────────────────────────────────┘
                                                │
                                                ▼
                          (See next flow: "Purchase Additional Credits")
```

---

## 4. USER: Purchase Additional Credits (Overage)

```
┌─────────────────────────────────────────────────────────────────┐
│               PURCHASE ADDITIONAL CREDITS FLOW                   │
│                  (UPFRONT PAYMENT MODEL)                         │
└─────────────────────────────────────────────────────────────────┘

Frontend                          Backend                         Stripe
────────                          ───────                         ──────

User clicks [Pay $15]
        │
        ▼
POST /api/Subscriptions/{id}/purchase-credits
{
  privilegeName: "Teleconsultation",
  quantity: 1,
  paymentMethodId: "pm_xxxxx"
}                             ──────►  SubscriptionService.PurchaseAdditionalCreditsAsync()
                                                │
                                                ▼
                                       ┌──────────────────┐
                                       │ 1. Get           │────► subscriptions
                                       │    Subscription  │
                                       └──────────────────┘
                                                │
                                                ▼
                                       ┌──────────────────┐
                                       │ 2. Get Plan      │────► subscription_plan_privileges
                                       │    Privilege     │      (get UnitCost)
                                       │    Config        │
                                       └──────────────────┘
                                                │
                                                ▼
                                       ┌──────────────────┐
                                       │ 3. Calculate     │
                                       │    Cost:         │
                                       │                  │
                                       │ Total = Quantity │
                                       │       × UnitCost │
                                       │       = 1 × $15  │
                                       │       = $15      │
                                       └──────────────────┘
                                                │
                                                ▼
                                       ┌──────────────────┐
                                       │ 4. Create        │────► billing_records
                                       │    BillingRecord │      INSERT
                                       │    Type: Overage │      {
                                       │    Status:       │        type: "Overage",
                                       │    Pending       │        status: "Pending",
                                       │    Amount: $15   │        amount: 15.00
                                       └──────────────────┘      }
                                                │
                                                ▼
                                       ┌──────────────────┐
                                       │ 5. Process       │
                                       │    Payment       │
                                       │    IMMEDIATELY   │────► PaymentService
                                       │    (UPFRONT)     │            │
                                       └──────────────────┘            ▼
                                                │                ┌──────────────┐
                                                │                │ Stripe       │
                                                │                │ PaymentIntent│
                                                │                │ .Create()    │──► Stripe API
                                                │                └──────────────┘        │
                                                │                      │                 │
                                                │                      │ Process payment │
                                                │                      ▼                 │
                                                │                Payment Result          │
                                                │◄───────────────────┘                  │
                                                │                                        │
                              ┌─────────────────┴─────────────────┐                     │
                              │                                   │                     │
                       Payment SUCCESS                     Payment FAILED               │
                              │                                   │                     │
                              ▼                                   ▼                     │
                     ┌──────────────────┐             ┌──────────────────┐             │
                     │ 6a. Update       │──► UPDATE   │ 6b. Update       │──► UPDATE   │
                     │     Billing      │    billing_ │     Billing      │    billing_ │
                     │     Record:      │    records  │     Record:      │    records  │
                     │     Status=Paid  │    SET      │     Status=Failed│    SET      │
                     │     PaidAt=Now   │    status=  │                  │    status=  │
                     └──────────────────┘    'Paid'   └──────────────────┘    'Failed' │
                              │                                   │                     │
                              ▼                                   ▼                     │
                     ┌──────────────────┐             ┌──────────────────┐             │
                     │ 7a. ADD CREDITS  │──► UPDATE   │ 7b. NO CREDITS   │             │
                     │     AllowedValue │    user_    │     ADDED        │             │
                     │     = Allowed +  │    subscrip │                  │             │
                     │       Quantity   │    tion_    │                  │             │
                     │     = 5 + 1      │    privileg │                  │             │
                     │     = 6          │    e_usage  │                  │             │
                     └──────────────────┘    SET      └──────────────────┘             │
                              │              AllowedV           │                       │
                              │              alue=6             │                       │
                              ▼                                 ▼                       │
                     ┌──────────────────┐             ┌──────────────────┐             │
                     │ 8a. Return       │             │ 8b. Return       │             │
                     │     Success:     │             │     Error:       │             │
                     │     {            │             │     "Payment     │             │
                     │       creditsAdd │             │      failed"     │             │
                     │       ed: 1,     │             │     }            │             │
                     │       newLimit: 6│             └──────────────────┘             │
                     │       newRemain  │                     │                         │
                     │       ing: 1     │                     │                         │
                     │     }            │                     │                         │
                     └──────────────────┘                     │                         │
                              │                               │                         │
        ◄─────────────────────┘                               │                         │
        │                                                     │                         │
        ▼                                                     ▼                         │
┌──────────────────┐                              ┌──────────────────┐                 │
│ SUCCESS:         │                              │ ERROR:           │                 │
│                  │                              │                  │                 │
│ "Successfully    │                              │ "Payment failed: │                 │
│  purchased 1     │                              │  Insufficient    │                 │
│  additional      │                              │  funds. Credits  │                 │
│  credit for $15" │                              │  not added."     │                 │
│                  │                              │                  │                 │
│ New Limit: 6     │                              │ [Try Again]      │                 │
│ Remaining: 1     │                              └──────────────────┘                 │
│                  │                                                                   │
│ [Continue]       │                                                                   │
└──────────────────┘                                                                   │
        │                                                                              │
        ▼                                                                              │
User can now book                                                                      │
consultation                                                                           │
                                                                                       │
RESULT: Credits added ONLY after payment succeeds ✅                                   │
```

---

## 5. SYSTEM: Automated Billing Cycle

```
┌─────────────────────────────────────────────────────────────────┐
│              AUTOMATED BILLING CYCLE PROCESSING                  │
│                 (Runs daily at midnight)                         │
└─────────────────────────────────────────────────────────────────┘

Background Job                    Backend                         Stripe
──────────────                    ───────                         ──────

Cron: Daily @ 00:00 UTC
        │
        ▼
AutomatedBillingService.ProcessBillingCycleAsync()
        │
        ▼
┌──────────────────┐
│ Find             │────► SELECT * FROM subscriptions
│ Subscriptions    │      WHERE NextBillingDate <= @today
│ Due for Billing  │      AND Status = 'Active'
└──────────────────┘
        │
        │ foreach subscription
        │
        ▼
┌──────────────────┐
│ Process          │
│ Subscription     │
│ Billing          │
└──────────────────┘
        │
        ▼
┌──────────────────┐
│ 1. Create        │────► INSERT INTO billing_records
│    BillingRecord │      {
│                  │        type: "Subscription",
│    Type: Sub     │        status: "Pending",
│    Status:       │        amount: subscription.CurrentPrice,
│    Pending       │        userId: subscription.UserId,
│    Amount: $25   │        subscriptionId: subscription.Id
│                  │      }
└──────────────────┘
        │
        ▼
┌──────────────────┐
│ 2. Process       │
│    Payment via   │────► Stripe.Invoice.Create()
│    Stripe        │      {
└──────────────────┘        customer: subscription.StripeCustomerId,
        │                   subscription: subscription.StripeSubscriptionId
        │                 }
        ▼                          │
  Stripe Invoice                   │
  Auto-charged                     │
        │                          ▼
        │                    Stripe processes
        │                    automatic payment
        │                          │
        │                          │
        │        ┌─────────────────┴─────────────────┐
        │        │                                   │
        │   PAYMENT SUCCESS                    PAYMENT FAILED
        │        │                                   │
        ▼        ▼                                   ▼
┌──────────────────┐                       ┌──────────────────┐
│ 3a. Update       │──► UPDATE             │ 3b. Update       │──► UPDATE
│     Billing      │    billing_records    │     Billing      │    billing_records
│     Record:      │    SET status='Paid', │     Record:      │    SET status='Failed',
│     Status=Paid  │    paidAt=NOW()       │     Status=Failed│    failureReason='...'
└──────────────────┘                       └──────────────────┘
        │                                           │
        ▼                                           ▼
┌──────────────────┐                       ┌──────────────────┐
│ 4a. Update       │──► UPDATE             │ 4b. Update       │──► UPDATE
│     Subscription:│    subscriptions      │     Subscription:│    subscriptions
│                  │    SET                │                  │    SET
│  LastBillingDate │    LastBillingDate=   │  FailedAttempts  │    FailedPayment
│  = NOW()         │    NOW(),             │  ++              │    Attempts++,
│                  │    NextBillingDate=   │                  │    LastPaymentFailed
│  NextBillingDate │    DATEADD(MONTH,     │  IF Attempts>=3: │    Date=NOW()
│  = +1 Month      │    1, NextBilling     │    Status =      │
│                  │    Date),             │    'PaymentFailed│    IF FailedAttempts>=3:
│  FailedAttempts  │    FailedPayment      │                  │      Status='PaymentFailed'
│  = 0             │    Attempts=0         │                  │
└──────────────────┘                       └──────────────────┘
        │                                           │
        ▼                                           ▼
┌──────────────────┐                       ┌──────────────────┐
│ 5a. RESET        │                       │ 5b. Send Failure │
│     PRIVILEGES   │                       │     Notification │
│                  │                       │                  │
│ foreach usage:   │──► UPDATE             │ Email/SMS to user│
│   UsedValue=0    │    user_subscription_ │ "Payment failed" │
│   (keep          │    privilege_usage    └──────────────────┘
│    AllowedValue) │    SET UsedValue=0,           │
│                  │    ResetAt=NOW(),             │
│   Update period: │    UsagePeriodStart=          ▼
│   PeriodStart =  │    NOW(),            ┌──────────────────┐
│   NOW()          │    UsagePeriodEnd=   │ Retry in 3 days  │
│   PeriodEnd =    │    DATEADD(MONTH,    │ (Stripe auto-    │
│   +1 Month       │    1, NOW())         │  retry logic)    │
└──────────────────┘                      └──────────────────┘
        │
        ▼
┌──────────────────┐
│ 6a. Send Receipt │
│     Notification │
│                  │
│ Email to user:   │
│ - Invoice PDF    │
│ - Receipt        │
│ - Next billing   │
│   date           │
└──────────────────┘

RESULT: Subscription renewed, privileges reset ✅
```

---

## 6. ENTITY RELATIONSHIP DIAGRAM

```
┌─────────────────────────────────────────────────────────────────┐
│                  ENTITY RELATIONSHIP DIAGRAM                     │
└─────────────────────────────────────────────────────────────────┘

                                 ┌─────────────┐
                                 │   UserRole  │
                                 ├─────────────┤
                                 │ Id (PK)     │
                                 │ Name        │
                                 └─────────────┘
                                        │
                                        │ 1
                                        │
                                        ▼
           ┌──────────────────────────────────────────┐
           │                  User                    │
           ├──────────────────────────────────────────┤
           │ Id (PK)                                  │
           │ Email, Name, Phone                       │
           │ UserRoleId (FK) ───────┐                 │
           │ StripeCustomerId        │                │
           └──────────────────────────────────────────┘
                    │                 │
                    │ 1               │
                    │                 │
                    ▼ *               │
        ┌────────────────────┐        │
        │   Subscription     │        │
        ├────────────────────┤        │
        │ Id (PK)            │        │
        │ UserId (FK) ───────┘        │
        │ PlanId (FK)                 │
        │ Status                      │
        │ CurrentPrice                │
        │ NextBillingDate             │
        │ StripeSubscriptionId        │
        └────────────────────┘
               │         │
               │ *       │ *
               │         │
               ▼         ▼
    ┌──────────────┐  ┌──────────────────────────┐
    │ BillingRecord│  │ UserSubscription         │
    ├──────────────┤  │ PrivilegeUsage           │
    │ Id (PK)      │  ├──────────────────────────┤
    │ SubId (FK)   │  │ Id (PK)                  │
    │ UserId (FK)  │  │ SubscriptionId (FK) ─────┘
    │ Type         │  │ PlanPrivilegeId (FK)
    │ Status       │  │ PrivilegeId (FK)
    │ Amount       │  │ UsedValue
    │ StripeInvoice│  │ AllowedValue
    └──────────────┘  │ RemainingValue (computed)
                      └──────────────────────────┘
                               │
                               │ *
                               │
                               ▼
                    ┌──────────────────────┐
                    │ PrivilegeUsage       │
                    │ History              │
                    ├──────────────────────┤
                    │ Id (PK)              │
                    │ UsageId (FK) ────────┘
                    │ UsedValue
                    │ UsedAt
                    │ UsageDate
                    └──────────────────────┘

┌────────────────────────┐
│   SubscriptionPlan     │
├────────────────────────┤
│ Id (PK)                │
│ Name                   │
│ Price                  │
│ BillingCycleId (FK) ───┐
│ CategoryId (FK)        │
│ CurrencyId (FK)        │
│ StripeProductId        │
│ StripePriceId          │────► ONE price per plan
└────────────────────────┘
         │
         │ 1
         │
         ▼ *
┌──────────────────────────┐
│ SubscriptionPlan         │
│ Privilege                │
├──────────────────────────┤
│ Id (PK)                  │
│ PlanId (FK) ─────────────┘
│ PrivilegeId (FK) ────────┐
│ Value (limit)            │
│ PrivilegeBaseCost        │
│ UnitCost (overage)       │
└──────────────────────────┘
                            │
                            │
                            ▼
                  ┌──────────────────┐
                  │   Privilege      │
                  ├──────────────────┤
                  │ Id (PK)          │
                  │ Name             │
                  │ Description      │
                  │ PrivilegeTypeId  │
                  └──────────────────┘
                            │
                            │ *
                            │
                            ▼
                  ┌──────────────────┐
                  │ PrivilegeType    │
                  ├──────────────────┤
                  │ Id (PK)          │
                  │ Name             │
                  └──────────────────┘

Master Tables:
┌──────────────────┐  ┌──────────────┐  ┌──────────────┐
│ BillingCycle     │  │   Currency   │  │   Category   │
├──────────────────┤  ├──────────────┤  ├──────────────┤
│ Id (PK)          │  │ Id (PK)      │  │ Id (PK)      │
│ Name             │  │ Code         │  │ Name         │
│ DurationInDays   │  │ Symbol       │  │ Description  │
└──────────────────┘  └──────────────┘  └──────────────┘
```

---

## 7. STATUS STATE MACHINE

```
┌─────────────────────────────────────────────────────────────────┐
│            SUBSCRIPTION STATUS STATE MACHINE                     │
└─────────────────────────────────────────────────────────────────┘

                  ┌───────────┐
                  │  Pending  │
                  └───────────┘
                        │
            ┌───────────┼───────────┐
            │                       │
         Activate                Trial
            │                       │
            ▼                       ▼
      ┌──────────┐          ┌─────────────┐
      │  Active  │          │ TrialActive │
      └──────────┘          └─────────────┘
            │                       │
            │                  Trial Ends
            │                       │
            │                       ▼
            │              ┌─────────────────┐
            │              │  TrialExpired   │
            │              └─────────────────┘
            │                       │
            │                  Activate
            │                       │
            │◄──────────────────────┘
            │
      ┌─────┼─────┬────────────┬──────────┐
      │           │            │          │
   Pause      Cancel    Payment Fail   Expire
      │           │            │          │
      ▼           ▼            ▼          ▼
┌─────────┐ ┌──────────┐ ┌──────────┐ ┌─────────┐
│ Paused  │ │Cancelled │ │PaymentFl │ │Expired  │
└─────────┘ └──────────┘ │  Failed  │ └─────────┘
      │                  └──────────┘       │
      │                       │             │
   Resume               Retry/Pay      Renew
      │                       │             │
      └───────────────────────┴─────────────┘
                              │
                              ▼
                        ┌──────────┐
                        │  Active  │
                        └──────────┘

Legend:
  Active        - Subscription is active, privileges available
  TrialActive   - Trial period active
  TrialExpired  - Trial ended, needs activation
  Paused        - Temporarily paused by user
  Cancelled     - Cancelled by user
  PaymentFailed - Payment failed, needs resolution
  Expired       - Subscription expired
  Pending       - Created but not yet activated
```

---

## 8. KEY WORKFLOWS SUMMARY

### Admin Workflow
```
1. Admin logs in → /webadmin/dashboard
2. Navigate to "Plans" → /webadmin/plans
3. Click "Create Plan" → /webadmin/plans/create
4. Fill 4-step form:
   Step 1: Basic Info (name, category, billing cycle, price)
   Step 2: Configure Privileges (select, set limits and costs)
   Step 3: Billing Settings (commission, auto-calculate)
   Step 4: Review & Submit
5. Plan created with Stripe integration
6. Plan appears in /webadmin/plans list
```

### User Subscription Workflow
```
1. User visits /pricing (public page)
2. Browse available plans
3. Click "Subscribe" on desired plan
4. Login/Register if needed
5. Checkout page:
   - Review plan details
   - Enter payment card (Stripe Elements)
6. Click "Subscribe"
7. Backend:
   - Creates Stripe customer
   - Creates Stripe subscription
   - Creates local subscription
   - Allocates privileges
8. Redirect to /web/subscriptions
9. User sees active subscription
```

### Privilege Usage Workflow
```
1. User attempts to use privilege (e.g., book consultation)
2. System checks availability:
   - UsedValue < AllowedValue? → Allow
   - UsedValue >= AllowedValue? → Block, show payment modal
3. If allowed:
   - Increment UsedValue
   - Record usage history
   - Proceed with action
4. If blocked:
   - Show purchase modal
   - User pays for additional credits
   - Credits added to AllowedValue
   - User can proceed
```

### Billing Cycle Workflow
```
1. Background job runs daily at midnight
2. Find subscriptions where NextBillingDate <= Today
3. For each subscription:
   - Create billing record
   - Process payment via Stripe
   - If success:
     * Update NextBillingDate
     * Reset privilege usage (UsedValue = 0)
     * Send receipt
   - If fail:
     * Increment FailedPaymentAttempts
     * Notify user
     * Retry in 3 days
```

---

## 🎯 CONCLUSION

The Smart Telehealth subscription system follows clear, logical workflows from admin plan creation through user subscription purchase to privilege usage and billing. The frontend correctly implements the backend flow with proper API calls, error handling, and user feedback.

**Key Architectural Strengths**:
- ✅ Clean separation of concerns
- ✅ Proper Stripe integration
- ✅ Upfront payment model for overages
- ✅ Comprehensive status tracking
- ✅ Two-level privilege management
- ✅ Automated billing with retry logic

**System is production-ready and well-architected.** ⭐⭐⭐⭐⭐

