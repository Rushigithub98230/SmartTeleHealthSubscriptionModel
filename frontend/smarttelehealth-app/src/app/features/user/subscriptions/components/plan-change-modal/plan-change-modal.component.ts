import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SubscriptionDto, SubscriptionPlanDto } from '../../../../../core/models';
import { SubscriptionService } from '../../../../../core/services/subscription.service';

export interface PlanChangeModalData {
  subscription: SubscriptionDto;
  availablePlans: SubscriptionPlanDto[];
  changeType: 'upgrade' | 'downgrade';
}

@Component({
  selector: 'app-plan-change-modal',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatSnackBarModule
  ],
  templateUrl: './plan-change-modal.component.html',
  styleUrls: ['./plan-change-modal.component.scss']
})
export class PlanChangeModalComponent implements OnInit {
  selectedPlan: SubscriptionPlanDto | null = null;
  isProcessing = false;
  errorMessage = '';
  
  // Filtered plans based on change type
  filteredPlans: SubscriptionPlanDto[] = [];

  constructor(
    public dialogRef: MatDialogRef<PlanChangeModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PlanChangeModalData,
    private subscriptionService: SubscriptionService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.filterPlans();
  }

  /**
   * Filter plans based on upgrade or downgrade
   */
  private filterPlans() {
    const currentPrice = this.data.subscription.currentPrice;
    
    this.filteredPlans = this.data.availablePlans.filter(plan => {
      if (this.data.changeType === 'upgrade') {
        // Show plans with higher price
        return plan.effectivePrice > currentPrice && plan.isActive;
      } else {
        // Show plans with lower price
        return plan.effectivePrice < currentPrice && plan.isActive;
      }
    });
    
    // Sort by price
    if (this.data.changeType === 'upgrade') {
      this.filteredPlans.sort((a, b) => a.effectivePrice - b.effectivePrice);
    } else {
      this.filteredPlans.sort((a, b) => b.effectivePrice - a.effectivePrice);
    }
  }

  /**
   * Select a plan
   */
  selectPlan(plan: SubscriptionPlanDto) {
    this.selectedPlan = plan;
    this.errorMessage = '';
  }

  /**
   * Calculate price difference
   */
  getPriceDifference(): number {
    if (!this.selectedPlan) return 0;
    return this.selectedPlan.effectivePrice - this.data.subscription.currentPrice;
  }

  /**
   * Get price difference display text
   */
  getPriceDifferenceText(): string {
    const diff = this.getPriceDifference();
    if (diff > 0) {
      return `+$${diff.toFixed(2)}/month`;
    } else {
      return `-$${Math.abs(diff).toFixed(2)}/month (You save!)`;
    }
  }

  /**
   * Confirm and schedule the plan change
   */
  async confirmChange() {
    if (!this.selectedPlan) {
      this.errorMessage = 'Please select a plan';
      return;
    }

    this.isProcessing = true;
    this.errorMessage = '';

    try {
      let response;
      
      if (this.data.changeType === 'upgrade') {
        response = await this.subscriptionService
          .scheduleUpgrade(this.data.subscription.id, this.selectedPlan.id)
          .toPromise();
      } else {
        response = await this.subscriptionService
          .scheduleDowngrade(this.data.subscription.id, this.selectedPlan.id)
          .toPromise();
      }

      if (response && (response.statusCode === 200 || response.statusCode === 201)) {
        this.snackBar.open(
          `${this.data.changeType === 'upgrade' ? 'Upgrade' : 'Downgrade'} scheduled successfully!`,
          'Close',
          { duration: 5000 }
        );
        this.dialogRef.close(true); // Return true to indicate success
      } else {
        this.errorMessage = response?.message || 'Failed to schedule plan change';
      }
    } catch (error: any) {
      console.error('Error scheduling plan change:', error);
      this.errorMessage = error.error?.message || 'An error occurred while scheduling the plan change';
    } finally {
      this.isProcessing = false;
    }
  }

  /**
   * Cancel and close modal
   */
  cancel() {
    this.dialogRef.close(false);
  }

  /**
   * Check if a plan is currently selected
   */
  isPlanSelected(plan: SubscriptionPlanDto): boolean {
    return this.selectedPlan?.id === plan.id;
  }

  /**
   * Get modal title
   */
  getTitle(): string {
    return this.data.changeType === 'upgrade' ? 'Upgrade Your Plan' : 'Downgrade Your Plan';
  }

  /**
   * Get effective date display
   */
  getEffectiveDate(): string {
    if (!this.data.subscription.nextBillingDate) {
      return 'Unknown';
    }
    return new Date(this.data.subscription.nextBillingDate).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  /**
   * Parse features from JSON string
   */
  parseFeatures(featuresJson: string | undefined): string[] {
    if (!featuresJson) return [];
    
    try {
      const parsed = JSON.parse(featuresJson);
      if (Array.isArray(parsed)) {
        return parsed;
      } else if (typeof parsed === 'object') {
        return Object.values(parsed);
      }
      return [featuresJson];
    } catch {
      return [featuresJson];
    }
  }
}

