import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { BaseChartDirective } from 'ng2-charts';
import { EnhancedAnalyticsService, DashboardMetrics, RealTimeMetrics } from '../../../../core/services/enhanced-analytics.service';

/**
 * Enhanced Dashboard Component
 * Comprehensive analytics dashboard with real-time updates and Chart.js visualizations
 */
@Component({
  selector: 'app-enhanced-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './enhanced-dashboard.component.html',
  styleUrls: ['./enhanced-dashboard.component.scss']
})
export class EnhancedDashboardComponent implements OnInit, OnDestroy {
  dashboardData: DashboardMetrics | null = null;
  realTimeMetrics: RealTimeMetrics | null = null;
  loading = false;
  error: string | null = null;
  
  // Date range options
  dateRangeOptions = [
    { value: '7d', label: 'Last 7 days' },
    { value: '30d', label: 'Last 30 days' },
    { value: '90d', label: 'Last 90 days' },
    { value: '1y', label: 'Last year' },
    { value: 'custom', label: 'Custom range' }
  ];
  selectedDateRange = '30d';
  customStartDate: Date | null = null;
  customEndDate: Date | null = null;
  
  // Real-time polling
  private pollingSubscription?: Subscription;
  private realTimeSubscription?: Subscription;
  
  // Chart data
  revenueChartData: any = null;
  subscriptionGrowthChartData: any = null;
  churnChartData: any = null;
  planPerformanceChartData: any = null;

  // Chart options
  planPerformanceChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        beginAtZero: true,
        ticks: {
          callback: (value: any) => '$' + value.toLocaleString()
        }
      }
    },
    plugins: {
      legend: {
        display: false
      }
    }
  };

  revenueChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value: any) => '$' + value.toLocaleString()
        }
      }
    },
    plugins: {
      legend: {
        display: false
      }
    }
  };

  subscriptionGrowthChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top'
      }
    }
  };

  churnChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'right'
      }
    }
  };

  constructor(private analyticsService: EnhancedAnalyticsService) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.startRealTimePolling();
  }

  ngOnDestroy(): void {
    this.stopRealTimePolling();
  }

  /**
   * Load comprehensive dashboard data
   */
  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    const startDate = this.getStartDate();
    const endDate = this.getEndDate();

    this.analyticsService.getDashboardMetrics(startDate, endDate).subscribe({
      next: (response: any) => {
        if (response.statusCode === 200) {
          this.dashboardData = response.data;
          this.initializeCharts();
        } else {
          this.error = response.message || 'Failed to load dashboard data';
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading dashboard data:', error);
        this.error = 'Failed to load dashboard data. Please try again.';
        this.loading = false;
      }
    });
  }

  /**
   * Start real-time polling for live metrics
   */
  startRealTimePolling(): void {
    this.pollingSubscription = this.analyticsService.startPolling((data: any) => {
      this.realTimeMetrics = data;
    });

    // Subscribe to real-time metrics updates
    this.realTimeSubscription = this.analyticsService.realTimeMetrics$.subscribe({
      next: (data: any) => {
        if (data) {
          this.realTimeMetrics = data;
        }
      },
      error: (error: any) => {
        console.error('Real-time metrics error:', error);
      }
    });
  }

  /**
   * Stop real-time polling
   */
  stopRealTimePolling(): void {
    this.analyticsService.stopPolling();
    this.pollingSubscription?.unsubscribe();
    this.realTimeSubscription?.unsubscribe();
  }

  /**
   * Handle date range change
   */
  onDateRangeChange(): void {
    this.loadDashboardData();
  }

  /**
   * Refresh dashboard data (can be called manually or by events)
   */
  refreshDashboard(): void {
    this.loadDashboardData();
  }

  /**
   * Handle real-time metrics update
   */
  onRealTimeMetricsUpdate(metrics: RealTimeMetrics): void {
    this.realTimeMetrics = metrics;
    // Optionally refresh charts or specific data
    this.updateChartsWithRealTimeData();
  }

  /**
   * Update charts with real-time data
   */
  private updateChartsWithRealTimeData(): void {
    if (this.realTimeMetrics && this.dashboardData) {
      // Update revenue chart with latest data
      if (this.revenueChartData) {
        this.revenueChartData.datasets[0].data.push(this.realTimeMetrics.revenueToday);
        this.revenueChartData.labels.push(new Date().toLocaleDateString());
        
        // Keep only last 30 data points
        if (this.revenueChartData.datasets[0].data.length > 30) {
          this.revenueChartData.datasets[0].data.shift();
          this.revenueChartData.labels.shift();
        }
      }
    }
  }

  /**
   * Export analytics data
   */
  exportAnalytics(format: 'pdf' | 'csv' | 'excel'): void {
    const startDate = this.getStartDate();
    const endDate = this.getEndDate();

    this.analyticsService.exportAnalytics(format, startDate, endDate).subscribe({
      next: (blob: any) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `analytics-${this.selectedDateRange}-${new Date().toISOString().split('T')[0]}.${format}`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error: any) => {
        console.error('Export error:', error);
        this.error = 'Failed to export analytics data';
      }
    });
  }

  /**
   * Initialize Chart.js charts
   */
  private initializeCharts(): void {
    if (!this.dashboardData) return;

    // Revenue Trend Chart
    this.revenueChartData = {
      labels: Array.isArray(this.dashboardData.revenue?.monthlyRevenue) 
        ? this.dashboardData.revenue.monthlyRevenue.map((m: any) => m.month) 
        : [],
      datasets: [{
        label: 'Revenue',
        data: Array.isArray(this.dashboardData.revenue?.monthlyRevenue) 
          ? this.dashboardData.revenue.monthlyRevenue.map((m: any) => m.amount) 
          : [],
        borderColor: '#007bff',
        backgroundColor: 'rgba(0, 123, 255, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };

    // Subscription Growth Chart
    this.subscriptionGrowthChartData = {
      labels: Object.keys(this.dashboardData.subscriptions?.subscriptionsByStatus || {}),
      datasets: [{
        label: 'Subscriptions',
        data: Object.values(this.dashboardData.subscriptions?.subscriptionsByStatus || {}),
        backgroundColor: [
          '#28a745', '#17a2b8', '#ffc107', '#6c757d', '#dc3545', '#343a40', '#fd7e14'
        ]
      }]
    };

    // Churn Analysis Chart
    this.churnChartData = {
      labels: this.dashboardData.churn?.churnReasons?.map((r: any) => r.reason) || [],
      datasets: [{
        label: 'Churn Reasons',
        data: this.dashboardData.churn?.churnReasons?.map((r: any) => r.count) || [],
        backgroundColor: [
          '#dc3545', '#fd7e14', '#ffc107', '#28a745', '#17a2b8', '#6c757d'
        ]
      }]
    };

    // Plan Performance Chart
    this.planPerformanceChartData = {
      labels: this.dashboardData.plans?.map((p: any) => p.planName) || [],
      datasets: [{
        label: 'Revenue',
        data: this.dashboardData.plans?.map((p: any) => p.revenue) || [],
        backgroundColor: '#007bff'
      }]
    };
  }

  /**
   * Get start date based on selected range
   */
  private getStartDate(): Date | undefined {
    if (this.selectedDateRange === 'custom') {
      return this.customStartDate || undefined;
    }

    const now = new Date();
    switch (this.selectedDateRange) {
      case '7d':
        return new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      case '30d':
        return new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
      case '90d':
        return new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000);
      case '1y':
        return new Date(now.getTime() - 365 * 24 * 60 * 60 * 1000);
      default:
        return undefined;
    }
  }

  /**
   * Get end date based on selected range
   */
  private getEndDate(): Date | undefined {
    if (this.selectedDateRange === 'custom') {
      return this.customEndDate || undefined;
    }
    return new Date();
  }

  /**
   * Format currency
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  /**
   * Format percentage
   */
  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  /**
   * Format number
   */
  formatNumber(value: number): string {
    return new Intl.NumberFormat('en-US').format(value);
  }

  /**
   * Get growth indicator class
   */
  getGrowthClass(growth: number): string {
    if (growth > 0) return 'text-success';
    if (growth < 0) return 'text-danger';
    return 'text-muted';
  }

  /**
   * Get growth icon
   */
  getGrowthIcon(growth: number): string {
    if (growth > 0) return 'bi-arrow-up';
    if (growth < 0) return 'bi-arrow-down';
    return 'bi-dash';
  }

  /**
   * Check if custom date range is selected
   */
  isCustomDateRange(): boolean {
    return this.selectedDateRange === 'custom';
  }

  /**
   * Get last updated time
   */
  getLastUpdatedTime(): string {
    if (!this.realTimeMetrics) return '';
    return new Date(this.realTimeMetrics.lastUpdated).toLocaleTimeString();
  }

  /**
   * Get object keys for template
   */
  getObjectKeys(obj: any): string[] {
    return Object.keys(obj || {});
  }

  /**
   * Math.abs helper for template
   */
  abs(value: number): number {
    return Math.abs(value);
  }
}
