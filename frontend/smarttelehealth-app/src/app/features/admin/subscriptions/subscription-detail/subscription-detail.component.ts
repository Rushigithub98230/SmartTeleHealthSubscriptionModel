import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SubscriptionService } from '../../../../core/services';
import { SubscriptionDto } from '../../../../core/models';

/**
 * Admin Subscription Detail Component
 * View full subscription details with admin actions
 * 
 * APIs Used:
 * - GET /api/Subscriptions/{id}
 * - POST /api/Admin/Subscriptions/{id}/grant-credits (future)
 * - POST /api/Admin/Subscriptions/{id}/suspend (future)
 * 
 * Route: /webadmin/subscriptions/:id
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-subscription-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './subscription-detail.component.html',
  styleUrls: ['./subscription-detail.component.scss']
})
export class AdminSubscriptionDetailComponent implements OnInit {
  subscriptionId!: string;
  subscription: SubscriptionDto | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private subscriptionService: SubscriptionService
  ) {}

  ngOnInit(): void {
    this.subscriptionId = this.route.snapshot.params['id'];
    this.loadSubscription();
  }

  /**
   * Load subscription details
   */
  loadSubscription(): void {
    this.loading = true;

    this.subscriptionService.getSubscriptionById(this.subscriptionId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscription = response.data;
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
      'Active': 'bg-success',
      'Pending': 'bg-warning',
      'Cancelled': 'bg-danger'
    };
    return map[status] || 'bg-secondary';
  }
}


