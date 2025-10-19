import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import {
  SubscriptionAnalyticsDto,
  RevenueAnalyticsDto,
  UsageStatisticsDto,
  BillingStatisticsDto
} from '../models';

/**
 * Analytics Service (Admin Only)
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/SubscriptionAnalytics (general analytics)
 * - GET /api/SubscriptionAnalytics/revenue
 * - GET /api/SubscriptionAnalytics/churn
 * - GET /api/Analytics/revenue
 * - GET /api/Analytics/usage
 */
@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  constructor(private commonService: CommonService) {}

  /**
   * Get subscription analytics overview
   * API: GET /api/SubscriptionAnalytics
   * Used in: Admin Dashboard
   */
  getSubscriptionAnalytics(startDate?: Date, endDate?: Date): Observable<ApiResponse<SubscriptionAnalyticsDto>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<SubscriptionAnalyticsDto>('SubscriptionAnalytics', params);
  }

  /**
   * Get Monthly Recurring Revenue (MRR) from revenue analytics
   * API: GET /api/SubscriptionAnalytics/revenue
   * Used in: Admin Dashboard KPI Card
   */
  getMRR(startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<any>('SubscriptionAnalytics/revenue', params);
  }

  /**
   * Get churn analytics
   * API: GET /api/SubscriptionAnalytics/churn
   * Used in: Admin Analytics Page
   */
  getChurnRate(startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<any>('SubscriptionAnalytics/churn', params);
  }

  /**
   * Get revenue analytics (Admin only)
   * API: GET /api/admin/analytics/revenue
   * Used in: Admin Revenue Analytics Page
   */
  getRevenueAnalytics(startDate?: Date, endDate?: Date): Observable<ApiResponse<RevenueAnalyticsDto>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<RevenueAnalyticsDto>('admin/analytics/revenue', params);
  }

  /**
   * Get usage analytics for specific subscription
   * API: GET /api/SubscriptionAnalytics/usage/{subscriptionId}
   * Used in: Subscription Detail Analytics
   */
  getUsageAnalytics(subscriptionId: string, startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<any>(`SubscriptionAnalytics/usage/${subscriptionId}`, params);
  }

  /**
   * Get general usage statistics (Admin only)
   * API: GET /api/admin/analytics/privilege-usage-analytics
   * Used in: Admin Usage Analytics Page
   */
  getUsageStatistics(startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<any>('admin/analytics/privilege-usage-analytics', params);
  }

  /**
   * Get billing statistics
   * API: GET /api/Billing/statistics
   * Used in: Admin Billing Analytics Page
   */
  getBillingStatistics(startDate?: Date, endDate?: Date): Observable<ApiResponse<BillingStatisticsDto>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get<BillingStatisticsDto>('Billing/statistics', params);
  }
}


