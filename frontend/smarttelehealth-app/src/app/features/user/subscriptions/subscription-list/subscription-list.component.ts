import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SubscriptionService, AuthService } from '../../../../core/services';
import { SubscriptionDto, UserDto } from '../../../../core/models';

/**
 * My Subscriptions List Component
 * Display all user subscriptions
 * 
 * APIs Used:
 * - GET /api/Subscriptions/user/{userId}
 * 
 * Route: /web/subscriptions
 * Access: Authenticated users only
 */
@Component({
  selector: 'app-subscription-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './subscription-list.component.html',
  styleUrls: ['./subscription-list.component.scss']
})
export class SubscriptionListComponent implements OnInit {
  currentUser: UserDto | null = null;
  subscriptions: SubscriptionDto[] = [];
  loading = false;
  error: string | null = null;

  // Filtered subscriptions
  activeSubscriptions: SubscriptionDto[] = [];
  pausedSubscriptions: SubscriptionDto[] = [];
  cancelledSubscriptions: SubscriptionDto[] = [];

  constructor(
    private authService: AuthService,
    private subscriptionService: SubscriptionService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.loadSubscriptions();
    }
  }

  /**
   * Load all user subscriptions
   * API: GET /api/Subscriptions/user/{userId}
   */
  loadSubscriptions(): void {
    if (!this.currentUser) return;

    this.loading = true;
    this.error = null;

    this.subscriptionService.getUserSubscriptions(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscriptions = response.data;
          this.categorizeSubscriptions();
        } else {
          this.error = response.message || 'Failed to load subscriptions';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'An error occurred';
        this.loading = false;
      }
    });
  }

  /**
   * Categorize subscriptions by status
   */
  categorizeSubscriptions(): void {
    this.activeSubscriptions = this.subscriptions.filter(
      s => s.status === 'Active' || s.status === 'TrialActive'
    );
    
    this.pausedSubscriptions = this.subscriptions.filter(
      s => s.status === 'Paused'
    );
    
    this.cancelledSubscriptions = this.subscriptions.filter(
      s => s.status === 'Cancelled' || s.status === 'Expired'
    );
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
   * Get days until next billing
   */
  getDaysUntilBilling(nextBillingDate: Date): number {
    const today = new Date();
    const billingDate = new Date(nextBillingDate);
    const diffTime = billingDate.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }
}


