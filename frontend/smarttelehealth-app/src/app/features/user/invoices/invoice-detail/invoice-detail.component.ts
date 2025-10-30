import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InvoiceService, AuthService } from '../../../../core/services';

/**
 * Invoice Detail Component
 * Display comprehensive details of a single invoice with actions
 * 
 * APIs Used:
 * - GET /api/Invoice/{invoiceNumber}
 * - GET /api/Invoice/{invoiceNumber}/download
 * - POST /api/Invoice/{invoiceNumber}/send
 * 
 * Route: /web/invoices/:invoiceNumber
 * Access: Authenticated users (invoice owner or admin)
 * 
 * Enhancement #4: Dedicated invoice-detail component
 */
@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.scss']
})
export class InvoiceDetailComponent implements OnInit {
  invoiceNumber: string = '';
  invoice: any = null; // Will contain nested billing record and user data
  loading = false;
  error: string | null = null;
  downloading = false;
  sending = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private invoiceService: InvoiceService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.invoiceNumber = this.route.snapshot.params['invoiceNumber'];
    
    console.log('🎯 [INVOICE-DETAIL] Component initialized for invoice:', this.invoiceNumber);
    
    if (!this.invoiceNumber) {
      this.error = 'Invoice number not provided';
      console.error('❌ [INVOICE-DETAIL] No invoice number in route');
      return;
    }
    
    this.loadInvoice();
  }

  /**
   * Load invoice details from API
   */
  loadInvoice(): void {
    this.loading = true;
    this.error = null;

    console.log('📥 [INVOICE-DETAIL] Loading invoice:', this.invoiceNumber);

    this.invoiceService.getInvoice(this.invoiceNumber).subscribe({
      next: (response) => {
        console.log('📄 [INVOICE-DETAIL] API Response:', {
          statusCode: response.statusCode,
          hasData: !!response.data,
          dataKeys: response.data ? Object.keys(response.data) : []
        });

        if (response.statusCode === 200 && response.data) {
          this.invoice = response.data;
          console.log('✅ [INVOICE-DETAIL] Invoice loaded successfully:', {
            invoiceNumber: this.getInvoiceNumber(),
            amount: this.getTotalAmount(),
            status: this.getStatus()
          });
        } else {
          this.error = response.message || 'Failed to load invoice';
          console.error('❌ [INVOICE-DETAIL] Failed to load invoice:', response.message);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('❌ [INVOICE-DETAIL] Error loading invoice:', {
          error: err,
          message: err.error?.message,
          status: err.status
        });
        this.error = err.error?.message || 'Failed to load invoice details';
        this.loading = false;
      }
    });
  }

  /**
   * Download invoice as PDF
   */
  downloadInvoicePdf(): void {
    this.downloading = true;
    console.log('⬇️ [INVOICE-DETAIL] Initiating PDF download for invoice:', this.invoiceNumber);

    this.invoiceService.downloadInvoice(this.invoiceNumber, 'pdf').subscribe({
      next: (response) => {
        console.log('📥 [INVOICE-DETAIL] Download response received:', {
          statusCode: response.statusCode,
          hasFileContent: !!response.data?.fileContent,
          fileName: response.data?.fileName
        });

        if (response.statusCode === 200 && response.data) {
          const { fileContent, fileName, contentType } = response.data;
          
          // Convert base64 to blob
          const byteCharacters = atob(fileContent);
          const byteNumbers = new Array(byteCharacters.length);
          for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
          }
          const byteArray = new Uint8Array(byteNumbers);
          const blob = new Blob([byteArray], { type: contentType });
          
          // Create and trigger download
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = fileName;
          link.click();
          
          window.URL.revokeObjectURL(url);
          
          console.log('✅ [INVOICE-DETAIL] Invoice PDF downloaded successfully:', fileName);
        } else {
          alert(response.message || 'Failed to download invoice');
          console.error('❌ [INVOICE-DETAIL] Download failed:', response.message);
        }
        this.downloading = false;
      },
      error: (err) => {
        console.error('❌ [INVOICE-DETAIL] Error downloading invoice:', err);
        alert(err.error?.message || 'Failed to download invoice');
        this.downloading = false;
      }
    });
  }

  /**
   * Print invoice (browser print dialog)
   */
  printInvoice(): void {
    console.log('🖨️ [INVOICE-DETAIL] Opening print dialog');
    window.print();
  }

  /**
   * Send invoice to email address
   */
  sendInvoiceEmail(): void {
    const email = prompt('Enter email address to send invoice:', this.getUserEmail());
    
    if (!email) {
      console.log('ℹ️ [INVOICE-DETAIL] Email send cancelled by user');
      return;
    }
    
    if (!this.isValidEmail(email)) {
      alert('Please enter a valid email address');
      console.warn('⚠️ [INVOICE-DETAIL] Invalid email format:', email);
      return;
    }

    this.sending = true;
    console.log('📧 [INVOICE-DETAIL] Sending invoice to email:', email);

    this.invoiceService.sendInvoice(this.invoiceNumber, email).subscribe({
      next: (response) => {
        console.log('📬 [INVOICE-DETAIL] Send email response:', {
          statusCode: response.statusCode,
          message: response.message
        });

        if (response.statusCode === 200) {
          alert(`Invoice sent successfully to ${email}`);
          console.log('✅ [INVOICE-DETAIL] Invoice email sent successfully');
        } else {
          alert(response.message || 'Failed to send invoice');
          console.error('❌ [INVOICE-DETAIL] Send failed:', response.message);
        }
        this.sending = false;
      },
      error: (err) => {
        console.error('❌ [INVOICE-DETAIL] Error sending invoice:', err);
        alert(err.error?.message || 'Failed to send invoice');
        this.sending = false;
      }
    });
  }

  /**
   * View invoice in Stripe dashboard
   */
  viewInStripe(): void {
    const stripeInvoiceId = this.getStripeInvoiceId();
    if (stripeInvoiceId) {
      const stripeUrl = `https://dashboard.stripe.com/invoices/${stripeInvoiceId}`;
      console.log('🔗 [INVOICE-DETAIL] Opening Stripe invoice:', stripeUrl);
      window.open(stripeUrl, '_blank');
    }
  }

  /**
   * Navigate back to invoice list
   */
  goBack(): void {
    console.log('⬅️ [INVOICE-DETAIL] Navigating back to invoice list');
    this.router.navigate(['/web/invoices']);
  }

  /**
   * Navigate to billing record detail
   */
  viewBillingDetail(): void {
    const billingRecordId = this.invoice?.billingRecordId || this.invoice?.BillingRecordId;
    if (billingRecordId) {
      console.log('🔗 [INVOICE-DETAIL] Navigating to billing record:', billingRecordId);
      this.router.navigate(['/web/billing', billingRecordId]);
    }
  }

  /**
   * Validate email format
   */
  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  // ===== GETTERS FOR TEMPLATE =====

  getInvoiceNumber(): string {
    return this.invoice?.invoiceNumber || 
           this.invoice?.InvoiceNumber || 
           this.invoiceNumber;
  }

  getTotalAmount(): number {
    return this.invoice?.totalAmount || 
           this.invoice?.billingRecord?.totalAmount || 
           0;
  }

  getAmount(): number {
    return this.invoice?.amount || 
           this.invoice?.billingRecord?.amount || 
           0;
  }

  getTaxAmount(): number {
    return this.invoice?.taxAmount || 
           this.invoice?.billingRecord?.taxAmount || 
           0;
  }

  getStatus(): string {
    return this.invoice?.status || 
           this.invoice?.billingRecord?.status || 
           'Unknown';
  }

  getInvoiceDate(): Date | null {
    const dateStr = this.invoice?.invoiceDate || 
                    this.invoice?.billingRecord?.billingDate ||
                    this.invoice?.billingRecord?.createdDate;
    return dateStr ? new Date(dateStr) : null;
  }

  getDueDate(): Date | null {
    const dateStr = this.invoice?.dueDate || 
                    this.invoice?.billingRecord?.dueDate;
    return dateStr ? new Date(dateStr) : null;
  }

  getPaidDate(): Date | null {
    const dateStr = this.invoice?.paidAt || 
                    this.invoice?.billingRecord?.paidAt;
    return dateStr ? new Date(dateStr) : null;
  }

  getUserName(): string {
    return this.invoice?.userName || 
           this.invoice?.user?.fullName || 
           this.invoice?.billingRecord?.user?.fullName || 
           'N/A';
  }

  getUserEmail(): string {
    return this.invoice?.userEmail || 
           this.invoice?.user?.email || 
           this.invoice?.billingRecord?.user?.email || 
           '';
  }

  getDescription(): string {
    return this.invoice?.description || 
           this.invoice?.billingRecord?.description || 
           'No description';
  }

  getStripeInvoiceId(): string {
    return this.invoice?.stripeInvoiceId || 
           this.invoice?.billingRecord?.stripeInvoiceId || 
           '';
  }

  hasStripeInvoice(): boolean {
    return !!this.getStripeInvoiceId();
  }

  isPaid(): boolean {
    const status = this.getStatus().toLowerCase();
    return status === 'paid';
  }

  isPending(): boolean {
    const status = this.getStatus().toLowerCase();
    return status === 'pending';
  }

  isOverdue(): boolean {
    if (this.isPaid()) return false;
    
    const dueDate = this.getDueDate();
    if (!dueDate) return false;
    
    return dueDate < new Date();
  }

  getStatusBadgeClass(): string {
    const status = this.getStatus();
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning text-dark',
      'Overdue': 'bg-danger',
      'Failed': 'bg-danger',
      'Refunded': 'bg-secondary'
    };
    return map[status] || 'bg-secondary';
  }

  getDaysUntilDue(): number {
    if (this.isPaid()) return 0;
    
    const dueDate = this.getDueDate();
    if (!dueDate) return 0;
    
    const now = new Date();
    const diffTime = dueDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return diffDays;
  }

  getStatusMessage(): string {
    if (this.isPaid()) {
      return 'This invoice has been paid';
    }
    
    if (this.isOverdue()) {
      const daysOverdue = Math.abs(this.getDaysUntilDue());
      return `This invoice is overdue by ${daysOverdue} ${daysOverdue === 1 ? 'day' : 'days'}`;
    }
    
    const daysUntilDue = this.getDaysUntilDue();
    if (daysUntilDue > 0) {
      return `Payment due in ${daysUntilDue} ${daysUntilDue === 1 ? 'day' : 'days'}`;
    }
    
    return 'Payment status pending';
  }
}



