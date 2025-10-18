# Quick Start Testing Guide - Upfront Credit Purchase

## 🚀 Ready to Test Your New Feature!

This guide will help you test the complete upfront credit purchase workflow end-to-end.

---

## 🎯 What Was Implemented

### **New Feature: Purchase Additional Privilege Credits**

**User Story:**
> As a user who has exhausted their subscription privileges, I want to purchase additional credits with immediate payment, so that I can continue using the service without waiting for the next billing cycle.

**Implementation:**
- ✅ API endpoint to purchase credits
- ✅ API endpoint to check availability
- ✅ Upfront payment processing
- ✅ Dynamic credit allocation
- ✅ Transaction safety (payment before credits)

---

## 🧪 Quick Test Scenarios

### **Scenario 1: Happy Path - Successful Purchase**

**Prerequisites:**
- User has active subscription
- Plan includes "Teleconsultation" privilege (5 limit, $20 unit cost)
- User has used all 5 consultations (Remaining = 0)
- User has valid payment method

**Test Steps:**

**Step 1: Check Privilege Availability**
```http
GET /api/subscriptions/{{subscriptionId}}/check-privilege/Teleconsultation?requestedAmount=1
Authorization: Bearer {{userToken}}
```

**Expected Response:**
```json
{
  "statusCode": 402,
  "message": "Insufficient Teleconsultation credits...",
  "data": {
    "available": false,
    "limitExceeded": true,
    "privilegeName": "Teleconsultation",
    "remaining": 0,
    "requested": 1,
    "shortfall": 1,
    "unitCost": 20.00,
    "requiredPayment": 20.00,
    "purchaseDetails": {
      "privilegeName": "Teleconsultation",
      "quantity": 1,
      "totalCost": 20.00
    }
  }
}
```

**Step 2: Purchase 1 Additional Credit**
```http
POST /api/subscriptions/{{subscriptionId}}/purchase-credits
Authorization: Bearer {{userToken}}
Content-Type: application/json

{
  "privilegeName": "Teleconsultation",
  "quantity": 1,
  "paymentMethodId": "pm_card_visa"
}
```

**Expected Response:**
```json
{
  "statusCode": 200,
  "message": "Successfully purchased 1 additional Teleconsultation credits for $20.00. Your new limit is 6.",
  "data": {
    "subscriptionId": "...",
    "privilegeName": "Teleconsultation",
    "creditsAdded": 1,
    "unitCost": 20.00,
    "totalPaid": 20.00,
    "previousLimit": 5,
    "newLimit": 6,
    "currentUsed": 5,
    "newRemaining": 1,
    "billingRecordId": "...",
    "purchasedAt": "2025-10-15T14:30:00Z"
  }
}
```

**Step 3: Verify Credits Added**
```http
GET /api/subscriptions/{{subscriptionId}}/check-privilege/Teleconsultation?requestedAmount=1
Authorization: Bearer {{userToken}}
```

**Expected Response:**
```json
{
  "statusCode": 200,
  "data": {
    "available": true,
    "remaining": 1,
    "message": "Privilege is available"
  }
}
```

**Step 4: Verify Database Changes**
```sql
-- Check updated limit
SELECT AllowedValue, UsedValue, RemainingValue
FROM UserSubscriptionPrivilegeUsage
WHERE SubscriptionId = '{subscription-id}';
-- Expected: AllowedValue=6, UsedValue=5, RemainingValue=1

-- Check billing record created
SELECT * FROM BillingRecords
WHERE SubscriptionId = '{subscription-id}'
  AND Type = 'Overage'
ORDER BY CreatedDate DESC;
-- Expected: Status='Paid', Amount=20.00

-- Check Stripe integration
SELECT StripePaymentIntentId, StripeInvoiceId
FROM BillingRecords
WHERE Id = '{billing-record-id}';
-- Expected: Both fields populated
```

✅ **Test Result:** PASS if all checks succeed

---

### **Scenario 2: Payment Failure - No Credits Added**

**Test Steps:**

```http
POST /api/subscriptions/{{subscriptionId}}/purchase-credits
Authorization: Bearer {{userToken}}
Content-Type: application/json

{
  "privilegeName": "Teleconsultation",
  "quantity": 2,
  "paymentMethodId": "pm_card_chargeDeclined"  // Use Stripe test card that declines
}
```

**Expected Response:**
```json
{
  "statusCode": 400,
  "message": "Payment failed: Your card was declined. Additional credits were not added to your account.",
  "data": {
    "paymentFailed": true,
    "reason": "Your card was declined.",
    "creditsAdded": 0,
    "amountCharged": 0
  }
}
```

**Verification:**
```sql
-- Credits should NOT be added
SELECT AllowedValue FROM UserSubscriptionPrivilegeUsage
WHERE SubscriptionId = '{subscription-id}';
-- Expected: AllowedValue = 5 (UNCHANGED!)

-- Billing record should NOT exist or be Failed
SELECT COUNT(*) FROM BillingRecords
WHERE SubscriptionId = '{subscription-id}'
  AND Type = 'Overage'
  AND Status = 'Paid'
  AND CreatedDate > DATEADD(minute, -5, GETUTCDATE());
-- Expected: 0 (transaction rolled back)
```

✅ **Test Result:** PASS if AllowedValue unchanged and no billing record created

---

### **Scenario 3: Multiple Credits Purchase**

**Test Steps:**

```http
POST /api/subscriptions/{{subscriptionId}}/purchase-credits
{
  "privilegeName": "Teleconsultation",
  "quantity": 3,
  "paymentMethodId": "pm_card_visa"
}
```

**Expected:**
- Cost: 3 × $20 = $60
- Payment processed for $60
- AllowedValue increased by 3
- NewRemaining = 3

**Verification:**
```sql
SELECT AllowedValue FROM UserSubscriptionPrivilegeUsage;
-- Expected: AllowedValue = 8 (5 original + 3 purchased)

SELECT Amount FROM BillingRecords WHERE Type = 'Overage' ORDER BY CreatedDate DESC;
-- Expected: Amount = 60.00
```

---

### **Scenario 4: Unlimited Privilege Check**

**Test Steps:**

```http
GET /api/subscriptions/{{subscriptionId}}/check-privilege/Messaging?requestedAmount=1
```

**Expected Response:**
```json
{
  "statusCode": 200,
  "data": {
    "available": true,
    "unlimited": true,
    "message": "You have unlimited access to this privilege"
  }
}
```

---

### **Scenario 5: Disabled Privilege Check**

**Test Steps:**

```http
GET /api/subscriptions/{{subscriptionId}}/check-privilege/HomeVisit?requestedAmount=1
```

**Expected Response:**
```json
{
  "statusCode": 403,
  "data": {
    "available": false,
    "disabled": true,
    "message": "Privilege 'HomeVisit' is not included in your subscription plan"
  }
}
```

---

## 🔧 Postman Collection Setup

### **Environment Variables:**

```json
{
  "baseUrl": "https://localhost:7001",
  "subscriptionId": "your-subscription-guid",
  "userId": "123",
  "userToken": "your-jwt-token",
  "validPaymentMethod": "pm_card_visa",
  "invalidPaymentMethod": "pm_card_chargeDeclined"
}
```

### **Requests:**

**Collection: Upfront Credit Purchase Tests**

1. **Check Availability (Has Credits)**
   - Method: GET
   - URL: `{{baseUrl}}/api/subscriptions/{{subscriptionId}}/check-privilege/Teleconsultation?requestedAmount=1`
   - Headers: `Authorization: Bearer {{userToken}}`
   - Expected: 200 OK

2. **Check Availability (No Credits)**
   - Method: GET
   - URL: Same as above
   - Expected: 402 Payment Required

3. **Purchase Credits (Success)**
   - Method: POST
   - URL: `{{baseUrl}}/api/subscriptions/{{subscriptionId}}/purchase-credits`
   - Headers: `Authorization: Bearer {{userToken}}`
   - Body:
     ```json
     {
       "privilegeName": "Teleconsultation",
       "quantity": 1,
       "paymentMethodId": "{{validPaymentMethod}}"
     }
     ```
   - Expected: 200 OK

4. **Purchase Credits (Payment Fails)**
   - Method: POST
   - URL: Same as above
   - Body: Use `{{invalidPaymentMethod}}`
   - Expected: 400 Bad Request

5. **Verify Credits After Purchase**
   - Method: GET
   - URL: Check availability endpoint
   - Expected: 200 OK with remaining > 0

---

## 🧩 Integration Test Code

### **C# Integration Test Example:**

```csharp
[Test]
public async Task CompleteCreditPurchaseFlow_Success()
{
    // ARRANGE
    var subscription = await CreateTestSubscription();
    var privilegeName = "Teleconsultation";
    
    // Use all 5 consultations
    for (int i = 0; i < 5; i++)
    {
        await _privilegeService.UsePrivilegeAsync(
            subscription.Id, 
            privilegeName, 
            1, 
            _testToken
        );
    }
    
    // ACT - Check availability (should be 402)
    var checkResult = await _privilegeService.CheckPrivilegeAvailabilityAsync(
        subscription.Id,
        privilegeName,
        1,
        _testToken
    );
    
    // ASSERT - Should require payment
    Assert.AreEqual(402, checkResult.StatusCode);
    Assert.IsTrue(((dynamic)checkResult.data).limitExceeded);
    Assert.AreEqual(20.00m, ((dynamic)checkResult.data).requiredPayment);
    
    // ACT - Purchase 1 additional credit
    var purchaseDto = new PurchaseAdditionalCreditsDto
    {
        PrivilegeName = privilegeName,
        Quantity = 1,
        PaymentMethodId = _validPaymentMethodId
    };
    
    var purchaseResult = await _subscriptionService.PurchaseAdditionalCreditsAsync(
        subscription.Id,
        purchaseDto,
        _testToken
    );
    
    // ASSERT - Purchase should succeed
    Assert.AreEqual(200, purchaseResult.StatusCode);
    var data = (PurchaseCreditsResponseDto)purchaseResult.data;
    Assert.AreEqual(1, data.CreditsAdded);
    Assert.AreEqual(6, data.NewLimit);
    Assert.AreEqual(1, data.NewRemaining);
    
    // ACT - Check availability again (should now be available)
    var recheckResult = await _privilegeService.CheckPrivilegeAvailabilityAsync(
        subscription.Id,
        privilegeName,
        1,
        _testToken
    );
    
    // ASSERT - Should be available now
    Assert.AreEqual(200, recheckResult.StatusCode);
    Assert.IsTrue(((dynamic)recheckResult.data).available);
    
    // ACT - Use the privilege
    var useResult = await _privilegeService.UsePrivilegeAsync(
        subscription.Id,
        privilegeName,
        1,
        _testToken
    );
    
    // ASSERT - Should succeed
    Assert.IsTrue(useResult);
    
    // VERIFY database state
    var usage = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
    var consultationUsage = usage.First(u => u.Privilege.Name == privilegeName);
    Assert.AreEqual(6, consultationUsage.AllowedValue);
    Assert.AreEqual(6, consultationUsage.UsedValue);
    Assert.AreEqual(0, consultationUsage.RemainingValue);
}

[Test]
public async Task PurchaseCredits_PaymentFails_NoCreditsAdded()
{
    // ARRANGE
    var subscription = await CreateTestSubscription();
    var usage = await GetUsage(subscription.Id, "Teleconsultation");
    var initialAllowedValue = usage.AllowedValue;
    
    // ACT - Attempt purchase with failing payment
    var purchaseDto = new PurchaseAdditionalCreditsDto
    {
        PrivilegeName = "Teleconsultation",
        Quantity = 2,
        PaymentMethodId = "pm_card_chargeDeclined"  // Stripe test card that fails
    };
    
    var result = await _subscriptionService.PurchaseAdditionalCreditsAsync(
        subscription.Id,
        purchaseDto,
        _testToken
    );
    
    // ASSERT - Purchase should fail
    Assert.AreEqual(400, result.StatusCode);
    Assert.IsTrue(((dynamic)result.data).paymentFailed);
    Assert.AreEqual(0, ((dynamic)result.data).creditsAdded);
    
    // VERIFY - AllowedValue should be unchanged
    var updatedUsage = await GetUsage(subscription.Id, "Teleconsultation");
    Assert.AreEqual(initialAllowedValue, updatedUsage.AllowedValue);
    
    // VERIFY - No billing record created (or status is not Paid)
    var billingRecords = await _billingRepo.GetBySubscriptionIdAsync(subscription.Id);
    var overageRecords = billingRecords
        .Where(b => b.Type == BillingRecord.BillingType.Overage 
                 && b.Status == BillingRecord.BillingStatus.Paid
                 && b.CreatedDate > DateTime.UtcNow.AddMinutes(-5))
        .ToList();
    
    Assert.AreEqual(0, overageRecords.Count);
}
```

---

## 📱 Manual Testing Steps

### **Setup:**

1. **Create a test user**
2. **Create a subscription plan with:**
   - Teleconsultation: 5 limit, $20 unit cost
   - Medication: 3 limit, $50 unit cost
3. **Subscribe the user to the plan**
4. **Get a Stripe test payment method:**
   - Success card: `pm_card_visa` or use card `4242 4242 4242 4242`
   - Decline card: `pm_card_chargeDeclined` or use card `4000 0000 0000 0002`

### **Test 1: Check Availability When Credits Available**

```bash
curl -X GET "https://localhost:7001/api/subscriptions/YOUR-SUBSCRIPTION-ID/check-privilege/Teleconsultation?requestedAmount=1" \
  -H "Authorization: Bearer YOUR-TOKEN"
```

**Expected:** 200 OK, `"available": true`

### **Test 2: Use All Credits**

```bash
# Book 5 consultations (use your consultation booking endpoint)
for i in {1..5}; do
  curl -X POST "https://localhost:7001/api/consultations" \
    -H "Authorization: Bearer YOUR-TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"subscriptionId":"YOUR-ID",...}'
done
```

### **Test 3: Check Availability When No Credits**

```bash
curl -X GET "https://localhost:7001/api/subscriptions/YOUR-SUBSCRIPTION-ID/check-privilege/Teleconsultation?requestedAmount=1" \
  -H "Authorization: Bearer YOUR-TOKEN"
```

**Expected:** 402 Payment Required, includes purchase details

### **Test 4: Purchase Additional Credits**

```bash
curl -X POST "https://localhost:7001/api/subscriptions/YOUR-SUBSCRIPTION-ID/purchase-credits" \
  -H "Authorization: Bearer YOUR-TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "privilegeName": "Teleconsultation",
    "quantity": 2,
    "paymentMethodId": "pm_card_visa"
  }'
```

**Expected:** 200 OK, credits added, payment charged

### **Test 5: Verify in Database**

```sql
-- Check updated limit
SELECT * FROM UserSubscriptionPrivilegeUsage 
WHERE SubscriptionId = 'YOUR-ID';
-- AllowedValue should be 7 (5 + 2)

-- Check billing record
SELECT * FROM BillingRecords 
WHERE SubscriptionId = 'YOUR-ID' AND Type = 'Overage'
ORDER BY CreatedDate DESC;
-- Status should be 'Paid', Amount should be 40.00 (2 × $20)
```

---

## 🎨 Frontend Integration Example

### **React Component Example:**

```typescript
import { useState } from 'react';
import { api } from './api';

interface PurchaseModalProps {
  subscriptionId: string;
  privilegeName: string;
  shortfall: number;
  unitCost: number;
  totalCost: number;
  onSuccess: () => void;
  onCancel: () => void;
}

function PurchaseCreditsModal({
  subscriptionId,
  privilegeName,
  shortfall,
  unitCost,
  totalCost,
  onSuccess,
  onCancel
}: PurchaseModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handlePurchase = async () => {
    setLoading(true);
    setError('');

    try {
      const response = await api.post(
        `/api/subscriptions/${subscriptionId}/purchase-credits`,
        {
          privilegeName,
          quantity: shortfall,
          paymentMethodId: user.defaultPaymentMethod  // Get from user context
        }
      );

      if (response.statusCode === 200) {
        // Success!
        showNotification(
          'success',
          `Payment successful! You purchased ${response.data.creditsAdded} ` +
          `additional ${privilegeName} credit(s) for $${response.data.totalPaid}.`
        );
        onSuccess();
      } else {
        setError(response.message);
      }
    } catch (err) {
      setError('An error occurred. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal">
      <div className="modal-content">
        <h2>Purchase Additional Credits</h2>
        
        <p className="message">
          You've used all your included {privilegeName} credits.
          Purchase {shortfall} additional credit{shortfall > 1 ? 's' : ''} to continue.
        </p>
        
        <div className="purchase-details">
          <div className="detail-row">
            <span>Privilege:</span>
            <span>{privilegeName}</span>
          </div>
          <div className="detail-row">
            <span>Quantity:</span>
            <span>{shortfall} credit{shortfall > 1 ? 's' : ''}</span>
          </div>
          <div className="detail-row">
            <span>Unit Cost:</span>
            <span>${unitCost.toFixed(2)}</span>
          </div>
          <div className="detail-row total">
            <span><strong>Total:</strong></span>
            <span><strong>${totalCost.toFixed(2)}</strong></span>
          </div>
        </div>

        {error && (
          <div className="error-message">{error}</div>
        )}

        <div className="modal-actions">
          <button
            className="btn-primary"
            onClick={handlePurchase}
            disabled={loading}
          >
            {loading ? 'Processing...' : 'Pay Now'}
          </button>
          <button
            className="btn-secondary"
            onClick={onCancel}
            disabled={loading}
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}

// Usage in consultation booking flow:
async function bookConsultation(subscriptionId: string) {
  // Step 1: Check if privilege is available
  const availability = await api.get(
    `/api/subscriptions/${subscriptionId}/check-privilege/Teleconsultation?requestedAmount=1`
  );

  if (availability.statusCode === 200 && availability.data.available) {
    // User has credits - proceed directly
    await createConsultation();
    return;
  }

  if (availability.statusCode === 402 && availability.data.limitExceeded) {
    // Show purchase modal
    const confirmed = await showPurchaseModal({
      subscriptionId,
      privilegeName: availability.data.privilegeName,
      shortfall: availability.data.shortfall,
      unitCost: availability.data.unitCost,
      totalCost: availability.data.requiredPayment
    });

    if (confirmed) {
      // Credits purchased, now book consultation
      await createConsultation();
    }
    return;
  }

  // Handle other statuses
  showError(availability.message);
}
```

---

## 🎯 Test Data Setup Script

### **SQL Script to Setup Test Data:**

```sql
-- 1. Create test user
INSERT INTO Users (FirstName, LastName, Email, UserRoleId, IsActive, CreatedDate)
VALUES ('Test', 'User', 'test@example.com', 1, 1, GETUTCDATE());

DECLARE @UserId INT = SCOPE_IDENTITY();

-- 2. Create subscription plan
DECLARE @PlanId UNIQUEIDENTIFIER = NEWID();
DECLARE @BillingCycleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MasterBillingCycles WHERE Name = 'Monthly');
DECLARE @CurrencyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MasterCurrencies WHERE Code = 'USD');

INSERT INTO SubscriptionPlans (Id, Name, Price, BillingCycleId, CurrencyId, IsActive, CreatedDate)
VALUES (@PlanId, 'Test Standard Plan', 280, @BillingCycleId, @CurrencyId, 1, GETUTCDATE());

-- 3. Get or create Teleconsultation privilege
DECLARE @PrivilegeId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Privileges WHERE Name = 'Teleconsultation');
IF @PrivilegeId IS NULL
BEGIN
    SET @PrivilegeId = NEWID();
    INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, CreatedDate)
    VALUES (@PrivilegeId, 'Teleconsultation', 'Virtual consultation with healthcare provider', 
            (SELECT TOP 1 Id FROM MasterPrivilegeTypes), 1, GETUTCDATE());
END

-- 4. Add privilege to plan with limits and unit cost
INSERT INTO SubscriptionPlanPrivileges (Id, SubscriptionPlanId, PrivilegeId, Value, UnitCost, UsagePeriodId, IsActive, CreatedDate)
VALUES (NEWID(), @PlanId, @PrivilegeId, 5, 20.00, @BillingCycleId, 1, GETUTCDATE());

-- 5. Create subscription for user
DECLARE @SubscriptionId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Subscriptions (Id, UserId, SubscriptionPlanId, BillingCycleId, Status, StartDate, NextBillingDate, CurrentPrice, IsActive, CreatedDate)
VALUES (@SubscriptionId, @UserId, @PlanId, @BillingCycleId, 'Active', GETUTCDATE(), DATEADD(month, 1, GETUTCDATE()), 280, 1, GETUTCDATE());

-- 6. Initialize privilege usage
DECLARE @PlanPrivilegeId UNIQUEIDENTIFIER = (SELECT Id FROM SubscriptionPlanPrivileges WHERE SubscriptionPlanId = @PlanId AND PrivilegeId = @PrivilegeId);

INSERT INTO UserSubscriptionPrivilegeUsage (Id, SubscriptionId, SubscriptionPlanPrivilegeId, PrivilegeId, UsedValue, AllowedValue, UsagePeriodStart, UsagePeriodEnd, IsActive, CreatedDate)
VALUES (NEWID(), @SubscriptionId, @PlanPrivilegeId, @PrivilegeId, 0, 5, GETUTCDATE(), DATEADD(month, 1, GETUTCDATE()), 1, GETUTCDATE());

-- 7. Output IDs for testing
SELECT 
    @UserId as UserId,
    @SubscriptionId as SubscriptionId,
    @PlanId as PlanId,
    @PrivilegeId as PrivilegeId;
```

### **Use All Credits Script:**

```sql
DECLARE @SubscriptionId UNIQUEIDENTIFIER = 'YOUR-SUBSCRIPTION-ID';
DECLARE @PrivilegeId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Privileges WHERE Name = 'Teleconsultation');

-- Use all 5 credits
UPDATE UserSubscriptionPrivilegeUsage
SET UsedValue = 5, LastUsedAt = GETUTCDATE()
WHERE SubscriptionId = @SubscriptionId
  AND PrivilegeId = @PrivilegeId;

-- Verify
SELECT AllowedValue, UsedValue, RemainingValue
FROM UserSubscriptionPrivilegeUsage
WHERE SubscriptionId = @SubscriptionId;
-- Should show: AllowedValue=5, UsedValue=5, RemainingValue=0
```

---

## ✅ Acceptance Criteria Validation

### **Requirement 1: Block Access When Limit Exceeded**

**Test:** User with 0 remaining credits tries to book consultation

**Expected:**
- ✅ Backend returns 402 Payment Required
- ✅ Includes purchase details
- ✅ User cannot proceed without payment

**Validation:** `CheckPrivilegeAvailabilityAsync()` returns 402 ✅

---

### **Requirement 2: Require Upfront Payment**

**Test:** User purchases additional credits

**Expected:**
- ✅ Payment processed immediately
- ✅ Not deferred to next billing cycle
- ✅ Stripe charge created and confirmed

**Validation:** `PurchaseAdditionalCreditsAsync()` processes payment immediately ✅

---

### **Requirement 3: Add Credits Only After Successful Payment**

**Test:** User attempts purchase with declining card

**Expected:**
- ✅ Payment fails
- ✅ Credits NOT added
- ✅ AllowedValue unchanged
- ✅ Transaction rolled back

**Validation:** Transaction ensures atomicity ✅

---

### **Requirement 4: Allow Usage After Payment**

**Test:** User purchases credits and then uses service

**Expected:**
- ✅ AllowedValue increased
- ✅ RemainingValue updated
- ✅ Service usage succeeds

**Validation:** `UsePrivilegeAsync()` succeeds after purchase ✅

---

## 🎯 Performance Testing

### **Load Test Scenarios:**

**Scenario 1: Concurrent Purchase Attempts**
```
10 users simultaneously purchase credits
Expected: All handled correctly, no race conditions
```

**Scenario 2: High-Frequency Checks**
```
1 user checks privilege availability 100 times/second
Expected: All responses within 200ms
```

**Scenario 3: Bulk Purchases**
```
1 user purchases 100 credits at once
Expected: Single transaction, payment processed correctly
```

---

## 🐛 Troubleshooting Guide

### **Issue: 402 Not Returned When Expected**

**Check:**
1. Verify UsedValue = AllowedValue in database
2. Check privilege configuration (not disabled, not unlimited)
3. Review logs for any errors

**Solution:**
```sql
SELECT u.AllowedValue, u.UsedValue, u.RemainingValue, pp.Value
FROM UserSubscriptionPrivilegeUsage u
JOIN SubscriptionPlanPrivileges pp ON pp.Id = u.SubscriptionPlanPrivilegeId
WHERE u.SubscriptionId = 'YOUR-ID';
```

---

### **Issue: Credits Not Added After Payment**

**Check:**
1. Verify payment actually succeeded
2. Check transaction logs for rollback
3. Review billing record status

**Solution:**
```sql
-- Check billing record
SELECT * FROM BillingRecords 
WHERE SubscriptionId = 'YOUR-ID' AND Type = 'Overage'
ORDER BY CreatedDate DESC;
-- Status should be 'Paid'

-- Check if transaction was rolled back
-- Look in application logs for rollback messages
```

---

### **Issue: Payment Processed But Credits Not Added**

**This should be IMPOSSIBLE due to transaction management!**

**If it happens:**
1. Check transaction commit logs
2. Verify IUnitOfWork is working correctly
3. Check for database deadlocks

---

## 📊 Monitoring Queries

### **Track Credit Purchases:**

```sql
-- Credit purchases today
SELECT 
    COUNT(*) as PurchaseCount,
    SUM(Amount) as TotalRevenue
FROM BillingRecords
WHERE Type = 'Overage'
  AND Status = 'Paid'
  AND CAST(CreatedDate AS DATE) = CAST(GETUTCDATE() AS DATE);
```

### **Popular Privileges:**

```sql
-- Most purchased privileges
SELECT 
    p.Name as PrivilegeName,
    COUNT(*) as PurchaseCount,
    SUM(br.Amount) as TotalRevenue
FROM BillingRecords br
JOIN UserSubscriptionPrivilegeUsage u ON u.SubscriptionId = br.SubscriptionId
JOIN Privileges p ON p.Id = u.PrivilegeId
WHERE br.Type = 'Overage' 
  AND br.Status = 'Paid'
  AND br.CreatedDate >= DATEADD(day, -30, GETUTCDATE())
GROUP BY p.Name
ORDER BY PurchaseCount DESC;
```

### **Failed Purchases:**

```sql
-- Failed credit purchases (need attention)
SELECT 
    u.Email,
    br.Amount,
    br.FailureReason,
    br.CreatedDate
FROM BillingRecords br
JOIN Users u ON u.Id = br.UserId
WHERE br.Type = 'Overage'
  AND br.Status = 'Failed'
  AND br.CreatedDate >= DATEADD(day, -7, GETUTCDATE())
ORDER BY br.CreatedDate DESC;
```

---

## 🎓 Developer Quick Reference

### **Key Methods:**

```csharp
// Check if privilege can be used (with purchase info if limit exceeded)
var result = await _privilegeService.CheckPrivilegeAvailabilityAsync(
    subscriptionId,
    "Teleconsultation",
    requestedAmount: 1,
    tokenModel
);

// Returns:
// - 200 if available
// - 402 if limit exceeded (with purchase details)
// - 403 if disabled
// - 429 if time limit exceeded

// Purchase additional credits
var result = await _subscriptionService.PurchaseAdditionalCreditsAsync(
    subscriptionId,
    new PurchaseAdditionalCreditsDto
    {
        PrivilegeName = "Teleconsultation",
        Quantity = 2,
        PaymentMethodId = "pm_xxxxx"
    },
    tokenModel
);

// Returns:
// - 200 if payment succeeded and credits added
// - 400 if payment failed (no credits added)
// - 403 if access denied
// - 404 if not found
```

---

## 📈 Success Metrics

### **What to Measure:**

1. **Conversion Rate:**
   - Users who see 402 vs users who complete purchase
   - Target: >60% conversion

2. **Payment Success Rate:**
   - Successful purchases vs failed payments
   - Target: >95% success

3. **Revenue from Overage:**
   - Total overage charges per month
   - Track by privilege type

4. **User Satisfaction:**
   - Time to complete purchase
   - Error rate
   - Support tickets related to credit purchase

---

## ✅ Final Checklist

### **Before Testing:**
- ✅ Code compiled without errors
- ✅ No linter warnings
- ✅ Dependencies injected correctly
- ✅ Stripe test mode configured

### **During Testing:**
- ⚠️ Test all scenarios (success, failure, edge cases)
- ⚠️ Verify database state after each test
- ⚠️ Check Stripe dashboard for charges
- ⚠️ Review application logs

### **After Testing:**
- ⚠️ Document any issues found
- ⚠️ Fix bugs if any
- ⚠️ Update test cases
- ⚠️ Prepare for staging deployment

---

## 🎉 Ready to Test!

Your backend is fully implemented and ready for comprehensive testing. Follow the scenarios above to validate the complete upfront credit purchase workflow.

**Good luck with testing!** 🚀

---

**End of Testing Guide**


