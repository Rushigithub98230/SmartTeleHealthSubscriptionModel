import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SubscriptionService, CommonService } from '../../../../core/services';
import { SubscriptionDto } from '../../../../core/models';

/**
 * Admin Subscription Detail Component
 * View full subscription details with admin actions
 * 
 * APIs Used:
 * - GET /api/Subscriptions/{id}
 * - POST /api/Admin/Subscriptions/{id}/grant-credits (future)
 * - POST /api/Admin/Subscriptions/{id}/suspend (future)
 * 
 * Route: /webadmin/subscriptions/:id
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-subscription-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './subscription-detail.component.html',
  styleUrls: ['./subscription-detail.component.scss']
})
export class AdminSubscriptionDetailComponent implements OnInit {
  subscriptionId!: string;
  subscription: SubscriptionDto | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private subscriptionService: SubscriptionService,
    private commonService: CommonService
  ) {}

  ngOnInit(): void {
    this.subscriptionId = this.route.snapshot.params['id'];
    this.loadSubscription();
  }

  /**
   * Load subscription details
   */
  loadSubscription(): void {
    this.loading = true;

    this.subscriptionService.getSubscriptionById(this.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscription = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success',
      'Pending': 'bg-warning',
      'Cancelled': 'bg-danger'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Cancel subscription
   */
  cancelSubscription(): void {
    const reason = prompt('Please provide a reason for cancellation:');
    if (reason) {
      this.subscriptionService.cancelAdminSubscription(this.subscriptionId, reason).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            alert('Subscription cancelled successfully');
            this.loadSubscription(); // Reload to get updated status
          } else {
            alert(`Error: ${response.message}`);
          }
        },
        error: (error) => {
          alert(`Error cancelling subscription: ${error.message}`);
        }
      });
    }
  }

  /**
   * Pause subscription
   */
  pauseSubscription(): void {
    const reason = prompt('Please provide a reason for pausing:');
    if (reason) {
      this.subscriptionService.pauseAdminSubscription(this.subscriptionId, reason).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            alert('Subscription paused successfully');
            this.loadSubscription(); // Reload to get updated status
          } else {
            alert(`Error: ${response.message}`);
          }
        },
        error: (error) => {
          alert(`Error pausing subscription: ${error.message}`);
        }
      });
    }
  }

  /**
   * Resume subscription
   */
  resumeSubscription(): void {
    if (confirm('Are you sure you want to resume this subscription?')) {
      this.subscriptionService.resumeAdminSubscription(this.subscriptionId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            alert('Subscription resumed successfully');
            this.loadSubscription(); // Reload to get updated status
          } else {
            alert(`Error: ${response.message}`);
          }
        },
        error: (error) => {
          alert(`Error resuming subscription: ${error.message}`);
        }
      });
    }
  }

  /**
   * Extend subscription
   */
  extendSubscription(): void {
    const days = prompt('How many days to extend?');
    if (days && !isNaN(Number(days))) {
      this.subscriptionService.extendAdminSubscription(this.subscriptionId, Number(days)).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            alert('Subscription extended successfully');
            this.loadSubscription(); // Reload to get updated dates
          } else {
            alert(`Error: ${response.message}`);
          }
        },
        error: (error) => {
          alert(`Error extending subscription: ${error.message}`);
        }
      });
    }
  }

  /**
   * Check if subscription can be cancelled
   */
  canCancel(): boolean {
    return this.subscription?.status === 'Active' || this.subscription?.status === 'Pending';
  }

  /**
   * Check if subscription can be paused
   */
  canPause(): boolean {
    return this.subscription?.status === 'Active';
  }

  /**
   * Check if subscription can be resumed
   */
  canResume(): boolean {
    return this.subscription?.status === 'Paused';
  }

  /**
   * Check if subscription can be extended
   */
  canExtend(): boolean {
    return this.subscription?.status === 'Active' || this.subscription?.status === 'Paused';
  }
}


