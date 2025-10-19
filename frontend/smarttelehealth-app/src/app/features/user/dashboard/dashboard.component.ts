import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { 
  SubscriptionService, 
  PrivilegeService, 
  BillingService,
  AuthService 
} from '../../../core/services';
import { 
  SubscriptionDto, 
  PrivilegeUsageSummary,
  BillingRecordDto,
  UserDto 
} from '../../../core/models';

/**
 * User Dashboard Component
 * Main landing page for authenticated users
 * 
 * APIs Used:
 * - GET /api/Subscriptions/user/{userId}
 * - GET /api/Privileges/usage/{subscriptionId}
 * - GET /api/Billing/records?userId={userId}&page=1&pageSize=5
 * 
 * Route: /web/dashboard
 * Access: Authenticated users only
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  currentUser: UserDto | null = null;
  subscriptions: SubscriptionDto[] = [];
  activeSubscription: SubscriptionDto | null = null;
  privilegeUsage: PrivilegeUsageSummary | null = null;
  recentBilling: BillingRecordDto[] = [];
  
  loading = {
    subscriptions: false,
    privileges: false,
    billing: false
  };

  constructor(
    private authService: AuthService,
    private subscriptionService: SubscriptionService,
    private privilegeService: PrivilegeService,
    private billingService: BillingService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.loadDashboardData();
    }
  }

  /**
   * Load all dashboard data
   */
  loadDashboardData(): void {
    if (!this.currentUser) return;

    this.loadSubscriptions();
    this.loadRecentBilling();
  }

  /**
   * Load user subscriptions
   * API: GET /api/Subscriptions/user/{userId}
   */
  loadSubscriptions(): void {
    if (!this.currentUser) return;

    this.loading.subscriptions = true;
    
    this.subscriptionService.getUserSubscriptions(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscriptions = response.data;
          
          // Find active subscription
          this.activeSubscription = this.subscriptions.find(
            s => s.status === 'Active' || s.status === 'TrialActive'
          ) || null;

          // Load privilege usage for active subscription
          if (this.activeSubscription) {
            this.loadPrivilegeUsage(this.activeSubscription.id);
          }
        }
        this.loading.subscriptions = false;
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        this.loading.subscriptions = false;
      }
    });
  }

  /**
   * Load privilege usage for subscription
   * API: GET /api/Privileges/usage/{subscriptionId}
   */
  loadPrivilegeUsage(subscriptionId: string): void {
    this.loading.privileges = true;
    
    this.privilegeService.getUsageSummary(subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.privilegeUsage = response.data;
        }
        this.loading.privileges = false;
      },
      error: (error) => {
        console.error('Error loading privilege usage:', error);
        this.loading.privileges = false;
      }
    });
  }

  /**
   * Load recent billing records
   * API: GET /api/Billing/records
   */
  loadRecentBilling(): void {
    if (!this.currentUser) return;

    this.loading.billing = true;
    
    this.billingService.getBillingRecords(this.currentUser.id, 1, 5).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.recentBilling = response.data;
        }
        this.loading.billing = false;
      },
      error: (error) => {
        console.error('Error loading billing records:', error);
        this.loading.billing = false;
      }
    });
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Active': 'bg-success',
      'TrialActive': 'bg-info',
      'Pending': 'bg-warning',
      'Paused': 'bg-secondary',
      'Cancelled': 'bg-danger',
      'Expired': 'bg-dark',
      'PaymentFailed': 'bg-danger',
      'Suspended': 'bg-danger'
    };
    return statusMap[status] || 'bg-secondary';
  }

  /**
   * Get billing status badge class
   */
  getBillingStatusClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning',
      'Failed': 'bg-danger',
      'Refunded': 'bg-info',
      'Overdue': 'bg-danger'
    };
    return statusMap[status] || 'bg-secondary';
  }

  /**
   * Calculate privilege usage percentage
   */
  getUsagePercentage(used: number, allowed: number): number {
    if (allowed === -1) return 0; // Unlimited
    if (allowed === 0) return 0;
    return Math.min(Math.round((used / allowed) * 100), 100);
  }

  /**
   * Get progress bar class based on usage
   */
  getProgressBarClass(percentage: number): string {
    if (percentage < 50) return 'bg-success';
    if (percentage < 80) return 'bg-warning';
    return 'bg-danger';
  }

  /**
   * Get count of pending billing records
   */
  getPendingBillingCount(): number {
    return this.recentBilling.filter(b => b.status === 'Pending').length;
  }
}

