import { Component, Inject, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SubscriptionService } from '../../services/subscription.service';
import { SubscriptionPlanDto } from '../../models/subscription.models';

export interface PlanSelectionData {
  title: string;
  currentPlanId: string;
  currentPlanName: string;
  actionType: 'upgrade' | 'downgrade';
}

@Component({
  selector: 'app-plan-selection-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatCardModule,
    MatListModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    
    <div mat-dialog-content>
      <div class="current-plan">
        <mat-icon>info</mat-icon>
        <span>Current Plan: <strong>{{ data.currentPlanName }}</strong></span>
      </div>

      <div *ngIf="loading" class="loading-container">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Loading available plans...</p>
      </div>

      <div *ngIf="!loading && availablePlans.length === 0" class="empty-state">
        <mat-icon>warning</mat-icon>
        <p>No plans available for {{ data.actionType }}</p>
      </div>

      <div *ngIf="!loading && availablePlans.length > 0" class="plans-list">
        <mat-list>
          <mat-list-item 
            *ngFor="let plan of availablePlans" 
            class="plan-item"
            [class.selected]="selectedPlan?.id === plan.id"
            (click)="selectPlan(plan)">
            <mat-icon matListItemIcon>{{ getPlanIcon(plan) }}</mat-icon>
            <div matListItemTitle>{{ plan.name }}</div>
            <div matListItemLine>{{ plan.description }}</div>
            <div matListItemLine class="plan-price">{{ plan.price | currency:'USD' }} / {{ getBillingCycle(plan.billingCycleId) }}</div>
          </mat-list-item>
        </mat-list>
      </div>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="primary" 
        [disabled]="!selectedPlan"
        (click)="onConfirm()">
        {{ data.actionType === 'upgrade' ? 'Upgrade' : 'Downgrade' }}
      </button>
    </div>
  `,
  styles: [`
    .current-plan {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background-color: #f5f5f5;
      border-radius: 4px;
      margin-bottom: 16px;
    }

    .loading-container {
      text-align: center;
      padding: 24px;
    }

    .loading-container mat-spinner {
      margin: 0 auto 16px;
    }

    .empty-state {
      text-align: center;
      padding: 24px;
      color: #666;
    }

    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }

    .plans-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .plan-item {
      cursor: pointer;
      border: 2px solid transparent;
      border-radius: 8px;
      margin-bottom: 8px;
      transition: all 0.2s ease;
    }

    .plan-item:hover {
      background-color: #f0f0f0;
      border-color: #e0e0e0;
    }

    .plan-item.selected {
      background-color: #e3f2fd;
      border-color: #2196f3;
    }

    .plan-price {
      font-weight: 600;
      color: #2196f3;
    }

    mat-dialog-content {
      min-width: 500px;
      max-width: 600px;
      min-height: 200px;
    }
  `]
})
export class PlanSelectionDialogComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);

  availablePlans: SubscriptionPlanDto[] = [];
  selectedPlan: SubscriptionPlanDto | null = null;
  loading = true;

  constructor(
    public dialogRef: MatDialogRef<PlanSelectionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PlanSelectionData
  ) {}

  ngOnInit() {
    this.loadAvailablePlans();
  }

  loadAvailablePlans() {
    this.loading = true;
    this.subscriptionService.getAllPlans(1, 100, undefined, undefined, true).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          // Filter out the current plan and apply upgrade/downgrade logic
          this.availablePlans = response.data.filter((plan: SubscriptionPlanDto) => 
            plan.id !== this.data.currentPlanId && plan.isActive
          );
        } else {
          this.availablePlans = [];
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.snackBar.open('Error loading plans: ' + error.message, 'Close', { duration: 5000 });
        this.availablePlans = [];
        this.loading = false;
      }
    });
  }

  selectPlan(plan: SubscriptionPlanDto) {
    this.selectedPlan = plan;
  }

  getPlanIcon(plan: SubscriptionPlanDto): string {
    if (plan.isMostPopular) return 'star';
    if (plan.isFeatured) return 'featured_play_list';
    if (plan.isTrending) return 'trending_up';
    return 'assignment';
  }

  getBillingCycle(billingCycleId: string): string {
    // Get billing cycle name from the service
    return this.subscriptionService.getBillingCycleName(billingCycleId);
  }

  onCancel() {
    this.dialogRef.close();
  }

  onConfirm() {
    if (this.selectedPlan) {
      this.dialogRef.close(this.selectedPlan);
    }
  }
}