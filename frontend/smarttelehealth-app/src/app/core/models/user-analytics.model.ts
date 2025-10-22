/**
 * User Analytics DTOs for Admin Portal
 * Matches backend DTOs for user monitoring and analytics
 */

export interface UserAnalyticsDto {
  userId: number;
  userName: string;
  userEmail: string;
  
  // Subscription Analytics
  totalSubscriptions: number;
  activeSubscriptions: number;
  pastSubscriptions: number;
  cancelledSubscriptions: number;
  averageSubscriptionDurationDays: number;
  currentPlan?: string;
  currentSubscriptionStartDate?: Date;
  nextBillingDate?: Date;
  
  // Financial Analytics
  totalRevenue: number;
  averageMonthlySpend: number;
  totalPaid: number;
  totalRefunded: number;
  
  // Payment Analytics
  totalPayments: number;
  successfulPayments: number;
  failedPayments: number;
  paymentSuccessRate: number;
  
  // Privilege Analytics
  activePrivileges: number;
  privilegeUsageRate: number;
  hasOverageCharges: boolean;
  
  // Account Analytics
  accountCreatedDate: Date;
  lastLoginDate?: Date;
  lastActivityDate?: Date;
  accountAgeDays: number;
  isActiveAccount: boolean;
}

export interface SubscriptionAnalyticsDetailDto {
  subscriptionTimeline: SubscriptionTimelineDto[];
  monthlyRevenue: MonthlyRevenueDto[];
  planDistribution: PlanDistributionDto[];
}

export interface SubscriptionTimelineDto {
  date: Date;
  planName: string;
  status: string;
  amount: number;
}

export interface MonthlyRevenueDto {
  month: string;
  revenue: number;
  paymentCount: number;
}

export interface PlanDistributionDto {
  planName: string;
  count: number;
  totalRevenue: number;
}

export interface PrivilegeUsageSummaryDto {
  userId: number;
  subscriptionId: string;
  privileges: PrivilegeUsageDetailDto[];
  totalUsedPercentage: number;
  nextResetDate: Date;
}

export interface PrivilegeUsageDetailDto {
  privilegeId: string;
  privilegeName: string;
  used: number;
  limit: number;
  percentage: number;
  isUnlimited: boolean;
  hasOverage: boolean;
}

export interface PaymentScheduleDto {
  subscriptionId: string;
  upcomingPayments: UpcomingPaymentDto[];
}

export interface UpcomingPaymentDto {
  date: Date;
  amount: number;
  description: string;
}

