# SmartTeleHealth Subscription Management Architecture Diagrams

## 1. Entity Relationship Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│      User       │    │  Subscription   │    │ SubscriptionPlan│
│                 │    │                 │    │                 │
│ • Id (int)      │◄───┤ • UserId        │◄───┤ • Id (Guid)     │
│ • FirstName     │    │ • PlanId        │    │ • Name          │
│ • LastName      │    │ • Status        │    │ • Price         │
│ • Email         │    │ • StartDate     │    │ • BillingCycleId│
│ • StripeCustId  │    │ • NextBilling   │    │ • CurrencyId    │
│ • UserRoleId    │    │ • StripeSubId   │    │ • CategoryId    │
└─────────────────┘    │ • ProviderId    │    │ • IsFeatured    │
         │              │ • CurrentPrice  │    │ • IsTrialAllowed│
         │              └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ BillingRecord   │    │PrivilegeUsage   │    │PlanPrivilege    │
│                 │    │                 │    │                 │
│ • UserId        │    │ • SubscriptionId│    │ • PlanId        │
│ • SubscriptionId│    │ • PrivilegeId   │    │ • PrivilegeId   │
│ • Status        │    │ • UsedValue     │    │ • Value         │
│ • Amount        │    │ • AllowedValue  │    │ • UsagePeriodId │
│ • BillingDate   │    │ • PeriodStart   │    │ • DailyLimit    │
│ • PaidAt        │    │ • PeriodEnd     │    │ • WeeklyLimit   │
│ • StripeIntentId│    │ • LastUsedAt    │    │ • MonthlyLimit  │
└─────────────────┘    └─────────────────┘    │ • UnitCost      │
         │                       │              └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Privilege     │    │ MasterBilling   │    │ MasterCurrency  │
│                 │    │     Cycle       │    │                 │
│ • Id (Guid)     │    │ • Id (Guid)     │    │ • Id (Guid)     │
│ • Name          │    │ • Name          │    │ • Code (USD)    │
│ • Description   │    │ • DurationDays  │    │ • Name          │
│ • PrivilegeTypeId│   │ • SortOrder     │    │ • Symbol ($)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 2. Service Layer Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Controllers                          │
├─────────────────┬─────────────────┬─────────────────────────────┤
│SubscriptionPlans│ Subscriptions   │      Billing                │
│Controller       │ Controller      │     Controller              │
└─────────────────┴─────────────────┴─────────────────────────────┘
         │                 │                       │
         ▼                 ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Service Layer                              │
├─────────────────┬─────────────────┬─────────────────────────────┤
│SubscriptionPlan │  Subscription   │     BillingService          │
│Service          │  Service        │                             │
├─────────────────┼─────────────────┼─────────────────────────────┤
│AutomatedBilling │   StripeService │   SubscriptionLifecycle     │
│Service          │                 │   Service                   │
└─────────────────┴─────────────────┴─────────────────────────────┘
         │                 │                       │
         ▼                 ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Repository Layer                            │
├─────────────────┬─────────────────┬─────────────────────────────┤
│SubscriptionPlan │  Subscription   │     BillingRepository       │
│Repository       │  Repository     │                             │
├─────────────────┼─────────────────┼─────────────────────────────┤
│PrivilegeRepo    │ PrivilegeUsage  │   UserRepository            │
│                 │ Repository      │                             │
└─────────────────┴─────────────────┴─────────────────────────────┘
         │                 │                       │
         ▼                 ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Database Layer                              │
│              Entity Framework Core + SQL Server                 │
└─────────────────────────────────────────────────────────────────┘
```

## 3. Subscription Lifecycle Flow

```
┌─────────────┐
│User Selects │
│    Plan     │
└──────┬──────┘
       │
       ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Validate  │───▶│Check Existing│───▶│Create Stripe│
│ Plan & User │    │Subscriptions │    │  Customer   │
└─────────────┘    └─────────────┘    └──────┬──────┘
                                              │
                                              ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│Send Welcome │◄───│Initialize   │◄───│Create Local │
│Notification │    │Privileges   │    │Subscription │
└─────────────┘    └─────────────┘    └──────┬──────┘
                                              │
                                              ▼
                                    ┌─────────────┐
                                    │Create Stripe│
                                    │Subscription │
                                    └─────────────┘
```

## 4. Billing Processing Flow

```
┌─────────────────┐
│Scheduled Billing│
│     Job         │
└────────┬────────┘
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│Get Subscriptions│───▶│Create Billing   │───▶│Process Stripe   │
│   Due for       │    │    Record       │    │   Payment       │
│   Billing       │    └─────────────────┘    └────────┬────────┘
└─────────────────┘                                   │
                                                      ▼
                                            ┌─────────────────┐
                                            │ Payment Success │
                                            │      ?          │
                                            └────────┬────────┘
                                                     │
                                    ┌────────────────┴────────────────┐
                                    │                                  │
                                    ▼                                  ▼
                        ┌─────────────────┐                ┌─────────────────┐
                        │Update Subscription│              │Mark as Failed   │
                        │    Status        │              │                 │
                        └────────┬─────────┘              └────────┬────────┘
                                 │                                 │
                                 ▼                                 ▼
                        ┌─────────────────┐                ┌─────────────────┐
                        │Calculate Next   │                │Schedule Retry   │
                        │Billing Date     │                │                 │
                        └────────┬─────────┘                └────────┬────────┘
                                 │                                 │
                                 ▼                                 ▼
                        ┌─────────────────┐                ┌─────────────────┐
                        │Send Success     │                │Send Failure     │
                        │Notification     │                │Notification     │
                        └─────────────────┘                └─────────────────┘
```

## 5. Privilege Usage Flow

```
┌─────────────────┐
│User Requests    │
│   Feature       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│Check Subscription│───▶│Validate Privilege│───▶│Has Remaining   │
│    Status       │    │    Access       │    │   Usage?        │
└─────────────────┘    └─────────────────┘    └────────┬────────┘
                                                        │
                                        ┌───────────────┴───────────────┐
                                        │                               │
                                        ▼                               ▼
                                ┌─────────────┐                ┌─────────────┐
                                │Allow Access │                │Check Overage│
                                │             │                │  Charges    │
                                └──────┬──────┘                └──────┬──────┘
                                       │                              │
                                       ▼                              ▼
                                ┌─────────────┐                ┌─────────────┐
                                │Increment    │                │Has Overage? │
                                │Usage Counter│                │             │
                                └──────┬──────┘                └──────┬──────┘
                                       │                              │
                                       ▼                              ▼
                                ┌─────────────┐                ┌─────────────┐
                                │Log Usage    │                │Charge       │
                                │History      │                │Overage Fee  │
                                └─────────────┘                └──────┬──────┘
                                                                      │
                                                                      ▼
                                                              ┌─────────────┐
                                                              │Allow Access │
                                                              │             │
                                                              └──────┬──────┘
                                                                     │
                                                                     ▼
                                                              ┌─────────────┐
                                                              │Log Usage    │
                                                              │History      │
                                                              └─────────────┘
```

## 6. Stripe Integration Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    SmartTeleHealth Backend                      │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │Subscription │  │   Billing   │  │   Stripe    │            │
│  │   Service   │  │   Service   │  │   Service   │            │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘            │
│         │                │                │                   │
│         ▼                ▼                ▼                   │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │              StripeWebhookController                    │  │
│  │                                                         │  │
│  │  • customer.subscription.created                        │  │
│  │  • customer.subscription.updated                        │  │
│  │  • invoice.payment_succeeded                            │  │
│  │  • invoice.payment_failed                               │  │
│  │  • customer.subscription.deleted                        │  │
│  └─────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
         │                │                │
         ▼                ▼                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Stripe API                                 │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │  Customers  │  │Subscriptions│  │  Payments   │            │
│  │             │  │             │  │             │            │
│  │ • Create    │  │ • Create    │  │ • Process   │            │
│  │ • Update    │  │ • Update    │  │ • Refund    │            │
│  │ • Retrieve  │  │ • Cancel    │  │ • Verify    │            │
│  │ • Delete    │  │ • Pause     │  │ • Retry     │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

## 7. Data Flow Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Backend API   │    │   Stripe API    │
│                 │    │                 │    │                 │
│ • Plan Selection│───▶│ • Validation    │    │ • Customer Mgmt │
│ • Subscription  │    │ • Business Logic│    │ • Payment Proc  │
│ • Billing       │    │ • Data Access   │    │ • Webhook Events│
│ • Usage Tracking│    │ • Integration   │    │ • Product Mgmt  │
└─────────────────┘    └────────┬────────┘    └─────────────────┘
                                │
                                ▼
                    ┌─────────────────┐
                    │   Database      │
                    │                 │
                    │ • User Data     │
                    │ • Subscriptions │
                    │ • Billing Recs  │
                    │ • Usage Data    │
                    │ • Audit Logs    │
                    └─────────────────┘
```

## 8. Security Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Security Layers                              │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │Authentication│  │Authorization│  │Input        │            │
│  │               │  │             │  │Validation   │            │
│  │ • JWT Tokens │  │ • Role-Based│  │ • Data      │            │
│  │ • OAuth 2.0  │  │ • Claims    │  │   Sanitization│         │
│  │ • Refresh    │  │ • Policies  │  │ • Schema    │            │
│  │   Tokens     │  │ • Permissions│  │   Validation│           │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │Rate Limiting│  │Audit Logging│  │Data         │            │
│  │             │  │             │  │Encryption   │            │
│  │ • Per User  │  │ • All Actions│  │ • At Rest   │            │
│  │ • Per IP    │  │ • Compliance │  │ • In Transit│            │
│  │ • Per Endpoint│ │ • Monitoring │  │ • Sensitive │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

## 9. Error Handling Flow

```
┌─────────────────┐
│   Error Occurs  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Error Type?   │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌─────────┐ ┌─────────┐
│Retryable│ │Fatal    │
│Error    │ │Error    │
└────┬────┘ └────┬────┘
     │           │
     ▼           ▼
┌─────────┐ ┌─────────┐
│Retry    │ │Log &    │
│Logic    │ │Notify   │
│         │ │         │
│ • Max   │ │ • Admin │
│   Attempts│ │ • User │
│ • Backoff│ │ • System│
│ • Delay │ │ • Support│
└────┬────┘ └─────────┘
     │
     ▼
┌─────────┐
│Success? │
└────┬────┘
     │
    No▼
┌─────────┐
│Escalate │
│to Admin │
└─────────┘
```

## 10. Monitoring and Analytics Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Monitoring Stack                             │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │Application  │  │Infrastructure│  │Business     │            │
│  │Monitoring   │  │Monitoring   │  │Analytics    │            │
│  │             │  │             │  │             │            │
│  │ • Performance│  │ • CPU/Memory│  │ • Revenue   │            │
│  │ • Errors    │  │ • Disk I/O  │  │ • Subscriptions│         │
│  │ • Requests  │  │ • Network   │  │ • Usage     │            │
│  │ • Response  │  │ • Database  │  │ • Churn     │            │
│  │   Times     │  │   Metrics   │  │ • Growth    │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │Logging      │  │Alerting     │  │Reporting    │            │
│  │             │  │             │  │             │            │
│  │ • Structured│  │ • Threshold │  │ • Scheduled │            │
│  │ • Centralized│  │ • Anomaly  │  │ • Ad-hoc    │            │
│  │ • Searchable│  │ • Escalation│  │ • Export    │            │
│  │ • Retention │  │ • Notification│ │ • Dashboards│            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

This comprehensive architecture shows how all components work together to provide a robust, scalable, and secure subscription management system for the SmartTeleHealth platform.
