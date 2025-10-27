import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SubscriptionPlanService } from '../../../../core/services';
import { SubscriptionPlanDto } from '../../../../core/models';

/**
 * Plan Detail Component (Marketing)
 * Display full subscription plan details
 * 
 * APIs Used:
 * - GET /api/SubscriptionPlans/{planId}
 * 
 * Route: /plans/:id
 * Access: Public
 */
@Component({
  selector: 'app-plan-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './plan-detail.component.html',
  styleUrls: ['./plan-detail.component.scss']
})
export class PlanDetailComponent implements OnInit {
  planId!: string;
  plan: SubscriptionPlanDto | null = null;
  loading = false;
  error: string | null = null;

  // Billing cycle calculations
  billingCycles = [
    { name: 'Monthly', months: 1, discount: 0 },
    { name: 'Quarterly', months: 3, discount: 5 },
    { name: 'Annual', months: 12, discount: 15 }
  ];

  selectedCycleIndex = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private planService: SubscriptionPlanService
  ) {}

  ngOnInit(): void {
    this.planId = this.route.snapshot.params['id'];
    console.log('🎯 [PLAN-DETAIL] Component initialized for plan:', this.planId);
    this.loadPlanDetail();
  }

  /**
   * Load plan details
   * API: GET /api/SubscriptionPlans/{planId}
   */
  loadPlanDetail(): void {
    console.log('📋 [PLAN-DETAIL] Loading plan details for plan:', this.planId);
    this.loading = true;
    this.error = null;

    this.planService.getPlanById(this.planId).subscribe({
      next: (response) => {
        console.log('✅ [PLAN-DETAIL] Plan details loaded:', {
          statusCode: response.statusCode,
          planName: response.data?.name,
          planPrice: response.data?.basePrice || response.data?.price
        });
        
        if (response.statusCode === 200) {
          this.plan = response.data;
        } else {
          this.error = response.message || 'Plan not found';
          console.error('❌ [PLAN-DETAIL] Plan not found:', response.message);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ [PLAN-DETAIL] Error loading plan details:', error);
        this.error = error.message || 'Failed to load plan details';
        this.loading = false;
      }
    });
  }

  /**
   * Get plan price (fixed - no billing cycle selection)
   * Each plan has a fixed billing cycle and price
   */
  getPlanPrice(): number {
    if (!this.plan) return 0;
    return this.plan.basePrice || this.plan.price || 0;
  }

  /**
   * Get billing cycle name from plan
   */
  getBillingCycleName(): string {
    if (!this.plan) return '';
    // Since billingCycle is not available, return based on billingCycleId or default
    return 'Monthly'; // Default billing cycle name
  }

  /**
   * Select billing cycle
   */
  selectCycle(index: number): void {
    this.selectedCycleIndex = index;
  }

  /**
   * Navigate to purchase
   */
  purchasePlan(): void {
    console.log('🛒 [PLAN-DETAIL] Purchase button clicked for plan:', this.planId);
    console.log('🛒 [PLAN-DETAIL] Plan details:', {
      name: this.plan?.name,
      price: this.getPlanPrice(),
      billingCycle: this.getBillingCycleName()
    });
    this.router.navigate(['/web/subscriptions/purchase', this.planId]);
  }
}


