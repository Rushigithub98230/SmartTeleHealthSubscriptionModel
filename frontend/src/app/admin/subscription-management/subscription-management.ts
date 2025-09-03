import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { SubscriptionService } from '../../services/subscription.service';
import { PlanStepperComponent } from './plan-stepper.component';
import { 
  SubscriptionDto, 
  SubscriptionPlanDto, 
  CreateSubscriptionPlanDto, 
  PaginatedResponse 
} from '../../models/subscription.models';

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

  // Subscriptions data
  subscriptions: SubscriptionDto[] = [];
  subscriptionColumns = ['user', 'plan', 'status', 'price', 'nextBilling', 'actions'];
  subscriptionTotalCount = 0;
  subscriptionPageSize = 20;
  subscriptionCurrentPage = 0;
  subscriptionSearchTerm = '';
  selectedStatus = '';

  // UI state
  selectedTab = 0;
  loading = false;

  ngOnInit() {
    this.loadPlans();
    this.loadSubscriptions();
  }

  // Plans management
  loadPlans() {
    this.loading = true;
    this.subscriptionService.getAllPlans(
      this.planCurrentPage + 1, 
      this.planPageSize, 
      this.planSearchTerm
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.plans = response.data;
          this.planTotalCount = response.meta?.totalRecords || this.plans.length;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.snackBar.open('Error loading plans', 'Close', { duration: 3000 });
        this.loading = false;
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
    if (confirm('Are you sure you want to delete this plan?')) {
      this.subscriptionService.deletePlan(planId).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Plan deleted successfully', 'Close', { duration: 3000 });
            this.loadPlans();
          }
        },
        error: (error) => {
          console.error('Error deleting plan:', error);
          this.snackBar.open('Error deleting plan', 'Close', { duration: 3000 });
        }
      });
    }
  }



  // Subscriptions management
  loadSubscriptions() {
    this.loading = true;
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
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        this.snackBar.open('Error loading subscriptions', 'Close', { duration: 3000 });
        this.loading = false;
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

  viewSubscription(subscription: SubscriptionDto) {
    this.snackBar.open(`Subscription: ${subscription.planName} for ${subscription.userName}`, 'Close', { duration: 3000 });
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
    const reason = prompt('Please provide a reason for cancellation:');
    if (reason) {
      this.subscriptionService.cancelSubscription(subscriptionId, reason).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Subscription cancelled successfully', 'Close', { duration: 3000 });
            this.loadSubscriptions();
          }
        },
        error: (error) => {
          console.error('Error cancelling subscription:', error);
          this.snackBar.open('Error cancelling subscription', 'Close', { duration: 3000 });
        }
      });
    }
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
