import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SubscriptionService, CommonService } from '../../../../core/services';
import { SubscriptionDto } from '../../../../core/models';
import { SubscriptionFilter, DEFAULT_SUBSCRIPTION_PRESETS } from '../../../../core/models/filter.model';
import { AdvancedFilterComponent } from '../../../../shared/components/advanced-filter/advanced-filter.component';
import { BulkActionsComponent, BulkActionRequest } from '../../../../shared/components/bulk-actions/bulk-actions.component';
import { Subscription } from 'rxjs';

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
  imports: [CommonModule, RouterLink, FormsModule, AdvancedFilterComponent, BulkActionsComponent],
  templateUrl: './subscription-list.component.html',
  styleUrls: ['./subscription-list.component.scss']
})
export class AdminSubscriptionListComponent implements OnInit, OnDestroy {
  Math = Math;  // Expose Math to template
  subscriptions: SubscriptionDto[] = [];
  loading = false;
  error: string | null = null;

  // Advanced Filtering
  currentFilter: SubscriptionFilter = {
    page: 1,
    pageSize: 20,
    sortColumn: 'CreatedDate',
    sortOrder: 'desc'
  };
  filterPresets = DEFAULT_SUBSCRIPTION_PRESETS;
  isFilterExpanded = false;

  // Bulk Actions
  selectedSubscriptions: SubscriptionDto[] = [];
  isBulkActionLoading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  statusOptions = ['All', 'Active', 'TrialActive', 'Pending', 'Paused', 'Cancelled', 'Expired', 'PaymentFailed'];
  planNames: string[] = [];

  private subscriptions$: Subscription[] = [];

  constructor(
    private subscriptionService: SubscriptionService,
    private commonService: CommonService
  ) {}

  ngOnInit(): void {
    this.loadSubscriptions();
    this.loadPlanNames();
  }

  ngOnDestroy(): void {
    this.subscriptions$.forEach(sub => sub.unsubscribe());
  }

  /**
   * Load subscriptions with advanced filtering
   */
  loadSubscriptions(): void {
    this.loading = true;
    this.error = null;

    // Call backend API with comprehensive filter
    this.commonService.get<SubscriptionDto[]>(
      'Subscriptions/admin/user-subscriptions',
      this.currentFilter
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscriptions = response.data;
          
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
            this.currentPage = response.meta.currentPage;
          }
        } else {
          this.error = response.message || 'Failed to load subscriptions';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        this.error = 'Failed to load subscriptions. Please try again.';
        this.loading = false;
      }
    });
  }

  /**
   * Load plan names for filter dropdown
   */
  loadPlanNames(): void {
    this.commonService.get<string[]>('SubscriptionPlans/names').subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.planNames = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading plan names:', error);
      }
    });
  }

  /**
   * Handle filter changes from advanced filter component
   */
  onFilterChange(filter: SubscriptionFilter): void {
    this.currentFilter = { ...filter };
    this.currentPage = 1;
    this.currentFilter.page = 1;
    this.loadSubscriptions();
  }

  /**
   * Handle filter preset changes
   */
  onPresetChange(presetId: string): void {
    const preset = this.filterPresets.find(p => p.id === presetId);
    if (preset) {
      this.currentFilter = { ...preset.filter };
      this.loadSubscriptions();
    }
  }

  /**
   * Handle filter expansion toggle
   */
  onFilterExpandToggle(expanded: boolean): void {
    this.isFilterExpanded = expanded;
  }

  /**
   * Handle bulk action requests
   */
  onBulkAction(request: BulkActionRequest): void {
    this.isBulkActionLoading = true;
    
    // Call bulk action API
    this.commonService.post('admin/subscriptions/bulk-action', request).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Reload subscriptions to reflect changes
          this.loadSubscriptions();
          // Clear selection
          this.selectedSubscriptions = [];
        } else {
          this.error = response.message || 'Bulk action failed';
        }
        this.isBulkActionLoading = false;
      },
      error: (error) => {
        console.error('Bulk action error:', error);
        this.error = 'Bulk action failed. Please try again.';
        this.isBulkActionLoading = false;
      }
    });
  }

  /**
   * Handle select all toggle
   */
  onSelectAll(selectAll: boolean): void {
    if (selectAll) {
      this.selectedSubscriptions = [...this.subscriptions];
    } else {
      this.selectedSubscriptions = [];
    }
  }

  /**
   * Handle individual subscription selection
   */
  onSubscriptionSelect(subscription: SubscriptionDto, selected: boolean): void {
    if (selected) {
      if (!this.selectedSubscriptions.find(s => s.id === subscription.id)) {
        this.selectedSubscriptions.push(subscription);
      }
    } else {
      this.selectedSubscriptions = this.selectedSubscriptions.filter(s => s.id !== subscription.id);
    }
  }

  /**
   * Clear selection
   */
  onClearSelection(): void {
    this.selectedSubscriptions = [];
  }

  /**
   * Check if subscription is selected
   */
  isSubscriptionSelected(subscription: SubscriptionDto): boolean {
    return this.selectedSubscriptions.some(s => s.id === subscription.id);
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.currentFilter.page = page;
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

