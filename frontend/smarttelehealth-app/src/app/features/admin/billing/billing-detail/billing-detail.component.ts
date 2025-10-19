import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BillingService } from '../../../../core/services';
import { BillingRecordDto } from '../../../../core/models';

/**
 * Admin Billing Detail Component
 * View detailed billing record with admin actions
 * 
 * APIs Used:
 * - GET /api/Billing/records/{id}
 * 
 * Route: /webadmin/billing/:id
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-billing-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './billing-detail.component.html',
  styleUrls: ['./billing-detail.component.scss']
})
export class AdminBillingDetailComponent implements OnInit {
  billingId!: string;
  billingRecord: BillingRecordDto | null = null;
  loading = false;
  error: string | null = null;

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

  processRefund(): void {
    if (!confirm('Are you sure you want to process a refund for this billing record?')) return;
    console.log('Process refund for billing record:', this.billingId);
    // Implementation: Call refund API
  }

  resendInvoice(): void {
    console.log('Resend invoice for billing record:', this.billingId);
    // Implementation: Call resend invoice API
  }
}


