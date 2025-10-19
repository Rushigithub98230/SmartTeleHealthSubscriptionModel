import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BillingService } from '../../../../core/services';
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

  constructor(
    private route: ActivatedRoute,
    private billingService: BillingService
  ) {}

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
   * Download invoice
   */
  downloadInvoice(): void {
    console.log('Download invoice PDF');
    // Implementation: Generate PDF or download from API
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


