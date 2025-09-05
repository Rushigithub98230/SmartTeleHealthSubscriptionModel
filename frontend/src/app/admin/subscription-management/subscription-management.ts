import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SubscriptionService } from '../../services/subscription.service';
import { PlanStepperComponent } from './plan-stepper.component';
import { SubscriptionDetailsDialogComponent } from './subscription-details-dialog.component';
import { ConfirmationDialogComponent, ConfirmationDialogData } from './confirmation-dialog.component';
import { 
  SubscriptionDto, 
  SubscriptionPlanDto, 
  CreateSubscriptionPlanDto, 
  PaginatedResponse,
  SubscriptionDetailsDto,
  BillingRecordDto,
  UserSubscriptionPrivilegeUsageDto
} from '../../models/subscription.models';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-subscription-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatCardModule,
    MatTabsModule,
    MatChipsModule,
    MatMenuModule,
    MatSnackBarModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    PlanStepperComponent
  ],
  templateUrl: './subscription-management.html',
  styleUrls: ['./subscription-management.scss']
})
export class SubscriptionManagementComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  // Plans data
  plans: SubscriptionPlanDto[] = [];
  planColumns = ['name', 'price', 'status', 'popular', 'actions'];
  planTotalCount = 0;
  planPageSize = 20;
  planCurrentPage = 0;
  planSearchTerm = '';
  plansLoading = false;

  // Subscriptions data
  subscriptions: SubscriptionDto[] = [];
  subscriptionColumns = ['user', 'plan', 'status', 'price', 'nextBilling', 'actions'];
  subscriptionTotalCount = 0;
  subscriptionPageSize = 20;
  subscriptionCurrentPage = 0;
  subscriptionSearchTerm = '';
  selectedStatus = '';
  subscriptionsLoading = false;

  // UI state
  selectedTab = 0;
  loading = false;

  ngOnInit() {
    this.loadPlans();
    this.loadSubscriptions();
  }

  // Plans management
  loadPlans() {
    this.plansLoading = true;
    this.subscriptionService.getAllPlans(
      this.planCurrentPage + 1, 
      this.planPageSize, 
      this.planSearchTerm
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.plans = response.data;
          this.planTotalCount = response.meta?.totalRecords || this.plans.length;
        } else {
          this.plans = [];
          this.planTotalCount = 0;
        }
        this.plansLoading = false;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.plans = [];
        this.planTotalCount = 0;
        this.snackBar.open('Error loading plans: ' + (error.message || 'Unknown error'), 'Close', { duration: 5000 });
        this.plansLoading = false;
      }
    });
  }

  searchPlans() {
    this.planCurrentPage = 0;
    this.loadPlans();
  }

  onPlanPageChange(event: PageEvent) {
    this.planCurrentPage = event.pageIndex;
    this.planPageSize = event.pageSize;
    this.loadPlans();
  }

  openCreatePlanDialog() {
    const dialogRef = this.dialog.open(PlanStepperComponent, {
      width: '90vw',
      maxWidth: '1200px',
      height: '90vh',
      data: { editingPlan: null }
    });

    dialogRef.componentInstance.planCreated.subscribe((planData: CreateSubscriptionPlanDto) => {
      this.subscriptionService.createPlan(planData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Plan created successfully', 'Close', { duration: 3000 });
            this.loadPlans();
            dialogRef.close();
          } else {
            this.snackBar.open(response.message || 'Failed to create plan', 'Close', { duration: 5000 });
          }
        },
        error: (err: any) => {
          this.snackBar.open(err.message || 'Failed to create plan', 'Close', { duration: 5000 });
        }
      });
    });

    dialogRef.componentInstance.cancelled.subscribe(() => {
      dialogRef.close();
    });
  }

  editPlan(plan: SubscriptionPlanDto) {
    const dialogRef = this.dialog.open(PlanStepperComponent, {
      width: '90vw',
      maxWidth: '1200px',
      height: '90vh',
      data: { editingPlan: plan }
    });

    dialogRef.componentInstance.planUpdated.subscribe((planData: any) => {
      this.subscriptionService.updatePlan(plan.id, planData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Plan updated successfully', 'Close', { duration: 3000 });
            this.loadPlans();
            dialogRef.close();
          } else {
            this.snackBar.open(response.message || 'Failed to update plan', 'Close', { duration: 5000 });
          }
        },
        error: (err: any) => {
          this.snackBar.open(err.message || 'Failed to update plan', 'Close', { duration: 5000 });
        }
      });
    });

    dialogRef.componentInstance.cancelled.subscribe(() => {
      dialogRef.close();
    });
  }

  viewPlan(plan: SubscriptionPlanDto) {
    this.snackBar.open(`Plan: ${plan.name} - $${plan.price}`, 'Close', { duration: 3000 });
  }

  deletePlan(planId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Delete Subscription Plan',
      message: 'Are you sure you want to delete this subscription plan? This action cannot be undone and may affect existing subscriptions.',
      confirmText: 'Delete Plan',
      cancelText: 'Cancel',
      type: 'danger'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subscriptionService.deletePlan(planId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Plan deleted successfully', 'Close', { duration: 3000 });
              this.loadPlans();
            } else {
              this.snackBar.open(response.message || 'Failed to delete plan', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error deleting plan:', error);
            this.snackBar.open(error.message || 'Error deleting plan', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }



  // Subscriptions management
  loadSubscriptions() {
    this.subscriptionsLoading = true;
    const statusFilter = this.selectedStatus ? [this.selectedStatus] : undefined;
    
    this.subscriptionService.getAllSubscriptions(
      this.subscriptionCurrentPage + 1,
      this.subscriptionPageSize,
      this.subscriptionSearchTerm,
      statusFilter
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.subscriptions = response.data;
          this.subscriptionTotalCount = response.meta?.totalRecords || this.subscriptions.length;
        } else {
          this.subscriptions = [];
          this.subscriptionTotalCount = 0;
        }
        this.subscriptionsLoading = false;
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        this.subscriptions = [];
        this.subscriptionTotalCount = 0;
        this.snackBar.open('Error loading subscriptions: ' + (error.message || 'Unknown error'), 'Close', { duration: 5000 });
        this.subscriptionsLoading = false;
      }
    });
  }

  searchSubscriptions() {
    this.subscriptionCurrentPage = 0;
    this.loadSubscriptions();
  }

  filterSubscriptions() {
    this.subscriptionCurrentPage = 0;
    this.loadSubscriptions();
  }

  onSubscriptionPageChange(event: PageEvent) {
    this.subscriptionCurrentPage = event.pageIndex;
    this.subscriptionPageSize = event.pageSize;
    this.loadSubscriptions();
  }

  viewSubscriptionDetails(subscription: SubscriptionDto) {
    const dialogRef = this.dialog.open(SubscriptionDetailsDialogComponent, {
      width: '90vw',
      maxWidth: '1200px',
      height: '80vh',
      data: { subscription }
    });

    dialogRef.afterClosed().subscribe(result => {
      // Refresh data if needed
      if (result === 'refresh') {
        this.loadSubscriptions();
      }
    });
  }

  upgradeSubscription(subscription: SubscriptionDto) {
    // TODO: Open plan selection dialog for upgrade
    const newPlanId = prompt('Enter new plan ID for upgrade:');
    if (newPlanId) {
      this.subscriptionService.upgradeSubscription(subscription.id, newPlanId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Subscription upgraded successfully', 'Close', { duration: 3000 });
            this.loadSubscriptions();
          }
        },
        error: (error) => {
          console.error('Error upgrading subscription:', error);
          this.snackBar.open('Error upgrading subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  downgradeSubscription(subscription: SubscriptionDto) {
    // TODO: Open plan selection dialog for downgrade
    const newPlanId = prompt('Enter new plan ID for downgrade:');
    if (newPlanId) {
      this.subscriptionService.downgradeSubscription(subscription.id, newPlanId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Subscription downgraded successfully', 'Close', { duration: 3000 });
            this.loadSubscriptions();
          }
        },
        error: (error) => {
          console.error('Error downgrading subscription:', error);
          this.snackBar.open('Error downgrading subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  extendSubscription(subscription: SubscriptionDto) {
    const additionalDays = prompt('Enter number of days to extend:');
    if (additionalDays && !isNaN(Number(additionalDays))) {
      this.subscriptionService.extendSubscription(subscription.id, Number(additionalDays)).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open(`Subscription extended by ${additionalDays} days`, 'Close', { duration: 3000 });
            this.loadSubscriptions();
          }
        },
        error: (error) => {
          console.error('Error extending subscription:', error);
          this.snackBar.open('Error extending subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  reactivateSubscription(subscriptionId: string) {
    if (confirm('Are you sure you want to reactivate this subscription?')) {
      this.subscriptionService.reactivateSubscription(subscriptionId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Subscription reactivated successfully', 'Close', { duration: 3000 });
            this.loadSubscriptions();
          }
        },
        error: (error) => {
          console.error('Error reactivating subscription:', error);
          this.snackBar.open('Error reactivating subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  viewBillingHistory(subscription: SubscriptionDto) {
    this.subscriptionService.getBillingHistory(subscription.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // TODO: Open billing history dialog with response.data
          this.snackBar.open(`Billing history loaded for ${subscription.userName}`, 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        console.error('Error loading billing history:', error);
        this.snackBar.open('Error loading billing history', 'Close', { duration: 3000 });
      }
    });
  }

  viewPrivilegeUsage(subscription: SubscriptionDto) {
    this.subscriptionService.getPrivilegeUsage(subscription.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // TODO: Open privilege usage dialog with response.data
          this.snackBar.open(`Privilege usage loaded for ${subscription.userName}`, 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        console.error('Error loading privilege usage:', error);
        this.snackBar.open('Error loading privilege usage', 'Close', { duration: 3000 });
      }
    });
  }

  pauseSubscription(subscriptionId: string) {
    if (confirm('Are you sure you want to pause this subscription?')) {
      this.subscriptionService.pauseSubscription(subscriptionId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Subscription paused successfully', 'Close', { duration: 3000 });
            this.loadSubscriptions();
          }
        },
        error: (error) => {
          console.error('Error pausing subscription:', error);
          this.snackBar.open('Error pausing subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  resumeSubscription(subscriptionId: string) {
    this.subscriptionService.resumeSubscription(subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.snackBar.open('Subscription resumed successfully', 'Close', { duration: 3000 });
          this.loadSubscriptions();
        }
      },
      error: (error) => {
        console.error('Error resuming subscription:', error);
        this.snackBar.open('Error resuming subscription', 'Close', { duration: 3000 });
      }
    });
  }

  cancelSubscription(subscriptionId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Cancel Subscription',
      message: 'Are you sure you want to cancel this subscription? The user will lose access to the services.',
      confirmText: 'Cancel Subscription',
      cancelText: 'Keep Active',
      type: 'warning',
      requireReason: true,
      reasonLabel: 'Cancellation Reason',
      reasonPlaceholder: 'Please provide a detailed reason for cancelling this subscription...'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '600px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.confirmed) {
        this.subscriptionService.cancelSubscription(subscriptionId, result.reason).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Subscription cancelled successfully', 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to cancel subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error cancelling subscription:', error);
            this.snackBar.open(error.message || 'Error cancelling subscription', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  getStatusColor(status: string): 'primary' | 'accent' | 'warn' | undefined {
    switch (status?.toLowerCase()) {
      case 'active': return 'primary';
      case 'paused': return 'accent';
      case 'cancelled':
      case 'expired': return 'warn';
      default: return undefined;
    }
  }
}
