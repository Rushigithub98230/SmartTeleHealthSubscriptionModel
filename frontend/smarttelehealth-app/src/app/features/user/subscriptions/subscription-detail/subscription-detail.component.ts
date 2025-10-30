import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SubscriptionService, BillingService, SubscriptionPlanService } from '../../../../core/services';
import { SubscriptionDto, BillingRecordDto, SubscriptionPlanDto } from '../../../../core/models';
import { SubscriptionRenewalPaymentModalComponent } from '../components/subscription-renewal-payment-modal/subscription-renewal-payment-modal.component';
import { PlanChangeModalComponent } from '../components/plan-change-modal/plan-change-modal.component';

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
  imports: [
    CommonModule, 
    RouterLink, 
    MatDialogModule, 
    MatSnackBarModule,
    SubscriptionRenewalPaymentModalComponent
  ],
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
    private billingService: BillingService,
    private planService: SubscriptionPlanService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.subscriptionId = this.route.snapshot.params['id'];
    console.log('🎯 [SUBSCRIPTION-DETAIL] Component initialized for subscription:', this.subscriptionId);
    this.loadSubscription();
  }

  /**
   * Load subscription details
   */
  loadSubscription(): void {
    console.log('📋 [SUBSCRIPTION-DETAIL] Loading subscription details for ID:', this.subscriptionId);
    this.loading = true;
    this.error = null;

    this.subscriptionService.getSubscriptionById(this.subscriptionId).subscribe({
      next: (response) => {
        console.log('✅ [SUBSCRIPTION-DETAIL] Subscription loaded:', {
          statusCode: response.statusCode,
          subscriptionId: response.data?.id,
          status: response.data?.status,
          planName: response.data?.planName
        });
        
        if (response.statusCode === 200) {
          this.subscription = response.data;
          
          // Check for pending/failed payments
          this.checkForPendingPayment();
        } else {
          this.error = response.message;
          console.error('❌ [SUBSCRIPTION-DETAIL] Failed to load subscription:', response.message);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ [SUBSCRIPTION-DETAIL] Error loading subscription:', error);
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
   * Retry failed payment
   * API: POST /api/Subscriptions/{subscriptionId}/retry-payment
   * Phase 2: Manual payment retry for PaymentFailed/Suspended subscriptions
   */
  retryPayment(): void {
    if (!confirm('This will attempt to charge your default payment method. Continue?')) {
      return;
    }

    console.log('🔄 [SUBSCRIPTION-DETAIL] Retrying payment for subscription:', this.subscriptionId);
    this.actionLoading = true;
    this.error = null;

    this.subscriptionService.retryPayment(this.subscriptionId).subscribe({
      next: (response) => {
        console.log('✅ [SUBSCRIPTION-DETAIL] Retry payment response:', {
          statusCode: response.statusCode,
          message: response.message
        });

        if (response.statusCode === 200) {
          alert('Payment successful! Your subscription has been reactivated.');
          this.loadSubscription(); // Reload to show updated status
        } else {
          this.error = response.message || 'Payment retry failed. Please try again or update your payment method.';
          alert(this.error);
        }
        this.actionLoading = false;
      },
      error: (error) => {
        console.error('❌ [SUBSCRIPTION-DETAIL] Error retrying payment:', error);
        this.error = error.error?.message || error.message || 'Failed to retry payment. Please try again.';
        alert(this.error);
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

  /**
   * Get status badge styling class
   */
  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success', 
      'TrialActive': 'bg-info', 
      'Pending': 'bg-warning',
      'Paused': 'bg-secondary', 
      'Cancelled': 'bg-danger', 
      'Expired': 'bg-dark',
      'PaymentFailed': 'bg-danger',
      'Suspended': 'bg-danger'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Check if subscription has payment issues
   */
  hasPaymentIssues(): boolean {
    return this.subscription?.status === 'PaymentFailed' || 
           this.subscription?.status === 'Suspended';
  }

  // ============================================
  // SCHEDULED PLAN CHANGES (NO PRORATION)
  // ============================================

  /**
   * Check if subscription has a pending plan change scheduled
   */
  hasPendingPlanChange(): boolean {
    return !!(this.subscription?.pendingPlanChangeId && this.subscription?.planChangeEffectiveDate);
  }

  /**
   * Get formatted effective date for scheduled plan change
   */
  getPlanChangeEffectiveDate(): string {
    if (!this.subscription?.planChangeEffectiveDate) return '';
    return new Date(this.subscription.planChangeEffectiveDate).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  /**
   * Check if upgrade button should be shown
   */
  canUpgrade(): boolean {
    return this.subscription?.status === 'Active' && !this.hasPendingPlanChange();
  }

  /**
   * Check if downgrade button should be shown
   */
  canDowngrade(): boolean {
    return this.subscription?.status === 'Active' && !this.hasPendingPlanChange();
  }

  /**
   * Open upgrade modal
   */
  async openUpgradeModal() {
    if (!this.subscription) return;

    this.actionLoading = true;
    console.log('⬆️ [SUBSCRIPTION-DETAIL] Opening upgrade modal for subscription:', this.subscriptionId);

    try {
      // Fetch all active plans
      const plansResponse = await this.planService.getActivePlans(1, 100).toPromise();
      
      if (plansResponse && plansResponse.statusCode === 200 && plansResponse.data) {
        const availablePlans = Array.isArray(plansResponse.data) ? plansResponse.data : [plansResponse.data];
        
        console.log('📦 [SUBSCRIPTION-DETAIL] Fetched plans for upgrade:', availablePlans.length);
        
        const dialogRef = this.dialog.open(PlanChangeModalComponent, {
          data: {
            subscription: this.subscription,
            availablePlans: availablePlans,
            changeType: 'upgrade'
          },
          width: '800px',
          maxHeight: '90vh',
          disableClose: false
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result === true) {
            console.log('✅ [SUBSCRIPTION-DETAIL] Upgrade scheduled successfully');
            this.loadSubscription(); // Reload to show pending change
            this.showSuccessMessage('Upgrade scheduled successfully!');
          }
        });
      } else {
        this.showErrorMessage('Failed to load available plans');
      }
    } catch (error) {
      console.error('❌ [SUBSCRIPTION-DETAIL] Error fetching plans for upgrade:', error);
      this.showErrorMessage('Failed to load available plans');
    } finally {
      this.actionLoading = false;
    }
  }

  /**
   * Open downgrade modal
   */
  async openDowngradeModal() {
    if (!this.subscription) return;

    this.actionLoading = true;
    console.log('⬇️ [SUBSCRIPTION-DETAIL] Opening downgrade modal for subscription:', this.subscriptionId);

    try {
      // Fetch all active plans
      const plansResponse = await this.planService.getActivePlans(1, 100).toPromise();
      
      if (plansResponse && plansResponse.statusCode === 200 && plansResponse.data) {
        const availablePlans = Array.isArray(plansResponse.data) ? plansResponse.data : [plansResponse.data];
        
        console.log('📦 [SUBSCRIPTION-DETAIL] Fetched plans for downgrade:', availablePlans.length);
        
        const dialogRef = this.dialog.open(PlanChangeModalComponent, {
          data: {
            subscription: this.subscription,
            availablePlans: availablePlans,
            changeType: 'downgrade'
          },
          width: '800px',
          maxHeight: '90vh',
          disableClose: false
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result === true) {
            console.log('✅ [SUBSCRIPTION-DETAIL] Downgrade scheduled successfully');
            this.loadSubscription(); // Reload to show pending change
            this.showSuccessMessage('Downgrade scheduled successfully!');
          }
        });
      } else {
        this.showErrorMessage('Failed to load available plans');
      }
    } catch (error) {
      console.error('❌ [SUBSCRIPTION-DETAIL] Error fetching plans for downgrade:', error);
      this.showErrorMessage('Failed to load available plans');
    } finally {
      this.actionLoading = false;
    }
  }

  /**
   * Cancel scheduled plan change
   */
  async cancelScheduledPlanChange() {
    if (!this.subscription || !confirm('Are you sure you want to cancel the scheduled plan change?')) {
      return;
    }

    this.actionLoading = true;
    console.log('❌ [SUBSCRIPTION-DETAIL] Canceling scheduled plan change for subscription:', this.subscriptionId);

    this.subscriptionService.cancelScheduledPlanChange(this.subscriptionId).subscribe({
      next: (response) => {
        console.log('✅ [SUBSCRIPTION-DETAIL] Cancel scheduled plan change response:', {
          statusCode: response.statusCode,
          message: response.message
        });

        if (response.statusCode === 200) {
          this.showSuccessMessage('Scheduled plan change canceled successfully!');
          this.loadSubscription(); // Reload to clear pending change
        } else {
          this.showErrorMessage(response.message || 'Failed to cancel scheduled plan change');
        }
        this.actionLoading = false;
      },
      error: (error) => {
        console.error('❌ [SUBSCRIPTION-DETAIL] Error canceling scheduled plan change:', error);
        this.showErrorMessage(error.error?.message || 'Failed to cancel scheduled plan change');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Show success message using snackbar
   */
  private showSuccessMessage(message: string) {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['success-snackbar'],
      horizontalPosition: 'end',
      verticalPosition: 'top'
    });
  }

  /**
   * Show error message using snackbar
   */
  private showErrorMessage(message: string) {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar'],
      horizontalPosition: 'end',
      verticalPosition: 'top'
    });
  }
}


