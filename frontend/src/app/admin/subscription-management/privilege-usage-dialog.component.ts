import { Component, Inject, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { SubscriptionService } from '../../services/subscription.service';
import { UserSubscriptionPrivilegeUsageDto } from '../../models/subscription.models';

export interface PrivilegeUsageDialogData {
  subscriptionId: string;
  userName: string;
  planName: string;
}

@Component({
  selector: 'app-privilege-usage-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTabsModule,
    MatCardModule,
    MatChipsModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>security</mat-icon>
      Privilege Usage
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
        <p>Loading privilege usage...</p>
      </div>

      <div *ngIf="!loading && privilegeUsage.length === 0" class="empty-state">
        <mat-icon>security</mat-icon>
        <h3>No Privilege Usage Found</h3>
        <p>No privilege usage data is available for this subscription.</p>
      </div>

      <div *ngIf="!loading && privilegeUsage.length > 0" class="privilege-usage">
        <div class="usage-cards">
          <mat-card *ngFor="let usage of privilegeUsage" class="usage-card">
            <mat-card-header>
              <mat-card-title>{{ usage.privilegeName }}</mat-card-title>
              <mat-card-subtitle>
                <mat-chip [color]="getUsageColor(usage)">
                  {{ getUsageStatus(usage) }}
                </mat-chip>
              </mat-card-subtitle>
            </mat-card-header>
            
            <mat-card-content>
              <div class="usage-stats">
                <div class="stat-item">
                  <span class="label">Used:</span>
                  <span class="value">{{ usage.usedCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">Allowed:</span>
                  <span class="value">{{ usage.allowedCount === -1 ? 'Unlimited' : usage.allowedCount }}</span>
                </div>
                <div class="stat-item" *ngIf="usage.lastUsedDate">
                  <span class="label">Last Used:</span>
                  <span class="value">{{ usage.lastUsedDate | date:'short' }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">Reset Date:</span>
                  <span class="value">{{ usage.resetDate | date:'short' }}</span>
                </div>
              </div>

              <div class="usage-progress" *ngIf="usage.allowedCount !== -1">
                <div class="progress-info">
                  <span>Usage Progress</span>
                  <span>{{ getUsagePercentage(usage) }}%</span>
                </div>
                <mat-progress-bar 
                  [value]="getUsagePercentage(usage)" 
                  [color]="getProgressColor(usage)">
                </mat-progress-bar>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <div class="usage-table">
          <h3>Detailed Usage Table</h3>
          <table mat-table [dataSource]="privilegeUsage" class="privilege-table">
            <ng-container matColumnDef="privilegeName">
              <th mat-header-cell *matHeaderCellDef>Privilege</th>
              <td mat-cell *matCellDef="let usage">{{ usage.privilegeName }}</td>
            </ng-container>

            <ng-container matColumnDef="usedCount">
              <th mat-header-cell *matHeaderCellDef>Used</th>
              <td mat-cell *matCellDef="let usage">{{ usage.usedCount }}</td>
            </ng-container>

            <ng-container matColumnDef="allowedCount">
              <th mat-header-cell *matHeaderCellDef>Allowed</th>
              <td mat-cell *matCellDef="let usage">{{ usage.allowedCount === -1 ? 'Unlimited' : usage.allowedCount }}</td>
            </ng-container>

            <ng-container matColumnDef="percentage">
              <th mat-header-cell *matHeaderCellDef>Usage %</th>
              <td mat-cell *matCellDef="let usage">
                {{ usage.allowedCount === -1 ? 'N/A' : getUsagePercentage(usage) + '%' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="lastUsedDate">
              <th mat-header-cell *matHeaderCellDef>Last Used</th>
              <td mat-cell *matCellDef="let usage">
                {{ usage.lastUsedDate ? (usage.lastUsedDate | date:'short') : 'Never' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="resetDate">
              <th mat-header-cell *matHeaderCellDef>Reset Date</th>
              <td mat-cell *matCellDef="let usage">{{ usage.resetDate | date:'short' }}</td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </div>
      </div>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="onClose()">Close</button>
      <button mat-raised-button color="primary" (click)="onRefresh()">
        <mat-icon>refresh</mat-icon>
        Refresh
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

    .privilege-usage {
      max-height: 600px;
      overflow-y: auto;
    }

    .usage-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .usage-card {
      transition: transform 0.2s ease;
    }

    .usage-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    }

    .usage-stats {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
      margin-bottom: 16px;
    }

    .stat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .stat-item .label {
      color: #666;
      font-size: 14px;
    }

    .stat-item .value {
      font-weight: 600;
      color: #333;
    }

    .usage-progress {
      margin-top: 16px;
    }

    .progress-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
      font-size: 14px;
    }

    .usage-table {
      margin-top: 24px;
    }

    .usage-table h3 {
      margin: 0 0 16px 0;
      color: #333;
    }

    .privilege-table {
      width: 100%;
    }

    .privilege-table th {
      font-weight: 600;
    }

    mat-dialog-content {
      min-width: 900px;
      max-width: 1200px;
      min-height: 500px;
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

      .usage-cards {
        grid-template-columns: 1fr;
      }

      .privilege-table {
        font-size: 12px;
      }
    }
  `]
})
export class PrivilegeUsageDialogComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);

  privilegeUsage: UserSubscriptionPrivilegeUsageDto[] = [];
  loading = true;
  displayedColumns = ['privilegeName', 'usedCount', 'allowedCount', 'percentage', 'lastUsedDate', 'resetDate'];

  constructor(
    public dialogRef: MatDialogRef<PrivilegeUsageDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PrivilegeUsageDialogData
  ) {}

  ngOnInit() {
    this.loadPrivilegeUsage();
  }

  loadPrivilegeUsage() {
    this.loading = true;
    this.subscriptionService.getPrivilegeUsage(this.data.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.privilegeUsage = Array.isArray(response.data) ? response.data : [];
        } else {
          this.privilegeUsage = [];
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading privilege usage:', error);
        this.snackBar.open('Error loading privilege usage: ' + error.message, 'Close', { duration: 5000 });
        this.privilegeUsage = [];
        this.loading = false;
      }
    });
  }

  getUsagePercentage(usage: UserSubscriptionPrivilegeUsageDto): number {
    if (usage.allowedCount === -1) return 0;
    if (usage.allowedCount === 0) return 100;
    return Math.round((usage.usedCount / usage.allowedCount) * 100);
  }

  getUsageStatus(usage: UserSubscriptionPrivilegeUsageDto): string {
    if (usage.allowedCount === -1) return 'Unlimited';
    
    const percentage = this.getUsagePercentage(usage);
    if (percentage >= 100) return 'Exhausted';
    if (percentage >= 80) return 'High Usage';
    if (percentage >= 50) return 'Medium Usage';
    return 'Low Usage';
  }

  getUsageColor(usage: UserSubscriptionPrivilegeUsageDto): 'primary' | 'accent' | 'warn' | undefined {
    if (usage.allowedCount === -1) return 'primary';
    
    const percentage = this.getUsagePercentage(usage);
    if (percentage >= 100) return 'warn';
    if (percentage >= 80) return 'accent';
    return 'primary';
  }

  getProgressColor(usage: UserSubscriptionPrivilegeUsageDto): 'primary' | 'accent' | 'warn' {
    const percentage = this.getUsagePercentage(usage);
    if (percentage >= 100) return 'warn';
    if (percentage >= 80) return 'accent';
    return 'primary';
  }

  onClose() {
    this.dialogRef.close();
  }

  onRefresh() {
    this.loadPrivilegeUsage();
  }
}