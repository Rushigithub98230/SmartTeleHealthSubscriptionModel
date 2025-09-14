import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { AnalyticsService, DashboardSummary, RevenueMetrics, ChurnAnalysis, PlanPerformance } from '../../services/analytics.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Admin Dashboard</h1>
        <div class="header-actions">
          <form [formGroup]="filterForm" class="filter-form">
            <mat-form-field appearance="outline">
              <mat-label>Date Range</mat-label>
              <mat-select formControlName="period" (selectionChange)="onPeriodChange()">
                <mat-option value="7days">Last 7 Days</mat-option>
                <mat-option value="30days">Last 30 Days</mat-option>
                <mat-option value="3months">Last 3 Months</mat-option>
                <mat-option value="year">This Year</mat-option>
                <mat-option value="custom">Custom Range</mat-option>
              </mat-select>
            </mat-form-field>
            
            <mat-form-field *ngIf="showCustomDateRange" appearance="outline">
              <mat-label>Start Date</mat-label>
              <input matInput [matDatepicker]="startPicker" formControlName="startDate">
              <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
              <mat-datepicker #startPicker></mat-datepicker>
            </mat-form-field>
            
            <mat-form-field *ngIf="showCustomDateRange" appearance="outline">
              <mat-label>End Date</mat-label>
              <input matInput [matDatepicker]="endPicker" formControlName="endDate">
              <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
              <mat-datepicker #endPicker></mat-datepicker>
            </mat-form-field>
            
            <button mat-raised-button color="primary" (click)="refreshData()">
              <mat-icon>refresh</mat-icon>
              Refresh
            </button>
          </form>
        </div>
      </div>

      <div class="dashboard-content" *ngIf="!loading; else loadingTemplate">
        <!-- Key Metrics Cards -->
        <div class="metrics-grid">
          <mat-card class="metric-card">
            <mat-card-content>
              <div class="metric-content">
                <div class="metric-value">{{ dashboardData?.totalSubscriptions || 0 | number }}</div>
                <div class="metric-label">Total Subscriptions</div>
                <mat-icon class="metric-icon">subscriptions</mat-icon>
              </div>
              <div class="metric-change positive">
                <mat-icon>trending_up</mat-icon>
                +12% from last month
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="metric-card">
            <mat-card-content>
              <div class="metric-content">
                <div class="metric-value">{{ dashboardData?.activeSubscriptions || 0 | number }}</div>
                <div class="metric-label">Active Subscriptions</div>
                <mat-icon class="metric-icon">check_circle</mat-icon>
              </div>
              <div class="metric-change positive">
                <mat-icon>trending_up</mat-icon>
                +8% from last month
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="metric-card">
            <mat-card-content>
              <div class="metric-content">
                <div class="metric-value">{{ formatCurrency(revenueData?.totalRevenue || 0) }}</div>
                <div class="metric-label">Total Revenue</div>
                <mat-icon class="metric-icon">attach_money</mat-icon>
              </div>
              <div class="metric-change positive">
                <mat-icon>trending_up</mat-icon>
                +{{ revenueData?.revenueGrowth || 0 }}% from last month
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="metric-card">
            <mat-card-content>
              <div class="metric-content">
                <div class="metric-value">{{ churnData?.churnRate || 0 | number:'1.1-1' }}%</div>
                <div class="metric-label">Churn Rate</div>
                <mat-icon class="metric-icon">trending_down</mat-icon>
              </div>
              <div class="metric-change negative">
                <mat-icon>trending_down</mat-icon>
                -2.1% from last month
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- Charts Section -->
        <div class="charts-grid">
          <mat-card class="chart-card">
            <mat-card-header>
              <mat-card-title>Revenue Trends</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="chart-placeholder">
                <p>Revenue Chart (Chart.js integration pending)</p>
                <div class="mock-chart revenue-chart"></div>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="chart-card">
            <mat-card-header>
              <mat-card-title>Subscription Status Distribution</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="chart-placeholder">
                <p>Status Distribution Chart (Chart.js integration pending)</p>
                <div class="mock-chart status-chart"></div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- Plan Performance Table -->
        <mat-card class="table-card">
          <mat-card-header>
            <mat-card-title>Plan Performance</mat-card-title>
            <div class="card-actions">
              <button mat-button (click)="exportData('plans', 'csv')">
                <mat-icon>download</mat-icon>
                Export CSV
              </button>
            </div>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="planPerformanceData" class="performance-table">
              <ng-container matColumnDef="planName">
                <th mat-header-cell *matHeaderCellDef>Plan Name</th>
                <td mat-cell *matCellDef="let plan">{{ plan.planName }}</td>
              </ng-container>

              <ng-container matColumnDef="totalSubscriptions">
                <th mat-header-cell *matHeaderCellDef>Total Subscriptions</th>
                <td mat-cell *matCellDef="let plan">{{ plan.totalSubscriptions | number }}</td>
              </ng-container>

              <ng-container matColumnDef="activeSubscriptions">
                <th mat-header-cell *matHeaderCellDef>Active Subscriptions</th>
                <td mat-cell *matCellDef="let plan">{{ plan.activeSubscriptions | number }}</td>
              </ng-container>

              <ng-container matColumnDef="revenue">
                <th mat-header-cell *matHeaderCellDef>Revenue</th>
                <td mat-cell *matCellDef="let plan">{{ formatCurrency(plan.revenue) }}</td>
              </ng-container>

              <ng-container matColumnDef="churnRate">
                <th mat-header-cell *matHeaderCellDef>Churn Rate</th>
                <td mat-cell *matCellDef="let plan">
                  <span [class]="getChurnRateClass(plan.churnRate)">
                    {{ plan.churnRate | number:'1.1-1' }}%
                  </span>
                </td>
              </ng-container>

              <ng-container matColumnDef="avgDuration">
                <th mat-header-cell *matHeaderCellDef>Avg Duration (Days)</th>
                <td mat-cell *matCellDef="let plan">{{ plan.avgDuration | number:'1.0-0' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="planColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: planColumns;"></tr>
            </table>
          </mat-card-content>
        </mat-card>

        <!-- Recent Activity -->
        <mat-card class="activity-card">
          <mat-card-header>
            <mat-card-title>Recent Activity</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="activity-list">
              <div class="activity-item" *ngFor="let activity of dashboardData?.recentActivity">
                <div class="activity-icon">
                  <mat-icon [color]="getActivityColor(activity.type)">{{ getActivityIcon(activity.type) }}</mat-icon>
                </div>
                <div class="activity-content">
                  <div class="activity-title">{{ activity.title }}</div>
                  <div class="activity-description">{{ activity.description }}</div>
                  <div class="activity-time">{{ formatRelativeTime(activity.timestamp) }}</div>
                </div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <ng-template #loadingTemplate>
        <div class="loading-container">
          <mat-spinner></mat-spinner>
          <p>Loading dashboard data...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 0;
      background: #f5f5f5;
      min-height: 100%;
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      background: white;
      padding: 20px 24px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .dashboard-header h1 {
      margin: 0;
      color: #333;
      font-size: 28px;
      font-weight: 600;
    }

    .filter-form {
      display: flex;
      gap: 16px;
      align-items: center;
    }

    .filter-form mat-form-field {
      min-width: 140px;
    }

    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 20px;
      margin-bottom: 24px;
    }

    .metric-card {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }

    .metric-content {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 12px;
    }

    .metric-value {
      font-size: 32px;
      font-weight: 700;
      line-height: 1;
    }

    .metric-label {
      font-size: 14px;
      opacity: 0.9;
      margin-top: 4px;
    }

    .metric-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
      opacity: 0.7;
    }

    .metric-change {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      font-weight: 500;
    }

    .metric-change.positive {
      color: #4caf50;
    }

    .metric-change.negative {
      color: #f44336;
    }

    .metric-change mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .charts-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 20px;
      margin-bottom: 24px;
    }

    .chart-card {
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .chart-card canvas {
      max-height: 300px;
    }

    .chart-placeholder {
      text-align: center;
      padding: 40px;
      color: #666;
    }

    .mock-chart {
      height: 200px;
      border: 2px dashed #ddd;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-top: 16px;
      background: linear-gradient(45deg, #f5f5f5 25%, transparent 25%), 
                  linear-gradient(-45deg, #f5f5f5 25%, transparent 25%), 
                  linear-gradient(45deg, transparent 75%, #f5f5f5 75%), 
                  linear-gradient(-45deg, transparent 75%, #f5f5f5 75%);
      background-size: 20px 20px;
      background-position: 0 0, 0 10px, 10px -10px, -10px 0px;
    }

    .table-card {
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      margin-bottom: 24px;
    }

    .card-actions {
      margin-left: auto;
    }

    .performance-table {
      width: 100%;
    }

    .churn-low { color: #4caf50; font-weight: 600; }
    .churn-medium { color: #ff9800; font-weight: 600; }
    .churn-high { color: #f44336; font-weight: 600; }

    .activity-card {
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .activity-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .activity-item {
      display: flex;
      gap: 16px;
      padding: 16px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .activity-item:last-child {
      border-bottom: none;
    }

    .activity-icon {
      flex-shrink: 0;
    }

    .activity-content {
      flex: 1;
    }

    .activity-title {
      font-weight: 600;
      margin-bottom: 4px;
      color: #333;
    }

    .activity-description {
      font-size: 14px;
      color: #666;
      margin-bottom: 4px;
    }

    .activity-time {
      font-size: 12px;
      color: #999;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 400px;
      gap: 16px;
    }

    @media (max-width: 768px) {
      .dashboard-header {
        flex-direction: column;
        gap: 16px;
        align-items: stretch;
      }

      .filter-form {
        flex-wrap: wrap;
      }

      .metrics-grid {
        grid-template-columns: 1fr;
      }

      .charts-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private analyticsService = inject(AnalyticsService);

  dashboardData: DashboardSummary | null = null;
  revenueData: RevenueMetrics | null = null;
  churnData: ChurnAnalysis | null = null;
  planPerformanceData: PlanPerformance[] = [];
  loading = true;
  showCustomDateRange = false;

  filterForm = new FormGroup({
    period: new FormControl('30days'),
    startDate: new FormControl(),
    endDate: new FormControl()
  });

  planColumns = ['planName', 'totalSubscriptions', 'activeSubscriptions', 'revenue', 'churnRate', 'avgDuration'];

  ngOnInit() {
    this.loadDashboardData();
  }

  onPeriodChange() {
    const period = this.filterForm.get('period')?.value;
    this.showCustomDateRange = period === 'custom';
    if (period !== 'custom') {
      this.refreshData();
    }
  }

  refreshData() {
    this.loadDashboardData();
  }

  private async loadDashboardData() {
    this.loading = true;
    try {
      // Load all dashboard data in parallel
      const [dashboardSummary, revenueMetrics, churnAnalysis, planPerformance] = await Promise.all([
        this.analyticsService.getDashboardSummary().toPromise(),
        this.analyticsService.getRevenueMetrics().toPromise(),
        this.analyticsService.getChurnAnalysis().toPromise(),
        this.analyticsService.getPlanPerformance().toPromise()
      ]);

      this.dashboardData = dashboardSummary || null;
      this.revenueData = revenueMetrics || null;
      this.churnData = churnAnalysis || null;
      this.planPerformanceData = planPerformance || [];

      // Create charts after data is loaded
      setTimeout(() => {
        // Chart creation will be implemented when Chart.js is properly configured
        console.log('Charts would be created here');
      }, 100);

    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      this.loading = false;
    }
  }

  exportData(type: string, format: string) {
    this.analyticsService.exportAnalytics(type, format).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${type}-data.${format}`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Export failed:', error);
      }
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getChurnRateClass(rate: number): string {
    if (rate < 5) return 'churn-low';
    if (rate < 10) return 'churn-medium';
    return 'churn-high';
  }

  getActivityIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'subscription': 'subscriptions',
      'payment': 'payment',
      'cancellation': 'cancel',
      'upgrade': 'upgrade',
      'downgrade': 'trending_down'
    };
    return icons[type] || 'info';
  }

  getActivityColor(type: string): string {
    const colors: { [key: string]: string } = {
      'subscription': 'primary',
      'payment': 'primary',
      'cancellation': 'warn',
      'upgrade': 'accent',
      'downgrade': 'warn'
    };
    return colors[type] || 'primary';
  }

  formatRelativeTime(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 60) return `${diffMins} minutes ago`;
    if (diffHours < 24) return `${diffHours} hours ago`;
    return `${diffDays} days ago`;
  }
}