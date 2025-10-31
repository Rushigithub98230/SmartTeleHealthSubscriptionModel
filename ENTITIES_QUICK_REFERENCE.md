# Entities Extraction - Quick Reference

## 🎯 TL;DR - Just the Entity List

**Total Entities to Extract: 25 files**

---

## 📦 Complete Entity List

### 1️⃣ FOUNDATION (Extract First)
```
✅ BaseEntity.cs
```

### 2️⃣ MASTER DATA (From MasterTables.cs - Extract 5 classes)
```
✅ MasterBillingCycle
✅ MasterCurrency
✅ MasterPrivilegeType
✅ PaymentStatus
✅ RefundStatus
```

### 3️⃣ SUBSCRIPTION PLANS
```
✅ SubscriptionPlan.cs
✅ SubscriptionPlanPrivilege.cs
```

### 4️⃣ SUBSCRIPTIONS
```
✅ Subscription.cs
✅ SubscriptionPayment.cs
✅ SubscriptionStatusHistory.cs
```

### 5️⃣ PRIVILEGES
```
✅ Privilege.cs
✅ UserSubscriptionPrivilegeUsage.cs
✅ PrivilegeUsageHistory.cs
```

### 6️⃣ BILLING
```
✅ BillingRecord.cs
✅ BillingAdjustment.cs
✅ PaymentRefund.cs
✅ FailedRefund.cs
```

### 7️⃣ VERSIONING
```
✅ ScheduledPlanMigration.cs
```

### 8️⃣ WEBHOOKS & SYNC
```
✅ ProcessedWebhookEvent.cs
✅ UnprocessedWebhookEvent.cs
✅ StripeSyncHistory.cs
```

### 9️⃣ SUPPORTING
```
✅ Category.cs
✅ SystemSettings.cs
✅ User.cs
```

---

## 📋 Quick Checklist

Copy this and check off as you go:

```
FOUNDATION:
[ ] BaseEntity.cs

MASTER DATA (from MasterTables.cs):
[ ] MasterBillingCycle
[ ] MasterCurrency
[ ] MasterPrivilegeType
[ ] PaymentStatus
[ ] RefundStatus

SUBSCRIPTION PLANS:
[ ] SubscriptionPlan.cs
[ ] SubscriptionPlanPrivilege.cs

SUBSCRIPTIONS:
[ ] Subscription.cs
[ ] SubscriptionPayment.cs
[ ] SubscriptionStatusHistory.cs

PRIVILEGES:
[ ] Privilege.cs
[ ] UserSubscriptionPrivilegeUsage.cs
[ ] PrivilegeUsageHistory.cs

BILLING:
[ ] BillingRecord.cs
[ ] BillingAdjustment.cs
[ ] PaymentRefund.cs
[ ] FailedRefund.cs

VERSIONING:
[ ] ScheduledPlanMigration.cs

WEBHOOKS & SYNC:
[ ] ProcessedWebhookEvent.cs
[ ] UnprocessedWebhookEvent.cs
[ ] StripeSyncHistory.cs

SUPPORTING:
[ ] Category.cs
[ ] SystemSettings.cs
[ ] User.cs
```

---

## 🔗 Source Path

**All files are in:** `backend/SmartTelehealth.Core/Entities/`

---

## ⚠️ Important Notes

1. **BaseEntity.cs** MUST be extracted first - all entities inherit from it
2. **MasterTables.cs** contains 5 classes you need - extract only those 5
3. **User.cs** - if extracting to standalone, create minimal stub
4. **Total:** 25 entity files to extract

---

✅ **DONE!** You now have your complete entity extraction list!

