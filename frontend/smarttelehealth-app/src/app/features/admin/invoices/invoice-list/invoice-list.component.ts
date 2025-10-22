import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InvoiceService } from '../../../../core/services';

/**
 * Admin Invoice List Component
 * View and manage all invoices
 * 
 * APIs Used:
 * - GET /api/Invoice/all
 * - GET /api/Invoice/stats
 * - POST /api/Invoice/{invoiceNumber}/regenerate
 * - POST /api/Invoice/bulk-send
 * - GET /api/Invoice/{invoiceNumber}/download
 * 
 * Route: /webadmin/invoices
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss']
})
export class AdminInvoiceListComponent implements OnInit {
  invoices: any[] = [];
  stats: any = null;
  loading = false;
  error: string | null = null;
  selectedInvoices: Set<string> = new Set();

  // Filters
  selectedStatus: string = '';
  startDate: string = '';
  endDate: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  statusOptions = ['All', 'Paid', 'Pending', 'Overdue', 'Failed'];

  constructor(private invoiceService: InvoiceService) {}

  ngOnInit(): void {
    this.loadInvoiceStats();
    this.loadInvoices();
  }

  /**
   * Load invoice statistics
   */
  loadInvoiceStats(): void {
    this.invoiceService.getInvoiceStats().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.stats = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading invoice stats:', err);
      }
    });
  }

  /**
   * Load all invoices with filters
   */
  loadInvoices(): void {
    this.loading = true;
    this.error = null;

    const status = this.selectedStatus && this.selectedStatus !== 'All' ? this.selectedStatus : undefined;
    const startDate = this.startDate ? new Date(this.startDate) : undefined;
    const endDate = this.endDate ? new Date(this.endDate) : undefined;

    this.invoiceService.getAllInvoices(
      this.currentPage,
      this.pageSize,
      status,
      startDate,
      endDate
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.invoices = response.data || [];
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load invoices';
        this.loading = false;
      }
    });
  }

  /**
   * Apply filters and reload invoices
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadInvoices();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.loadInvoices();
  }

  /**
   * Download invoice PDF
   */
  downloadInvoice(invoice: any): void {
    this.invoiceService.downloadInvoice(invoice.invoiceNumber, 'pdf').subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Handle file download
          alert('Invoice download initiated');
        }
      },
      error: (err) => alert(err.message || 'Failed to download invoice')
    });
  }

  /**
   * Regenerate invoice
   */
  regenerateInvoice(invoice: any): void {
    if (!confirm(`Regenerate invoice ${invoice.invoiceNumber}?`)) {
      return;
    }

    this.invoiceService.regenerateInvoice(invoice.invoiceNumber).subscribe({
      next: () => {
        alert('Invoice regenerated successfully');
        this.loadInvoices();
      },
      error: (err) => alert(err.message || 'Failed to regenerate invoice')
    });
  }

  /**
   * Send single invoice
   */
  sendInvoice(invoice: any): void {
    if (!invoice.userEmail) {
      alert('No email address available');
      return;
    }

    this.invoiceService.sendInvoice(invoice.invoiceNumber, invoice.userEmail).subscribe({
      next: () => alert('Invoice sent successfully'),
      error: (err) => alert(err.message || 'Failed to send invoice')
    });
  }

  /**
   * Toggle selection for bulk operations
   */
  toggleSelection(invoiceNumber: string): void {
    if (this.selectedInvoices.has(invoiceNumber)) {
      this.selectedInvoices.delete(invoiceNumber);
    } else {
      this.selectedInvoices.add(invoiceNumber);
    }
  }

  /**
   * Bulk send selected invoices
   */
  bulkSendInvoices(): void {
    if (this.selectedInvoices.size === 0) {
      alert('Please select invoices to send');
      return;
    }

    if (!confirm(`Send ${this.selectedInvoices.size} invoices?`)) {
      return;
    }

    const invoiceNumbers = Array.from(this.selectedInvoices);
    this.invoiceService.bulkSendInvoices({
      invoiceNumbers,
      delayBetweenEmailsMs: 500,
      continueOnError: true
    }).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          const result = response.data;
          alert(`Bulk send complete: ${result.successCount} sent, ${result.failureCount} failed`);
          this.selectedInvoices.clear();
          this.loadInvoices();
        }
      },
      error: (err) => alert(err.message || 'Failed to bulk send invoices')
    });
  }

  /**
   * Get selected count
   */
  get selectedCount(): number {
    return this.selectedInvoices.size;
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning text-dark',
      'Overdue': 'bg-danger',
      'Failed': 'bg-danger'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Format currency
   */
  formatCurrency(amount: number): string {
    return `$${amount.toFixed(2)}`;
  }
}

