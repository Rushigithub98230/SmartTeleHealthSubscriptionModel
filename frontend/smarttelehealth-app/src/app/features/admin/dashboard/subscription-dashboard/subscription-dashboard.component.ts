import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AnalyticsService, DashboardMetrics, RealTimeMetrics } from '../../../../core/services/analytics.service';

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
  loading = true;
  error: string | null = null;

  private refreshSubscription?: Subscription;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.loadDashboardData();
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
}

