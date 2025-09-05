import { Component, OnInit, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { SubscriptionService } from '../../services/subscription.service';
import { 
  SubscriptionDto, 
  SubscriptionDetailsDto,
  BillingRecordDto,
  SubscriptionPaymentDto,
  SubscriptionStatusHistoryDto,
  UserSubscriptionPrivilegeUsageDto
} from '../../models/subscription.models';

@Component({
  selector: 'app-subscription-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="subscription-details-dialog">
      <div mat-dialog-title class="dialog-header">
        <div class="header-info">
          <mat-icon>person</mat-icon>
          <div>
            <h2>{{ subscription.userName }}</h2>
            <p>{{ subscription.planName }} Subscription</p>
          </div>
        </div>
        <button mat-icon-button mat-dialog-close>
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div mat-dialog-content class="dialog-content">
        <mat-tab-group>
          <!-- Overview Tab -->
          <mat-tab label="Overview">
            <div class="tab-content">
              <div class="overview-grid">
                <mat-card class="info-card">
                  <mat-card-header>
                    <mat-card-title>Subscription Info</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="info-row">
                      <span class="label">Status:</span>
                      <mat-chip [color]="getStatusColor(subscription.status)">
                        {{ subscription.status }}
                      </mat-chip>
                    </div>
                    <div class="info-row">
                      <span class="label">Current Price:</span>
                      <span class="value">{{ subscription.currentPrice | currency:'USD' }}</span>
                    </div>
                    <div class="info-row">
                      <span class="label">Start Date:</span>
                      <span class="value">{{ subscription.startDate | date:'medium' }}</span>
                    </div>
                    <div class="info-row">
                      <span class="label">Next Billing:</span>
                      <span class="value">{{ subscription.nextBillingDate | date:'medium' }}</span>
                    </div>
                    <div class="info-row">
                      <span class="label">Auto Renew:</span>
                      <mat-chip [color]="subscription.autoRenew ? 'primary' : 'warn'">
                        {{ subscription.autoRenew ? 'Yes' : 'No' }}
                      </mat-chip>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="info-card">
                  <mat-card-header>
                    <mat-card-title>Plan Details</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="info-row">
                      <span class="label">Plan:</span>
                      <span class="value">{{ subscription.planName }}</span>
                    </div>
                    <div class="info-row">
                      <span class="label">Description:</span>
                      <span class="value">{{ subscription.planDescription || 'N/A' }}</span>
                    </div>
                    <div class="info-row" *ngIf="subscription.notes">
                      <span class="label">Notes:</span>
                      <span class="value">{{ subscription.notes }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="info-card" *ngIf="subscription.isPaused || subscription.pausedDate">
                  <mat-card-header>
                    <mat-card-title>Pause Information</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="info-row" *ngIf="subscription.pausedDate">
                      <span class="label">Paused Date:</span>
                      <span class="value">{{ subscription.pausedDate | date:'medium' }}</span>
                    </div>
                    <div class="info-row" *ngIf="subscription.resumedDate">
                      <span class="label">Resumed Date:</span>
                      <span class="value">{{ subscription.resumedDate | date:'medium' }}</span>
                    </div>
                    <div class="info-row" *ngIf="subscription.pauseReason">
                      <span class="label">Pause Reason:</span>
                      <span class="value">{{ subscription.pauseReason }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="info-card" *ngIf="subscription.isCancelled || subscription.cancelledDate">
                  <mat-card-header>
                    <mat-card-title>Cancellation Information</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="info-row" *ngIf="subscription.cancelledDate">
                      <span class="label">Cancelled Date:</span>
                      <span class="value">{{ subscription.cancelledDate | date:'medium' }}</span>
                    </div>
                    <div class="info-row" *ngIf="subscription.cancellationReason">
                      <span class="label">Reason:</span>
                      <span class="value">{{ subscription.cancellationReason }}</span>
                    </div>
                    <div class="info-row" *ngIf="subscription.expirationDate">
                      <span class="label">Expires:</span>
                      <span class="value">{{ subscription.expirationDate | date:'medium' }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- Billing History Tab -->
          <mat-tab label="Billing History">
            <div class="tab-content">
              <div *ngIf="loadingBilling" class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
                <p>Loading billing history...</p>
              </div>
              
              <div *ngIf="!loadingBilling && billingHistory.length === 0" class="empty-state">
                <mat-icon>receipt_long</mat-icon>
                <h3>No Billing Records</h3>
                <p>No billing records found for this subscription.</p>
              </div>

              <div *ngIf="!loadingBilling && billingHistory.length > 0" class="billing-table">
                <table mat-table [dataSource]="billingHistory">
                  <ng-container matColumnDef="billingDate">
                    <th mat-header-cell *matHeaderCellDef>Billing Date</th>
                    <td mat-cell *matCellDef="let record">{{ record.billingDate | date:'short' }}</td>
                  </ng-container>

                  <ng-container matColumnDef="amount">
                    <th mat-header-cell *matHeaderCellDef>Amount</th>
                    <td mat-cell *matCellDef="let record">{{ record.amount | currency:record.currency }}</td>
                  </ng-container>

                  <ng-container matColumnDef="dueDate">
                    <th mat-header-cell *matHeaderCellDef>Due Date</th>
                    <td mat-cell *matCellDef="let record">{{ record.dueDate | date:'short' }}</td>
                  </ng-container>

                  <ng-container matColumnDef="paidDate">
                    <th mat-header-cell *matHeaderCellDef>Paid Date</th>
                    <td mat-cell *matCellDef="let record">
                      {{ record.paidDate ? (record.paidDate | date:'short') : 'Unpaid' }}
                    </td>
                  </ng-container>

                  <ng-container matColumnDef="status">
                    <th mat-header-cell *matHeaderCellDef>Status</th>
                    <td mat-cell *matCellDef="let record">
                      <mat-chip [color]="getBillingStatusColor(record.status)">
                        {{ record.status }}
                      </mat-chip>
                    </td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="billingColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: billingColumns;"></tr>
                </table>
              </div>
            </div>
          </mat-tab>

          <!-- Privilege Usage Tab -->
          <mat-tab label="Privilege Usage">
            <div class="tab-content">
              <div *ngIf="loadingPrivileges" class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
                <p>Loading privilege usage...</p>
              </div>
              
              <div *ngIf="!loadingPrivileges && privilegeUsage.length === 0" class="empty-state">
                <mat-icon>security</mat-icon>
                <h3>No Privilege Usage</h3>
                <p>No privilege usage records found for this subscription.</p>
              </div>

              <div *ngIf="!loadingPrivileges && privilegeUsage.length > 0" class="privilege-cards">
                <mat-card *ngFor="let usage of privilegeUsage" class="privilege-card">
                  <mat-card-header>
                    <mat-card-title>{{ usage.privilegeName }}</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="usage-info">
                      <div class="usage-bar">
                        <div class="usage-progress" 
                             [style.width.%]="getUsagePercentage(usage.usedCount, usage.allowedCount)">
                        </div>
                      </div>
                      <div class="usage-text">
                        <span>{{ usage.usedCount }} / {{ usage.allowedCount === -1 ? 'Unlimited' : usage.allowedCount }}</span>
                        <span class="usage-percentage">{{ getUsagePercentage(usage.usedCount, usage.allowedCount) }}%</span>
                      </div>
                      <div class="usage-dates">
                        <div *ngIf="usage.lastUsedDate">
                          <strong>Last Used:</strong> {{ usage.lastUsedDate | date:'medium' }}
                        </div>
                        <div>
                          <strong>Reset Date:</strong> {{ usage.resetDate | date:'medium' }}
                        </div>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- Status History Tab -->
          <mat-tab label="Status History">
            <div class="tab-content">
              <div *ngIf="loadingHistory" class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
                <p>Loading status history...</p>
              </div>
              
              <div *ngIf="!loadingHistory && statusHistory.length === 0" class="empty-state">
                <mat-icon>history</mat-icon>
                <h3>No Status History</h3>
                <p>No status change history found for this subscription.</p>
              </div>

              <div *ngIf="!loadingHistory && statusHistory.length > 0" class="history-timeline">
                <div *ngFor="let history of statusHistory" class="timeline-item">
                  <div class="timeline-marker"></div>
                  <div class="timeline-content">
                    <div class="timeline-header">
                      <span class="status-change">
                        <mat-chip [color]="getStatusColor(history.fromStatus)">{{ history.fromStatus }}</mat-chip>
                        <mat-icon>arrow_forward</mat-icon>
                        <mat-chip [color]="getStatusColor(history.toStatus)">{{ history.toStatus }}</mat-chip>
                      </span>
                      <span class="change-date">{{ history.changedDate | date:'medium' }}</span>
                    </div>
                    <div class="timeline-details" *ngIf="history.reason || history.notes">
                      <p *ngIf="history.reason"><strong>Reason:</strong> {{ history.reason }}</p>
                      <p *ngIf="history.notes"><strong>Notes:</strong> {{ history.notes }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </div>

      <div mat-dialog-actions class="dialog-actions">
        <button mat-button mat-dialog-close>Close</button>
        <button mat-raised-button color="primary" (click)="refreshData()">
          <mat-icon>refresh</mat-icon>
          Refresh
        </button>
      </div>
    </div>
  `,
  styles: [`
    .subscription-details-dialog {
      width: 90vw;
      max-width: 1200px;
      height: 80vh;
      display: flex;
      flex-direction: column;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      border-bottom: 1px solid #e0e0e0;
    }

    .header-info {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .header-info mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: #1976d2;
    }

    .header-info h2 {
      margin: 0;
      font-size: 24px;
      font-weight: 600;
    }

    .header-info p {
      margin: 0;
      color: #666;
    }

    .dialog-content {
      flex: 1;
      overflow: hidden;
    }

    .tab-content {
      padding: 24px;
      height: calc(80vh - 200px);
      overflow-y: auto;
    }

    .overview-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
    }

    .info-card {
      height: fit-content;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 8px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .info-row:last-child {
      border-bottom: none;
    }

    .label {
      font-weight: 500;
      color: #666;
    }

    .value {
      font-weight: 600;
      color: #333;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 200px;
      gap: 16px;
    }

    .empty-state {
      text-align: center;
      padding: 48px 24px;
      color: #666;
    }

    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
      color: #ccc;
    }

    .billing-table,
    .privilege-cards {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .privilege-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      padding: 0;
      background: transparent;
      box-shadow: none;
    }

    .privilege-card {
      border-radius: 8px;
    }

    .usage-info {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .usage-bar {
      width: 100%;
      height: 8px;
      background-color: #e0e0e0;
      border-radius: 4px;
      overflow: hidden;
    }

    .usage-progress {
      height: 100%;
      background-color: #1976d2;
      transition: width 0.3s ease;
    }

    .usage-text {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 500;
    }

    .usage-percentage {
      color: #1976d2;
    }

    .usage-dates {
      font-size: 14px;
      color: #666;
    }

    .usage-dates div {
      margin-bottom: 4px;
    }

    .history-timeline {
      position: relative;
      padding-left: 30px;
    }

    .history-timeline::before {
      content: '';
      position: absolute;
      left: 15px;
      top: 0;
      bottom: 0;
      width: 2px;
      background-color: #e0e0e0;
    }

    .timeline-item {
      position: relative;
      margin-bottom: 24px;
    }

    .timeline-marker {
      position: absolute;
      left: -37px;
      top: 8px;
      width: 12px;
      height: 12px;
      background-color: #1976d2;
      border-radius: 50%;
      border: 3px solid white;
      box-shadow: 0 0 0 2px #1976d2;
    }

    .timeline-content {
      background: white;
      border-radius: 8px;
      padding: 16px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .timeline-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .status-change {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .change-date {
      color: #666;
      font-size: 14px;
    }

    .timeline-details p {
      margin: 4px 0;
      font-size: 14px;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    @media (max-width: 768px) {
      .subscription-details-dialog {
        width: 95vw;
        height: 90vh;
      }

      .overview-grid {
        grid-template-columns: 1fr;
      }

      .privilege-cards {
        grid-template-columns: 1fr;
      }

      .timeline-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
    }
  `]
})
export class SubscriptionDetailsDialogComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);

  subscription: SubscriptionDto;
  billingHistory: BillingRecordDto[] = [];
  privilegeUsage: UserSubscriptionPrivilegeUsageDto[] = [];
  statusHistory: SubscriptionStatusHistoryDto[] = [];

  loadingBilling = false;
  loadingPrivileges = false;
  loadingHistory = false;

  billingColumns = ['billingDate', 'amount', 'dueDate', 'paidDate', 'status'];

  constructor(
    public dialogRef: MatDialogRef<SubscriptionDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { subscription: SubscriptionDto }
  ) {
    this.subscription = data.subscription;
  }

  ngOnInit() {
    this.loadBillingHistory();
    this.loadPrivilegeUsage();
    this.loadStatusHistory();
  }

  private loadBillingHistory() {
    this.loadingBilling = true;
    this.subscriptionService.getBillingHistory(this.subscription.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingHistory = response.data || [];
        }
        this.loadingBilling = false;
      },
      error: (error) => {
        console.error('Error loading billing history:', error);
        this.loadingBilling = false;
      }
    });
  }

  private loadPrivilegeUsage() {
    this.loadingPrivileges = true;
    this.subscriptionService.getPrivilegeUsage(this.subscription.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.privilegeUsage = response.data || [];
        }
        this.loadingPrivileges = false;
      },
      error: (error) => {
        console.error('Error loading privilege usage:', error);
        this.loadingPrivileges = false;
      }
    });
  }

  private loadStatusHistory() {
    this.loadingHistory = true;
    this.subscriptionService.getSubscriptionHistory(this.subscription.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.statusHistory = response.data || [];
        }
        this.loadingHistory = false;
      },
      error: (error) => {
        console.error('Error loading status history:', error);
        this.loadingHistory = false;
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

  getBillingStatusColor(status: string): 'primary' | 'accent' | 'warn' | undefined {
    switch (status?.toLowerCase()) {
      case 'paid': return 'primary';
      case 'pending': return 'accent';
      case 'failed':
      case 'overdue': return 'warn';
      default: return undefined;
    }
  }

  getUsagePercentage(used: number, allowed: number): number {
    if (allowed === -1) return 0; // Unlimited
    if (allowed === 0) return 100; // Disabled
    return Math.min((used / allowed) * 100, 100);
  }

  refreshData() {
    this.loadBillingHistory();
    this.loadPrivilegeUsage();
    this.loadStatusHistory();
    this.snackBar.open('Data refreshed successfully', 'Close', { duration: 3000 });
  }
}

