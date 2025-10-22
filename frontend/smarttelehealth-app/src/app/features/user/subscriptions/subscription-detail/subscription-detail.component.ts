import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SubscriptionService, BillingService } from '../../../../core/services';
import { SubscriptionDto, BillingRecordDto } from '../../../../core/models';
import { SubscriptionRenewalPaymentModalComponent } from '../components/subscription-renewal-payment-modal/subscription-renewal-payment-modal.component';

/**
 * Subscription Detail Component
 * View and manage a specific subscription
 * 
 * APIs Used:
 * - GET /api/Subscriptions/{id}
 * - POST /api/Subscriptions/{id}/cancel
 * - POST /api/Subscriptions/{id}/pause
 * - POST /api/Subscriptions/{id}/resume
 * 
 * Route: /web/subscriptions/:id
 */
@Component({
  selector: 'app-subscription-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, SubscriptionRenewalPaymentModalComponent],
  templateUrl: './subscription-detail.component.html',
  styleUrls: ['./subscription-detail.component.scss']
})
export class SubscriptionDetailComponent implements OnInit {
  subscription: SubscriptionDto | null = null;
  loading = false;
  actionLoading = false;
  error: string | null = null;
  subscriptionId!: string;
  
  // Payment handling
  showRenewalPaymentModal = false;
  hasPendingPayment = false;
  pendingBillingAmount = 0;
  pendingBillingRecord: BillingRecordDto | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private subscriptionService: SubscriptionService,
    private billingService: BillingService
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
    this.error = null;

    this.subscriptionService.getSubscriptionById(this.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscription = response.data;
          
          // Check for pending/failed payments
          this.checkForPendingPayment();
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load subscription';
        this.loading = false;
      }
    });
  }

  /**
   * Check if subscription has pending or failed payment
   * API: GET /api/Billing/subscription/{subscriptionId}
   */
  checkForPendingPayment(): void {
    this.billingService.getSubscriptionBillingHistory(this.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Find first pending or failed billing record
          this.pendingBillingRecord = response.data.find(
            b => b.status === 'Pending' || b.status === 'Failed'
          ) || null;
          
          if (this.pendingBillingRecord) {
            this.hasPendingPayment = true;
            this.pendingBillingAmount = this.pendingBillingRecord.totalAmount;
          }
        }
      },
      error: (error) => {
        console.error('Error checking for pending payments:', error);
      }
    });
  }

  /**
   * Pause subscription
   */
  pauseSubscription(): void {
    if (!confirm('Are you sure you want to pause this subscription?')) return;

    this.actionLoading = true;
    this.subscriptionService.pauseSubscription(this.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.loadSubscription();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to pause subscription');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Resume subscription
   */
  resumeSubscription(): void {
    this.actionLoading = true;
    this.subscriptionService.resumeSubscription(this.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.loadSubscription();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to resume subscription');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Cancel subscription
   */
  cancelSubscription(): void {
    const reason = prompt('Please provide a reason for cancellation:');
    if (!reason) return;

    this.actionLoading = true;
    this.subscriptionService.cancelSubscription(this.subscriptionId, reason).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          alert('Subscription cancelled successfully');
          this.router.navigate(['/web/subscriptions']);
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to cancel subscription');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Open renewal payment modal
   */
  openPaymentModal(): void {
    this.showRenewalPaymentModal = true;
  }

  /**
   * Handle payment success
   */
  onPaymentSuccess(): void {
    // Reload subscription to reflect updated status
    this.loadSubscription();
    this.hasPendingPayment = false;
    this.pendingBillingAmount = 0;
  }

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success', 
      'TrialActive': 'bg-info', 
      'Pending': 'bg-warning',
      'Paused': 'bg-secondary', 
      'Cancelled': 'bg-danger', 
      'Expired': 'bg-dark',
      'PaymentFailed': 'bg-danger'
    };
    return map[status] || 'bg-secondary';
  }
}


