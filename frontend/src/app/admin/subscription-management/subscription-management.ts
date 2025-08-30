import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
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
import { SubscriptionStepperComponent } from '../subscription-stepper/subscription-stepper';
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
    SubscriptionStepperComponent
  ],
  template: `
    <div class="admin-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>
            <mat-icon>subscriptions</mat-icon>
            Subscription Management
          </mat-card-title>
        </mat-card-header>

        <mat-card-content>
          <mat-tab-group [(selectedIndex)]="selectedTab">
            <!-- Subscription Plans Tab -->
            <mat-tab label="Subscription Plans">
              <div class="tab-content">
                <div class="actions-bar">
                  <button mat-raised-button color="primary" (click)="openCreatePlanDialog()">
                    <mat-icon>add</mat-icon>
                    Create New Plan
                  </button>
                  
                  <mat-form-field appearance="outline" class="search-field">
                    <mat-label>Search Plans</mat-label>
                    <input matInput [(ngModel)]="planSearchTerm" (input)="searchPlans()">
                    <mat-icon matSuffix>search</mat-icon>
                  </mat-form-field>
                </div>

                <div class="table-container">
                  <table mat-table [dataSource]="plans" class="plans-table">
                    <ng-container matColumnDef="name">
                      <th mat-header-cell *matHeaderCellDef>Name</th>
                      <td mat-cell *matCellDef="let plan">{{ plan.name }}</td>
                    </ng-container>

                    <ng-container matColumnDef="price">
                      <th mat-header-cell *matHeaderCellDef>Price</th>
                      <td mat-cell *matCellDef="let plan">${{ plan.price }}</td>
                    </ng-container>

                    <ng-container matColumnDef="status">
                      <th mat-header-cell *matHeaderCellDef>Status</th>
                      <td mat-cell *matCellDef="let plan">
                        <mat-chip [color]="plan.isActive ? 'primary' : 'warn'">
                          {{ plan.isActive ? 'Active' : 'Inactive' }}
                        </mat-chip>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="popular">
                      <th mat-header-cell *matHeaderCellDef>Tags</th>
                      <td mat-cell *matCellDef="let plan">
                        <mat-chip *ngIf="plan.isMostPopular" color="accent">Popular</mat-chip>
                        <mat-chip *ngIf="plan.isTrending" color="primary">Trending</mat-chip>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let plan">
                        <button mat-icon-button [matMenuTriggerFor]="menu">
                          <mat-icon>more_vert</mat-icon>
                        </button>
                        <mat-menu #menu="matMenu">
                          <button mat-menu-item (click)="editPlan(plan)">
                            <mat-icon>edit</mat-icon>
                            Edit
                          </button>
                          <button mat-menu-item (click)="viewPlan(plan)">
                            <mat-icon>visibility</mat-icon>
                            View Details
                          </button>
                          <button mat-menu-item (click)="deletePlan(plan.id)">
                            <mat-icon>delete</mat-icon>
                            Delete
                          </button>
                        </mat-menu>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="planColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: planColumns;"></tr>
                  </table>
                </div>

                <mat-paginator 
                  [length]="planTotalCount"
                  [pageSize]="planPageSize"
                  [pageSizeOptions]="[10, 20, 50]"
                  (page)="onPlanPageChange($event)">
                </mat-paginator>
              </div>
            </mat-tab>

            <!-- User Subscriptions Tab -->
            <mat-tab label="User Subscriptions">
              <div class="tab-content">

              // Subscription dialog state
              showSubscriptionDialog = false;
              editingSubscription?: SubscriptionDto;
                <div class="actions-bar">
                  <mat-form-field appearance="outline" class="search-field">
                    <mat-label>Search Subscriptions</mat-label>
                    <input matInput [(ngModel)]="subscriptionSearchTerm" (input)="searchSubscriptions()">
                    <mat-icon matSuffix>search</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                this.editingSubscription = subscription;
                this.showSubscriptionDialog = true;
                    <mat-label>Filter by Status</mat-label>
                    <mat-select [(ngModel)]="selectedStatus" (selectionChange)="filterSubscriptions()">
                      <mat-option value="">All</mat-option>
                      <mat-option value="active">Active</mat-option>
                      <mat-option value="paused">Paused</mat-option>
                      <mat-option value="cancelled">Cancelled</mat-option>
                      <mat-option value="expired">Expired</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>


              // Create/Edit Subscription Dialog
              openCreateSubscriptionDialog() {
                this.editingSubscription = undefined;
                this.showSubscriptionDialog = true;
              }

              closeSubscriptionDialog() {
                this.showSubscriptionDialog = false;
                this.editingSubscription = undefined;
              }

              onSubscriptionCreated(subscriptionData: CreateSubscriptionDto) {
                this.subscriptionService.createSubscription(subscriptionData).subscribe({
                  next: (response) => {
                    if (response.statusCode === 200) {
                      this.snackBar.open('Subscription created successfully', 'Close', { duration: 3000 });
                      this.loadSubscriptions();
                      this.closeSubscriptionDialog();
                    }
                  },
                  error: (error) => {
                    console.error('Error creating subscription:', error);
                    this.snackBar.open('Error creating subscription', 'Close', { duration: 3000 });
                  }
                });
              }

              onSubscriptionUpdated(event: { id: string, subscription: CreateSubscriptionDto }) {
                this.subscriptionService.updateSubscription(event.id, event.subscription).subscribe({
                  next: (response) => {
                    if (response.statusCode === 200) {
                      this.snackBar.open('Subscription updated successfully', 'Close', { duration: 3000 });
                      this.loadSubscriptions();
                      this.closeSubscriptionDialog();
                    }
                  },
                  error: (error) => {
                    console.error('Error updating subscription:', error);
                    this.snackBar.open('Error updating subscription', 'Close', { duration: 3000 });
                  }
                });
              }
                <div class="table-container">
                  <table mat-table [dataSource]="subscriptions" class="subscriptions-table">
                    <ng-container matColumnDef="user">
                      <th mat-header-cell *matHeaderCellDef>User</th>
                      <td mat-cell *matCellDef="let subscription">{{ subscription.userName }}</td>
                    </ng-container>

                    <ng-container matColumnDef="plan">
                      <th mat-header-cell *matHeaderCellDef>Plan</th>
                      <td mat-cell *matCellDef="let subscription">{{ subscription.planName }}</td>
                    </ng-container>

                    <ng-container matColumnDef="status">
                      <th mat-header-cell *matHeaderCellDef>Status</th>
                      <td mat-cell *matCellDef="let subscription">
                        <mat-chip [color]="getStatusColor(subscription.status)">
                          {{ subscription.status }}
                        </mat-chip>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="price">
                      <th mat-header-cell *matHeaderCellDef>Price</th>
                        <td mat-cell *matCellDef="let subscription">{{ subscription.currentPrice | currency:'USD' }}</td>
                    </ng-container>

                    <ng-container matColumnDef="nextBilling">
                      <th mat-header-cell *matHeaderCellDef>Next Billing</th>
                      <td mat-cell *matCellDef="let subscription">{{ subscription.nextBillingDate | date:'short' }}</td>
                    </ng-container>

                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let subscription">
                        <button mat-icon-button [matMenuTriggerFor]="subscriptionMenu">
                          <mat-icon>more_vert</mat-icon>
                        </button>
                        <mat-menu #subscriptionMenu="matMenu">
                          <button mat-menu-item (click)="viewSubscription(subscription)">
                            <mat-icon>visibility</mat-icon>
                            View Details
                          </button>
                          <button mat-menu-item (click)="pauseSubscription(subscription.id)" [disabled]="subscription.isPaused">
                            <mat-icon>pause</mat-icon>
                            Pause
                          </button>
                          <button mat-menu-item (click)="resumeSubscription(subscription.id)" [disabled]="!subscription.isPaused">
                            <mat-icon>play_arrow</mat-icon>
                            Resume
                          </button>
                          <button mat-menu-item (click)="cancelSubscription(subscription.id)" [disabled]="subscription.isCancelled">
                            <mat-icon>cancel</mat-icon>
                            Cancel
                          </button>
                        </mat-menu>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="subscriptionColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: subscriptionColumns;"></tr>
                  </table>
                </div>

                <mat-paginator 
                  [length]="subscriptionTotalCount"
                  [pageSize]="subscriptionPageSize"
                  [pageSizeOptions]="[10, 20, 50]"
                  (page)="onSubscriptionPageChange($event)">
                </mat-paginator>
              </div>
            </mat-tab>
          </mat-tab-group>
        </mat-card-content>
      </mat-card>
    </div>

    <!-- Create/Edit Plan Dialog -->
    <div *ngIf="showCreateDialog" class="dialog-overlay" (click)="closeCreateDialog()">
      <div class="dialog-content" (click)="$event.stopPropagation()">
        <div class="dialog-header">
          <h2>{{ editingPlan ? 'Edit Plan' : 'Create New Plan' }}</h2>
          <button mat-icon-button (click)="closeCreateDialog()">
            <mat-icon>close</mat-icon>
          </button>
        </div>
        <app-subscription-stepper 
          [editMode]="!!editingPlan"
          [existingPlan]="editingPlan"
          (planSubmitted)="onPlanCreated($event)"
          (planUpdated)="onPlanUpdated($event)">
        </app-subscription-stepper>
      </div>
    </div>
  `,
  styles: [`
    .admin-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .tab-content {
      padding: 20px 0;
    }

    .actions-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      gap: 16px;
    }

    .search-field {
      min-width: 300px;
    }

    .table-container {
      margin: 20px 0;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
      overflow: auto;
    }

    .plans-table,
    .subscriptions-table {
      width: 100%;
    }

    .dialog-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }

    .dialog-content {
      background: white;
      border-radius: 8px;
      padding: 0;
      max-width: 800px;
      width: 90%;
      max-height: 90%;
      overflow: auto;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
    }

    .dialog-header h2 {
      margin: 0;
    }

    mat-chip {
      margin-right: 8px;
    }
  `]
})
export class SubscriptionManagementComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);

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
  showCreateDialog = false;
  editingPlan?: SubscriptionPlanDto;
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
          this.plans = response.data.items;
          this.planTotalCount = response.data.totalCount;
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
    this.editingPlan = undefined;
    this.showCreateDialog = true;
  }

  editPlan(plan: SubscriptionPlanDto) {
    this.editingPlan = plan;
    this.showCreateDialog = true;
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

  onPlanCreated(planData: CreateSubscriptionPlanDto) {
    this.subscriptionService.createPlan(planData).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.snackBar.open('Plan created successfully', 'Close', { duration: 3000 });
          this.loadPlans();
          this.closeCreateDialog();
        }
      },
      error: (error) => {
        console.error('Error creating plan:', error);
        this.snackBar.open('Error creating plan', 'Close', { duration: 3000 });
      }
    });
  }

  onPlanUpdated(event: { id: string, plan: CreateSubscriptionPlanDto }) {
    const updateData = { id: event.id, ...event.plan } as any;
    this.subscriptionService.updatePlan(event.id, updateData).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.snackBar.open('Plan updated successfully', 'Close', { duration: 3000 });
          this.loadPlans();
          this.closeCreateDialog();
        }
      },
      error: (error) => {
        console.error('Error updating plan:', error);
        this.snackBar.open('Error updating plan', 'Close', { duration: 3000 });
      }
    });
  }

  closeCreateDialog() {
    this.showCreateDialog = false;
    this.editingPlan = undefined;
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
          this.subscriptions = response.data.items;
          this.subscriptionTotalCount = response.data.totalCount;
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
