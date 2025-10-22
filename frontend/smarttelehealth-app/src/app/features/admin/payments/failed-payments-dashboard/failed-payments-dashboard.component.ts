import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../../../core/services';

/**
 * Failed Payments Dashboard Component
 * Manage and retry failed payment attempts
 * 
 * APIs Used:
 * - GET /api/payments/failed
 * - POST /api/payments/retry-payment/{id}
 * - POST /api/payments/{id}/send-reminder
 * - POST /api/payments/bulk-retry
 * 
 * Route: /webadmin/payments
 * Access: Admin only
 */
@Component({
  selector: 'app-failed-payments-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './failed-payments-dashboard.component.html',
  styleUrls: ['./failed-payments-dashboard.component.scss']
})
export class FailedPaymentsDashboardComponent implements OnInit {
  failedPayments: any[] = [];
  loading = false;
  error: string | null = null;
  selectedPayments: Set<string> = new Set();

  constructor(private paymentService: PaymentService) {}

  ngOnInit(): void {
    this.loadFailedPayments();
  }

  /**
   * Load all failed payments from API
   */
  loadFailedPayments(): void {
    this.loading = true;
    this.error = null;

    this.paymentService.getFailedPayments().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.failedPayments = response.data || [];
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load failed payments';
        this.loading = false;
      }
    });
  }

  /**
   * Retry a single failed payment
   */
  retryPayment(payment: any): void {
    if (!confirm(`Retry payment of $${payment.amount} for ${payment.userName}?`)) {
      return;
    }

    this.paymentService.retryPayment(payment.billingRecordId).subscribe({
      next: () => {
        alert('Payment retry initiated successfully');
        this.loadFailedPayments();
      },
      error: (err) => alert(err.message || 'Failed to retry payment')
    });
  }

  /**
   * Send payment reminder email to customer
   */
  sendReminder(payment: any): void {
    this.paymentService.sendPaymentReminder(payment.billingRecordId, {
      includePaymentLink: true
    }).subscribe({
      next: () => alert('Payment reminder sent successfully'),
      error: (err) => alert(err.message || 'Failed to send reminder')
    });
  }

  /**
   * Toggle selection of a payment for bulk operations
   */
  toggleSelection(paymentId: string): void {
    if (this.selectedPayments.has(paymentId)) {
      this.selectedPayments.delete(paymentId);
    } else {
      this.selectedPayments.add(paymentId);
    }
  }

  /**
   * Bulk retry all selected failed payments
   */
  bulkRetry(): void {
    if (this.selectedPayments.size === 0) {
      alert('Please select payments to retry');
      return;
    }

    if (!confirm(`Retry ${this.selectedPayments.size} failed payments?`)) {
      return;
    }

    const billingRecordIds = Array.from(this.selectedPayments);
    this.paymentService.bulkRetryPayments({
      billingRecordIds,
      delayBetweenRetriesMs: 1000,
      notifyOnSuccess: true,
      continueOnError: true
    }).subscribe({
      next: () => {
        alert('Bulk retry initiated successfully');
        this.selectedPayments.clear();
        this.loadFailedPayments();
      },
      error: (err) => alert(err.message || 'Failed to bulk retry payments')
    });
  }

  /**
   * Get count of selected payments
   */
  get selectedCount(): number {
    return this.selectedPayments.size;
  }

  /**
   * Calculate total amount of failed payments
   */
  get totalFailedAmount(): number {
    return this.failedPayments.reduce((sum, p) => sum + (p.amount || 0), 0);
  }
}

