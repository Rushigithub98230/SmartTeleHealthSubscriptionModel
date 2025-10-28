import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SubscriptionPlanService, CategoryService } from '../../../../core/services';
import { SubscriptionPlanDto, CategoryDto } from '../../../../core/models';

/**
 * Admin Plan List Component
 * Manage all subscription plans (admin view includes inactive plans)
 * 
 * APIs Used:
 * - GET /api/SubscriptionPlans/admin
 * - POST /api/SubscriptionPlans/filter
 * - POST /api/SubscriptionPlans/{id}/deactivate
 * 
 * Route: /webadmin/plans
 * Access: Admin only
 */
@Component({
  selector: 'app-plan-list-admin',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './plan-list.component.html',
  styleUrls: ['./plan-list.component.scss']
})
export class PlanListAdminComponent implements OnInit {
  Math = Math;  // Expose Math to template
  plans: SubscriptionPlanDto[] = [];
  categories: CategoryDto[] = [];
  loading = false;
  actionLoading = false;
  error: string | null = null;

  // Filters
  searchTerm = '';
  selectedCategoryId: string | null = null;
  selectedStatus: string = 'all'; // 'all', 'active', 'inactive'

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  constructor(
    private planService: SubscriptionPlanService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadPlans();
  }

  /**
   * Load all categories
   * API: GET /api/Categories
   */
  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.categories = response.data || [];
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.categories = [];
      }
    });
  }

  /**
   * Load plans (admin view includes inactive)
   * API: GET /api/SubscriptionPlans/admin
   */
  loadPlans(): void {
    this.loading = true;
    this.error = null;

    this.planService.getAllPlansAdmin(this.currentPage, this.pageSize).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Ensure planPrivileges is initialized for each plan
          const plans = response.data || [];
          plans.forEach(plan => {
            if (!plan.planPrivileges) {
              plan.planPrivileges = [];
            }
          });
          this.plans = this.filterPlans(plans);
          
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load plans';
        this.loading = false;
      }
    });
  }

  /**
   * Filter plans client-side (or use advanced filter API)
   */
  filterPlans(plans: SubscriptionPlanDto[]): SubscriptionPlanDto[] {
    let filtered = plans;

    // Search filter
    if (this.searchTerm) {
      filtered = filtered.filter(p => 
        p.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        p.description?.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }

    // Category filter
    if (this.selectedCategoryId) {
      filtered = filtered.filter(p => p.categoryId === this.selectedCategoryId);
    }

    // Status filter
    if (this.selectedStatus === 'active') {
      filtered = filtered.filter(p => p.isActive);
    } else if (this.selectedStatus === 'inactive') {
      filtered = filtered.filter(p => !p.isActive);
    }

    return filtered;
  }

  /**
   * Apply filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadPlans();
  }

  /**
   * Deactivate plan
   * API: POST /api/SubscriptionPlans/{id}/deactivate
   */
  deactivatePlan(planId: string): void {
    if (!confirm('Are you sure you want to deactivate this plan? Active subscriptions will not be affected.')) {
      return;
    }

    this.actionLoading = true;

    this.planService.deactivatePlan(planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.loadPlans(); // Reload to reflect changes
        } else {
          alert(response.message || 'Failed to deactivate plan');
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'An error occurred');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Reactivate plan
   * API: POST /api/SubscriptionPlans/{id}/reactivate
   */
  reactivatePlan(planId: string): void {
    if (!confirm('Are you sure you want to reactivate this plan?')) {
      return;
    }

    this.actionLoading = true;

    this.planService.reactivatePlan(planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.loadPlans(); // Reload to reflect changes
        } else {
          alert(response.message || 'Failed to reactivate plan');
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'An error occurred');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.loadPlans();
  }

  /**
   * Get category name
   */
  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category?.name || 'Unknown';
  }
}

