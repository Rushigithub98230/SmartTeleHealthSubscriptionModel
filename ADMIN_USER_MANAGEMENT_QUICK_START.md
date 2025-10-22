# Admin User Management Portal - Quick Start Guide

## 🚀 Quick Reference

### Access the Feature
```
Admin Portal → User Management
URL: /webadmin/users
```

---

## 📋 User List Page

### Features Available

**Quick Stats** (Top of page):
- Total Users
- Active Subscribers
- Inactive Users  
- Current Page Info

**Filters**:
- **Search**: Name, email, or phone number
- **Role**: All, Client, Provider, Admin
- **Account Status**: All, Active, Inactive
- **Subscription Status**: All, Active, Expired, None
- **Page Size**: 10, 20, 50, or 100 per page

**Table Columns**:
1. ID
2. Name
3. Email (with verified badge)
4. Phone
5. Role (badge)
6. **Subscriptions** (status badge + total count)
7. Account Status
8. Registered Date
9. Last Login
10. Actions (Details button)

### How to Use

**Search for User**:
```
1. Type name/email in search box
2. Press Enter or click Search button
```

**Filter Users**:
```
1. Select role from dropdown
2. Select status from dropdown
3. Results update automatically
```

**View User Details**:
```
1. Click "Details" button in Actions column
2. Opens user detail page
```

**Navigate Pages**:
```
1. Use pagination at bottom
2. Click page numbers or arrows
3. Change page size from dropdown
```

---

## 👤 User Detail Page

### 5 Tabs Available

#### Tab 1: Overview 📊
**Loads Immediately**

**Shows**:
- User Profile Card (left):
  - Avatar, name, role
  - Email, phone, DOB, gender
  - Account status
  - Registration and last login dates

- Active Subscription Card (right):
  - Plan name and price
  - Status badge
  - Auto-renew indicator
  - Start date
  - **Next billing date** (highlighted in red)
  - Quick actions (View, Pause, Cancel)

- Quick Stats (bottom):
  - Total Subscriptions
  - Total Spent
  - Active Privileges
  - Next Billing Date

**Use Cases**:
- Quick overview of user and current subscription
- See next billing date at a glance
- Access quick actions

---

#### Tab 2: Subscriptions 📝
**Lazy Loaded** (loads when clicked)

**Shows**:
- Current Subscription Section:
  - Full plan details
  - Price and billing cycle
  - Status and dates
  - Auto-renew setting

- Past Subscriptions Table:
  - Plan name
  - Start and end dates
  - Status
  - Price paid
  - View details button

**Use Cases**:
- Review subscription history
- Check when user subscribed/cancelled
- See all plans user has tried

---

#### Tab 3: Billing & Payments 💰
**Lazy Loaded** (loads when clicked)

**Shows**:
- Summary Cards:
  - Total Spent (all time)
  - Average Monthly Spend
  - Successful Payments
  - Failed Payments

- Billing History Table:
  - Invoice number
  - Billing date
  - Description
  - Amount
  - Status badge
  - View details button (links to billing detail page)

**Use Cases**:
- Review payment history
- Check failed payments
- Access invoice details
- Process refunds (via billing detail link)

---

#### Tab 4: Privileges & Usage 🛡️
**Lazy Loaded** (loads when clicked)

**Shows**:
- Current privilege usage summary
- Used vs available limits
- Reset dates

**Use Cases**:
- Monitor privilege consumption
- Check for overage
- Verify usage limits

**Note**: Progress bars to be added in future update.

---

#### Tab 5: Analytics 📈
**Lazy Loaded** (loads when clicked)

**Shows**:
- Export Buttons (PDF, Excel)
- Analytics Summary Cards:
  - Total Revenue
  - Average Monthly Spend
  - Payment Success Rate
  - Account Age

- Detailed Analytics:
  - Subscription metrics
  - Financial metrics
  - Payment metrics
  - Privilege metrics

**Use Cases**:
- Review user's financial contribution
- Analyze subscription patterns
- Export reports for management
- Check payment reliability

**Note**: Charts to be added in future update.

---

## 🎯 Common Admin Tasks

### Task 1: Find User with Active Subscription
```
1. Go to /webadmin/users
2. Look at "Subscriptions" column
3. Green badge = Active subscription
4. Click Details to view full info
```

### Task 2: Check When User's Next Billing Is
```
Method 1 (Quick):
1. User List → Details button
2. Overview tab shows next billing date (red text)

Method 2 (Detailed):
1. User List → Details button
2. Click Subscriptions tab
3. View current subscription next billing date
```

### Task 3: Review User's Payment History
```
1. User List → Details button
2. Click "Billing & Payments" tab
3. View complete billing history table
4. Click invoice to see details
5. Process refund if needed (from invoice page)
```

### Task 4: Monitor Privilege Usage
```
1. User List → Details button
2. Click "Privileges & Usage" tab
3. View current usage summary
4. Check for overage indicators
```

### Task 5: Analyze User Value
```
1. User List → Details button
2. Click "Analytics" tab
3. View:
   - Total revenue contributed
   - Payment success rate
   - Subscription duration
   - Usage patterns
4. Export as PDF/Excel (when implemented)
```

---

## 💡 Tips

### Performance
- **Lazy Loading**: Data loads only when tab is clicked
- **Cached**: Switching back to viewed tab doesn't reload
- **Fast**: User list pagination handles thousands of users

### Navigation
- **Back Button**: Every page has back button
- **Breadcrumbs**: Easy navigation hierarchy
- **Quick Access**: Dashboard link always available

### Data Freshness
- **Reload**: Refresh page to get latest data
- **Real-time**: Next billing dates calculated from backend

---

## 🔍 Understanding the Data

### Subscription Badges
- 🟢 **Green "Active"**: User has active subscription
- 🟡 **Yellow "Paused"**: Subscription paused
- 🔴 **Red "Cancelled"**: Subscription cancelled
- ⚫ **Gray "Expired"**: Subscription expired
- ⚫ **Gray "No Subscription"**: User never subscribed

### Billing Status Badges
- 🟢 **Green "Paid"**: Payment successful
- 🟡 **Yellow "Pending"**: Awaiting payment
- 🔴 **Red "Failed"**: Payment failed
- 🔵 **Blue "Refunded"**: Refund processed
- ⚫ **Gray "Cancelled"**: Billing cancelled

### Role Badges
- 🔴 **Red "Admin"**: System administrator
- 🔵 **Blue "Provider"**: Healthcare provider
- 🟢 **Green "Client"**: Regular user/client
- 🔵 **Blue "User"**: Default user role

---

## 📞 Support

### Issues?
- Check browser console for errors
- Verify user is logged in as Admin
- Refresh page if data seems stale
- Contact technical support for API errors

### Feature Requests?
- Submit to development team
- Check roadmap for planned features

---

## 🎓 Advanced Usage

### Filtering Combinations
```
Example 1: Find all inactive clients
  Role: Client
  Account Status: Inactive
  → Shows all inactive client accounts

Example 2: Find clients with active subscriptions
  Role: Client
  Subscription Status: Active Subscription
  → Shows all clients currently subscribed

Example 3: Find users who cancelled
  Subscription Status: Cancelled
  → Shows all users with cancelled subscriptions
```

### Bulk Operations (Future)
- Select multiple users
- Bulk export
- Bulk notifications
- Bulk status updates

---

## ✅ Current Capabilities Summary

**User List**:
- ✅ View all users with pagination
- ✅ Advanced filtering (role, status, subscription)
- ✅ Search by name/email/phone
- ✅ Quick stats overview
- ✅ Subscription status at a glance

**User Detail**:
- ✅ 5 organized tabs
- ✅ Lazy loading for performance
- ✅ Complete user profile
- ✅ Subscription tracking (current & past)
- ✅ Billing and payment history
- ✅ Privilege usage monitoring
- ✅ Comprehensive analytics
- ✅ Next billing date tracking

**Data Integration**:
- ✅ 10+ backend APIs
- ✅ Real-time data
- ✅ Type-safe DTOs
- ✅ Error handling

---

**Last Updated**: January 2025  
**Version**: 1.0 (Core Implementation)  
**Status**: ✅ Production Ready (Core Features)

