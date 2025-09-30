import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AnalyticsService } from '../../services/analytics.service';
import { SubscriptionService } from '../../services/subscription.service';

export interface ExportDialogData {
  exportType: 'subscriptions' | 'analytics' | 'plans';
  selectedSubscriptions?: string[];
}

export interface ExportOptions {
  format: 'csv' | 'excel' | 'pdf';
  includeFields: string[];
  dateRange?: {
    startDate: string;
    endDate: string;
  };
  filters?: {
    status?: string[];
    planId?: string[];
  };
}

@Component({
  selector: 'app-export-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="export-dialog">
      <div mat-dialog-title class="dialog-header">
        <div class="header-info">
          <mat-icon>download</mat-icon>
          <div>
            <h2>Export Data</h2>
            <p>{{ getExportTypeTitle() }}</p>
          </div>
        </div>
        <button mat-icon-button mat-dialog-close>
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div mat-dialog-content class="dialog-content">
        <form [formGroup]="exportForm" (ngSubmit)="onExport()">
          <!-- Export Format -->
          <div class="form-section">
            <h3>Export Format</h3>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>File Format</mat-label>
              <mat-select formControlName="format">
                <mat-option value="csv">CSV (Comma Separated Values)</mat-option>
                <mat-option value="excel">Excel (.xlsx)</mat-option>
                <mat-option value="pdf">PDF Document</mat-option>
              </mat-select>
            </mat-form-field>
          </div>

          <!-- Date Range (for analytics) -->
          <div *ngIf="data.exportType === 'analytics'" class="form-section">
            <h3>Date Range</h3>
            <div class="date-range">
              <mat-form-field appearance="outline" class="half-width">
                <mat-label>Start Date</mat-label>
                <input matInput type="date" formControlName="startDate">
              </mat-form-field>
              <mat-form-field appearance="outline" class="half-width">
                <mat-label>End Date</mat-label>
                <input matInput type="date" formControlName="endDate">
              </mat-form-field>
            </div>
          </div>

          <!-- Include Fields -->
          <div class="form-section">
            <h3>Include Fields</h3>
            <div class="fields-grid">
              <mat-checkbox 
                *ngFor="let field of getAvailableFields()" 
                [formControlName]="'field_' + field.key"
                [checked]="isFieldSelected(field.key)">
                {{ field.label }}
              </mat-checkbox>
            </div>
          </div>

          <!-- Filters (for subscriptions) -->
          <div *ngIf="data.exportType === 'subscriptions'" class="form-section">
            <h3>Filters</h3>
            <div class="filters-grid">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Status Filter</mat-label>
                <mat-select formControlName="statusFilter" multiple>
                  <mat-option value="active">Active</mat-option>
                  <mat-option value="paused">Paused</mat-option>
                  <mat-option value="cancelled">Cancelled</mat-option>
                  <mat-option value="expired">Expired</mat-option>
                </mat-select>
              </mat-form-field>
            </div>
          </div>

          <!-- Export Summary -->
          <div class="form-section summary-section">
            <h3>Export Summary</h3>
            <div class="summary-info">
              <div class="summary-item">
                <span class="label">Export Type:</span>
                <span class="value">{{ getExportTypeTitle() }}</span>
              </div>
              <div class="summary-item">
                <span class="label">File Format:</span>
                <span class="value">{{ exportForm.get('format')?.value?.toUpperCase() }}</span>
              </div>
              <div class="summary-item" *ngIf="data.selectedSubscriptions && data.selectedSubscriptions.length">
                <span class="label">Selected Items:</span>
                <span class="value">{{ data.selectedSubscriptions.length }} subscriptions</span>
              </div>
              <div class="summary-item">
                <span class="label">Fields to Include:</span>
                <span class="value">{{ getSelectedFieldsCount() }} fields</span>
              </div>
            </div>
          </div>
        </form>
      </div>

      <div mat-dialog-actions class="dialog-actions">
        <button mat-button mat-dialog-close [disabled]="isExporting">Cancel</button>
        <button mat-raised-button 
                color="primary" 
                (click)="onExport()" 
                [disabled]="!exportForm.valid || isExporting">
          <mat-spinner *ngIf="isExporting" diameter="20"></mat-spinner>
          <mat-icon *ngIf="!isExporting">download</mat-icon>
          {{ isExporting ? 'Exporting...' : 'Export Data' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .export-dialog {
      width: 90vw;
      max-width: 700px;
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
      padding: 24px;
      max-height: 70vh;
      overflow-y: auto;
    }

    .form-section {
      margin-bottom: 24px;
    }

    .form-section h3 {
      margin: 0 0 16px 0;
      font-size: 18px;
      font-weight: 500;
      color: #333;
    }

    .full-width {
      width: 100%;
    }

    .half-width {
      width: 48%;
    }

    .date-range {
      display: flex;
      gap: 16px;
    }

    .fields-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 12px;
    }

    .filters-grid {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .summary-section {
      background-color: #f5f5f5;
      padding: 16px;
      border-radius: 8px;
      border: 1px solid #e0e0e0;
    }

    .summary-info {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .summary-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 4px 0;
    }

    .summary-item .label {
      font-weight: 500;
      color: #666;
    }

    .summary-item .value {
      font-weight: 600;
      color: #333;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .dialog-actions button {
      min-width: 120px;
    }

    @media (max-width: 768px) {
      .export-dialog {
        width: 95vw;
        height: 90vh;
      }

      .dialog-content {
        padding: 16px;
      }

      .header-info {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }

      .date-range {
        flex-direction: column;
        gap: 12px;
      }

      .half-width {
        width: 100%;
      }

      .fields-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ExportDialogComponent implements OnInit {
  exportForm: FormGroup;
  isExporting = false;

  constructor(
    private fb: FormBuilder,
    private analyticsService: AnalyticsService,
    private subscriptionService: SubscriptionService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ExportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ExportDialogData
  ) {
    this.exportForm = this.fb.group({
      format: ['csv', Validators.required],
      startDate: [''],
      endDate: [''],
      statusFilter: [[]]
    });

    // Add form controls for each field
    this.getAvailableFields().forEach(field => {
      this.exportForm.addControl(`field_${field.key}`, this.fb.control(this.isFieldSelected(field.key)));
    });
  }

  ngOnInit() {
    // Set default date range for analytics
    if (this.data.exportType === 'analytics') {
      const endDate = new Date();
      const startDate = new Date();
      startDate.setMonth(startDate.getMonth() - 1);
      
      this.exportForm.patchValue({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0]
      });
    }
  }

  getExportTypeTitle(): string {
    switch (this.data.exportType) {
      case 'subscriptions': return 'User Subscriptions';
      case 'analytics': return 'Analytics Data';
      case 'plans': return 'Subscription Plans';
      default: return 'Data';
    }
  }

  getAvailableFields() {
    switch (this.data.exportType) {
      case 'subscriptions':
        return [
          { key: 'id', label: 'Subscription ID' },
          { key: 'userName', label: 'User Name' },
          { key: 'userEmail', label: 'User Email' },
          { key: 'planName', label: 'Plan Name' },
          { key: 'status', label: 'Status' },
          { key: 'currentPrice', label: 'Current Price' },
          { key: 'startDate', label: 'Start Date' },
          { key: 'nextBillingDate', label: 'Next Billing Date' },
          { key: 'autoRenew', label: 'Auto Renew' },
          { key: 'createdDate', label: 'Created Date' }
        ];
      case 'analytics':
        return [
          { key: 'totalSubscriptions', label: 'Total Subscriptions' },
          { key: 'activeSubscriptions', label: 'Active Subscriptions' },
          { key: 'totalRevenue', label: 'Total Revenue' },
          { key: 'monthlyRecurringRevenue', label: 'Monthly Recurring Revenue' },
          { key: 'churnRate', label: 'Churn Rate' },
          { key: 'averageRevenuePerUser', label: 'Average Revenue Per User' }
        ];
      case 'plans':
        return [
          { key: 'id', label: 'Plan ID' },
          { key: 'name', label: 'Plan Name' },
          { key: 'description', label: 'Description' },
          { key: 'price', label: 'Price' },
          { key: 'billingCycle', label: 'Billing Cycle' },
          { key: 'isActive', label: 'Is Active' },
          { key: 'isFeatured', label: 'Is Featured' },
          { key: 'createdDate', label: 'Created Date' }
        ];
      default:
        return [];
    }
  }

  isFieldSelected(fieldKey: string): boolean {
    // Default to selecting all fields
    return true;
  }

  getSelectedFieldsCount(): number {
    return this.getAvailableFields().filter(field => 
      this.exportForm.get(`field_${field.key}`)?.value
    ).length;
  }

  onExport() {
    if (!this.exportForm.valid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    this.isExporting = true;
    const formValue = this.exportForm.value;

    // Get selected fields
    const selectedFields = this.getAvailableFields()
      .filter(field => formValue[`field_${field.key}`])
      .map(field => field.key);

    const exportOptions: ExportOptions = {
      format: formValue.format,
      includeFields: selectedFields,
      dateRange: this.data.exportType === 'analytics' ? {
        startDate: formValue.startDate,
        endDate: formValue.endDate
      } : undefined,
      filters: this.data.exportType === 'subscriptions' ? {
        status: formValue.statusFilter
      } : undefined
    };

    // Perform export based on type
    let export$;
    switch (this.data.exportType) {
      case 'analytics':
        export$ = this.analyticsService.exportAnalytics('analytics', exportOptions.format);
        break;
      case 'subscriptions':
        export$ = this.subscriptionService.exportSubscriptions(exportOptions);
        break;
      case 'plans':
        export$ = this.subscriptionService.exportPlans(exportOptions);
        break;
      default:
        this.snackBar.open('Invalid export type', 'Close', { duration: 3000 });
        this.isExporting = false;
        return;
    }

    export$.subscribe({
      next: (blob) => {
        this.isExporting = false;
        this.downloadFile(blob, this.getFileName(exportOptions.format));
        this.snackBar.open('Export completed successfully', 'Close', { duration: 3000 });
        this.dialogRef.close();
      },
      error: (error) => {
        this.isExporting = false;
        console.error('Export error:', error);
        this.snackBar.open('Export failed: ' + (error.message || 'Unknown error'), 'Close', { duration: 5000 });
      }
    });
  }

  private downloadFile(blob: Blob, fileName: string) {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  private getFileName(format: string): string {
    const timestamp = new Date().toISOString().split('T')[0];
    const type = this.data.exportType;
    return `${type}_export_${timestamp}.${format}`;
  }
}
