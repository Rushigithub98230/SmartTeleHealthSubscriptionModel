/**
 * Subscription Plan DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/SubscriptionPlanDto.cs
 * and backend/SmartTelehealth.Core/Entities/SubscriptionPlan.cs
 */

import { CategoryDto } from './category.model';
import { PrivilegeDto } from './privilege.model';

/**
 * Subscription Plan DTO (Response)
 */
export interface SubscriptionPlanDto {
  id: string;                    // GUID
  name: string;
  description: string;
  shortDescription?: string;
  basePrice: number;             // Base plan price (PrivilegesTotalCost + AdminCommission)
  discountPercentage?: number;   // Promotional or admin-defined discount (%)
  discountValidUntil?: Date;     // Discount validity end date
  billingCycleId: string;
  billingCycleName: string;
  billingCycleDescription: string;
  billingCycleDurationInDays: number;
  currencyId: string;
  categoryId: string;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  
  // Marketing Properties
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  
  // Stripe Integration
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  
  // Metadata
  features?: string;             // JSON string
  terms?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  
  // Computed Properties
  effectivePrice: number;
  hasActiveDiscount: boolean;
  isCurrentlyAvailable: boolean;
  
  // Healthcare Pricing Model
  isAutoCalculatedPrice: boolean;
  adminCommissionPercent?: number;
  priceChangeNoticeDays: number;
  privilegesTotalCost: number;
  
  // Billing Discount
  billingDiscountPercentage?: number; // 0-100, billing cycle discount (%)
  
  // Legacy fields for backward compatibility (will be removed)
  price?: number;                     // Legacy - use basePrice
  discountedPrice?: number;           // Legacy - use discountPercentage
  billingDiscount?: number;           // Legacy - use billingDiscountPercentage
  
  // Plan Features
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  maxConcurrentUsers: number;
  gracePeriodDays: number;
  
  // Versioning
  versionNumber: number;
  isLatestVersion: boolean;
  parentPlanId?: string;
  
  // Relationships
  planPrivileges?: PlanPrivilegeDto[];
  category?: CategoryDto;
  
  // Audit
  createdDate: Date;
  updatedDate?: Date;
}

/**
 * Create Subscription Plan DTO (Request - Admin Only)
 */
export interface CreateSubscriptionPlanDto {
  name: string;                    // Required, max 100
  description?: string;            // Max 500
  shortDescription?: string;       // Max 200
  basePrice?: number;              // ✅ Optional - calculated automatically from privileges
  discountPercentage?: number;     // Promotional discount (%)
  discountValidUntil?: Date;
  billingCycleId: string;          // Required GUID
  currencyId: string;              // Required GUID
  categoryId: string;              // Required GUID
  
  // Trial Configuration
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  
  // Marketing Properties
  isFeatured: boolean;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  
  // Plan Features
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  maxConcurrentUsers: number;
  gracePeriodDays: number;
  
  // Status
  isActive: boolean;
  
  // Metadata
  features?: string;
  terms?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  
  // Stripe Integration
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  
  // Privileges (can be included in create request)
  privileges: PlanPrivilegeDto[];
  
  // Healthcare Pricing Model
  isAutoCalculatedPrice: boolean;
  adminCommissionPercent?: number;   // 0-100
  priceChangeNoticeDays: number;     // Default: 10
  
  // Billing Cycle Discount
  billingDiscountPercentage?: number; // 0-100, billing cycle discount (%)
}

/**
 * Update Subscription Plan DTO (Request - Admin Only)
 */
export interface UpdateSubscriptionPlanDto {
  id: string;
  name: string;
  description?: string;
  basePrice: number;
  billingCycleId: string;
  currencyId: string;
  categoryId: string;
  isActive: boolean;
  
  // Marketing Properties
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder?: number;
  
  // Healthcare Pricing Model
  isAutoCalculatedPrice: boolean;
  adminCommissionPercent?: number;
  priceChangeNoticeDays: number;
  
  // Promotional Discount
  discountPercentage?: number;       // Promotional discount (%)
  discountValidUntil?: Date;
  
  // Billing Discount
  billingDiscountPercentage?: number; // 0-100, billing cycle discount (%)
}

/**
 * Plan Privilege DTO (Configuration within a plan)
 */
export interface PlanPrivilegeDto {
  privilegeId: string;           // Required GUID
  value: number;                 // Total count: -1=unlimited, 0=disabled, >0=count
  durationMonths: number;
  description?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  
  // Pricing (Healthcare Model)
  privilegeBaseCost: number;     // Cost per unit for plan price calculation
  unitCost: number;              // Overage cost per unit when user exceeds total
}

/**
 * Subscription Plan Filter DTO (Request)
 * Matches backend/SmartTelehealth.Core/DTOs/SubscriptionPlanFilterDto.cs
 */
export interface SubscriptionPlanFilterDto {
  // Pagination
  page: number;                  // Default: 1
  pageSize: number;              // Default: 50
  
  // Search
  searchTerm?: string;
  
  // Category and Classification
  categoryId?: string;
  categoryName?: string;
  
  // Status and Features
  isActive?: boolean;
  isFeatured?: boolean;
  isMostPopular?: boolean;
  isTrending?: boolean;
  isTrialAllowed?: boolean;
  
  // Pricing
  minPrice?: number;
  maxPrice?: number;
  exactPrice?: number;
  currencyId?: string;
  
  // Billing Cycle
  billingCycleId?: string;
  billingCycleName?: string;
  
  // Date Ranges
  createdDateFrom?: Date;
  createdDateTo?: Date;
  updatedDateFrom?: Date;
  updatedDateTo?: Date;
  effectiveDateFrom?: Date;
  effectiveDateTo?: Date;
  
  // Trial Duration
  minTrialDuration?: number;
  maxTrialDuration?: number;
  
  // Display Order
  minDisplayOrder?: number;
  maxDisplayOrder?: number;
  
  // Stripe Integration
  stripeProductId?: string;
  hasStripeIntegration?: boolean;
  
  // Sorting
  sortColumn: string;            // Default: "DisplayOrder"
  sortOrder: string;             // "asc" or "desc"
  
  // Additional Filters
  planIds?: string[];
  excludePlanIds?: string[];
  hasActiveSubscriptions?: boolean;
  hasSubscriptions?: boolean;
}


