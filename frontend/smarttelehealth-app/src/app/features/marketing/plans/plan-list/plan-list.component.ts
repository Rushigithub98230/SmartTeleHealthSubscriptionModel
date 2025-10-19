import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SubscriptionPlanService, CategoryService } from '../../../../core/services';
import { SubscriptionPlanDto, CategoryDto } from '../../../../core/models';

/**
 * Plan List Component
 * Displays all active subscription plans with filtering
 * 
 * APIs Used:
 * - GET /api/SubscriptionPlans/active
 * - GET /api/Categories
 */
@Component({
  selector: 'app-plan-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './plan-list.component.html',
  styleUrls: ['./plan-list.component.scss']
})
export class PlanListComponent implements OnInit {
  plans: SubscriptionPlanDto[] = [];
  categories: CategoryDto[] = [];
  loading = false;
  
  // Filters
  selectedCategoryId: string | null = null;
  searchTerm = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 9;
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
   * Get privilege name by ID
   */
  getPrivilegeName(privilegeId: string): string {
    // Since we don't have access to privilege details in marketing view,
    // return a generic name
    return 'Privilege';
  }

  /**
   * Load all categories for filtering
   * API: GET /api/Categories
   */
  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.categories = response.data;
        }
      },
      error: (error) => console.error('Error loading categories:', error)
    });
  }

  /**
   * Load plans with current filters
   * API: GET /api/SubscriptionPlans/active
   */
  loadPlans(): void {
    this.loading = true;
    
    this.planService.getActivePlans(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedCategoryId || undefined
    ).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.plans = response.data;
          
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Filter by category
   */
  filterByCategory(categoryId: string | null): void {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.loadPlans();
  }

  /**
   * Search plans
   */
  search(): void {
    this.currentPage = 1;
    this.loadPlans();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.loadPlans();
  }

  /**
   * Calculate savings for annual billing
   */
  getAnnualSavings(plan: SubscriptionPlanDto): number {
    const monthlyTotal = plan.price * 12;
    const annualPrice = monthlyTotal * (1 - plan.annualBillingDiscount / 100);
    return monthlyTotal - annualPrice;
  }
}

