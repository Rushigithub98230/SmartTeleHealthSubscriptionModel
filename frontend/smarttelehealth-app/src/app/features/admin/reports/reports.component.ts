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
    { id: 'tax', name: 'Tax Collection Report' },
    { id: 'reconciliation', name: 'Billing Reconciliation' },
    { id: 'usage', name: 'Usage Report' }
  ];

  // Report data
  reportData: any = null;
  error: string | null = null;

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
    this.error = null;
    this.reportData = null;

    const startDateObj = this.startDate ? new Date(this.startDate) : undefined;
    const endDateObj = this.endDate ? new Date(this.endDate) : undefined;

    switch (this.reportType) {
      case 'revenue':
        this.analyticsService.getRevenueAnalytics(startDateObj, endDateObj).subscribe({
          next: (response) => {
            this.reportData = response.data;
            this.loading = false;
          },
          error: (error) => {
            this.error = 'Failed to load revenue report';
            this.loading = false;
          }
        });
        break;

      case 'billing':
        this.billingService.getBillingStatistics(startDateObj, endDateObj).subscribe({
          next: (response) => {
            this.reportData = response.data;
            this.loading = false;
          },
          error: (error) => {
            this.error = 'Failed to load billing report';
            this.loading = false;
          }
        });
        break;

      case 'tax':
        this.generateTaxReport(startDateObj, endDateObj);
        break;

      case 'reconciliation':
        this.generateReconciliationReport(startDateObj, endDateObj);
        break;

      default:
        this.loading = false;
    }
  }

  /**
   * Generate tax collection report
   */
  private generateTaxReport(startDate?: Date, endDate?: Date): void {
    // This would call a new API endpoint for tax collection statistics
    // For now, we'll simulate the data structure
    this.reportData = {
      totalTaxCollected: 0,
      taxByPlan: [],
      taxByMonth: [],
      summary: {
        totalPlansWithTax: 0,
        averageTaxRate: 0,
        totalTaxableAmount: 0
      }
    };
    this.loading = false;
  }

  /**
   * Generate billing reconciliation report
   */
  private generateReconciliationReport(startDate?: Date, endDate?: Date): void {
    // This would call a new API endpoint for billing reconciliation
    // For now, we'll simulate the data structure
    this.reportData = {
      subscriptionsWithoutBilling: [],
      billingWithoutSubscriptions: [],
      mismatchedAmounts: [],
      summary: {
        totalSubscriptions: 0,
        totalBillingRecords: 0,
        discrepancies: 0
      }
    };
    this.loading = false;
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


