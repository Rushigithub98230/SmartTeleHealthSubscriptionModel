import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DashboardSummary {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalRevenue: number;
  monthlyRevenue: number;
  churnRate: number;
  avgSubscriptionValue: number;
  topPlans: any[];
  recentActivity: any[];
}

export interface RevenueMetrics {
  totalRevenue: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  revenueGrowth: number;
  revenueByPlan: any[];
  revenueByMonth: any[];
}

export interface ChurnAnalysis {
  churnRate: number;
  churnByPlan: any[];
  churnReasons: any[];
  retentionRate: number;
  customerLifetime: number;
}

export interface PlanPerformance {
  planId: string;
  planName: string;
  totalSubscriptions: number;
  activeSubscriptions: number;
  revenue: number;
  churnRate: number;
  avgDuration: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly apiUrl = `${environment.apiUrl}/api/admin/analytics`;

  constructor(private http: HttpClient) {}

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.apiUrl}/dashboard`);
  }

  getRevenueMetrics(startDate?: string, endDate?: string): Observable<RevenueMetrics> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.http.get<RevenueMetrics>(`${this.apiUrl}/revenue`, { params });
  }

  getChurnAnalysis(period: string = 'month'): Observable<ChurnAnalysis> {
    return this.http.get<ChurnAnalysis>(`${this.apiUrl}/churn`, { 
      params: { period } 
    });
  }

  getPlanPerformance(): Observable<PlanPerformance[]> {
    return this.http.get<PlanPerformance[]>(`${this.apiUrl}/plan-performance`);
  }

  exportAnalytics(type: string, format: string = 'csv'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, {
      params: { type, format },
      responseType: 'blob'
    });
  }

  getSubscriptionStatistics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/statistics`);
  }

  getSubscriptionTrends(period: string = '30days'): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/trends`, {
      params: { period }
    });
  }

  getUserGrowthMetrics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/user-growth`);
  }

  getPaymentAnalytics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/payments`);
  }
}