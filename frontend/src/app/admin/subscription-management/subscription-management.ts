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
import { ActivatedRoute } from '@angular/router';
import { SubscriptionService } from '../../services/subscription.service';
import { PlanStepperComponent } from './plan-stepper.component';
import { PlanDetailsDialogComponent } from './plan-details-dialog.component';
import { SubscriptionDetailsDialogComponent } from './subscription-details-dialog.component';
import { ConfirmationDialogComponent, ConfirmationDialogData } from './confirmation-dialog.component';
import { PlanSelectionDialogComponent, PlanSelectionData } from './plan-selection-dialog.component';
import { ExtensionDialogComponent, ExtensionDialogData, ExtensionResult } from './extension-dialog.component';
import { BillingHistoryDialogComponent, BillingHistoryDialogData } from './billing-history-dialog.component';
import { PrivilegeUsageDialogComponent, PrivilegeUsageDialogData } from './privilege-usage-dialog.component';
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
  styles: [`
    .warning-action {
      color: #f57c00 !important;
    }
    .info-action {
      color: #1976d2 !important;
    }
    .danger-action {
      color: #d32f2f !important;
    }
  `],
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
  private route = inject(ActivatedRoute);

  // View mode - determines what content to show
  viewMode: 'subscriptions' | 'plans' = 'subscriptions';

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
  loading = false;

  ngOnInit() {
    // Get view mode from route data
    this.route.data.subscribe(data => {
      this.viewMode = data['view'] || 'subscriptions';
      
      // Load appropriate data based on view mode
      if (this.viewMode === 'plans') {
        this.loadPlans();
      } else {
        this.loadSubscriptions();
      }
    });
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
          if (response.statusCode === 200 || response.statusCode === 201) {
            this.snackBar.open('Plan created successfully', 'Close', { duration: 3000 });
            this.loadPlans();
            dialogRef.close();
          } else {
            // Handle validation errors
            if (response.statusCode === 400 && response.errors) {
              dialogRef.componentInstance.setBackendValidationErrors(response.errors);
            } else {
              this.snackBar.open(response.message || 'Failed to create plan', 'Close', { duration: 5000 });
            }
          }
        },
        error: (err: any) => {
          // Handle validation errors from error response
          if (err.status === 400 && err.error && err.error.errors) {
            dialogRef.componentInstance.setBackendValidationErrors(err.error.errors);
          } else {
            this.snackBar.open(err.message || 'Failed to create plan', 'Close', { duration: 5000 });
          }
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
            // Handle validation errors
            if (response.statusCode === 400 && response.errors) {
              dialogRef.componentInstance.setBackendValidationErrors(response.errors);
            } else {
              this.snackBar.open(response.message || 'Failed to update plan', 'Close', { duration: 5000 });
            }
          }
        },
        error: (err: any) => {
          // Handle validation errors from error response
          if (err.status === 400 && err.error && err.error.errors) {
            dialogRef.componentInstance.setBackendValidationErrors(err.error.errors);
          } else {
            this.snackBar.open(err.message || 'Failed to update plan', 'Close', { duration: 5000 });
          }
        }
      });
    });

    dialogRef.componentInstance.cancelled.subscribe(() => {
      dialogRef.close();
    });
  }

  viewPlan(plan: SubscriptionPlanDto) {
    const dialogRef = this.dialog.open(PlanDetailsDialogComponent, {
      width: '90vw',
      maxWidth: '1000px',
      height: '90vh',
      data: { plan: plan },
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.action === 'edit') {
        this.editPlan(result.plan);
      }
    });
  }

  deactivatePlan(planId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Deactivate Subscription Plan',
      message: 'Are you sure you want to deactivate this subscription plan? This will prevent new subscriptions but preserve existing data. The plan can be reactivated later.',
      confirmText: 'Deactivate Plan',
      cancelText: 'Cancel',
      type: 'warning'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subscriptionService.deactivatePlan(planId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Plan deactivated successfully', 'Close', { duration: 3000 });
              this.loadPlans();
            } else {
              this.snackBar.open(response.message || 'Failed to deactivate plan', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error deactivating plan:', error);
            this.snackBar.open(error.message || 'Error deactivating plan', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  reactivatePlan(planId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Reactivate Subscription Plan',
      message: 'Are you sure you want to reactivate this subscription plan? This will make it available for new subscriptions again.',
      confirmText: 'Reactivate Plan',
      cancelText: 'Cancel',
      type: 'info'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subscriptionService.reactivatePlan(planId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Plan reactivated successfully', 'Close', { duration: 3000 });
              this.loadPlans();
            } else {
              this.snackBar.open(response.message || 'Failed to reactivate plan', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error reactivating plan:', error);
            this.snackBar.open(error.message || 'Error reactivating plan', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  // DEPRECATED: Use deactivatePlan instead for better data integrity
  deletePlan(planId: string) {
    console.warn('deletePlan is deprecated. Use deactivatePlan instead for better data integrity.');
    this.deactivatePlan(planId);
  }

  activatePlan(planId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Activate Subscription Plan',
      message: 'Are you sure you want to activate this subscription plan? It will become available for new user subscriptions.',
      confirmText: 'Activate Plan',
      cancelText: 'Cancel',
      type: 'info'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subscriptionService.activatePlan(planId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Plan activated successfully', 'Close', { duration: 3000 });
              this.loadPlans();
            } else {
              this.snackBar.open(response.message || 'Failed to activate plan', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error activating plan:', error);
            this.snackBar.open(error.message || 'Error activating plan', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  deactivatePlan(planId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Deactivate Subscription Plan',
      message: 'Are you sure you want to deactivate this subscription plan? It will no longer be available for new subscriptions.',
      confirmText: 'Deactivate Plan',
      cancelText: 'Cancel',
      type: 'warning',
      requireReason: true,
      reasonLabel: 'Deactivation Reason',
      reasonPlaceholder: 'Please provide a reason for deactivating this plan...'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '600px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.confirmed) {
        this.subscriptionService.deactivatePlan(planId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Plan deactivated successfully', 'Close', { duration: 3000 });
              this.loadPlans();
            } else {
              this.snackBar.open(response.message || 'Failed to deactivate plan', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error deactivating plan:', error);
            this.snackBar.open(error.message || 'Error deactivating plan', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  // Subscriptions management
  loadSubscriptions() {
    this.subscriptionsLoading = true;
    const statusFilter = this.selectedStatus || undefined;
    
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
    const dialogData: PlanSelectionData = {
      title: 'Upgrade Subscription Plan',
      currentPlanId: subscription.planId,
      currentPlanName: subscription.planName,
      actionType: 'upgrade'
    };

    const dialogRef = this.dialog.open(PlanSelectionDialogComponent, {
      width: '600px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(selectedPlan => {
      if (selectedPlan) {
        this.subscriptionService.upgradeSubscription(subscription.id, selectedPlan.id).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Subscription upgraded successfully', 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to upgrade subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error upgrading subscription:', error);
            this.snackBar.open(error.message || 'Error upgrading subscription', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  downgradeSubscription(subscription: SubscriptionDto) {
    const dialogData: PlanSelectionData = {
      title: 'Downgrade Subscription Plan',
      currentPlanId: subscription.planId,
      currentPlanName: subscription.planName,
      actionType: 'downgrade'
    };

    const dialogRef = this.dialog.open(PlanSelectionDialogComponent, {
      width: '600px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(selectedPlan => {
      if (selectedPlan) {
        this.subscriptionService.downgradeSubscription(subscription.id, selectedPlan.id).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Subscription downgraded successfully', 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to downgrade subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error downgrading subscription:', error);
            this.snackBar.open(error.message || 'Error downgrading subscription', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  extendSubscription(subscription: SubscriptionDto) {
    const dialogData: ExtensionDialogData = {
      subscriptionId: subscription.id,
      currentEndDate: subscription.endDate || subscription.nextBillingDate,
      userName: subscription.userName
    };

    const dialogRef = this.dialog.open(ExtensionDialogComponent, {
      width: '600px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe((result: ExtensionResult) => {
      if (result && result.days) {
        this.subscriptionService.extendSubscription(subscription.id, result.days).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open(`Subscription extended by ${result.days} days`, 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to extend subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error extending subscription:', error);
            this.snackBar.open(error.message || 'Error extending subscription', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  reactivateSubscription(subscriptionId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Reactivate Subscription',
      message: 'Are you sure you want to reactivate this subscription? The user will regain access to the services.',
      confirmText: 'Reactivate',
      cancelText: 'Cancel',
      type: 'info'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subscriptionService.reactivateSubscription(subscriptionId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Subscription reactivated successfully', 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to reactivate subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error reactivating subscription:', error);
            this.snackBar.open(error.message || 'Error reactivating subscription', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  viewBillingHistory(subscription: SubscriptionDto) {
    const dialogData: BillingHistoryDialogData = {
      subscriptionId: subscription.id,
      userName: subscription.userName,
      planName: subscription.planName
    };

    const dialogRef = this.dialog.open(BillingHistoryDialogComponent, {
      width: '90vw',
      maxWidth: '1000px',
      height: '80vh',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(() => {
      // Dialog closed, no action needed
    });
  }

  viewPrivilegeUsage(subscription: SubscriptionDto) {
    const dialogData: PrivilegeUsageDialogData = {
      subscriptionId: subscription.id,
      userName: subscription.userName,
      planName: subscription.planName
    };

    const dialogRef = this.dialog.open(PrivilegeUsageDialogComponent, {
      width: '90vw',
      maxWidth: '1200px',
      height: '80vh',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(() => {
      // Dialog closed, no action needed
    });
  }

  pauseSubscription(subscriptionId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Pause Subscription',
      message: 'Are you sure you want to pause this subscription? The user will temporarily lose access to the services.',
      confirmText: 'Pause Subscription',
      cancelText: 'Cancel',
      type: 'warning',
      requireReason: true,
      reasonLabel: 'Pause Reason',
      reasonPlaceholder: 'Please provide a reason for pausing this subscription...'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '600px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.confirmed) {
        this.subscriptionService.pauseSubscription(subscriptionId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Subscription paused successfully', 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to pause subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error pausing subscription:', error);
            this.snackBar.open(error.message || 'Error pausing subscription', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  resumeSubscription(subscriptionId: string) {
    const dialogData: ConfirmationDialogData = {
      title: 'Resume Subscription',
      message: 'Are you sure you want to resume this paused subscription? The user will regain access to the services.',
      confirmText: 'Resume Subscription',
      cancelText: 'Cancel',
      type: 'info'
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subscriptionService.resumeSubscription(subscriptionId).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.snackBar.open('Subscription resumed successfully', 'Close', { duration: 3000 });
              this.loadSubscriptions();
            } else {
              this.snackBar.open(response.message || 'Failed to resume subscription', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            console.error('Error resuming subscription:', error);
            this.snackBar.open(error.message || 'Error resuming subscription', 'Close', { duration: 5000 });
          }
        });
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
