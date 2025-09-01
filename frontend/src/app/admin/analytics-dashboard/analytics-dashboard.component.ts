import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { AnalyticsDashboardService } from './analytics-dashboard.service';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule
  ],
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss']
})
export class AnalyticsDashboardComponent implements OnInit {
  loading = false;
  error: string | null = null;
  summary: any = null;
  revenue: any = null;
  churn: any = null;
  planPerformance: any = null;

  constructor(private analyticsService: AnalyticsDashboardService) {}

  ngOnInit() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.loading = true;
    this.error = null;
    this.analyticsService.getSummary().subscribe({
      next: (res: any) => {
        this.summary = res?.data;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err?.error?.Message || 'Failed to load summary';
        this.loading = false;
      }
    });
    this.analyticsService.getRevenue().subscribe({
      next: (res: any) => this.revenue = res?.data,
      error: () => {}
    });
    this.analyticsService.getChurn().subscribe({
      next: (res: any) => this.churn = res?.data,
      error: () => {}
    });
    this.analyticsService.getPlanPerformance().subscribe({
      next: (res: any) => this.planPerformance = res?.data,
      error: () => {}
    });
  }

  exportReport(format: string) {
    this.analyticsService.exportReport(format).subscribe();
  }
}
