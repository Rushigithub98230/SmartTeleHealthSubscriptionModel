import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AnalyticsService, DashboardMetrics, RealTimeMetrics, PlanMigrationAnalytics } from '../../../../core/services/analytics.service';

@Component({
  selector: 'app-subscription-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription-dashboard.component.html',
  styleUrls: ['./subscription-dashboard.component.scss']
})
export class SubscriptionDashboardComponent implements OnInit, OnDestroy {
  dashboardMetrics: DashboardMetrics | null = null;
  realTimeMetrics: RealTimeMetrics | null = null;
  migrationAnalytics: PlanMigrationAnalytics | null = null;
  loading = true;
  loadingMigration = false;
  error: string | null = null;
  migrationError: string | null = null;

  private refreshSubscription?: Subscription;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.loadMigrationAnalytics();
    this.startRealTimeUpdates();
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    this.analyticsService.getDashboardMetrics('overview').subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.dashboardMetrics = response.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load dashboard data';
        this.loading = false;
        console.error('Dashboard error:', err);
      }
    });
  }

  /**
   * Load plan migration analytics
   */
  loadMigrationAnalytics(): void {
    this.loadingMigration = true;
    this.migrationError = null;

    this.analyticsService.getPlanMigrationAnalytics().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.migrationAnalytics = response.data;
        } else {
          this.migrationError = response.message || 'Failed to load migration analytics';
        }
        this.loadingMigration = false;
      },
      error: (err) => {
        this.migrationError = 'Failed to load migration analytics';
        this.loadingMigration = false;
        console.error('Migration analytics error:', err);
      }
    });
  }

  startRealTimeUpdates(): void {
    // Update real-time metrics every 60 seconds
    this.refreshSubscription = interval(60000)
      .pipe(switchMap(() => this.analyticsService.getRealTimeMetrics()))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.realTimeMetrics = response.data;
          }
        },
        error: (err) => {
          console.error('Real-time metrics error:', err);
        }
      });

    // Initial load
    this.analyticsService.getRealTimeMetrics().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.realTimeMetrics = response.data;
        }
      },
      error: (err) => {
        console.error('Real-time metrics error:', err);
      }
    });
  }

  refresh(): void {
    this.loadDashboardData();
    this.loadMigrationAnalytics();
    this.analyticsService.getRealTimeMetrics().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.realTimeMetrics = response.data;
        }
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case 'purchase':
        return 'bi-cart-check';
      case 'trial':
        return 'bi-clock-history';
      case 'cancel':
        return 'bi-x-circle';
      default:
        return 'bi-activity';
    }
  }

  getActivityColor(type: string): string {
    switch (type) {
      case 'purchase':
        return 'text-success';
      case 'trial':
        return 'text-info';
      case 'cancel':
        return 'text-danger';
      default:
        return 'text-secondary';
    }
  }

  /**
   * Get migration status badge class
   */
  getMigrationStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending':
        return 'bg-warning';
      case 'completed':
        return 'bg-success';
      case 'useroptedout':
        return 'bg-danger';
      case 'failed':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  /**
   * Get user decision badge class
   */
  getUserDecisionClass(decision: string | null): string {
    switch (decision?.toLowerCase()) {
      case 'accept':
        return 'bg-success';
      case 'cancel':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }
}

