import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AnalyticsService } from '../../../core/services';
import { SubscriptionAnalyticsDto } from '../../../core/models';
import { SubscriptionDashboardComponent } from './subscription-dashboard/subscription-dashboard.component';

/**
 * Admin Dashboard Component
 * Display KPIs, charts, and analytics overview
 * 
 * APIs Used:
 * - GET /api/SubscriptionAnalytics (analytics overview)
 * - GET /api/SubscriptionAnalytics/revenue (for MRR)
 * - GET /api/SubscriptionAnalytics/churn (for churn rate)
 * 
 * Route: /webadmin/dashboard
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SubscriptionDashboardComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  analytics: SubscriptionAnalyticsDto | null = null;
  mrr: number = 0;
  churnRate: number = 0;
  loading = {
    overview: false,
    mrr: false,
    churn: false
  };
  error: string | null = null;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.loadAnalytics();
  }

  /**
   * Load all analytics data
   */
  loadAnalytics(): void {
    this.loadOverview();
    this.loadMRR();
    this.loadChurnRate();
  }

  /**
   * Load subscription analytics overview
   * API: GET /api/SubscriptionAnalytics
   */
  loadOverview(): void {
    this.loading.overview = true;

    this.analyticsService.getSubscriptionAnalytics().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.analytics = response.data;
        }
        this.loading.overview = false;
      },
      error: (error) => {
        console.error('Error loading analytics:', error);
        this.analytics = null; // Set default on error
        this.loading.overview = false;
      }
    });
  }

  /**
   * Load Monthly Recurring Revenue
   * API: GET /api/SubscriptionAnalytics/revenue
   */
  loadMRR(): void {
    this.loading.mrr = true;

    this.analyticsService.getMRR().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          // Extract MRR from revenue analytics response
          this.mrr = response.data.mrr || response.data.totalMRR || response.data.monthlyRecurringRevenue || 0;
        }
        this.loading.mrr = false;
      },
      error: (error) => {
        console.error('Error loading MRR:', error);
        this.mrr = 0; // Set default value on error
        this.loading.mrr = false;
      }
    });
  }

  /**
   * Load Churn Rate
   * API: GET /api/SubscriptionAnalytics/churn
   */
  loadChurnRate(): void {
    this.loading.churn = true;

    this.analyticsService.getChurnRate().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          // Extract churn rate from churn analytics response
          this.churnRate = response.data.churnRate || response.data.rate || response.data.churnPercentage || 0;
        }
        this.loading.churn = false;
      },
      error: (error) => {
        console.error('Error loading churn rate:', error);
        this.churnRate = 0; // Set default value on error
        this.loading.churn = false;
      }
    });
  }

  /**
   * Get status entries as array for ngFor
   */
  getStatusEntries(): Array<{status: string, count: number}> {
    if (!this.analytics?.subscriptionsByStatus) return [];
    
    return Object.entries(this.analytics.subscriptionsByStatus).map(([status, count]) => ({
      status,
      count: count as number
    }));
  }

  /**
   * Get plan entries as array for ngFor
   */
  getPlanEntries(): Array<{plan: string, count: number}> {
    if (!this.analytics?.subscriptionsByPlan) return [];
    
    return Object.entries(this.analytics.subscriptionsByPlan).map(([plan, count]) => ({
      plan,
      count: count as number
    }));
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success',
      'Pending': 'bg-warning',
      'Cancelled': 'bg-danger',
      'Paused': 'bg-secondary',
      'Expired': 'bg-dark'
    };
    return map[status] || 'bg-secondary';
  }
}


