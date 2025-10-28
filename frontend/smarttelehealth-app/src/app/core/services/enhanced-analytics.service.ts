import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, interval, Subscription } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';

/**
 * Enhanced Analytics Service
 * Provides comprehensive analytics data for the admin dashboard with real-time polling
 */
@Injectable({
  providedIn: 'root'
})
export class EnhancedAnalyticsService {
  private readonly apiUrl = '/api/admin/analytics';
  private readonly pollingInterval = 60000; // 60 seconds
  private pollingSubscription?: Subscription;
  
  // Real-time metrics subject for live updates
  private realTimeMetricsSubject = new BehaviorSubject<RealTimeMetrics | null>(null);
  public realTimeMetrics$ = this.realTimeMetricsSubject.asObservable();

  constructor(private http: HttpClient) {
    // Listen for relevant events that should trigger analytics refresh
    this.setupEventListeners();
  }

  /**
   * Setup event listeners for real-time updates
   */
  private setupEventListeners(): void {
    // Listen for subscription events
    window.addEventListener('subscription.created', () => this.refreshMetrics());
    window.addEventListener('subscription.cancelled', () => this.refreshMetrics());
    window.addEventListener('subscription.activated', () => this.refreshMetrics());
    window.addEventListener('subscription.paused', () => this.refreshMetrics());
    
    // Listen for payment events
    window.addEventListener('payment.completed', () => this.refreshMetrics());
    window.addEventListener('payment.failed', () => this.refreshMetrics());
    window.addEventListener('payment.refunded', () => this.refreshMetrics());
    
    // Listen for plan events
    window.addEventListener('plan.created', () => this.refreshMetrics());
    window.addEventListener('plan.updated', () => this.refreshMetrics());
    window.addEventListener('plan.deleted', () => this.refreshMetrics());
  }

  /**
   * Refresh metrics immediately
   */
  private refreshMetrics(): void {
    this.getRealTimeMetrics().subscribe({
      next: (response: any) => {
        if (response.statusCode === 200 && response.data) {
          this.realTimeMetricsSubject.next(response.data);
        }
      },
      error: (error: any) => {
        console.error('Error refreshing metrics:', error);
      }
    });
  }

  /**
   * Get comprehensive dashboard metrics
   */
  getDashboardMetrics(startDate?: Date, endDate?: Date): Observable<ApiResponse<DashboardMetrics>> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get<ApiResponse<DashboardMetrics>>(`${this.apiUrl}/dashboard`, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Get real-time metrics for live dashboard updates
   */
  getRealTimeMetrics(): Observable<ApiResponse<RealTimeMetrics>> {
    return this.http.get<ApiResponse<RealTimeMetrics>>(`${this.apiUrl}/real-time-metrics`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Get revenue analytics for specified period
   */
  getRevenueAnalytics(period: string = '30d'): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/revenue`, {
      params: { period }
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get churn analytics
   */
  getChurnAnalytics(period: string = '30d'): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/churn`, {
      params: { period }
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get plan performance analytics
   */
  getPlanPerformance(): Observable<ApiResponse<PlanPerformance[]>> {
    return this.http.get<ApiResponse<PlanPerformance[]>>(`${this.apiUrl}/plan-performance`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Get subscription management dashboard data
   */
  getSubscriptionManagementDashboard(startDate?: Date, endDate?: Date): Observable<ApiResponse<SubscriptionManagementDashboard>> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get<ApiResponse<SubscriptionManagementDashboard>>(`${this.apiUrl}/subscription-management-dashboard`, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Start real-time polling for metrics updates
   */
  startPolling(callback?: (data: RealTimeMetrics) => void): Subscription {
    // Stop existing polling if any
    this.stopPolling();

    this.pollingSubscription = interval(this.pollingInterval)
      .pipe(
        map(() => this.getRealTimeMetrics()),
        catchError(this.handleError)
      )
      .subscribe({
        next: (response: any) => {
          if (response.statusCode === 200 && response.data) {
            this.realTimeMetricsSubject.next(response.data);
            if (callback) {
              callback(response.data);
            }
          }
        },
        error: (error: any) => {
          console.error('Error in real-time polling:', error);
        }
      });

    // Initial load
    this.getRealTimeMetrics().subscribe({
      next: (response: any) => {
        if (response.statusCode === 200 && response.data) {
          this.realTimeMetricsSubject.next(response.data);
          if (callback) {
            callback(response.data);
          }
        }
      },
      error: (error: any) => {
        console.error('Error loading initial real-time metrics:', error);
      }
    });

    return this.pollingSubscription;
  }

  /**
   * Stop real-time polling
   */
  stopPolling(): void {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      this.pollingSubscription = undefined;
    }
  }

  /**
   * Get current real-time metrics (synchronous)
   */
  getCurrentRealTimeMetrics(): RealTimeMetrics | null {
    return this.realTimeMetricsSubject.value;
  }

  /**
   * Export analytics data
   */
  exportAnalytics(format: 'pdf' | 'csv' | 'excel', startDate?: Date, endDate?: Date): Observable<Blob> {
    const params: any = { format };
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get(`${this.apiUrl}/export`, {
      params,
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError = (error: any): Observable<never> => {
    console.error('Analytics service error:', error);
    throw error;
  };
}

// Data Models
export interface DashboardMetrics {
  overview: OverviewMetrics;
  revenue: RevenueMetrics;
  subscriptions: SubscriptionMetrics;
  churn: ChurnMetrics;
  plans: PlanMetrics[];
  realTime: RealTimeMetrics;
}

export interface OverviewMetrics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  cancelledSubscriptions: number;
  pausedSubscriptions: number;
  trialSubscriptions: number;
  newSubscriptionsThisPeriod: number;
  cancelledSubscriptionsThisPeriod: number;
  averageSubscriptionValue: number;
  totalRevenue: number;
}

export interface RevenueMetrics {
  totalRevenue: number;
  monthlyRevenue: number;
  annualRevenue: number;
  totalSubscriptions: number;
  activeSubscriptions: number;
  newSubscriptionsThisMonth: number;
  cancelledSubscriptionsThisMonth: number;
  averageRevenuePerSubscription: number;
  totalRefunds: number;
  monthlyRecurringRevenue: number;
  averageRevenuePerUser: number;
  revenueGrowth: number;
  revenueByPlan: PlanRevenue[];
}

export interface SubscriptionMetrics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  pausedSubscriptions: number;
  cancelledSubscriptions: number;
  newSubscriptionsThisMonth: number;
  churnRate: number;
  averageSubscriptionValue: number;
  totalRevenue: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  subscriptionsByStatus: { [status: string]: number };
  subscriptionsByPlan: { [planName: string]: number };
}

export interface ChurnMetrics {
  churnRate: number;
  churnCount: number;
  retentionRate: number;
  averageLifetime: number;
  churnReasons: ChurnReason[];
  cohortRetention: CohortRetention[];
}

export interface PlanMetrics {
  planName: string;
  totalSubscriptions: number;
  activeSubscriptions: number;
  cancelledSubscriptions: number;
  newSubscriptionsThisPeriod: number;
  revenue: number;
  averageRevenue: number;
  churnRate: number;
  averageSubscriptionValue: number;
  conversionRate: number;
}

export interface RealTimeMetrics {
  activeSubscriptionsNow: number;
  revenueToday: number;
  newSubscriptionsToday: number;
  trialsEndingThisWeek: number;
  pendingPayments: number;
  lastUpdated: Date;
}

export interface PlanRevenue {
  planName: string;
  revenue: number;
  activeSubscriptions: number;
}

export interface ChurnReason {
  reason: string;
  count: number;
  percentage: number;
}

export interface CohortRetention {
  cohort: string;
  initialSubscriptions: number;
  retainedSubscriptions: number;
  retentionRate: number;
}

export interface PlanPerformance {
  planName: string;
  totalSubscriptions: number;
  activeSubscriptions: number;
  revenue: number;
  churnRate: number;
  growthRate: number;
}

export interface SubscriptionManagementDashboard {
  churnAnalytics: any;
  privilegeUsageAnalytics: any;
  subscriptionLifecycleAnalytics: any;
  enhancedBillingAnalytics: any;
  generatedAt: Date;
  period: {
    startDate: Date;
    endDate: Date;
  };
}
