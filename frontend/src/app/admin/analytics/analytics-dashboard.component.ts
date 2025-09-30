import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { AnalyticsService } from '../../services/analytics.service';

export interface DashboardSummary {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  churnRate: number;
  averageRevenuePerUser: number;
  newSubscriptionsThisMonth: number;
  cancelledSubscriptionsThisMonth: number;
}

export interface RevenueMetrics {
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  annualRecurringRevenue: number;
  averageRevenuePerUser: number;
  revenueGrowth: number;
  revenueByPlan: Array<{
    planName: string;
    revenue: number;
    percentage: number;
  }>;
  monthlyRevenue: Array<{
    month: string;
    revenue: number;
  }>;
}

export interface ChurnAnalysis {
  totalChurnedSubscriptions: number;
  churnRate: number;
  churnByPlan: Array<{
    planName: string;
    churnCount: number;
    churnRate: number;
  }>;
  churnByReason: Array<{
    reason: string;
    count: number;
    percentage: number;
  }>;
  churnTrend: Array<{
    month: string;
    churnRate: number;
  }>;
}

export interface PlanPerformance {
  planName: string;
  totalSubscriptions: number;
  activeSubscriptions: number;
  revenue: number;
  churnRate: number;
  avgDuration: number;
}

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatChipsModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="analytics-dashboard">
      <div class="dashboard-header">
        <h1>
          <mat-icon>analytics</mat-icon>
          Analytics Dashboard
        </h1>
        <div class="header-actions">
          <mat-form-field appearance="outline" class="period-selector">
            <mat-label>Time Period</mat-label>
            <mat-select [formControl]="periodControl" (selectionChange)="onPeriodChange()">
              <mat-option value="7days">Last 7 Days</mat-option>
              <mat-option value="30days">Last 30 Days</mat-option>
              <mat-option value="90days">Last 90 Days</mat-option>
              <mat-option value="1year">Last Year</mat-option>
            </mat-select>
          </mat-form-field>
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            Refresh
          </button>
        </div>
      </div>

      <div class="dashboard-content">
        <!-- Summary Cards -->
        <div class="summary-cards" *ngIf="!loading">
          <mat-card class="summary-card">
            <mat-card-content>
              <div class="card-header">
                <mat-icon>subscriptions</mat-icon>
                <h3>Total Subscriptions</h3>
              </div>
              <div class="card-value">{{ dashboardData?.totalSubscriptions || 0 }}</div>
              <div class="card-subtitle">All time</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="summary-card">
            <mat-card-content>
              <div class="card-header">
                <mat-icon>check_circle</mat-icon>
                <h3>Active Subscriptions</h3>
              </div>
              <div class="card-value">{{ dashboardData?.activeSubscriptions || 0 }}</div>
              <div class="card-subtitle">Currently active</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="summary-card">
            <mat-card-content>
              <div class="card-header">
                <mat-icon>attach_money</mat-icon>
                <h3>Total Revenue</h3>
              </div>
              <div class="card-value">{{ dashboardData?.totalRevenue | currency:'USD' }}</div>
              <div class="card-subtitle">All time</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="summary-card">
            <mat-card-content>
              <div class="card-header">
                <mat-icon>trending_up</mat-icon>
                <h3>Monthly Recurring Revenue</h3>
              </div>
              <div class="card-value">{{ dashboardData?.monthlyRecurringRevenue | currency:'USD' }}</div>
              <div class="card-subtitle">Current MRR</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="summary-card">
            <mat-card-content>
              <div class="card-header">
                <mat-icon>trending_down</mat-icon>
                <h3>Churn Rate</h3>
              </div>
              <div class="card-value">{{ dashboardData?.churnRate | percent:'1.1-1' }}</div>
              <div class="card-subtitle">Monthly churn</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="summary-card">
            <mat-card-content>
              <div class="card-header">
                <mat-icon>person</mat-icon>
                <h3>ARPU</h3>
              </div>
              <div class="card-value">{{ dashboardData?.averageRevenuePerUser | currency:'USD' }}</div>
              <div class="card-subtitle">Average per user</div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- Loading State -->
        <div *ngIf="loading" class="loading-container">
          <mat-spinner diameter="50"></mat-spinner>
          <p>Loading analytics data...</p>
        </div>

        <!-- Detailed Analytics Tabs -->
        <mat-tab-group *ngIf="!loading" class="analytics-tabs">
          <!-- Revenue Analytics Tab -->
          <mat-tab label="Revenue Analytics">
            <div class="tab-content">
              <div class="metrics-grid">
                <mat-card class="metric-card">
                  <mat-card-header>
                    <mat-card-title>Revenue Overview</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="metric-row">
                      <span class="metric-label">Total Revenue:</span>
                      <span class="metric-value">{{ revenueData?.totalRevenue | currency:'USD' }}</span>
                    </div>
                    <div class="metric-row">
                      <span class="metric-label">Monthly Recurring Revenue:</span>
                      <span class="metric-value">{{ revenueData?.monthlyRecurringRevenue | currency:'USD' }}</span>
                    </div>
                    <div class="metric-row">
                      <span class="metric-label">Annual Recurring Revenue:</span>
                      <span class="metric-value">{{ revenueData?.annualRecurringRevenue | currency:'USD' }}</span>
                    </div>
                    <div class="metric-row">
                      <span class="metric-label">Revenue Growth:</span>
                      <span class="metric-value" [class.positive]="revenueData?.revenueGrowth > 0" [class.negative]="revenueData?.revenueGrowth < 0">
                        {{ revenueData?.revenueGrowth | percent:'1.1-1' }}
                      </span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="metric-card">
                  <mat-card-header>
                    <mat-card-title>Revenue by Plan</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div *ngFor="let plan of revenueData?.revenueByPlan" class="plan-revenue">
                      <div class="plan-info">
                        <span class="plan-name">{{ plan.planName }}</span>
                        <span class="plan-percentage">{{ plan.percentage | percent:'1.1-1' }}</span>
                      </div>
                      <div class="plan-amount">{{ plan.revenue | currency:'USD' }}</div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- Churn Analytics Tab -->
          <mat-tab label="Churn Analytics">
            <div class="tab-content">
              <div class="metrics-grid">
                <mat-card class="metric-card">
                  <mat-card-header>
                    <mat-card-title>Churn Overview</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="metric-row">
                      <span class="metric-label">Total Churned:</span>
                      <span class="metric-value">{{ churnData?.totalChurnedSubscriptions || 0 }}</span>
                    </div>
                    <div class="metric-row">
                      <span class="metric-label">Churn Rate:</span>
                      <span class="metric-value">{{ churnData?.churnRate | percent:'1.1-1' }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="metric-card">
                  <mat-card-header>
                    <mat-card-title>Churn by Plan</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div *ngFor="let plan of churnData?.churnByPlan" class="plan-churn">
                      <div class="plan-info">
                        <span class="plan-name">{{ plan.planName }}</span>
                        <span class="plan-rate">{{ plan.churnRate | percent:'1.1-1' }}</span>
                      </div>
                      <div class="plan-count">{{ plan.churnCount }} churned</div>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="metric-card">
                  <mat-card-header>
                    <mat-card-title>Churn by Reason</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div *ngFor="let reason of churnData?.churnByReason" class="reason-churn">
                      <div class="reason-info">
                        <span class="reason-name">{{ reason.reason }}</span>
                        <span class="reason-percentage">{{ reason.percentage | percent:'1.1-1' }}</span>
                      </div>
                      <div class="reason-count">{{ reason.count }} users</div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- Plan Performance Tab -->
          <mat-tab label="Plan Performance">
            <div class="tab-content">
              <mat-card class="performance-table-card">
                <mat-card-header>
                  <mat-card-title>Plan Performance Metrics</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <table mat-table [dataSource]="planPerformanceData" class="performance-table">
                    <ng-container matColumnDef="planName">
                      <th mat-header-cell *matHeaderCellDef>Plan Name</th>
                      <td mat-cell *matCellDef="let plan">{{ plan.planName }}</td>
                    </ng-container>

                    <ng-container matColumnDef="totalSubscriptions">
                      <th mat-header-cell *matHeaderCellDef>Total Subscriptions</th>
                      <td mat-cell *matCellDef="let plan">{{ plan.totalSubscriptions }}</td>
                    </ng-container>

                    <ng-container matColumnDef="activeSubscriptions">
                      <th mat-header-cell *matHeaderCellDef>Active</th>
                      <td mat-cell *matCellDef="let plan">
                        <mat-chip [color]="plan.activeSubscriptions > 0 ? 'primary' : 'warn'">
                          {{ plan.activeSubscriptions }}
                        </mat-chip>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="revenue">
                      <th mat-header-cell *matHeaderCellDef>Revenue</th>
                      <td mat-cell *matCellDef="let plan">{{ plan.revenue | currency:'USD' }}</td>
                    </ng-container>

                    <ng-container matColumnDef="churnRate">
                      <th mat-header-cell *matHeaderCellDef>Churn Rate</th>
                      <td mat-cell *matCellDef="let plan">
                        <mat-chip [color]="plan.churnRate < 0.05 ? 'primary' : plan.churnRate < 0.1 ? 'accent' : 'warn'">
                          {{ plan.churnRate | percent:'1.1-1' }}
                        </mat-chip>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="avgDuration">
                      <th mat-header-cell *matHeaderCellDef>Avg Duration</th>
                      <td mat-cell *matCellDef="let plan">{{ plan.avgDuration }} days</td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="performanceColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: performanceColumns;"></tr>
                  </table>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>
        </mat-tab-group>
      </div>
    </div>
  `,
  styles: [`
    .analytics-dashboard {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      flex-wrap: wrap;
      gap: 16px;
    }

    .dashboard-header h1 {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0;
      font-size: 32px;
      font-weight: 600;
      color: #333;
    }

    .dashboard-header h1 mat-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
      color: #1976d2;
    }

    .header-actions {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .period-selector {
      min-width: 150px;
    }

    .summary-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 24px;
      margin-bottom: 32px;
    }

    .summary-card {
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    .summary-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
    }

    .summary-card .mat-card-content {
      padding: 24px;
    }

    .card-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .card-header mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
      color: #1976d2;
    }

    .card-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 500;
      color: #666;
    }

    .card-value {
      font-size: 32px;
      font-weight: 700;
      color: #333;
      margin-bottom: 8px;
    }

    .card-subtitle {
      font-size: 14px;
      color: #999;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 300px;
      gap: 16px;
    }

    .analytics-tabs {
      margin-top: 24px;
    }

    .tab-content {
      padding: 24px 0;
    }

    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 24px;
    }

    .metric-card {
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .metric-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .metric-row:last-child {
      border-bottom: none;
    }

    .metric-label {
      font-weight: 500;
      color: #666;
    }

    .metric-value {
      font-weight: 600;
      color: #333;
    }

    .metric-value.positive {
      color: #4caf50;
    }

    .metric-value.negative {
      color: #f44336;
    }

    .plan-revenue,
    .plan-churn,
    .reason-churn {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 8px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .plan-revenue:last-child,
    .plan-churn:last-child,
    .reason-churn:last-child {
      border-bottom: none;
    }

    .plan-info,
    .reason-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .plan-name,
    .reason-name {
      font-weight: 500;
      color: #333;
    }

    .plan-percentage,
    .plan-rate,
    .reason-percentage {
      font-size: 12px;
      color: #666;
    }

    .plan-amount,
    .plan-count,
    .reason-count {
      font-weight: 600;
      color: #1976d2;
    }

    .performance-table-card {
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .performance-table {
      width: 100%;
    }

    .performance-table th {
      background-color: #f5f5f5;
      font-weight: 600;
      color: #333;
    }

    .performance-table td {
      padding: 16px 12px;
      border-bottom: 1px solid #f0f0f0;
    }

    .performance-table tr:hover {
      background-color: #f9f9f9;
    }

    @media (max-width: 768px) {
      .analytics-dashboard {
        padding: 16px;
      }

      .dashboard-header {
        flex-direction: column;
        align-items: stretch;
        gap: 16px;
      }

      .dashboard-header h1 {
        font-size: 24px;
      }

      .summary-cards {
        grid-template-columns: 1fr;
        gap: 16px;
      }

      .metrics-grid {
        grid-template-columns: 1fr;
        gap: 16px;
      }

      .header-actions {
        flex-direction: column;
        align-items: stretch;
        gap: 12px;
      }

      .period-selector {
        min-width: unset;
      }
    }
  `]
})
export class AnalyticsDashboardComponent implements OnInit {
  private analyticsService = inject(AnalyticsService);
  private snackBar = inject(MatSnackBar);

  // Form controls
  periodControl = new FormControl('30days');

  // Data properties
  dashboardData: DashboardSummary | null = null;
  revenueData: RevenueMetrics | null = null;
  churnData: ChurnAnalysis | null = null;
  planPerformanceData: PlanPerformance[] = [];

  // UI state
  loading = false;

  // Table columns
  performanceColumns = ['planName', 'totalSubscriptions', 'activeSubscriptions', 'revenue', 'churnRate', 'avgDuration'];

  ngOnInit() {
    this.loadAnalyticsData();
  }

  onPeriodChange() {
    this.loadAnalyticsData();
  }

  refreshData() {
    this.loadAnalyticsData();
  }

  private loadAnalyticsData() {
    this.loading = true;

    // Load dashboard summary
    this.analyticsService.getDashboardSummary().subscribe({
      next: (data) => {
        this.dashboardData = data;
      },
      error: (error) => {
        console.error('Error loading dashboard summary:', error);
        this.snackBar.open('Error loading dashboard summary', 'Close', { duration: 3000 });
      }
    });

    // Load revenue metrics
    this.analyticsService.getRevenueMetrics().subscribe({
      next: (data) => {
        this.revenueData = data;
      },
      error: (error) => {
        console.error('Error loading revenue metrics:', error);
        this.snackBar.open('Error loading revenue metrics', 'Close', { duration: 3000 });
      }
    });

    // Load churn analysis
    this.analyticsService.getChurnAnalysis(this.periodControl.value || 'month').subscribe({
      next: (data) => {
        this.churnData = data;
      },
      error: (error) => {
        console.error('Error loading churn analysis:', error);
        this.snackBar.open('Error loading churn analysis', 'Close', { duration: 3000 });
      }
    });

    // Load plan performance
    this.analyticsService.getPlanPerformance().subscribe({
      next: (data) => {
        this.planPerformanceData = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading plan performance:', error);
        this.snackBar.open('Error loading plan performance', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
