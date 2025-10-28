import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AnalyticsService } from '../../../../core/services';
import { 
  SubscriptionAnalyticsDto, 
  RevenueAnalyticsDto, 
  UsageStatisticsDto 
} from '../../../../core/models';

/**
 * Admin Analytics Component
 * Advanced analytics with charts and visualizations
 * 
 * APIs Used:
 * - GET /api/SubscriptionAnalytics/overview
 * - GET /api/Analytics/revenue
 * - GET /api/Analytics/usage
 * 
 * Route: /webadmin/analytics
 * Access: Admin only
 * 
 * Note: Chart.js integration would be added here for visual charts
 */
@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss']
})
export class AnalyticsComponent implements OnInit {
  subscriptionAnalytics: SubscriptionAnalyticsDto | null = null;
  revenueAnalytics: RevenueAnalyticsDto | null = null;
  usageStatistics: UsageStatisticsDto[] = [];
  
  loading = {
    subscriptions: false,
    revenue: false,
    usage: false
  };

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.loadAllAnalytics();
  }

  /**
   * Load all analytics data
   */
  loadAllAnalytics(): void {
    this.loadSubscriptionAnalytics();
    this.loadRevenueAnalytics();
    this.loadUsageStatistics();
  }

  /**
   * Load subscription analytics
   * API: GET /api/SubscriptionAnalytics/overview
   */
  loadSubscriptionAnalytics(): void {
    this.loading.subscriptions = true;

    this.analyticsService.getSubscriptionAnalytics().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscriptionAnalytics = response.data;
        }
        this.loading.subscriptions = false;
      },
      error: (error) => {
        console.error('Error loading subscription analytics:', error);
        this.loading.subscriptions = false;
      }
    });
  }

  /**
   * Load revenue analytics
   * API: GET /api/Analytics/revenue
   */
  loadRevenueAnalytics(): void {
    this.loading.revenue = true;

    this.analyticsService.getRevenueAnalytics().subscribe({
      next: (response) => {
        console.log('[AnalyticsComponent] Revenue analytics response:', response);
        if (response.statusCode === 200) {
          this.revenueAnalytics = response.data;
          console.log('[AnalyticsComponent] Revenue analytics data:', this.revenueAnalytics);
          console.log('[AnalyticsComponent] RevenueByPlan type:', typeof this.revenueAnalytics?.revenueByPlan);
          console.log('[AnalyticsComponent] RevenueByPlan value:', this.revenueAnalytics?.revenueByPlan);
        }
        this.loading.revenue = false;
      },
      error: (error) => {
        console.error('Error loading revenue analytics:', error);
        this.loading.revenue = false;
      }
    });
  }

  /**
   * Load usage statistics
   * API: GET /api/Analytics/usage
   */
  loadUsageStatistics(): void {
    this.loading.usage = true;

    this.analyticsService.getUsageStatistics().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.usageStatistics = response.data;
        }
        this.loading.usage = false;
      },
      error: (error) => {
        console.error('Error loading usage statistics:', error);
        this.loading.usage = false;
      }
    });
  }

  /**
   * Get status entries as array
   */
  getStatusEntries(): Array<{status: string, count: number}> {
    if (!this.subscriptionAnalytics?.subscriptionsByStatus) return [];
    return Object.entries(this.subscriptionAnalytics.subscriptionsByStatus).map(([status, count]) => ({
      status,
      count: count as number
    }));
  }

  /**
   * Get plan revenue entries
   */
  getPlanRevenueEntries(): Array<{plan: string, revenue: number}> {
    if (!this.revenueAnalytics?.revenueByPlan || !Array.isArray(this.revenueAnalytics.revenueByPlan)) {
      console.log('[AnalyticsComponent] revenueByPlan is not an array:', this.revenueAnalytics?.revenueByPlan);
      return [];
    }
    return this.revenueAnalytics.revenueByPlan.map(pr => ({
      plan: pr.planName,
      revenue: pr.revenue
    }));
  }
}


