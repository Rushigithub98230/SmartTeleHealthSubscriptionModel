import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AnalyticsService, BillingService } from '../../../core/services';

/**
 * Admin Reports Dashboard Component
 * Advanced reporting and data export
 * 
 * APIs Used:
 * - GET /api/Analytics/revenue
 * - GET /api/Billing/statistics
 * - POST /api/Billing/export
 * 
 * Route: /webadmin/reports
 * Access: Admin only
 */
@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  loading = false;
  
  // Date filters
  startDate: string = '';
  endDate: string = '';
  reportType: string = 'revenue';

  // Report types
  reportTypes = [
    { id: 'revenue', name: 'Revenue Report' },
    { id: 'subscriptions', name: 'Subscription Report' },
    { id: 'billing', name: 'Billing Report' },
    { id: 'usage', name: 'Usage Report' }
  ];

  constructor(
    private analyticsService: AnalyticsService,
    private billingService: BillingService
  ) {}

  ngOnInit(): void {
    this.setDefaultDates();
  }

  /**
   * Set default date range (last 30 days)
   */
  setDefaultDates(): void {
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);

    this.endDate = today.toISOString().split('T')[0];
    this.startDate = thirtyDaysAgo.toISOString().split('T')[0];
  }

  /**
   * Generate report
   */
  generateReport(): void {
    this.loading = true;

    const startDateObj = this.startDate ? new Date(this.startDate) : undefined;
    const endDateObj = this.endDate ? new Date(this.endDate) : undefined;

    switch (this.reportType) {
      case 'revenue':
        this.analyticsService.getRevenueAnalytics(startDateObj, endDateObj).subscribe({
          next: (response) => {
            console.log('Revenue report:', response.data);
            this.loading = false;
          },
          error: () => this.loading = false
        });
        break;

      case 'billing':
        this.billingService.getBillingStatistics(startDateObj, endDateObj).subscribe({
          next: (response) => {
            console.log('Billing report:', response.data);
            this.loading = false;
          },
          error: () => this.loading = false
        });
        break;

      default:
        this.loading = false;
    }
  }

  /**
   * Export report
   */
  exportReport(format: string): void {
    console.log(`Exporting ${this.reportType} report as ${format}`);
    
    if (this.reportType === 'billing') {
      this.billingService.exportBillingRecords(format).subscribe({
        next: (data) => {
          console.log('Export successful');
          // Handle file download
        },
        error: (error) => console.error('Export failed:', error)
      });
    }
  }
}


