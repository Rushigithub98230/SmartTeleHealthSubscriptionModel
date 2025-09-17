import { Component, Inject, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { SubscriptionService } from '../../services/subscription.service';
import { BillingRecordDto } from '../../models/subscription.models';

export interface BillingHistoryDialogData {
  subscriptionId: string;
  userName: string;
  planName: string;
}

@Component({
  selector: 'app-billing-history-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatCardModule,
    MatChipsModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>receipt</mat-icon>
      Billing History
    </h2>
    
    <div mat-dialog-content>
      <div class="subscription-info">
        <mat-card>
          <mat-card-content>
            <p><strong>User:</strong> {{ data.userName }}</p>
            <p><strong>Plan:</strong> {{ data.planName }}</p>
          </mat-card-content>
        </mat-card>
      </div>

      <div *ngIf="loading" class="loading-container">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Loading billing history...</p>
      </div>

      <div *ngIf="!loading && billingRecords.length === 0" class="empty-state">
        <mat-icon>receipt_long</mat-icon>
        <h3>No Billing Records Found</h3>
        <p>No billing history is available for this subscription.</p>
      </div>

      <div *ngIf="!loading && billingRecords.length > 0" class="billing-history">
        <table mat-table [dataSource]="billingRecords" class="billing-table">
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
              {{ record.paidDate ? (record.paidDate | date:'short') : 'Not Paid' }}
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let record">
              <mat-chip [color]="getStatusColor(record.status)">
                {{ record.status }}
              </mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="paymentMethod">
            <th mat-header-cell *matHeaderCellDef>Payment Method</th>
            <td mat-cell *matCellDef="let record">{{ record.paymentMethodId || 'N/A' }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="onClose()">Close</button>
      <button mat-raised-button color="primary" (click)="onExport()" [disabled]="billingRecords.length === 0">
        <mat-icon>download</mat-icon>
        Export
      </button>
    </div>
  `,
  styles: [`
    .subscription-info {
      margin-bottom: 16px;
    }

    .subscription-info mat-card {
      background-color: #f5f5f5;
    }

    .subscription-info p {
      margin: 0 0 8px 0;
    }

    .loading-container {
      text-align: center;
      padding: 40px;
    }

    .loading-container mat-spinner {
      margin: 0 auto 16px;
    }

    .empty-state {
      text-align: center;
      padding: 40px;
      color: #666;
    }

    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      color: #ccc;
    }

    .empty-state h3 {
      margin: 0 0 8px 0;
      color: #999;
    }

    .billing-history {
      max-height: 500px;
      overflow-y: auto;
    }

    .billing-table {
      width: 100%;
    }

    .billing-table th {
      font-weight: 600;
    }

    mat-dialog-content {
      min-width: 800px;
      max-width: 1000px;
      min-height: 400px;
    }

    mat-dialog-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    mat-dialog-title mat-icon {
      color: #2196f3;
    }

    @media (max-width: 768px) {
      mat-dialog-content {
        min-width: 90vw;
      }

      .billing-table {
        font-size: 12px;
      }
    }
  `]
})
export class BillingHistoryDialogComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);

  billingRecords: BillingRecordDto[] = [];
  loading = true;
  displayedColumns = ['billingDate', 'amount', 'dueDate', 'paidDate', 'status', 'paymentMethod'];

  constructor(
    public dialogRef: MatDialogRef<BillingHistoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: BillingHistoryDialogData
  ) {}

  ngOnInit() {
    this.loadBillingHistory();
  }

  loadBillingHistory() {
    this.loading = true;
    this.subscriptionService.getBillingHistory(this.data.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.billingRecords = Array.isArray(response.data) ? response.data : [];
        } else {
          this.billingRecords = [];
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading billing history:', error);
        this.snackBar.open('Error loading billing history: ' + error.message, 'Close', { duration: 5000 });
        this.billingRecords = [];
        this.loading = false;
      }
    });
  }

  getStatusColor(status: string): 'primary' | 'accent' | 'warn' | undefined {
    switch (status?.toLowerCase()) {
      case 'paid': return 'primary';
      case 'pending': return 'accent';
      case 'failed':
      case 'overdue': return 'warn';
      default: return undefined;
    }
  }

  onClose() {
    this.dialogRef.close();
  }

  onExport() {
    // TODO: Implement export functionality
    this.snackBar.open('Export functionality will be implemented soon', 'Close', { duration: 3000 });
  }
}