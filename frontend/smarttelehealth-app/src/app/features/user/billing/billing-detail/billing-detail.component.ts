import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BillingService, InvoiceService } from '../../../../core/services';
import { BillingRecordDto } from '../../../../core/models';

/**
 * Billing Detail / Invoice View Component
 * Display full billing record details and invoice
 * 
 * APIs Used:
 * - GET /api/Billing/records/{id}
 * 
 * Route: /web/billing/:id
 * Access: Authenticated users
 */
@Component({
  selector: 'app-billing-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './billing-detail.component.html',
  styleUrls: ['./billing-detail.component.scss']
})
export class BillingDetailComponent implements OnInit {
  billingId!: string;
  billingRecord: BillingRecordDto | null = null;
  loading = false;
  error: string | null = null;
  downloadingInvoice = false;
  generatingInvoice = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private billingService: BillingService,
    private invoiceService: InvoiceService
  ) {}

  /**
   * Check if invoice exists for this billing record
   */
  hasInvoice(): boolean {
    return !!(this.billingRecord?.invoiceNumber || this.billingRecord?.stripeInvoiceId);
  }

  /**
   * Navigate to dedicated invoice detail page
   */
  viewInvoiceDetail(): void {
    if (this.billingRecord?.invoiceNumber) {
      console.log('🔗 [BILLING-DETAIL] Navigating to invoice detail:', this.billingRecord.invoiceNumber);
      this.router.navigate(['/web/invoices', this.billingRecord.invoiceNumber]);
    }
  }

  /**
   * Generate invoice for this billing record
   */
  generateInvoiceForBilling(): void {
    if (!this.billingRecord?.id) {
      console.error('❌ [BILLING-DETAIL] No billing record ID to generate invoice');
      return;
    }

    this.generatingInvoice = true;
    console.log('🔄 [BILLING-DETAIL] Generating invoice for billing record:', this.billingRecord.id);

    this.invoiceService.generateInvoice(this.billingRecord.id).subscribe({
      next: (response) => {
        console.log('📄 [BILLING-DETAIL] Generate invoice response:', response);

        if (response.statusCode === 200) {
          alert('Invoice generated successfully!');
          this.loadBillingDetail(); // Reload to show new invoice
        } else {
          alert(response.message || 'Failed to generate invoice');
          console.error('❌ [BILLING-DETAIL] Failed to generate invoice:', response.message);
        }
        this.generatingInvoice = false;
      },
      error: (error) => {
        console.error('❌ [BILLING-DETAIL] Error generating invoice:', error);
        alert(error.error?.message || 'Failed to generate invoice');
        this.generatingInvoice = false;
      }
    });
  }

  /**
   * View invoice in Stripe dashboard
   */
  viewInStripe(): void {
    if (this.billingRecord?.stripeInvoiceId) {
      const stripeUrl = `https://dashboard.stripe.com/invoices/${this.billingRecord.stripeInvoiceId}`;
      console.log('🔗 [BILLING-DETAIL] Opening Stripe invoice:', stripeUrl);
      window.open(stripeUrl, '_blank');
    }
  }

  ngOnInit(): void {
    this.billingId = this.route.snapshot.params['id'];
    this.loadBillingDetail();
  }

  /**
   * Load billing record details
   * API: GET /api/Billing/records/{id}
   */
  loadBillingDetail(): void {
    this.loading = true;

    this.billingService.getBillingRecordById(this.billingId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingRecord = response.data;
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

  /**
   * Print invoice
   */
  printInvoice(): void {
    window.print();
  }

  /**
   * Download invoice PDF
   * API: GET /api/Invoice/{invoiceNumber}/download
   */
  downloadInvoice(): void {
    if (!this.billingRecord?.invoiceNumber) {
      alert('Invoice not available for this billing record');
      return;
    }

    this.downloadingInvoice = true;
    
    this.invoiceService.downloadInvoice(this.billingRecord.invoiceNumber, 'pdf').subscribe({
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
        this.downloadingInvoice = false;
      },
      error: (error) => {
        console.error('Error downloading invoice:', error);
        alert('Failed to download invoice. Please try again.');
        this.downloadingInvoice = false;
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

  /**
   * Navigate to pay now (for failed/pending bills)
   */
  payNow(): void {
    if (this.billingRecord?.subscriptionId) {
      this.router.navigate(['/web/subscriptions', this.billingRecord.subscriptionId]);
    }
  }

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning',
      'Failed': 'bg-danger',
      'Refunded': 'bg-info'
    };
    return map[status] || 'bg-secondary';
  }
}


