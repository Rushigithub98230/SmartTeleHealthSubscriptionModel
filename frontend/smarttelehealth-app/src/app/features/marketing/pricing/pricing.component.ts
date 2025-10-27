import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionPlanService } from '../../../core/services';
import { SubscriptionPlanDto } from '../../../core/models';

@Component({
  selector: 'app-pricing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pricing.component.html',
  styleUrls: ['./pricing.component.scss']
})
export class PricingComponent implements OnInit {
  // Component properties
  plans: SubscriptionPlanDto[] = [];
  loading = false;
  
  // Calculator settings
  selectedBillingCycle = 'monthly'; // 'monthly', 'quarterly', 'annual'
  
  billingCycles = [
    { id: 'monthly', name: 'Monthly', months: 1, label: '/month' },
    { id: 'quarterly', name: 'Quarterly', months: 3, label: '/3 months' },
    { id: 'annual', name: 'Annual', months: 12, label: '/year' }
  ];

  constructor(private planService: SubscriptionPlanService) {}

  ngOnInit(): void {
    this.loadPlans();
  }

  /**
   * Get selected billing cycle label
   */
  get selectedCycleLabel(): string {
    return this.billingCycles.find(c => c.id === this.selectedBillingCycle)?.label || '';
  }

  /**
   * Load active plans
   */
  loadPlans(): void {
    this.loading = true;
    this.planService.getActivePlans(1, 10).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.plans = response.data;
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  /**
   * Calculate price for selected billing cycle
   */
  calculatePrice(plan: SubscriptionPlanDto): number {
    const cycle = this.billingCycles.find(c => c.id === this.selectedBillingCycle);
    if (!cycle) return plan.basePrice || plan.price || 0;

    const basePrice = plan.basePrice || plan.price || 0;
    let discount = 0;

    // NEW ARCHITECTURE: Each plan has a single billing discount for its specific billing cycle
    // The plan is already tied to a specific billing cycle, so use its billingDiscountPercentage
    if (this.selectedBillingCycle === 'quarterly' || this.selectedBillingCycle === 'annual') {
      discount = plan.billingDiscountPercentage || plan.billingDiscount || 0;
    }

    const discountedPrice = basePrice * (1 - discount / 100);
    return discountedPrice * cycle.months;
  }

  /**
   * Get savings amount
   */
  getSavings(plan: SubscriptionPlanDto): number {
    const cycle = this.billingCycles.find(c => c.id === this.selectedBillingCycle);
    if (!cycle) return 0;

    const basePrice = plan.basePrice || plan.price || 0;
    const regularPrice = basePrice * cycle.months;
    const discountedPrice = this.calculatePrice(plan);
    
    return regularPrice - discountedPrice;
  }
}