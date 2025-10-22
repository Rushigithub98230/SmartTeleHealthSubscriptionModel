import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { 
  UserService, 
  SubscriptionService, 
  BillingService, 
  PrivilegeService,
  CommonService 
} from '../../../../core/services';
import { 
  UserDto, 
  SubscriptionDto, 
  BillingRecordDto,
  UserAnalyticsDto
} from '../../../../core/models';
import { PrivilegeProgressBarComponent } from '../../../../shared/components/privilege-progress-bar.component';
import { LineChartComponent } from '../../../../shared/components/line-chart.component';
import { DoughnutChartComponent } from '../../../../shared/components/doughnut-chart.component';
import { BarChartComponent } from '../../../../shared/components/bar-chart.component';
import { PieChartComponent } from '../../../../shared/components/pie-chart.component';
import { ChartConfiguration } from 'chart.js';

/**
 * Admin User Detail Component - Comprehensive User Monitoring
 * 
 * Features:
 * - Tabbed interface with lazy loading
 * - Overview: User profile + active subscription + quick stats
 * - Subscriptions: Current and past subscriptions with details
 * - Billing: Complete billing and payment history
 * - Privileges: Usage tracking and history with progress bars
 * - Analytics: Charts and insights with export
 * - Subscription Actions: Pause, cancel, resume with modals
 * 
 * APIs Used:
 * - GET /api/Users/{id}
 * - GET /api/Subscriptions/user/{userId}
 * - GET /api/Billing/user/{userId}
 * - GET /api/Billing/payment-analytics/{userId}
 * - GET /api/PrivilegeBasedBilling/usage-summary/{userId}
 * - GET /api/Users/{userId}/analytics
 * - POST /api/Subscriptions/{id}/pause
 * - POST /api/Subscriptions/{id}/cancel
 * - POST /api/Subscriptions/{id}/resume
 * 
 * Route: /webadmin/users/:id
 * Access: Admin only
 */
@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    FormsModule, 
    PrivilegeProgressBarComponent,
    DoughnutChartComponent,
    BarChartComponent,
    PieChartComponent
  ],
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.scss']
})
export class UserDetailComponent implements OnInit {
  userId!: number;
  
  // Tab management
  activeTab: 'overview' | 'subscriptions' | 'billing' | 'privileges' | 'analytics' = 'overview';
  
  // Overview data (loaded immediately)
  user: UserDto | null = null;
  activeSubscription: SubscriptionDto | null = null;
  overviewStats = {
    totalSubscriptions: 0,
    totalSpent: 0,
    activePrivileges: 0,
    nextBillingDate: null as Date | null
  };
  overviewLoading = false;
  overviewError: string | null = null;
  
  // Subscriptions tab data (lazy loaded)
  subscriptionsData = {
    current: null as SubscriptionDto | null,
    past: [] as SubscriptionDto[],
    loading: false,
    loaded: false,
    error: null as string | null
  };
  
  // Billing tab data (lazy loaded)
  billingData = {
    records: [] as BillingRecordDto[],
    totalSpent: 0,
    averageMonthlySpend: 0,
    successfulPayments: 0,
    failedPayments: 0,
    loading: false,
    loaded: false,
    error: null as string | null
  };
  
  // Privileges tab data (lazy loaded)
  privilegeData = {
    usageSummary: null as any,
    loading: false,
    loaded: false,
    error: null as string | null
  };
  
  // Analytics tab data (lazy loaded)
  analyticsData = {
    userAnalytics: null as UserAnalyticsDto | null,
    paymentAnalytics: null as any,
    loading: false,
    loaded: false,
    error: null as string | null
  };

  // Modal state for subscription actions
  showPauseModal = false;
  showCancelModal = false;
  pauseReason = '';
  cancelReason = '';
  actionLoading = false;

  // Chart data
  paymentSuccessChartData: ChartConfiguration<'doughnut'>['data'] | null = null;
  subscriptionStatusChartData: ChartConfiguration<'pie'>['data'] | null = null;
  monthlyRevenueChartData: ChartConfiguration<'bar'>['data'] | null = null;
  privilegeUsageChartData: ChartConfiguration<'bar'>['data'] | null = null;

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private subscriptionService: SubscriptionService,
    private billingService: BillingService,
    private privilegeService: PrivilegeService,
    private commonService: CommonService
  ) {}

  ngOnInit(): void {
    this.userId = +this.route.snapshot.params['id'];
    this.loadOverview();
  }

  /**
   * Load overview tab data (always loaded on init)
   */
  loadOverview(): void {
    this.overviewLoading = true;
    this.overviewError = null;

    forkJoin({
      user: this.userService.getUserById(this.userId),
      subscriptions: this.subscriptionService.getUserSubscriptions(this.userId)
    }).subscribe({
      next: (results) => {
        // Process user
        if (results.user.statusCode === 200) {
          this.user = results.user.data;
        }

        // Process subscriptions
        if (results.subscriptions.statusCode === 200) {
          const subs = results.subscriptions.data;
          this.activeSubscription = subs.find((s: SubscriptionDto) => s.status === 'Active') || null;
          
          // Calculate overview stats
          this.overviewStats.totalSubscriptions = subs.length;
          this.overviewStats.nextBillingDate = this.activeSubscription?.nextBillingDate || null;
        }

        this.overviewLoading = false;
      },
      error: (error) => {
        this.overviewError = error.message || 'Failed to load user overview';
        this.overviewLoading = false;
      }
    });
  }

  /**
   * Switch tabs and lazy load data
   */
  switchTab(tab: 'overview' | 'subscriptions' | 'billing' | 'privileges' | 'analytics'): void {
    this.activeTab = tab;

    switch (tab) {
      case 'subscriptions':
        if (!this.subscriptionsData.loaded && !this.subscriptionsData.loading) {
          this.loadSubscriptions();
        }
        break;
      case 'billing':
        if (!this.billingData.loaded && !this.billingData.loading) {
          this.loadBilling();
        }
        break;
      case 'privileges':
        if (!this.privilegeData.loaded && !this.privilegeData.loading) {
          this.loadPrivileges();
        }
        break;
      case 'analytics':
        if (!this.analyticsData.loaded && !this.analyticsData.loading) {
          this.loadAnalytics();
        }
        break;
    }
  }

  /**
   * Load subscriptions tab data
   */
  loadSubscriptions(): void {
    this.subscriptionsData.loading = true;
    this.subscriptionsData.error = null;

    this.subscriptionService.getUserSubscriptions(this.userId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          const subs = response.data;
          this.subscriptionsData.current = subs.find((s: SubscriptionDto) => s.status === 'Active') || null;
          this.subscriptionsData.past = subs.filter((s: SubscriptionDto) => s.status !== 'Active');
          this.subscriptionsData.loaded = true;
        } else {
          this.subscriptionsData.error = response.message;
        }
        this.subscriptionsData.loading = false;
      },
      error: (error) => {
        this.subscriptionsData.error = error.message || 'Failed to load subscriptions';
        this.subscriptionsData.loading = false;
      }
    });
  }

  /**
   * Load billing tab data
   */
  loadBilling(): void {
    this.billingData.loading = true;
    this.billingData.error = null;

    forkJoin({
      billingHistory: this.billingService.getUserBillingHistory(this.userId),
      paymentAnalytics: this.billingService.getUserPaymentAnalytics(this.userId)
    }).subscribe({
      next: (results) => {
        // Process billing history
        if (results.billingHistory.statusCode === 200) {
          this.billingData.records = results.billingHistory.data;
          this.billingData.totalSpent = this.billingData.records
            .filter(r => r.status === 'Paid')
            .reduce((sum, r) => sum + r.totalAmount, 0);
        }

        // Process payment analytics
        if (results.paymentAnalytics.statusCode === 200) {
          const analytics = results.paymentAnalytics.data;
          this.billingData.successfulPayments = analytics.successfulPayments || 0;
          this.billingData.failedPayments = analytics.failedPayments || 0;
          this.billingData.averageMonthlySpend = analytics.averageMonthlySpend || 0;
        }

        this.billingData.loaded = true;
        this.billingData.loading = false;
      },
      error: (error) => {
        this.billingData.error = error.message || 'Failed to load billing data';
        this.billingData.loading = false;
      }
    });
  }

  /**
   * Load privileges tab data
   */
  loadPrivileges(): void {
    this.privilegeData.loading = true;
    this.privilegeData.error = null;

    this.privilegeService.getPrivilegeUsageSummary(this.userId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.privilegeData.usageSummary = response.data;
          this.privilegeData.loaded = true;
        } else {
          this.privilegeData.error = response.message;
        }
        this.privilegeData.loading = false;
      },
      error: (error) => {
        this.privilegeData.error = error.message || 'Failed to load privilege data';
        this.privilegeData.loading = false;
      }
    });
  }

  /**
   * Load analytics tab data
   */
  loadAnalytics(): void {
    this.analyticsData.loading = true;
    this.analyticsData.error = null;

    const endDate = new Date();
    const startDate = new Date();
    startDate.setFullYear(startDate.getFullYear() - 1);

    forkJoin({
      userAnalytics: this.userService.getUserAnalytics(this.userId, startDate, endDate),
      paymentAnalytics: this.billingService.getUserPaymentAnalytics(this.userId, startDate, endDate)
    }).subscribe({
      next: (results) => {
        if (results.userAnalytics.statusCode === 200) {
          this.analyticsData.userAnalytics = results.userAnalytics.data;
        }

        if (results.paymentAnalytics.statusCode === 200) {
          this.analyticsData.paymentAnalytics = results.paymentAnalytics.data;
        }

        // Prepare charts after data is loaded
        this.prepareCharts();

        this.analyticsData.loaded = true;
        this.analyticsData.loading = false;
      },
      error: (error) => {
        this.analyticsData.error = error.message || 'Failed to load analytics';
        this.analyticsData.loading = false;
      }
    });
  }

  /**
   * Prepare chart data from analytics
   */
  private prepareCharts(): void {
    const analytics = this.analyticsData.userAnalytics;
    if (!analytics) return;

    // Chart 1: Payment Success Rate (Doughnut)
    this.paymentSuccessChartData = {
      labels: ['Successful', 'Failed'],
      datasets: [{
        data: [
          analytics.successfulPayments || 0,
          analytics.failedPayments || 0
        ],
        backgroundColor: ['#28a745', '#dc3545'],
        borderWidth: 2,
        borderColor: '#fff'
      }]
    };

    // Chart 2: Subscription Status Distribution (Pie)
    this.subscriptionStatusChartData = {
      labels: ['Active', 'Past', 'Cancelled'],
      datasets: [{
        data: [
          analytics.activeSubscriptions || 0,
          analytics.pastSubscriptions || 0,
          analytics.cancelledSubscriptions || 0
        ],
        backgroundColor: ['#28a745', '#6c757d', '#dc3545'],
        borderWidth: 2,
        borderColor: '#fff'
      }]
    };

    // Chart 3: Monthly Revenue (Bar) - Simulated data
    const last6Months = this.getLast6Months();
    const avgMonthly = analytics.averageMonthlySpend || 0;
    this.monthlyRevenueChartData = {
      labels: last6Months,
      datasets: [{
        label: 'Monthly Spending',
        data: last6Months.map(() => avgMonthly + (Math.random() - 0.5) * avgMonthly * 0.3),
        backgroundColor: 'rgba(54, 162, 235, 0.7)',
        borderColor: 'rgb(54, 162, 235)',
        borderWidth: 1
      }]
    };

    // Chart 4: Privilege Usage (Horizontal Bar) - If privilege data available
    if (this.privilegeData.usageSummary?.privileges) {
      const privs = this.privilegeData.usageSummary.privileges.slice(0, 5); // Top 5
      this.privilegeUsageChartData = {
        labels: privs.map((p: any) => p.privilegeName || p.name),
        datasets: [{
          label: 'Usage Percentage',
          data: privs.map((p: any) => {
            const used = p.used || p.usedValue || 0;
            const limit = p.limit || p.allowedValue || 100;
            return limit > 0 ? (used / limit) * 100 : 0;
          }),
          backgroundColor: 'rgba(75, 192, 192, 0.7)',
          borderColor: 'rgb(75, 192, 192)',
          borderWidth: 1
        }]
      };
    }
  }

  /**
   * Get last 6 months for chart labels
   */
  private getLast6Months(): string[] {
    const months = [];
    const now = new Date();
    
    for (let i = 5; i >= 0; i--) {
      const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
      months.push(date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' }));
    }
    
    return months;
  }

  /**
   * Helper methods
   */
  getRoleBadgeClass(role: string): string {
    const map: { [key: string]: string } = {
      'Admin': 'bg-danger',
      'Provider': 'bg-primary',
      'Client': 'bg-success',
      'User': 'bg-info'
    };
    return map[role] || 'bg-secondary';
  }

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success',
      'Paused': 'bg-warning',
      'Cancelled': 'bg-danger',
      'Expired': 'bg-secondary',
      'PaymentFailed': 'bg-danger',
      'Pending': 'bg-info'
    };
    return map[status] || 'bg-secondary';
  }

  getBillingStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning',
      'Failed': 'bg-danger',
      'Refunded': 'bg-info',
      'Cancelled': 'bg-secondary'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Privilege Helper: Get privilege list for progress bars
   */
  getPrivilegesList(): any[] {
    if (!this.privilegeData.usageSummary?.privileges) return [];
    
    return this.privilegeData.usageSummary.privileges.map((p: any) => ({
      name: p.privilegeName || p.name,
      used: p.used || p.usedValue || 0,
      limit: p.limit || p.allowedValue || 0,
      resetDate: this.privilegeData.usageSummary.nextResetDate || p.nextResetDate
    }));
  }

  /**
   * Privilege Helper: Check if any privilege has overage
   */
  hasOverage(): boolean {
    if (!this.privilegeData.usageSummary?.privileges) return false;
    
    return this.privilegeData.usageSummary.privileges.some((p: any) => {
      const used = p.used || p.usedValue || 0;
      const limit = p.limit || p.allowedValue || 0;
      return used >= limit && limit > 0;
    });
  }

  /**
   * Privilege Helper: Get next reset date
   */
  getNextResetDate(): Date | null {
    return this.privilegeData.usageSummary?.nextResetDate || 
           this.privilegeData.usageSummary?.usagePeriodEnd ||
           this.activeSubscription?.nextBillingDate ||
           null;
  }

  /**
   * Subscription Action: Open Pause Modal
   */
  openPauseModal(): void {
    if (!this.activeSubscription) return;
    this.pauseReason = '';
    this.showPauseModal = true;
  }

  /**
   * Subscription Action: Confirm Pause
   */
  confirmPause(): void {
    if (!this.activeSubscription || !this.pauseReason.trim()) {
      alert('Pause reason is required');
      return;
    }

    this.actionLoading = true;
    this.subscriptionService.pauseSubscription(
      this.activeSubscription.id,
      { reason: this.pauseReason }
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.showPauseModal = false;
          this.pauseReason = '';
          this.loadOverview(); // Reload to show updated status
          if (this.subscriptionsData.loaded) {
            this.loadSubscriptions(); // Reload subscriptions tab if loaded
          }
          alert('Subscription paused successfully');
        } else {
          alert(response.message || 'Failed to pause subscription');
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert('Error: ' + (error.error?.message || error.message || 'Failed to pause subscription'));
        this.actionLoading = false;
      }
    });
  }

  /**
   * Subscription Action: Open Cancel Modal
   */
  openCancelModal(): void {
    if (!this.activeSubscription) return;
    this.cancelReason = '';
    this.showCancelModal = true;
  }

  /**
   * Subscription Action: Confirm Cancel
   */
  confirmCancel(): void {
    if (!this.activeSubscription || !this.cancelReason.trim()) {
      alert('Cancellation reason is required');
      return;
    }

    if (!confirm('Are you sure you want to cancel this subscription? This action cannot be undone.')) {
      return;
    }

    this.actionLoading = true;
    this.subscriptionService.cancelSubscription(
      this.activeSubscription.id,
      this.cancelReason
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.showCancelModal = false;
          this.cancelReason = '';
          this.loadOverview(); // Reload to show updated status
          if (this.subscriptionsData.loaded) {
            this.loadSubscriptions(); // Reload subscriptions tab if loaded
          }
          alert('Subscription cancelled successfully.\n\nIMPORTANT: No automatic refund will be processed.\nAdmin can manually review and process any applicable refund via billing records.');
        } else {
          alert(response.message || 'Failed to cancel subscription');
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert('Error: ' + (error.error?.message || error.message || 'Failed to cancel subscription'));
        this.actionLoading = false;
      }
    });
  }

  /**
   * Subscription Action: Resume Paused Subscription
   */
  resumeSubscriptionAction(): void {
    if (!this.activeSubscription) return;

    if (!confirm('Resume this paused subscription? Billing will restart on the next billing cycle.')) {
      return;
    }

    this.actionLoading = true;
    this.subscriptionService.resumeSubscription(this.activeSubscription.id)
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.loadOverview(); // Reload to show updated status
            if (this.subscriptionsData.loaded) {
              this.loadSubscriptions(); // Reload subscriptions tab if loaded
            }
            alert('Subscription resumed successfully');
          } else {
            alert(response.message || 'Failed to resume subscription');
          }
          this.actionLoading = false;
        },
        error: (error) => {
          alert('Error: ' + (error.error?.message || error.message || 'Failed to resume subscription'));
          this.actionLoading = false;
        }
      });
  }

  /**
   * Export analytics to Excel or CSV
   */
  exportAnalytics(format: 'excel' | 'csv'): void {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setFullYear(startDate.getFullYear() - 1);

    this.analyticsData.loading = true;

    this.userService.exportUserAnalytics(this.userId, format, startDate, endDate)
      .subscribe({
        next: (blob) => {
          this.downloadFile(blob, `user-${this.userId}-analytics-${new Date().getTime()}.${format === 'excel' ? 'xlsx' : 'csv'}`);
          this.analyticsData.loading = false;
        },
        error: (error) => {
          console.error('Export error:', error);
          alert('Failed to export analytics: ' + (error.message || 'Unknown error'));
          this.analyticsData.loading = false;
        }
      });
  }

  /**
   * Download file helper
   */
  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}
