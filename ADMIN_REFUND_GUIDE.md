# 🔄 Admin Refund Processing Guide

## Quick Reference for Admin Portal

---

## 🎯 When to Use This Feature

Use the manual refund feature when:
- ✅ User cancels subscription mid-cycle
- ✅ Service not delivered as expected
- ✅ Customer complaint requires refund
- ✅ Billing error needs correction
- ✅ Goodwill gesture to customer

---

## 📍 How to Access Refund Feature

### Step 1: Navigate to Billing Records
```
Admin Portal → Billing → Billing Records
```

### Step 2: Select Billing Record
```
Click on specific billing record to view details
```

### Step 3: Verify Eligibility
```
Check: Status = "Paid" ✅
If not paid, refund button won't be visible
```

---

## 🔄 Processing a Refund

### Step-by-Step Process

```
┌─────────────────────────────────────────────────────────────┐
│                   REFUND PROCESSING FLOW                    │
└─────────────────────────────────────────────────────────────┘

STEP 1: Open Billing Detail Page
──────────────────────────────────
URL: /webadmin/billing/{billingRecordId}

View:
  ┌────────────────────────────────────────────────┐
  │ Billing Record Details                         │
  ├────────────────────────────────────────────────┤
  │ User: John Doe (john@example.com)              │
  │ Invoice #: INV-2025-001                        │
  │ Type: [Subscription]                           │
  │ Billing Date: Jan 1, 2025                      │
  │ Due Date: Jan 31, 2025                         │
  │                                                │
  │ Amount Breakdown:                              │
  │ Base Amount: $25.00                            │
  │ Tax: $2.50                                     │
  │ Total: $27.50                                  │
  │                                                │
  │ Status: [Paid] ← Must be "Paid" to refund    │
  └────────────────────────────────────────────────┘

STEP 2: Click "Process Refund" Button
──────────────────────────────────────
Location: Right sidebar under "Admin Actions"

  ┌────────────────────────────┐
  │ Admin Actions              │
  ├────────────────────────────┤
  │ [🔄 Process Refund]       │ ← Click here
  │ [📧 Resend Invoice]       │
  │ [👤 View User]            │
  └────────────────────────────┘

STEP 3: Refund Modal Opens
───────────────────────────
  ┌──────────────────────────────────────────────────────┐
  │ 🔄 Process Refund                            [X]     │
  ├──────────────────────────────────────────────────────┤
  │                                                       │
  │ ⚠️ Important: This will process refund via Stripe   │
  │                                                       │
  │ Billing Record: INV-2025-001                         │
  │ User: John Doe                                       │
  │ Total Amount: $27.50                                 │
  │ Status: [Paid]                                       │
  │                                                       │
  │ ────────────────────────────────────────────────     │
  │                                                       │
  │ Refund Amount: *                                     │
  │ [$27.50        ] ← Pre-filled, you can edit         │
  │ Maximum: $27.50 (Full Refund)                       │
  │                                                       │
  │ Refund Reason: *                                     │
  │ ┌───────────────────────────────────────────────┐   │
  │ │ [Type reason here]                            │   │
  │ │                                               │   │
  │ └───────────────────────────────────────────────┘   │
  │ This reason will be saved for audit purposes.        │
  │                                                       │
  │        [Cancel]       [Process Refund $27.50]       │
  └──────────────────────────────────────────────────────┘

STEP 4: Enter Refund Details
─────────────────────────────
A. Choose Refund Amount

   Option 1: Full Refund
   ────────────────────
   Keep amount: $27.50
   Use when: Complete refund needed

   Option 2: Partial Refund (Prorated)
   ───────────────────────────────────
   Example: User cancelled on Day 15 of 30-day cycle
   
   Calculate:
     Daily Rate = $27.50 / 30 days = $0.92/day
     Days Used = 15 days
     Days Remaining = 30 - 15 = 15 days
     Refund = 15 × $0.92 = $13.75
   
   Enter: $13.75
   
   Option 3: Custom Amount
   ──────────────────────
   Enter any amount: $0.01 to $27.50
   Use when: Special circumstances

B. Enter Refund Reason (Required)

   Good Examples:
   ──────────────
   ✅ "Mid-cycle cancellation on Day 15. Prorated refund for 15 unused days."
   ✅ "Service not delivered due to technical issues. Full refund."
   ✅ "Customer complaint - poor service quality. Goodwill refund."
   ✅ "Billing error - customer charged twice. Corrective refund."
   ✅ "Early cancellation within 7-day grace period. Full refund per policy."

   Bad Examples:
   ─────────────
   ❌ "Refund" (too vague)
   ❌ "User asked" (no context)
   ❌ "" (empty - won't be accepted)

STEP 5: Review and Confirm
───────────────────────────
Check your entries:
  ✅ Amount: $13.75 (correct?)
  ✅ Reason: "Mid-cycle cancellation..." (clear?)

Click: [Process Refund $13.75]

Confirmation popup:
  "Process refund of $13.75?"
  [Yes] [No]

Click: [Yes]

STEP 6: System Processes
─────────────────────────
You'll see:
  🔄 Processing spinner
  "Processing refund through Stripe..."

Backend does:
  ✅ Validate your inputs
  ✅ Create Stripe refund
  ✅ Update billing record
  ✅ Create refund audit record
  ✅ Log who processed it (you)

STEP 7: Success Confirmation
─────────────────────────────
Success message appears:
  ┌────────────────────────────────────────────────┐
  │ ✅ Refund processed successfully. Customer    │
  │    will receive $13.75 back to their payment  │
  │    method.                                    │
  └────────────────────────────────────────────────┘

Modal closes automatically
Billing record page reloads
Status may update to "Refunded" (if full refund)

Customer receives:
  💰 $13.75 back in 5-10 business days
  📧 Refund confirmation email
```

---

## 💡 Refund Decision Guide

### When to Refund (and How Much)

#### Scenario 1: Early Cancellation
```
Situation: User cancels within 7 days
Policy: Full refund grace period
Action: Refund 100%
Amount: $27.50
Reason: "Early cancellation within 7-day grace period. Full refund per policy."
```

#### Scenario 2: Mid-Cycle Cancellation
```
Situation: User cancels on Day 15 of 30-day cycle
Policy: Prorated refund for unused days
Action: Calculate prorated amount
Amount: $13.75 (50% unused)
Reason: "Mid-cycle cancellation on Day 15. Prorated refund for 15 unused days."
```

#### Scenario 3: Service Not Delivered
```
Situation: Technical issue prevented service access
Policy: Full refund for non-delivery
Action: Refund 100%
Amount: $27.50
Reason: "Service not delivered due to technical issues. Full refund."
```

#### Scenario 4: Customer Complaint
```
Situation: Unhappy customer, service quality issue
Policy: Goodwill partial refund
Action: Refund 50% as gesture
Amount: $13.75
Reason: "Customer complaint regarding service quality. Partial refund as goodwill gesture."
```

#### Scenario 5: Late Cancellation
```
Situation: User cancels on Day 28 of 30-day cycle
Policy: No refund (used most of service)
Action: Don't process refund
Amount: $0.00
Reason: Not applicable (explain to customer via email)
```

---

## ⚠️ Important Validation Rules

### Refund Will Be Rejected If:

❌ **Amount = $0.00**
```
Error: "Refund amount must be greater than 0"
Fix: Enter amount > $0.01
```

❌ **Amount > Total**
```
Example: Total = $27.50, You entered $30.00
Error: "Refund amount cannot exceed billing amount"
Fix: Enter amount ≤ $27.50
```

❌ **No Reason Entered**
```
Error: "Refund reason is required"
Fix: Enter detailed reason
```

❌ **Billing Record Not Paid**
```
Status: Pending / Failed / Cancelled
Error: Refund button not visible
Fix: Only "Paid" records can be refunded
```

---

## 📊 Refund Type Reference

### Full Refund
```
Amount: $27.50 (100%)
Indicator: "Full Refund"
Use when: Complete service failure or grace period
```

### Partial Refund
```
Amount: $13.75 (any amount < total)
Indicator: "Partial Refund"
Use when: Prorated, goodwill, or custom amount
```

### Prorated Refund Calculation
```
Formula:
  Daily Rate = Total Amount / Billing Period Days
  Unused Days = Total Days - Days Used
  Refund Amount = Daily Rate × Unused Days

Example (Monthly):
  Total: $27.50
  Period: 30 days
  Used: 15 days
  
  Daily Rate = $27.50 / 30 = $0.92/day
  Unused Days = 30 - 15 = 15 days
  Refund = $0.92 × 15 = $13.75

Example (Quarterly):
  Total: $75.00
  Period: 90 days
  Used: 30 days
  
  Daily Rate = $75.00 / 90 = $0.83/day
  Unused Days = 90 - 30 = 60 days
  Refund = $0.83 × 60 = $49.80
```

---

## 🔍 Finding Billing Records to Refund

### Method 1: From Subscription
```
1. Go to: /webadmin/subscriptions
2. Filter: Status = "Cancelled"
3. Click on cancelled subscription
4. View billing history
5. Click on billing record
6. Process refund
```

### Method 2: From Billing List
```
1. Go to: /webadmin/billing
2. Filter: Status = "Paid"
3. Search by user or invoice number
4. Click on billing record
5. Process refund
```

### Method 3: From User Profile
```
1. Go to: /webadmin/users
2. Search for user
3. View user's billing history
4. Click on billing record
5. Process refund
```

---

## 📝 Best Practices

### ✅ DO

1. **Review Before Refunding**
   - Check cancellation date
   - Verify usage statistics
   - Confirm refund eligibility per policy

2. **Calculate Prorated Amounts Accurately**
   - Use daily rate formula
   - Round to 2 decimal places
   - Document calculation in reason

3. **Write Clear Refund Reasons**
   - Explain why refund is being issued
   - Include relevant dates/details
   - Be professional and concise

4. **Double-Check Amount**
   - Verify calculation
   - Ensure doesn't exceed total
   - Consider tax implications

5. **Confirm Before Submitting**
   - Review all details
   - Verify customer info
   - Click "Yes" on confirmation

### ❌ DON'T

1. **Don't Refund Without Review**
   - Always check eligibility first
   - Don't auto-approve all requests

2. **Don't Use Vague Reasons**
   - Avoid "Refund", "User asked"
   - Provide context and details

3. **Don't Exceed Total Amount**
   - System will reject anyway
   - Can cause confusion

4. **Don't Process Multiple Times**
   - Check if refund already processed
   - Avoid duplicate refunds

5. **Don't Refund Non-Paid Records**
   - Only "Paid" status eligible
   - Button won't show for others

---

## 🆘 Troubleshooting

### Problem: Refund Button Not Visible

**Possible Causes**:
1. Billing record status ≠ "Paid"
2. Already fully refunded
3. Insufficient permissions

**Solution**:
- Check billing record status
- Verify you're logged in as admin
- Contact support if issue persists

---

### Problem: Error When Submitting Refund

**Error**: "Refund amount must be greater than 0"
**Solution**: Enter amount > $0.01

**Error**: "Refund amount cannot exceed billing amount"
**Solution**: Reduce amount to ≤ total amount

**Error**: "Refund reason is required"
**Solution**: Enter detailed reason in textarea

**Error**: "Failed to process refund"
**Solution**: Check Stripe connection, retry, or contact technical support

---

### Problem: Refund Processed But Status Didn't Change

**Explanation**: 
- Partial refunds don't change status to "Refunded"
- Only full refunds change status
- Both are tracked in refund history

**Verification**:
- Check refund history (if available)
- Verify in Stripe dashboard
- Check database refund records

---

## 📞 Support

### Need Help?
- **Technical Issues**: Contact IT Support
- **Policy Questions**: Review refund policy documentation
- **Stripe Issues**: Check Stripe dashboard or contact Stripe support

---

## 📚 Related Documentation

- **REFUND_POLICY_AND_IMPLEMENTATION.md** - Complete policy guide
- **REFUND_IMPLEMENTATION_SUMMARY.md** - Technical changes
- **IMPROVEMENTS_COMPLETED_SUMMARY.md** - Feature overview

---

## ✅ Quick Checklist

Before processing refund, verify:

- [ ] Billing record status = "Paid"
- [ ] Reviewed cancellation reason
- [ ] Calculated refund amount correctly
- [ ] Wrote clear, detailed refund reason
- [ ] Amount > $0 and ≤ total
- [ ] Confirmed customer details correct
- [ ] Ready to process refund

After processing refund:

- [ ] Success message appeared
- [ ] Billing record reloaded
- [ ] Customer will receive money (5-10 business days)
- [ ] Refund tracked in system
- [ ] Can verify in Stripe if needed

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Feature Status**: ✅ Active

