/**
 * Analytics DTOs
 * For admin dashboard and reporting
 */

/**
 * Subscription Analytics DTO (Response - Admin)
 */
export interface SubscriptionAnalyticsDto {
  totalActiveSubscriptions: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;    // MRR
  annualRecurringRevenue: number;     // ARR
  averageRevenuePerUser: number;      // ARPU
  churnRate: number;
  subscriptionsByStatus: { [status: string]: number };
  subscriptionsByPlan: { [planName: string]: number };
  growthRate: number;
  period: string;
}

/**
 * Revenue Analytics DTO (Response - Admin)
 */
export interface RevenueAnalyticsDto {
  totalRevenue: number;
  subscriptionRevenue: number;
  overageRevenue: number;
  consultationRevenue: number;
  revenueByMonth: MonthlyRevenue[];
  revenueByPlan: PlanRevenue[];
  period: string;
}

export interface MonthlyRevenue {
  month: string;               // "2025-01"
  revenue: number;
  subscriptions: number;
}

export interface PlanRevenue {
  planName: string;
  revenue: number;
  activeSubscriptions: number;
}

/**
 * Usage Statistics DTO (Response)
 */
export interface UsageStatisticsDto {
  privilegeName: string;
  totalUsage: number;
  averageUsagePerUser: number;
  overageCount: number;
  overageRevenue: number;
}

/**
 * Billing Statistics DTO (Response - Admin)
 */
export interface BillingStatisticsDto {
  totalBillingRecords: number;
  paidRecords: number;
  pendingRecords: number;
  failedRecords: number;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  successRate: number;
  period: string;
}


