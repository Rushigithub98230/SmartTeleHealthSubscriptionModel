import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BillingService } from '../../../../core/services';
import { BillingRecordDto, BillingStatus, BillingType } from '../../../../core/models';

/**
 * Admin Billing Management Component
 * View and manage all billing records
 * 
 * APIs Used:
 * - GET /api/Billing/records
 * - POST /api/Billing/export
 * 
 * Route: /webadmin/billing
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-billing-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './billing-list.component.html',
  styleUrls: ['./billing-list.component.scss']
})
export class AdminBillingListComponent implements OnInit {
  billingRecords: BillingRecordDto[] = [];
  loading = false;
  error: string | null = null;

  // Filters
  searchTerm = '';
  selectedStatus: string = '';
  selectedType: string = '';
  startDate: string = '';
  endDate: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  statusOptions = ['All', 'Paid', 'Pending', 'Failed', 'Overdue', 'Refunded', 'Cancelled'];
  typeOptions = ['All', 'Subscription', 'Overage', 'Consultation', 'Medication', 'LateFee'];

  constructor(private billingService: BillingService) {}

  ngOnInit(): void {
    this.loadBillingRecords();
  }

  /**
   * Get count of failed billing records
   */
  get failedBillingCount(): number {
    return this.billingRecords.filter(r => r.status === 'Failed').length;
  }

  /**
   * Load all billing records
   * API: GET /api/Billing/records
   */
  loadBillingRecords(): void {
    this.loading = true;
    this.error = null;

    const filters: any = {};
    if (this.selectedStatus && this.selectedStatus !== 'All') filters.status = [this.selectedStatus];
    if (this.selectedType && this.selectedType !== 'All') filters.type = [this.selectedType];
    if (this.startDate) filters.startDate = new Date(this.startDate);
    if (this.endDate) filters.endDate = new Date(this.endDate);

    this.billingService.getBillingRecords(undefined, this.currentPage, this.pageSize, filters).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingRecords = response.data;
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadBillingRecords();
  }

  changePage(page: number): void {
    this.currentPage = page;
    this.loadBillingRecords();
  }

  exportBilling(): void {
    this.billingService.exportBillingRecords('csv').subscribe({
      next: (data) => {
        console.log('Export successful', data);
        // Handle file download
      },
      error: (error) => alert(error.message)
    });
  }

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning text-dark',
      'Failed': 'bg-danger',
      'Overdue': 'bg-danger',
      'Refunded': 'bg-info'
    };
    return map[status] || 'bg-secondary';
  }

  getTotalAmount(): number {
    return this.billingRecords.reduce((sum, r) => sum + r.totalAmount, 0);
  }

  getPaidAmount(): number {
    return this.billingRecords.filter(r => r.status === 'Paid').reduce((sum, r) => sum + r.totalAmount, 0);
  }
}

