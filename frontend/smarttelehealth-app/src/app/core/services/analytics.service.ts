import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

export interface DashboardMetrics {
  kpis: {
    totalActive: number;
    totalTrial: number;
    totalPaused: number;
    totalCancelled: number;
    mrr: number;
    arr: number;
    churnRate: number;
    growthRate: number;
    totalSubscriptions: number;
  };
  actionItems: {
    renewalsDueToday: number;
    failedPayments: number;
    trialsEnding: number;
    suspendedAccounts: number;
  };
  recentActivity: Array<{
    type: string;
    userId: number;
    userName: string;
    planId: string;
    planName: string;
    amount: number;
    timestamp: Date;
  }>;
}

export interface GrowthData {
  period: string;
  startDate: Date;
  endDate: Date;
  growthData: Array<{
    date?: string;
    weekStart?: string;
    weekEnd?: string;
    month?: string;
    monthName?: string;
    newSubscriptions: number;
    cancellations: number;
    netGrowth: number;
  }>;
}

export interface RealTimeMetrics {
  timestamp: Date;
  activeSubscriptions: number;
  trialSubscriptions: number;
  newSubscriptionsToday: number;
  cancellationsToday: number;
  revenueToday: number;
  failedPaymentsToday: number;
  systemStatus: string;
}

export interface SubscriptionDueForRenewal {
  id: string;
  userId: number;
  userName: string;
  subscriptionPlanId: string;
  planName: string;
  currentPrice: number;
  nextBillingDate: Date;
  daysUntilRenewal: number;
  status: string;
  autoRenew: boolean;
}

export interface TrialEnding {
  id: string;
  userId: number;
  userName: string;
  userEmail: string;
  subscriptionPlanId: string;
  planName: string;
  currentPrice: number;
  trialStartDate: Date;
  trialEndDate: Date;
  trialDurationInDays: number;
  daysUntilEnd: number;
  daysInTrial: number;
  status: string;
  hasPaymentMethod: boolean;
}

export interface PlanMigrationAnalytics {
  summary: {
    totalMigrations: number;
    pendingMigrations: number;
    completedMigrations: number;
    userOptedOutMigrations: number;
    failedMigrations: number;
    dueToday: number;
    dueInNext7Days: number;
    noDecisionCount: number;
  };
  userDecisions: {
    acceptCount: number;
    cancelCount: number;
    noDecisionCount: number;
    acceptanceRate: number;
    totalDecisions: number;
  };
  migrationsByPlan: Array<{
    planId: string;
    totalMigrations: number;
    pending: number;
    completed: number;
    userOptedOut: number;
    failed: number;
  }>;
  recentMigrations: Array<{
    id: string;
    subscriptionId: string;
    fromPlanId: string;
    toPlanId: string;
    status: string;
    userDecision: string | null;
    notificationDate: Date;
    scheduledMigrationDate: Date;
    userDecisionDate: Date | null;
    completedDate: Date | null;
  }>;
  generatedAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly baseUrl = 'SubscriptionAnalytics';
  private readonly adminUrl = 'admin/subscriptions';

  constructor(private commonService: CommonService) {}

  /**
   * Get dashboard metrics with KPIs, action items, and recent activity
   */
  getDashboardMetrics(
    dashboardType: string = 'overview',
    startDate?: Date,
    endDate?: Date
  ): Observable<ApiResponse<DashboardMetrics>> {
    let url = `${this.baseUrl}/dashboard?dashboardType=${dashboardType}`;
    
    if (startDate) {
      url += `&startDate=${startDate.toISOString()}`;
    }
    if (endDate) {
      url += `&endDate=${endDate.toISOString()}`;
    }

    return this.commonService.get<DashboardMetrics>(url);
  }

  /**
   * Get growth analytics with time-series data
   */
  getGrowthData(
    period: 'daily' | 'weekly' | 'monthly' = 'monthly',
    startDate?: Date,
    endDate?: Date
  ): Observable<ApiResponse<GrowthData>> {
    let url = `${this.baseUrl}/growth?period=${period}`;
    
    if (startDate) {
      url += `&startDate=${startDate.toISOString()}`;
    }
    if (endDate) {
      url += `&endDate=${endDate.toISOString()}`;
    }

    return this.commonService.get<GrowthData>(url);
  }

  /**
   * Get revenue analytics for a date range
   */
  getRevenueAnalytics(startDate?: Date, endDate?: Date): Observable<any> {
    let url = `${this.baseUrl}/revenue`;
    
    if (startDate) {
      url += `?startDate=${startDate.toISOString()}`;
    }
    if (endDate) {
      url += `${startDate ? '&' : '?'}endDate=${endDate.toISOString()}`;
    }

    return this.commonService.get<any>(url);
  }

  /**
   * Get churn analytics for a date range
   */
  getChurnAnalytics(startDate?: Date, endDate?: Date): Observable<any> {
    let url = `${this.baseUrl}/churn`;
    
    if (startDate) {
      url += `?startDate=${startDate.toISOString()}`;
    }
    if (endDate) {
      url += `${startDate ? '&' : '?'}endDate=${endDate.toISOString()}`;
    }

    return this.commonService.get<any>(url);
  }

  /**
   * Get real-time metrics for live dashboard updates
   */
  getRealTimeMetrics(): Observable<ApiResponse<RealTimeMetrics>> {
    return this.commonService.get<RealTimeMetrics>(`${this.baseUrl}/realtime`);
  }

  /**
   * Get subscriptions due for renewal
   */
  getSubscriptionsDueForRenewal(daysAhead: number = 7): Observable<ApiResponse<SubscriptionDueForRenewal[]>> {
    return this.commonService.get<SubscriptionDueForRenewal[]>(
      `${this.adminUrl}/due-for-renewal?daysAhead=${daysAhead}`
    );
  }

  /**
   * Get trial subscriptions ending soon
   */
  getTrialsEnding(daysAhead: number = 7): Observable<ApiResponse<TrialEnding[]>> {
    return this.commonService.get<TrialEnding[]>(
      `${this.adminUrl}/trials-ending?daysAhead=${daysAhead}`
    );
  }

  /**
   * Get subscription analytics (alias for getDashboardMetrics)
   */
  getSubscriptionAnalytics(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/subscription-analytics`);
  }

  /**
   * Get usage statistics
   */
  getUsageStatistics(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/usage-statistics`);
  }

  /**
   * Get MRR (Monthly Recurring Revenue)
   */
  getMRR(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/mrr`);
  }

  /**
   * Get churn rate
   */
  getChurnRate(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/churn-rate`);
  }

  /**
   * Get plan migration analytics for admin dashboard
   * Shows scheduled migrations, user decisions, and auto-cancellations
   */
  getPlanMigrationAnalytics(): Observable<ApiResponse<PlanMigrationAnalytics>> {
    return this.commonService.get<PlanMigrationAnalytics>(`admin/analytics/plan-migrations`);
  }
}
