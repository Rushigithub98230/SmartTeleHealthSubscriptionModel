import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InvoiceService, AuthService } from '../../../../core/services';

/**
 * Invoice List Component
 * Display paginated list of user's invoices with filtering and search
 * 
 * APIs Used:
 * - GET /api/Invoice/user/{userId}
 * - GET /api/Invoice/{invoiceNumber}/download
 * 
 * Route: /web/invoices
 * Access: Authenticated users
 * 
 * Enhancement #6: User invoice list with filtering
 */
@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss']
})
export class InvoiceListComponent implements OnInit {
  invoices: any[] = [];
  filteredInvoices: any[] = [];
  loading = false;
  error: string | null = null;
  downloadingInvoice: string | null = null;

  // Filters
  selectedStatus: string = 'All';
  startDate: string = '';
  endDate: string = '';
  searchTerm: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;
  displayedInvoices: any[] = [];

  // Filter options
  statusOptions = ['All', 'Paid', 'Pending', 'Overdue', 'Failed', 'Refunded'];
  pageSizeOptions = [10, 20, 50, 100];

  constructor(
    private invoiceService: InvoiceService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('🎯 [INVOICE-LIST] Component initialized');
    this.loadInvoices();
  }

  /**
   * Load user invoices from API
   */
  loadInvoices(): void {
    this.loading = true;
    this.error = null;

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser) {
      this.error = 'User not authenticated';
      this.loading = false;
      console.error('❌ [INVOICE-LIST] No authenticated user');
      return;
    }

    console.log('📥 [INVOICE-LIST] Loading invoices for user:', currentUser.id);

    this.invoiceService.getUserInvoices(
      currentUser.id,
      1, // Load all pages initially
      1000 // Large page size to get all records
    ).subscribe({
      next: (response) => {
        console.log('📄 [INVOICE-LIST] API Response:', {
          statusCode: response.statusCode,
          hasData: !!response.data,
          hasMeta: !!response.meta
        });

        if (response.statusCode === 200) {
          // Handle both array and single object responses
          this.invoices = Array.isArray(response.data) 
            ? response.data 
            : (response.data ? [response.data] : []);
          
          console.log('✅ [INVOICE-LIST] Loaded invoices:', {
            count: this.invoices.length,
            hasMeta: !!response.meta
          });

          // Apply filters and pagination
          this.applyFilters();
        } else {
          this.error = response.message || 'Failed to load invoices';
          console.error('❌ [INVOICE-LIST] Failed to load invoices:', response.message);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('❌ [INVOICE-LIST] Error loading invoices:', {
          error: err,
          message: err.error?.message,
          status: err.status
        });
        this.error = err.error?.message || 'Failed to load invoices';
        this.loading = false;
      }
    });
  }

  /**
   * Apply all filters to invoices
   */
  applyFilters(): void {
    console.log('🔍 [INVOICE-LIST] Applying filters:', {
      status: this.selectedStatus,
      startDate: this.startDate,
      endDate: this.endDate,
      searchTerm: this.searchTerm
    });

    let filtered = [...this.invoices];

    // Status filter
    if (this.selectedStatus && this.selectedStatus !== 'All') {
      filtered = filtered.filter(inv => {
        const status = this.getInvoiceStatus(inv);
        return status.toLowerCase() === this.selectedStatus.toLowerCase();
      });
    }

    // Date range filter
    if (this.startDate) {
      const start = new Date(this.startDate);
      filtered = filtered.filter(inv => {
        const invDate = new Date(this.getInvoiceDate(inv));
        return invDate >= start;
      });
    }

    if (this.endDate) {
      const end = new Date(this.endDate);
      end.setHours(23, 59, 59, 999); // End of day
      filtered = filtered.filter(inv => {
        const invDate = new Date(this.getInvoiceDate(inv));
        return invDate <= end;
      });
    }

    // Search filter
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(inv =>
        this.getInvoiceNumber(inv)?.toLowerCase().includes(search) ||
        this.getDescription(inv)?.toLowerCase().includes(search)
      );
    }

    this.filteredInvoices = filtered;
    this.totalRecords = filtered.length;
    this.totalPages = Math.ceil(this.totalRecords / this.pageSize);
    
    // Reset to page 1 if current page exceeds total pages
    if (this.currentPage > this.totalPages && this.totalPages > 0) {
      this.currentPage = 1;
    }

    this.updateDisplayedInvoices();

    console.log('✅ [INVOICE-LIST] Filters applied:', {
      originalCount: this.invoices.length,
      filteredCount: this.filteredInvoices.length,
      totalPages: this.totalPages
    });
  }

  /**
   * Update displayed invoices based on current page
   */
  updateDisplayedInvoices(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedInvoices = this.filteredInvoices.slice(startIndex, endIndex);

    console.log('📄 [INVOICE-LIST] Updated displayed invoices:', {
      page: this.currentPage,
      pageSize: this.pageSize,
      startIndex,
      endIndex,
      displayedCount: this.displayedInvoices.length
    });
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    console.log('🧹 [INVOICE-LIST] Clearing all filters');
    this.selectedStatus = 'All';
    this.startDate = '';
    this.endDate = '';
    this.searchTerm = '';
    this.currentPage = 1;
    this.applyFilters();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updateDisplayedInvoices();
      console.log('📄 [INVOICE-LIST] Changed to page:', page);
      
      // Scroll to top
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  /**
   * Change page size
   */
  changePageSize(): void {
    console.log('📊 [INVOICE-LIST] Changed page size to:', this.pageSize);
    this.currentPage = 1;
    this.applyFilters();
  }

  /**
   * Download invoice PDF
   */
  downloadInvoice(invoiceNumber: string): void {
    this.downloadingInvoice = invoiceNumber;
    console.log('⬇️ [INVOICE-LIST] Downloading invoice:', invoiceNumber);

    this.invoiceService.downloadInvoice(invoiceNumber, 'pdf').subscribe({
      next: (response) => {
        console.log('📥 [INVOICE-LIST] Download response:', {
          statusCode: response.statusCode,
          hasFileContent: !!response.data?.fileContent
        });

        if (response.statusCode === 200 && response.data) {
          const { fileContent, fileName, contentType } = response.data;
          
          // Convert base64 to blob and download
          const byteCharacters = atob(fileContent);
          const byteNumbers = new Array(byteCharacters.length);
          for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
          }
          const byteArray = new Uint8Array(byteNumbers);
          const blob = new Blob([byteArray], { type: contentType });
          
          // Create download link
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = fileName;
          link.click();
          
          window.URL.revokeObjectURL(url);
          
          console.log('✅ [INVOICE-LIST] Invoice downloaded:', fileName);
        } else {
          alert(response.message || 'Failed to download invoice');
          console.error('❌ [INVOICE-LIST] Download failed:', response.message);
        }
        this.downloadingInvoice = null;
      },
      error: (err) => {
        console.error('❌ [INVOICE-LIST] Error downloading invoice:', err);
        alert(err.error?.message || 'Failed to download invoice');
        this.downloadingInvoice = null;
      }
    });
  }

  /**
   * Get page numbers for pagination
   */
  getPageNumbers(): number[] {
    const pages = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  // ===== HELPER METHODS FOR FLEXIBLE DATA ACCESS =====

  getInvoiceNumber(invoice: any): string {
    return invoice?.invoiceNumber || invoice?.InvoiceNumber || 'N/A';
  }

  getInvoiceDate(invoice: any): Date | string {
    return invoice?.invoiceDate || 
           invoice?.billingDate || 
           invoice?.createdDate || 
           new Date();
  }

  getDueDate(invoice: any): Date | string {
    return invoice?.dueDate || 
           invoice?.billingDate || 
           new Date();
  }

  getTotalAmount(invoice: any): number {
    return invoice?.totalAmount || 
           invoice?.amount || 
           0;
  }

  getInvoiceStatus(invoice: any): string {
    return invoice?.status || 
           invoice?.paymentStatus || 
           'Unknown';
  }

  getDescription(invoice: any): string {
    return invoice?.description || 
           invoice?.type || 
           'No description';
  }

  /**
   * Get status badge class for styling
   */
  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning text-dark',
      'Overdue': 'bg-danger',
      'Failed': 'bg-danger',
      'Refunded': 'bg-secondary',
      'Cancelled': 'bg-secondary'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Check if any filters are active
   */
  hasActiveFilters(): boolean {
    return this.selectedStatus !== 'All' || 
           !!this.startDate || 
           !!this.endDate || 
           !!this.searchTerm;
  }

  /**
   * Get summary text for current view
   */
  getSummaryText(): string {
    if (this.totalRecords === 0) {
      return 'No invoices found';
    }

    const start = (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, this.totalRecords);
    
    return `Showing ${start}-${end} of ${this.totalRecords} invoice${this.totalRecords !== 1 ? 's' : ''}`;
  }
}




