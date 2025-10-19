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
    this.loadPlanDetail();
  }

  /**
   * Load plan details
   * API: GET /api/SubscriptionPlans/{planId}
   */
  loadPlanDetail(): void {
    this.loading = true;
    this.error = null;

    this.planService.getPlanById(this.planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.plan = response.data;
        } else {
          this.error = response.message || 'Plan not found';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load plan details';
        this.loading = false;
      }
    });
  }

  /**
   * Calculate price for selected billing cycle
   */
  getCalculatedPrice(): number {
    if (!this.plan) return 0;
    
    const cycle = this.billingCycles[this.selectedCycleIndex];
    const basePrice = this.plan.price * cycle.months;
    const discount = basePrice * (cycle.discount / 100);
    
    return basePrice - discount;
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
    this.router.navigate(['/web/subscriptions/purchase', this.planId]);
  }
}


