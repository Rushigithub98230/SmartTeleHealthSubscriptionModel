import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SubscriptionService, CommonService } from '../../../../core/services';
import { SubscriptionDto } from '../../../../core/models';

/**
 * Admin Subscription List Component
 * View and manage all user subscriptions (admin view)
 * 
 * APIs Used:
 * - GET /api/Subscriptions/admin/user-subscriptions
 * FIXED: Now properly connected to backend API
 * 
 * Route: /webadmin/subscriptions
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-subscription-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './subscription-list.component.html',
  styleUrls: ['./subscription-list.component.scss']
})
export class AdminSubscriptionListComponent implements OnInit {
  Math = Math;  // Expose Math to template
  subscriptions: SubscriptionDto[] = [];
  loading = false;
  error: string | null = null;

  // Filters
  searchTerm = '';
  selectedStatus: string = '';
  selectedPlan: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  statusOptions = ['All', 'Active', 'TrialActive', 'Pending', 'Paused', 'Cancelled', 'Expired', 'PaymentFailed'];
  planNames: string[] = [];

  constructor(
    private subscriptionService: SubscriptionService,
    private commonService: CommonService
  ) {}

  ngOnInit(): void {
    this.loadSubscriptions();
  }

  /**
   * Load all subscriptions (admin view)
   * API: GET /api/Subscriptions/admin/user-subscriptions
   * FIXED: Now calling actual backend endpoint
   */
  loadSubscriptions(): void {
    this.loading = true;
    this.error = null;

    const params: any = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    
    if (this.searchTerm) {
      params.searchTerm = this.searchTerm;
    }
    
    if (this.selectedStatus && this.selectedStatus !== 'All') {
      params.status = [this.selectedStatus];
    }
    
    if (this.selectedPlan) {
      params.planId = [this.selectedPlan];
    }

    // Call actual backend API
    this.commonService.get<SubscriptionDto[]>(
      'Subscriptions/admin/user-subscriptions',
      params
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscriptions = response.data;
          
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }
        } else {
          this.error = response.message || 'Failed to load subscriptions';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'An error occurred loading subscriptions';
        this.loading = false;
      }
    });
  }

  /**
   * Apply filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.loadSubscriptions();
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success',
      'TrialActive': 'bg-info',
      'Pending': 'bg-warning text-dark',
      'Paused': 'bg-secondary',
      'Cancelled': 'bg-danger',
      'Expired': 'bg-dark',
      'PaymentFailed': 'bg-danger'
    };
    return map[status] || 'bg-secondary';
  }

  /**
   * Export subscriptions
   */
  exportSubscriptions(): void {
    console.log('Export subscriptions to CSV');
    // Implementation: Call export API
  }

  /**
   * Get subscription metrics
   */
  getSubscriptionMetrics() {
    return {
      total: this.subscriptions.length,
      active: this.subscriptions.filter(s => s.status === 'Active').length,
      paused: this.subscriptions.filter(s => s.status === 'Paused').length,
      cancelled: this.subscriptions.filter(s => s.status === 'Cancelled').length
    };
  }
}

