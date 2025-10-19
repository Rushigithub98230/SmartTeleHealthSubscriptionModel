import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { 
  SubscriptionService, 
  PrivilegeService,
  AuthService 
} from '../../../../core/services';
import { 
  SubscriptionDto, 
  PrivilegeUsageSummary,
  UserDto 
} from '../../../../core/models';

/**
 * Privilege Usage Component
 * Display detailed privilege usage with visual progress indicators
 * 
 * APIs Used:
 * - GET /api/Subscriptions/user/{userId}
 * - GET /api/Privileges/usage/{subscriptionId}
 * 
 * Route: /web/privileges
 * Access: Authenticated users
 */
@Component({
  selector: 'app-privilege-usage',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './privilege-usage.component.html',
  styleUrls: ['./privilege-usage.component.scss']
})
export class PrivilegeUsageComponent implements OnInit {
  currentUser: UserDto | null = null;
  activeSubscription: SubscriptionDto | null = null;
  privilegeUsage: PrivilegeUsageSummary | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private authService: AuthService,
    private subscriptionService: SubscriptionService,
    private privilegeService: PrivilegeService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.loadData();
    }
  }

  /**
   * Load subscription and privilege usage
   */
  loadData(): void {
    if (!this.currentUser) return;

    this.loading = true;
    this.error = null;

    // First, get active subscription
    this.subscriptionService.getUserSubscriptions(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Find active subscription
          this.activeSubscription = response.data.find(
            s => s.status === 'Active' || s.status === 'TrialActive'
          ) || null;

          if (this.activeSubscription) {
            this.loadPrivilegeUsage(this.activeSubscription.id);
          } else {
            this.loading = false;
          }
        } else {
          this.error = response.message;
          this.loading = false;
        }
      },
      error: (error) => {
        this.error = error.message || 'Failed to load subscription';
        this.loading = false;
      }
    });
  }

  /**
   * Load privilege usage for active subscription
   */
  loadPrivilegeUsage(subscriptionId: string): void {
    this.privilegeService.getUsageSummary(subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.privilegeUsage = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load privilege usage';
        this.loading = false;
      }
    });
  }

  /**
   * Get usage percentage
   */
  getUsagePercentage(used: number, allowed: number): number {
    if (allowed === -1) return 0; // Unlimited
    if (allowed === 0) return 0;
    return Math.min(Math.round((used / allowed) * 100), 100);
  }

  /**
   * Get progress bar color class
   */
  getProgressBarClass(percentage: number): string {
    if (percentage < 50) return 'bg-success';
    if (percentage < 80) return 'bg-warning';
    return 'bg-danger';
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(isExhausted: boolean, isUnlimited: boolean): string {
    if (isUnlimited) return 'bg-success';
    if (isExhausted) return 'bg-danger';
    return 'bg-primary';
  }

  /**
   * Get status text
   */
  getStatusText(priv: any): string {
    if (priv.isUnlimited) return 'Unlimited';
    if (priv.isExhausted) return 'Exhausted';
    if (priv.remainingValue === 0) return 'Used Up';
    return 'Active';
  }
}


