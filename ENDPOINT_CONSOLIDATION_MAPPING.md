# 🎯 **ENDPOINT CONSOLIDATION MAPPING**
## **Subscription Plan & Privilege Endpoints Audit**

---

## 📋 **EXECUTIVE SUMMARY**

This document provides a comprehensive mapping of all subscription plan and privilege related endpoints across all controllers. This audit ensures we capture **EVERY** endpoint before consolidation to avoid missing any functionality.

---

## 🔍 **CURRENT ENDPOINT INVENTORY**

### **📋 SUBSCRIPTION PLAN ENDPOINTS**

#### **SubscriptionPlansController** (Current - 15 endpoints)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| GET | `/api/subscriptionplans/active` | Get active plans (public) | Public |
| POST | `/api/subscriptionplans/filter` | Filter plans (public) | Public |
| GET | `/api/subscriptionplans/category/{categoryId}` | Get plans by category | Public |
| GET | `/api/subscriptionplans/{id}` | Get specific plan | Public |
| POST | `/api/subscriptionplans/{planId}/activate` | Activate plan | Admin |
| POST | `/api/subscriptionplans/{planId}/deactivate` | Deactivate plan | Admin |
| GET | `/api/subscriptionplans/admin` | Get all plans (admin) | Admin |
| GET | `/api/subscriptionplans/admin/active` | Get active plans (admin) | Admin |
| GET | `/api/subscriptionplans/admin/category/{category}` | Get plans by category (admin) | Admin |
| GET | `/api/subscriptionplans/admin/{planId}` | Get specific plan (admin) | Admin |
| POST | `/api/subscriptionplans/admin` | Create plan | Admin |
| PUT | `/api/subscriptionplans/admin/{planId}` | Update plan | Admin |
| DELETE | `/api/subscriptionplans/admin/{planId}` | Delete plan | Admin |
| GET | `/api/subscriptionplans/admin/paged` | Get paginated plans | Admin |
| GET | `/api/subscriptionplans/public` | Get public plans | Public |

#### **SubscriptionManagementController** (To be moved - 8 endpoints)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| GET | `/webadmin/subscription-management/plans` | Get all plans (admin) | Admin |
| POST | `/webadmin/subscription-management/plans/filter` | Filter plans (admin) | Admin |
| POST | `/webadmin/subscription-management/plans` | Create plan | Admin |
| PUT | `/webadmin/subscription-management/plans/{id}` | Update plan | Admin |
| DELETE | `/webadmin/subscription-management/plans/{id}` | Delete plan | Admin |
| GET | `/webadmin/subscription-management/plans/{id}` | Get specific plan | Admin |
| POST | `/webadmin/subscription-management/plans/{id}/activate` | Activate plan | Admin |
| POST | `/webadmin/subscription-management/plans/{id}/deactivate` | Deactivate plan | Admin |

#### **AdminSubscriptionController** (To be moved - 4 endpoints)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| GET | `/api/admin/adminsubscription/plans` | Get all plans | Admin |
| POST | `/api/admin/adminsubscription/plans` | Create plan | Admin |
| PUT | `/api/admin/adminsubscription/plans/{id}` | Update plan | Admin |
| DELETE | `/api/admin/adminsubscription/plans/{id}` | Delete plan | Admin |

#### **SubscriptionsController** (To be moved - 2 endpoints)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| GET | `/api/subscriptions/plans` | Get all plans | Public |
| GET | `/api/subscriptions/plans/{planId}` | Get specific plan | Public |

---

### **🔐 PRIVILEGE ENDPOINTS**

#### **SubscriptionPlanPrivilegesController** (Current - 2 endpoints)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| PUT | `/api/subscriptionplanprivileges/time-based-limits` | Update time-based limits | Admin |
| GET | `/api/subscriptionplanprivileges/{planPrivilegeId}/time-based-limits` | Get time-based limits | Admin |

#### **PrivilegesController** (To be moved - 8 endpoints)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| GET | `/api/privileges` | Get all privileges | Admin |
| GET | `/api/privileges/{id}` | Get specific privilege | Admin |
| POST | `/api/privileges` | Create privilege | Admin |
| PUT | `/api/privileges/{id}` | Update privilege | Admin |
| DELETE | `/api/privileges/{id}` | Delete privilege | Admin |
| GET | `/api/privileges/categories` | Get privilege categories | Admin |
| GET | `/api/privileges/types` | Get privilege types | Admin |
| GET | `/api/privileges/usage-history` | Get usage history | Admin |
| GET | `/api/privileges/usage-summary` | Get usage summary | Admin |
| GET | `/api/privileges/usage-export` | Export usage data | Admin |

#### **ProviderPrivilegesController** (To be moved - 1 endpoint)
| Method | Endpoint | Purpose | Access Level |
|--------|----------|---------|--------------|
| GET | `/api/providerprivileges/{userId}/privileges` | Get user privileges | Provider |

---

## 🎯 **CONSOLIDATION PLAN**

### **📋 SUBSCRIPTION PLANS CONSOLIDATION**

#### **Target Controller: SubscriptionPlansController**

**Endpoints to KEEP (15 existing):**
- All current endpoints in `SubscriptionPlansController` ✅

**Endpoints to MOVE (14 from other controllers):**
- 8 from `SubscriptionManagementController`
- 4 from `AdminSubscriptionController` 
- 2 from `SubscriptionsController`

**Total after consolidation: 29 endpoints**

#### **Consolidation Strategy:**
1. **Keep existing endpoints** in `SubscriptionPlansController`
2. **Move admin operations** from other controllers
3. **Standardize endpoint naming** (remove `/webadmin/` and `/api/admin/` prefixes)
4. **Consolidate duplicate functionality** (merge similar endpoints)
5. **Maintain both public and admin access levels**

### **🔐 PRIVILEGES CONSOLIDATION**

#### **Target Controller: SubscriptionPlanPrivilegesController**

**Endpoints to KEEP (2 existing):**
- Current time-based limits endpoints ✅

**Endpoints to MOVE (9 from other controllers):**
- 8 from `PrivilegesController`
- 1 from `ProviderPrivilegesController`

**Total after consolidation: 11 endpoints**

#### **Consolidation Strategy:**
1. **Keep existing endpoints** in `SubscriptionPlanPrivilegesController`
2. **Move all privilege management** from `PrivilegesController`
3. **Move user privilege operations** from `ProviderPrivilegesController`
4. **Add comprehensive privilege CRUD operations**
5. **Add privilege usage tracking and analytics**

---

## 📊 **DETAILED ENDPOINT MAPPING**

### **SUBSCRIPTION PLANS - FINAL STRUCTURE**

```
SubscriptionPlansController
├── PUBLIC ENDPOINTS
│   ├── GET /api/subscriptionplans/active
│   ├── POST /api/subscriptionplans/filter
│   ├── GET /api/subscriptionplans/category/{categoryId}
│   ├── GET /api/subscriptionplans/{id}
│   └── GET /api/subscriptionplans/public
│
├── ADMIN ENDPOINTS
│   ├── GET /api/subscriptionplans/admin
│   ├── GET /api/subscriptionplans/admin/active
│   ├── GET /api/subscriptionplans/admin/category/{category}
│   ├── GET /api/subscriptionplans/admin/{planId}
│   ├── GET /api/subscriptionplans/admin/paged
│   ├── POST /api/subscriptionplans/admin
│   ├── PUT /api/subscriptionplans/admin/{planId}
│   ├── DELETE /api/subscriptionplans/admin/{planId}
│   ├── POST /api/subscriptionplans/{planId}/activate
│   └── POST /api/subscriptionplans/{planId}/deactivate
```

### **PRIVILEGES - FINAL STRUCTURE**

```
SubscriptionPlanPrivilegesController
├── PRIVILEGE MANAGEMENT
│   ├── GET /api/subscriptionplanprivileges
│   ├── GET /api/subscriptionplanprivileges/{id}
│   ├── POST /api/subscriptionplanprivileges
│   ├── PUT /api/subscriptionplanprivileges/{id}
│   └── DELETE /api/subscriptionplanprivileges/{id}
│
├── PLAN-PRIVILEGE RELATIONSHIPS
│   ├── GET /api/subscriptionplanprivileges/plans/{planId}
│   ├── POST /api/subscriptionplanprivileges/plans/{planId}
│   └── DELETE /api/subscriptionplanprivileges/plans/{planId}/{privilegeId}
│
├── USER PRIVILEGE OPERATIONS
│   ├── GET /api/subscriptionplanprivileges/users/{userId}
│   └── PUT /api/subscriptionplanprivileges/users/{userId}/{privilegeId}
│
├── TIME-BASED LIMITS
│   ├── GET /api/subscriptionplanprivileges/{planPrivilegeId}/time-based-limits
│   └── PUT /api/subscriptionplanprivileges/time-based-limits
│
├── USAGE TRACKING
│   ├── GET /api/subscriptionplanprivileges/usage-history
│   ├── GET /api/subscriptionplanprivileges/usage-summary
│   └── GET /api/subscriptionplanprivileges/usage-export
│
└── MASTER DATA
    ├── GET /api/subscriptionplanprivileges/categories
    └── GET /api/subscriptionplanprivileges/types
```

---

## 🚨 **DUPLICATE ENDPOINTS TO CONSOLIDATE**

### **Subscription Plan Duplicates:**

1. **Plan Creation:**
   - `SubscriptionPlansController` → `POST /api/subscriptionplans/admin`
   - `SubscriptionManagementController` → `POST /webadmin/subscription-management/plans`
   - `AdminSubscriptionController` → `POST /api/admin/adminsubscription/plans`
   - **Action:** Keep one, remove others

2. **Plan Updates:**
   - `SubscriptionPlansController` → `PUT /api/subscriptionplans/admin/{planId}`
   - `SubscriptionManagementController` → `PUT /webadmin/subscription-management/plans/{id}`
   - `AdminSubscriptionController` → `PUT /api/admin/adminsubscription/plans/{id}`
   - **Action:** Keep one, remove others

3. **Plan Deletion:**
   - `SubscriptionPlansController` → `DELETE /api/subscriptionplans/admin/{planId}`
   - `SubscriptionManagementController` → `DELETE /webadmin/subscription-management/plans/{id}`
   - `AdminSubscriptionController` → `DELETE /api/admin/adminsubscription/plans/{id}`
   - **Action:** Keep one, remove others

4. **Get All Plans:**
   - `SubscriptionPlansController` → `GET /api/subscriptionplans/admin`
   - `SubscriptionManagementController` → `GET /webadmin/subscription-management/plans`
   - `AdminSubscriptionController` → `GET /api/admin/adminsubscription/plans`
   - **Action:** Keep one, remove others

---

## ✅ **IMPLEMENTATION CHECKLIST**

### **Phase 1: SubscriptionPlansController Consolidation**

- [ ] **Audit existing endpoints** in `SubscriptionPlansController`
- [ ] **Move admin operations** from `SubscriptionManagementController`
- [ ] **Move admin operations** from `AdminSubscriptionController`
- [ ] **Move public operations** from `SubscriptionsController`
- [ ] **Consolidate duplicate endpoints** (keep best implementation)
- [ ] **Standardize endpoint naming** and structure
- [ ] **Test all consolidated endpoints**
- [ ] **Update documentation**

### **Phase 2: SubscriptionPlanPrivilegesController Consolidation**

- [ ] **Audit existing endpoints** in `SubscriptionPlanPrivilegesController`
- [ ] **Move privilege management** from `PrivilegesController`
- [ ] **Move user privilege operations** from `ProviderPrivilegesController`
- [ ] **Add missing privilege CRUD operations**
- [ ] **Add privilege usage tracking and analytics**
- [ ] **Standardize endpoint naming** and structure
- [ ] **Test all consolidated endpoints**
- [ ] **Update documentation**

### **Phase 3: Cleanup**

- [ ] **Remove obsolete controllers:**
  - [ ] `SubscriptionManagementController`
  - [ ] `AdminSubscriptionController`
  - [ ] `PrivilegesController`
  - [ ] `ProviderPrivilegesController`
- [ ] **Clean up unused dependencies**
- [ ] **Update routing and references**
- [ ] **Final testing and validation**

---

## 📝 **NOTES**

1. **Total Endpoints to Consolidate:** 23 endpoints
2. **Final Controller Count:** 2 controllers (down from 5)
3. **Duplicate Endpoints:** 12 duplicates to be consolidated
4. **Estimated Implementation Time:** 3-4 days
5. **Risk Level:** Medium (comprehensive testing required)

---

**📅 Created:** [Current Date]  
**👤 Prepared By:** [Your Name]  
**📧 Contact:** [Your Email]  
** Version:** 1.0 (Initial Audit)
