import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BillingService } from '../../../../core/services';
import { BillingRecordDto } from '../../../../core/models';

/**
 * Admin Billing Detail Component
 * View detailed billing record with admin actions
 * 
 * APIs Used:
 * - GET /api/Billing/records/{id}
 * - POST /api/Billing/{id}/process-refund (Manual Refund Processing)
 * 
 * Route: /webadmin/billing/:id
 * Access: Admin only
 * 
 * IMPORTANT: Mid-cycle subscription cancellations do NOT auto-refund.
 * Admin must manually process refunds through this interface.
 */
@Component({
  selector: 'app-admin-billing-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './billing-detail.component.html',
  styleUrls: ['./billing-detail.component.scss']
})
export class AdminBillingDetailComponent implements OnInit {
  billingId!: string;
  billingRecord: BillingRecordDto | null = null;
  loading = false;
  processing = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Refund modal state
  showRefundModal = false;
  refundAmount: number = 0;
  refundReason: string = '';

  constructor(
    private route: ActivatedRoute,
    private billingService: BillingService
  ) {}

  ngOnInit(): void {
    this.billingId = this.route.snapshot.params['id'];
    this.loadBillingDetail();
  }

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

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Paid': 'bg-success',
      'Pending': 'bg-warning',
      'Failed': 'bg-danger',
      'Refunded': 'bg-info'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Opens refund modal
   */
  processRefund(): void {
    if (!this.billingRecord) return;

    // Check if billing record can be refunded
    if (this.billingRecord.status !== 'Paid') {
      alert('Can only refund paid billing records. Current status: ' + this.billingRecord.status);
      return;
    }

    // Initialize refund modal with full amount
    this.refundAmount = this.billingRecord.totalAmount;
    this.refundReason = '';
    this.showRefundModal = true;
  }

  /**
   * Submit refund processing
   * API: POST /api/Billing/{id}/process-refund
   */
  submitRefund(): void {
    // Validate inputs
    if (!this.refundAmount || this.refundAmount <= 0) {
      alert('Refund amount must be greater than 0');
      return;
    }

    if (!this.refundReason || this.refundReason.trim() === '') {
      alert('Refund reason is required');
      return;
    }

    if (this.refundAmount > (this.billingRecord?.totalAmount || 0)) {
      alert('Refund amount cannot exceed billing amount');
      return;
    }

    if (!confirm(`Process refund of $${this.refundAmount.toFixed(2)}?`)) {
      return;
    }

    this.processing = true;
    this.error = null;
    this.successMessage = null;

    this.billingService.processRefund(this.billingId, this.refundAmount, this.refundReason).subscribe({
      next: (response) => {
        this.processing = false;

        if (response.statusCode === 200) {
          this.successMessage = 'Refund processed successfully. Customer will receive ' +
            `$${this.refundAmount.toFixed(2)} back to their payment method.`;
          this.showRefundModal = false;
          this.loadBillingDetail(); // Reload to show updated status
        } else {
          this.error = response.message || 'Failed to process refund';
        }
      },
      error: (error) => {
        this.processing = false;
        this.error = error.error?.message || error.message || 'An error occurred while processing refund';
        console.error('❌ Refund Error:', error);
      }
    });
  }

  /**
   * Close refund modal
   */
  cancelRefund(): void {
    this.showRefundModal = false;
    this.refundAmount = 0;
    this.refundReason = '';
  }

  /**
   * Check if billing record can be refunded
   */
  canRefund(): boolean {
    return this.billingRecord?.status === 'Paid';
  }

  resendInvoice(): void {
    console.log('Resend invoice for billing record:', this.billingId);
    // Implementation: Call resend invoice API
  }
}


