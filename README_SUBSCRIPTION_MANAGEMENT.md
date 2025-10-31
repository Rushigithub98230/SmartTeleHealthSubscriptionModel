# SmartTelehealth Subscription Management System

## 📖 Welcome

This is your **complete guide** to the SmartTelehealth Subscription Management System. This enterprise-grade system handles subscription plans, privilege-based billing, automated renewals, plan versioning, and full Stripe integration.

---

## 🚀 Quick Start

### For New Developers
**👋 New here?** Start with the **[Quick Reference Guide](QUICK_REFERENCE_GUIDE.md)** (15 min read)

### For System Understanding
**🔍 Need deep knowledge?** Read the **[Comprehensive Analysis](COMPREHENSIVE_SUBSCRIPTION_MANAGEMENT_ANALYSIS.md)** (1 hour read)

### For Visual Learners
**📊 Like diagrams?** Check out **[Flow Diagrams](SUBSCRIPTION_FLOW_DIAGRAMS.md)** (20 min read)

### For Extraction
**🧰 Extracting module?** Follow the **[Complete Extraction Guide](COMPLETE_EXTRACTION_GUIDE.md)** (3-5 days work)

### For Checklists
**✓ Need tracking?** Use **[Extraction Checklist](EXTRACTION_CHECKLIST.md)**

### For Navigation
**🗺️ Finding things?** Use **[Extraction Guide Index](EXTRACTION_GUIDE_INDEX.md)**

---

## 📚 Documentation Overview

| Document | Purpose | Read Time | Lines |
|----------|---------|-----------|-------|
| [Quick Reference Guide](QUICK_REFERENCE_GUIDE.md) | Quick API/reference | 15 min | 523 |
| [Comprehensive Analysis](COMPREHENSIVE_SUBSCRIPTION_MANAGEMENT_ANALYSIS.md) | Deep technical understanding | 1 hour | 1,037 |
| [Flow Diagrams](SUBSCRIPTION_FLOW_DIAGRAMS.md) | Visual workflows | 20 min | 656 |
| [Extraction Guide](COMPLETE_EXTRACTION_GUIDE.md) | Step-by-step extraction | 1 hour read | 1,500+ |
| [Extraction Checklist](EXTRACTION_CHECKLIST.md) | Track extraction progress | N/A | 350 |
| [Extraction Guide Index](EXTRACTION_GUIDE_INDEX.md) | Navigation & quick access | 5 min | 200 |

**Total Documentation: ~4,200 lines**

---

## 🎯 System Overview

### What It Does
✅ **Subscription Plan Management** - Create, update, version plans  
✅ **User Subscriptions** - Manage user subscription lifecycle  
✅ **Privilege-Based Pricing** - Healthcare-focused pricing model  
✅ **Automated Billing** - Hourly recurring billing  
✅ **Payment Processing** - Full Stripe integration  
✅ **Webhook Handling** - Real-time Stripe event processing  
✅ **Plan Versioning** - Migrate users without disrupting  
✅ **Usage Tracking** - Monitor privilege consumption  
✅ **Background Jobs** - 7 automated background services  

### Key Features
🎨 **Healthcare-Focused** - Privilege-based pricing  
🔄 **Automated** - Multiple background services  
🔒 **Secure** - Stripe PCI compliance, JWT auth  
📊 **Auditable** - Complete audit trail  
🚀 **Scalable** - Clean architecture  
🧪 **Tested** - Comprehensive test coverage  

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────┐
│                  API LAYER                      │
│  Controllers, Swagger, Authentication           │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│              APPLICATION LAYER                  │
│  Services (Business Logic), DTOs, Utilities    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│            INFRASTRUCTURE LAYER                 │
│  Repositories, Stripe, Background Services      │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│                 CORE LAYER                      │
│  Entities, Interfaces, Enums, DTOs              │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│              DATABASE LAYER                     │
│  SQL Server, Entity Framework Core              │
└─────────────────────────────────────────────────┘
```

---

## 📂 Key Components

### Core Entities (24 files)
- **SubscriptionPlan** - Plan templates
- **Subscription** - User subscription instances
- **Privilege** - Feature definitions
- **SubscriptionPlanPrivilege** - Plan ↔ Privilege mapping
- **UserSubscriptionPrivilegeUsage** - Usage tracking
- **BillingRecord** - Master billing records
- **SubscriptionPayment** - Payment tracking
- **ScheduledPlanMigration** - Version migration
- **Master Tables** - 5 lookup tables

### Services (19 files)
- **SubscriptionPlanService** - Plan CRUD
- **SubscriptionService** - Subscription queries
- **SubscriptionLifecycleService** - Lifecycle operations
- **SubscriptionBillingService** - Billing operations (51 methods)
- **PrivilegeService** - Privilege tracking
- **PlanPricingService** - Pricing calculations
- **PlanVersioningService** - Version management
- **StripeService** - Stripe integration
- **AutomatedBillingService** - Automated billing

### Background Services (7 services)
- **AutomatedBillingBackgroundService** ⏰ - Runs hourly
- **PrivilegeResetBackgroundService** ⏰ - Resets counters
- **ScheduledMigrationBackgroundService** ⏰ - Processes migrations
- **FailedRefundRetryBackgroundService** ⏰ - Retries refunds
- **UnprocessedWebhookRetryService** ⏰ - Retries webhooks
- **StripeSyncJob** ⏰ - Syncs with Stripe (hourly)
- **ReconciliationBackgroundService** ⏰ - Nightly integrity checks

### Controllers (6 files)
- **SubscriptionPlansController** - Plan management
- **SubscriptionsController** - Subscription management
- **BillingController** - Billing operations
- **StripeController** - Checkout & payment methods
- **StripeWebhookController** - Webhook processing

### Utilities (7 files)
- **BillingCalculationService** - Price calculations
- **BillingCycleCalculator** - Date calculations
- **PrivilegeAllocationCalculator** - Privilege allocation
- **PrivilegeResetHelper** - Reset utilities
- **SubscriptionConstants** - Constants

---

## 🔄 Core Workflows

### 1. Subscription Creation
```
User → Browse Plans → Select Plan → Stripe Checkout → 
Payment Success → Webhook → Create Subscription → 
Initialize Privileges → Create Billing Record → 
Send Welcome Email ✅
```

### 2. Automated Billing
```
Background Service → Query Due Subscriptions → 
Create BillingRecord → Process Stripe Payment → 
Create SubscriptionPayment → Update NextBillingDate → 
Reset Privilege Counters → Send Confirmation ✅
```

### 3. Privilege Usage
```
User Uses Service → Check Remaining → Update Usage → 
Log History → If Over Limit → Charge Overage ✅
```

### 4. Plan Versioning
```
Admin Updates Plan → Create New Version → 
Mark Old as Not Latest → Schedule Migrations → 
At Renewal → Migrate Users → Update Billing ✅
```

---

## 💰 Pricing Model

### Privilege-Based Pricing Formula
```
For each Privilege in Plan:
  Contribution = Value × PrivilegeBaseCost

PrivilegesTotalCost = Σ(Contributions)

AdminCommission = PrivilegesTotalCost × (CommissionPercent / 100)

BasePrice = PrivilegesTotalCost + AdminCommission

EffectivePrice = BasePrice
  → Apply Promotional Discount
  → Apply Billing Cycle Discount

Final = max(EffectivePrice, 0)
```

### Example Calculation
```
Plan: Basic - Monthly
Privileges:
  - TeleConsultation: 5 × $3 = $15
  - Messaging: 100 × $0.01 = $1

PrivilegesTotalCost = $16
AdminCommission = $16 × 10% = $1.60
BasePrice = $17.60

Promotional Discount: 20%
After discount: $17.60 × 80% = $14.08

Billing Discount: 10%
Final Effective Price: $14.08 × 90% = $12.67
```

---

## 📊 Database Schema

### Entity Relationships
```
User (1) ──┬──→ (M) Subscription
           │         ├──→ (1) SubscriptionPlan
           │         ├──→ (M) SubscriptionPayment
           │         ├──→ (M) UserSubscriptionPrivilegeUsage
           │         └──→ (M) ScheduledPlanMigration
           │
           └──→ (M) BillingRecord
                      ├──→ (1) MasterCurrency
                      └──→ (0..1) Subscription

SubscriptionPlan (1) ───→ (M) SubscriptionPlanPrivilege
                        ├──→ (1) MasterBillingCycle
                        ├──→ (1) MasterCurrency
                        ├──→ (1) Category
                        ├──→ (M) ChildVersions (self-reference)
                        └──→ (M) Subscriptions

Privilege (1) ───→ (M) SubscriptionPlanPrivilege
              └──→ (M) UserSubscriptionPrivilegeUsage

UserSubscriptionPrivilegeUsage (1) ───→ (M) PrivilegeUsageHistory
```

### Key Tables
- **SubscriptionPlans** - Plan templates (24 columns)
- **Subscriptions** - User subscriptions (40+ columns)
- **SubscriptionPlanPrivileges** - Plan ↔ Privilege mapping
- **UserSubscriptionPrivilegeUsages** - Usage tracking
- **BillingRecords** - Master billing records
- **SubscriptionPayments** - Payment tracking
- **ScheduledPlanMigrations** - Version migrations

---

## 🔐 Security & Authentication

### Authentication
- JWT token-based authentication
- Token includes: UserID, RoleID, permissions
- Public endpoints for plan browsing

### Authorization
- Users can only access own subscriptions
- Admins can access all
- Role-based access control

### Payment Security
- Stripe handles PCI compliance
- Webhook signature validation
- Idempotency checks
- Full audit trail

---

## 🎛️ Configuration

### Required Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "StripeSettings": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "...",
    "Audience": "...",
    "ExpirationHours": 24
  }
}
```

---

## 🧪 Testing

### Test Coverage
✅ Unit tests for services  
✅ Integration tests for workflows  
✅ Stripe webhook tests  
✅ Billing calculation tests  
✅ Plan versioning tests  

### Test Structure
```
SmartTelehealth.Tests/
├── Services/
│   ├── SubscriptionPlanServiceTests.cs
│   ├── SubscriptionLifecycleServiceTests.cs
│   ├── SubscriptionBillingServiceTests.cs
│   └── PrivilegeServiceTests.cs
└── Integration/
    ├── SubscriptionFlowTests.cs
    └── BillingIntegrationTests.cs
```

---

## 📦 NuGet Packages

### Required Packages
```
Microsoft.EntityFrameworkCore (8.0.0)
Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)
Stripe.net (43.0.0)
Swashbuckle.AspNetCore (6.5.0)
Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
Serilog.AspNetCore (8.0.0)
```

---

## 🚀 Getting Started

### Prerequisites
✅ .NET 8.0 SDK  
✅ SQL Server (or compatible)  
✅ Stripe account  
✅ Visual Studio / VS Code  

### Setup Steps
1. Clone repository
2. Restore NuGet packages
3. Configure appsettings.json
4. Run migrations
5. Seed master data
6. Start application

### Running the Application
```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run --project SmartTelehealth.API
```

---

## 📈 System Statistics

### Code Metrics
- **Total Files**: ~163 files
- **Entities**: 24 files
- **Services**: 19 files
- **Background Services**: 7 services
- **Repositories**: 20 files
- **DTOs**: 34 files
- **Controllers**: 6 files
- **Lines of Code**: ~30,000+

### Database Metrics
- **Total Tables**: 15+ tables
- **Master Tables**: 5 tables
- **Indexes**: 50+ indexes
- **Foreign Keys**: 30+ relationships

---

## 📞 Support & Resources

### Documentation Files
📄 [Quick Reference Guide](QUICK_REFERENCE_GUIDE.md)  
📄 [Comprehensive Analysis](COMPREHENSIVE_SUBSCRIPTION_MANAGEMENT_ANALYSIS.md)  
📄 [Flow Diagrams](SUBSCRIPTION_FLOW_DIAGRAMS.md)  
📄 [Extraction Guide](COMPLETE_EXTRACTION_GUIDE.md)  
📄 [Extraction Checklist](EXTRACTION_CHECKLIST.md)  
📄 [Extraction Guide Index](EXTRACTION_GUIDE_INDEX.md)  

### External Resources
🔗 [Stripe Documentation](https://stripe.com/docs)  
🔗 [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)  
🔗 [AutoMapper](https://automapper.org/)  

---

## 🎯 Quick Links

### API Endpoints
- `GET /api/subscriptionplans/active` - Browse active plans
- `GET /api/subscriptions/{id}` - Get subscription
- `POST /api/Checkout/create-session/{planId}` - Stripe checkout
- `POST /api/stripewebhook/webhook` - Webhook handler

### Business Logic
- Subscription Lifecycle Service - Lifecycle operations
- Subscription Billing Service - Billing operations
- Privilege Service - Usage tracking
- Plan Versioning Service - Version management

### Background Services
- Automated Billing - Runs every hour
- Privilege Reset - Runs periodically
- Stripe Sync - Runs hourly
- Migration Processor - Runs periodically

---

## ✅ Success Indicators

### System is Working If:
✅ Background services start without errors  
✅ API endpoints respond  
✅ Database tables created  
✅ Master data seeded  
✅ Stripe webhooks processed  
✅ Automated billing executes  
✅ Privilege usage tracked  
✅ Plan versioning works  

---

## 🎉 Summary

You now have a **production-ready, enterprise-grade** subscription management system with:

✅ Complete documentation (4,200+ lines)  
✅ Step-by-step extraction guide  
✅ Visual flow diagrams  
✅ Comprehensive technical analysis  
✅ Ready-to-use checklist  
✅ 163 files ready to extract  

**Everything you need to understand, extract, and deploy this system is ready!** 🚀

---

**Generated:** ${new Date().toISOString()}  
**Documentation Version:** 1.0  
**System Version:** As of current codebase

