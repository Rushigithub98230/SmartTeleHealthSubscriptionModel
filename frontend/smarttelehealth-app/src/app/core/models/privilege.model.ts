/**
 * Privilege DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/PrivilegeDto.cs
 * and backend/SmartTelehealth.Core/Entities/Privilege.cs
 */

/**
 * Privilege DTO (Response)
 */
export interface PrivilegeDto {
  id: string;                    // GUID
  name: string;
  description?: string;
  privilegeTypeId: string;
  privilegeTypeName: string;
  isActive: boolean;
  createdDate: Date;
  updatedDate: Date;
}

/**
 * Create Privilege DTO (Request - Admin Only)
 */
export interface CreatePrivilegeDto {
  name: string;                  // Required, max 100
  description?: string;          // Max 500
  privilegeTypeId: string;       // Required GUID
  isActive: boolean;
}

/**
 * Update Privilege DTO (Request - Admin Only)
 */
export interface UpdatePrivilegeDto {
  name: string;
  description?: string;
  privilegeTypeId: string;
  isActive: boolean;
}

/**
 * User Subscription Privilege Usage DTO (Response)
 * Matches backend/SmartTelehealth.Core/Entities/UserSubscriptionPrivilegeUsage.cs
 */
export interface PrivilegeUsageDto {
  id: string;
  subscriptionId: string;
  privilegeId: string;
  subscriptionPlanPrivilegeId: string;
  privilegeName: string;
  
  // Usage Tracking
  allocatedLimit: number;        // Total allowed (-1 = unlimited)
  usedValue: number;             // Currently consumed
  allowedValue: number;          // Remaining available
  
  // Period Management
  usagePeriodStart: Date;
  usagePeriodEnd: Date;
  lastUsedAt?: Date;
  resetAt?: Date;
  notes?: string;
  
  // Computed Properties
  remainingValue: number;        // allowedValue - usedValue
  usagePercentage: number;       // (usedValue / allowedValue) * 100
  isUnlimited: boolean;          // allocatedLimit === -1
  isExhausted: boolean;          // usedValue >= allowedValue
  isCurrentPeriod: boolean;
  
  // Navigation
  privilege: PrivilegeDto;
}

/**
 * Privilege Usage Summary (Response)
 * Container for subscription's all privilege usages
 */
export interface PrivilegeUsageSummary {
  subscriptionId: string;
  periodStart: Date;
  periodEnd: Date;
  privileges: PrivilegeUsageDto[];
}

/**
 * Privilege Availability Check (Response)
 */
export interface PrivilegeAvailability {
  available: boolean;
  remaining: number;
  allowed: number;
  used: number;
  isUnlimited: boolean;
  usagePercentage: number;
  periodEnd: Date;
  requiresPayment?: boolean;
  overageCost?: number;
}

/**
 * Use Privilege DTO (Request)
 */
export interface UsePrivilegeDto {
  subscriptionId: string;
  privilegeName: string;
  amount: number;                // Quantity to use
  relatedEntityId?: string;      // Optional link (appointment ID, etc.)
}

/**
 * Privilege Usage Result (Response)
 */
export interface PrivilegeUsageResult {
  usedValue: number;
  remainingValue: number;
  allowedValue: number;
  success: boolean;
  message?: string;
}

/**
 * Privilege Usage History DTO (Response)
 * Matches backend/SmartTelehealth.Core/Entities/PrivilegeUsageHistory.cs
 */
export interface PrivilegeUsageHistory {
  id: string;
  userSubscriptionPrivilegeUsageId: string;
  usedValue: number;
  usedAt: Date;
  usageDate: Date;
  usageWeek: string;             // YYYY-WW format
  usageMonth: string;            // YYYY-MM format
  notes?: string;
  
  // Computed
  weekKey: string;
  monthKey: string;
}

/**
 * Update Time-Based Limits DTO (Request - Admin)
 */
export interface UpdateTimeBasedLimitsDto {
  privilegeId: string;           // Required
  dailyLimit: number;
  weeklyLimit: number;
  monthlyLimit: number;
  // usagePeriodId: string;      // REMOVED: Not used - resets based on billing cycle
  durationMonths: number;
  description?: string;
  effectiveDate: Date;
  expirationDate?: Date;
}


