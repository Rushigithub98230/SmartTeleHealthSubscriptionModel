import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BillingService, AuthService, InvoiceService } from '../../../../core/services';
import { BillingRecordDto, UserDto, BillingStatus, BillingType } from '../../../../core/models';

/**
 * Billing History Component
 * Display all user billing records with filtering
 * 
 * APIs Used:
 * - GET /api/Billing/records
 * 
 * Route: /web/billing
 * Access: Authenticated users
 */
@Component({
  selector: 'app-billing-history',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './billing-history.component.html',
  styleUrls: ['./billing-history.component.scss']
})
export class BillingHistoryComponent implements OnInit {
  Math = Math;  // Expose Math to template
  currentUser: UserDto | null = null;
  billingRecords: BillingRecordDto[] = [];
  loading = false;
  error: string | null = null;
  downloadingInvoice: string | null = null;

  // Filters
  selectedStatus: string = '';
  selectedType: string = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  statusOptions = ['All', 'Paid', 'Pending', 'Failed', 'Overdue', 'Refunded'];
  typeOptions = ['All', 'Subscription', 'Overage', 'Consultation', 'Medication'];

  constructor(
    private authService: AuthService,
    private billingService: BillingService,
    private invoiceService: InvoiceService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.loadBillingRecords();
    }
  }

  /**
   * Load billing records with filters
   * API: GET /api/Billing/records
   */
  loadBillingRecords(): void {
    if (!this.currentUser) return;

    this.loading = true;
    this.error = null;

    const filters: any = {};
    if (this.selectedStatus && this.selectedStatus !== 'All') {
      filters.status = [this.selectedStatus];
    }
    if (this.selectedType && this.selectedType !== 'All') {
      filters.type = [this.selectedType];
    }

    this.billingService.getBillingRecords(
      this.currentUser.id,
      this.currentPage,
      this.pageSize,
      filters
    ).subscribe({
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
        this.error = error.message || 'Failed to load billing records';
        this.loading = false;
      }
    });
  }

  /**
   * Apply filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadBillingRecords();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.loadBillingRecords();
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning text-dark',
      'Failed': 'bg-danger',
      'Refunded': 'bg-info',
      'Overdue': 'bg-danger',
      'Cancelled': 'bg-secondary'
    };
    return statusMap[status] || 'bg-secondary';
  }

  /**
   * Get type badge class
   */
  getTypeBadgeClass(type: string): string {
    const typeMap: { [key: string]: string } = {
      'Subscription': 'bg-primary',
      'Overage': 'bg-warning text-dark',
      'Consultation': 'bg-info',
      'Medication': 'bg-success',
      'LateFee': 'bg-danger',
      'Refund': 'bg-secondary'
    };
    return typeMap[type] || 'bg-secondary';
  }

  /**
   * Calculate total for visible records
   */
  getTotalAmount(): number {
    return this.billingRecords.reduce((sum, record) => sum + record.totalAmount, 0);
  }

  /**
   * Download invoice PDF
   * API: GET /api/Invoice/{invoiceNumber}/download
   */
  downloadInvoice(invoiceNumber: string): void {
    this.downloadingInvoice = invoiceNumber;
    
    this.invoiceService.downloadInvoice(invoiceNumber, 'pdf').subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Convert base64 to blob and trigger download
          const blob = this.base64ToBlob(
            response.data.fileContent,
            'application/pdf'
          );
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = response.data.fileName;
          link.click();
          window.URL.revokeObjectURL(url);
        }
        this.downloadingInvoice = null;
      },
      error: (error) => {
        console.error('Error downloading invoice:', error);
        alert('Failed to download invoice. Please try again.');
        this.downloadingInvoice = null;
      }
    });
  }

  /**
   * Convert base64 string to Blob
   */
  private base64ToBlob(base64: string, contentType: string): Blob {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: contentType });
  }
}

