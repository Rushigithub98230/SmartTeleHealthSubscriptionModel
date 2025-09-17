import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { SubscriptionService } from '../../services/subscription.service';
import { SubscriptionPlanDto, PlanPrivilegeDto } from '../../models/subscription.models';

export interface PlanDetailsDialogData {
  plan: SubscriptionPlanDto;
}

@Component({
  selector: 'app-plan-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="plan-details-dialog">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <mat-icon>subscriptions</mat-icon>
          {{ data.plan.name }}
        </h2>
        <button mat-icon-button (click)="close()" class="close-button">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="dialog-content" *ngIf="!loading; else loadingTemplate">
        <mat-tab-group>
          <!-- Basic Information Tab -->
          <mat-tab label="Basic Information">
            <div class="tab-content">
              <mat-card class="info-card">
                <mat-card-header>
                  <mat-card-title>Plan Details</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-grid">
                    <div class="info-item">
                      <label>Name:</label>
                      <span>{{ data.plan.name }}</span>
                    </div>
                    <div class="info-item">
                      <label>Description:</label>
                      <span>{{ data.plan.description || 'No description' }}</span>
                    </div>
                    <div class="info-item">
                      <label>Short Description:</label>
                      <span>{{ data.plan.shortDescription || 'No short description' }}</span>
                    </div>
                    <div class="info-item">
                      <label>Price:</label>
                      <span class="price">{{ data.plan.price | currency:'USD' }}</span>
                    </div>
                    <div class="info-item" *ngIf="data.plan.discountedPrice">
                      <label>Discounted Price:</label>
                      <span class="discounted-price">{{ data.plan.discountedPrice | currency:'USD' }}</span>
                    </div>
                    <div class="info-item">
                      <label>Billing Cycle ID:</label>
                      <span>{{ data.plan.billingCycleId || 'N/A' }}</span>
                    </div>
                    <div class="info-item">
                      <label>Currency ID:</label>
                      <span>{{ data.plan.currencyId || 'USD' }}</span>
                    </div>
                    <div class="info-item">
                      <label>Category ID:</label>
                      <span>{{ data.plan.categoryId || 'N/A' }}</span>
                    </div>
                    <div class="info-item">
                      <label>Status:</label>
                      <mat-chip [color]="data.plan.isActive ? 'primary' : 'warn'">
                        {{ data.plan.isActive ? 'Active' : 'Inactive' }}
                      </mat-chip>
                    </div>
                    <div class="info-item">
                      <label>Created:</label>
                      <span>{{ data.plan.createdDate | date:'medium' }}</span>
                    </div>
                    <div class="info-item" *ngIf="data.plan.updatedDate">
                      <label>Last Updated:</label>
                      <span>{{ data.plan.updatedDate | date:'medium' }}</span>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>

              <!-- Plan Tags -->
              <mat-card class="tags-card" *ngIf="hasTags()">
                <mat-card-header>
                  <mat-card-title>Plan Tags</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="tags-container">
                    <mat-chip *ngIf="data.plan.isMostPopular" color="accent">Most Popular</mat-chip>
                    <mat-chip *ngIf="data.plan.isTrending" color="primary">Trending</mat-chip>
                    <mat-chip *ngIf="data.plan.isFeatured" color="warn">Featured</mat-chip>
                    <mat-chip *ngIf="data.plan.isTrialAllowed" color="accent">Trial Available</mat-chip>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>

          <!-- Features Tab -->
          <mat-tab label="Features & Terms">
            <div class="tab-content">
              <mat-card class="features-card" *ngIf="data.plan.features">
                <mat-card-header>
                  <mat-card-title>Features</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="features-content">{{ data.plan.features }}</div>
                </mat-card-content>
              </mat-card>

              <mat-card class="terms-card" *ngIf="data.plan.terms">
                <mat-card-header>
                  <mat-card-title>Terms & Conditions</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="terms-content">{{ data.plan.terms }}</div>
                </mat-card-content>
              </mat-card>

              <mat-card class="trial-card" *ngIf="data.plan.isTrialAllowed">
                <mat-card-header>
                  <mat-card-title>Trial Information</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-grid">
                    <div class="info-item">
                      <label>Trial Duration:</label>
                      <span>{{ data.plan.trialDurationInDays }} days</span>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>

          <!-- Privileges Tab -->
          <mat-tab label="Privileges">
            <div class="tab-content">
              <div *ngIf="privileges.length === 0" class="no-privileges">
                <mat-icon>info</mat-icon>
                <p>No privileges assigned to this plan.</p>
              </div>
              
              <div *ngIf="privileges.length > 0" class="privileges-container">
                <mat-card *ngFor="let privilege of privileges" class="privilege-card">
                  <mat-card-header>
                    <mat-card-title>{{ privilege.privilegeName }}</mat-card-title>
                    <mat-card-subtitle>Privilege</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="privilege-details">
                      <div class="info-item">
                        <label>Value:</label>
                        <span>{{ privilege.value === -1 ? 'Unlimited' : privilege.value }}</span>
                      </div>
                      <div class="info-item">
                        <label>Usage Period:</label>
                        <span>{{ privilege.usagePeriodName || 'N/A' }}</span>
                      </div>
                      <div class="info-item" *ngIf="privilege.description">
                        <label>Description:</label>
                        <span>{{ privilege.description }}</span>
                      </div>
                      <div class="info-item" *ngIf="privilege.effectiveDate">
                        <label>Effective Date:</label>
                        <span>{{ privilege.effectiveDate | date:'medium' }}</span>
                      </div>
                      <div class="info-item" *ngIf="privilege.expirationDate">
                        <label>Expiration Date:</label>
                        <span>{{ privilege.expirationDate | date:'medium' }}</span>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- Stripe Integration Tab -->
          <mat-tab label="Stripe Integration" *ngIf="hasStripeIntegration()">
            <div class="tab-content">
              <mat-card class="stripe-card">
                <mat-card-header>
                  <mat-card-title>Stripe Configuration</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-grid">
                    <div class="info-item" *ngIf="data.plan.stripeProductId">
                      <label>Product ID:</label>
                      <span class="stripe-id">{{ data.plan.stripeProductId }}</span>
                    </div>
                    <div class="info-item" *ngIf="data.plan.stripeMonthlyPriceId">
                      <label>Monthly Price ID:</label>
                      <span class="stripe-id">{{ data.plan.stripeMonthlyPriceId }}</span>
                    </div>
                    <div class="info-item" *ngIf="data.plan.stripeQuarterlyPriceId">
                      <label>Quarterly Price ID:</label>
                      <span class="stripe-id">{{ data.plan.stripeQuarterlyPriceId }}</span>
                    </div>
                    <div class="info-item" *ngIf="data.plan.stripeAnnualPriceId">
                      <label>Annual Price ID:</label>
                      <span class="stripe-id">{{ data.plan.stripeAnnualPriceId }}</span>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>
        </mat-tab-group>
      </div>

      <ng-template #loadingTemplate>
        <div class="loading-container">
          <mat-spinner diameter="40"></mat-spinner>
          <p>Loading plan details...</p>
        </div>
      </ng-template>

      <div class="dialog-actions">
        <button mat-button (click)="close()">Close</button>
        <button mat-raised-button color="primary" (click)="editPlan()">
          <mat-icon>edit</mat-icon>
          Edit Plan
        </button>
      </div>
    </div>
  `,
  styles: [`
    .plan-details-dialog {
      max-width: 800px;
      max-height: 90vh;
      overflow: hidden;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      border-bottom: 1px solid #e0e0e0;
    }

    .dialog-header h2 {
      margin: 0;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .close-button {
      margin-left: auto;
    }

    .dialog-content {
      max-height: 60vh;
      overflow-y: auto;
      padding: 16px;
    }

    .tab-content {
      padding: 16px 0;
    }

    .info-card, .tags-card, .features-card, .terms-card, .trial-card, .stripe-card {
      margin-bottom: 16px;
    }

    .info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .info-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .info-item label {
      font-weight: 500;
      color: #666;
      font-size: 0.9em;
    }

    .info-item span {
      font-size: 1em;
    }

    .price {
      font-size: 1.2em;
      font-weight: 600;
      color: #2e7d32;
    }

    .discounted-price {
      font-size: 1.1em;
      font-weight: 600;
      color: #f57c00;
    }

    .tags-container {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .features-content, .terms-content {
      white-space: pre-wrap;
      line-height: 1.6;
    }

    .no-privileges {
      text-align: center;
      padding: 40px;
      color: #666;
    }

    .no-privileges mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }

    .privileges-container {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .privilege-card {
      border-left: 4px solid #2196f3;
    }

    .privilege-details .info-item {
      margin-bottom: 8px;
    }

    .stripe-id {
      font-family: monospace;
      background-color: #f5f5f5;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.9em;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px;
    }

    .loading-container p {
      margin-top: 16px;
      color: #666;
    }

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
    }

    @media (max-width: 768px) {
      .info-grid {
        grid-template-columns: 1fr;
      }
      
      .plan-details-dialog {
        max-width: 95vw;
      }
    }
  `]
})
export class PlanDetailsDialogComponent implements OnInit {
  privileges: PlanPrivilegeDto[] = [];
  loading = false;

  constructor(
    public dialogRef: MatDialogRef<PlanDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PlanDetailsDialogData,
    private subscriptionService: SubscriptionService
  ) {}

  ngOnInit() {
    this.loadPlanPrivileges();
  }

  loadPlanPrivileges() {
    if (this.data.plan.id) {
      this.loading = true;
      this.subscriptionService.getPlanPrivileges(this.data.plan.id).subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.privileges = response.data;
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading plan privileges:', error);
          this.loading = false;
        }
      });
    }
  }

  hasTags(): boolean {
    return !!(this.data.plan.isMostPopular || this.data.plan.isTrending || 
              this.data.plan.isFeatured || this.data.plan.isTrialAllowed);
  }

  hasStripeIntegration(): boolean {
    return !!(this.data.plan.stripeProductId || this.data.plan.stripeMonthlyPriceId || 
              this.data.plan.stripeQuarterlyPriceId || this.data.plan.stripeAnnualPriceId);
  }

  close() {
    this.dialogRef.close();
  }

  editPlan() {
    this.dialogRef.close({ action: 'edit', plan: this.data.plan });
  }
}
