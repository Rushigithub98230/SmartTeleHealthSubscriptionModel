import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SubscriptionPlanService, CategoryService, AuthService } from '../../../../core/services';
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
    private categoryService: CategoryService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    console.log('🎯 [PLAN-LIST] Component initialized');
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
   * Handle plan purchase - redirect to appropriate flow
   */
  purchasePlan(planId: string): void {
    console.log('🛒 [PLAN-LIST] Purchase button clicked for plan:', planId);
    console.log('🔐 [PLAN-LIST] User authenticated:', this.authService.isAuthenticated());
    
    if (this.authService.isAuthenticated()) {
      // User is authenticated - go directly to purchase flow
      console.log('✅ [PLAN-LIST] User authenticated - redirecting to purchase flow');
      this.router.navigate(['/web/subscriptions/purchase', planId]);
    } else {
      // User is not authenticated - redirect to registration
      console.log('📝 [PLAN-LIST] User not authenticated - redirecting to registration');
      this.router.navigate(['/web/register'], { 
        queryParams: { planId: planId, redirect: 'purchase' } 
      });
    }
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
    console.log('📋 [PLAN-LIST] Loading plans with filters:', {
      page: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm,
      categoryId: this.selectedCategoryId
    });
    
    this.loading = true;
    
    this.planService.getActivePlans(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedCategoryId || undefined
    ).subscribe({
      next: (response) => {
        console.log('✅ [PLAN-LIST] Plans loaded successfully:', {
          statusCode: response.statusCode,
          planCount: response.data?.length || 0,
          totalRecords: response.meta?.totalRecords || 0
        });
        
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
        console.error('❌ [PLAN-LIST] Error loading plans:', error);
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
  getSavings(plan: SubscriptionPlanDto): number {
    // NEW ARCHITECTURE: Each plan has a single billing discount for its specific billing cycle
    const billingDiscount = plan.billingDiscountPercentage || plan.billingDiscount;
    if (!billingDiscount || billingDiscount <= 0) return 0;
    
    const basePrice = plan.basePrice || plan.price || 0;
    const discountAmount = basePrice * (billingDiscount / 100);
    return discountAmount;
  }
}

