import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionPlanDto } from '../../../core/models/subscription-plan.model';
import { PlanPricingService } from '../../../core/services/plan-pricing.service';

@Component({
  selector: 'app-plan-card',
  templateUrl: './plan-card.component.html',
  styleUrls: ['./plan-card.component.scss'],
  imports: [CommonModule],
  standalone: true
})
export class PlanCardComponent implements OnInit {
  @Input() plan!: SubscriptionPlanDto;
  @Input() showDiscount: boolean = true;
  @Input() showFeatures: boolean = true;
  @Input() showTrial: boolean = true;

  effectivePrice: number = 0;
  hasActiveDiscount: boolean = false;
  discountPercentage: number = 0;
  discountAmount: number = 0;
  discountStatusText: string = '';
  daysUntilExpiry: number | null = null;

  constructor(private planPricingService: PlanPricingService) {}

  ngOnInit(): void {
    this.calculatePricing();
  }

  private calculatePricing(): void {
    this.effectivePrice = this.planPricingService.getEffectivePrice(this.plan);
    this.hasActiveDiscount = this.planPricingService.hasActiveDiscount(this.plan);
    this.discountPercentage = this.planPricingService.getDiscountPercentage(this.plan);
    this.discountAmount = this.planPricingService.getDiscountAmount(this.plan);
    this.discountStatusText = this.planPricingService.getDiscountStatusText(this.plan);
    this.daysUntilExpiry = this.planPricingService.getDaysUntilDiscountExpires(this.plan);
  }

  getFormattedPrice(price: number): string {
    return this.planPricingService.formatPrice(price);
  }

  getFormattedDiscountAmount(): string {
    return this.planPricingService.formatPrice(this.discountAmount);
  }

  getFormattedBasePrice(): string {
    return this.planPricingService.formatPrice(this.plan.basePrice || this.plan.price || 0);
  }

  getFormattedEffectivePrice(): string {
    return this.planPricingService.formatPrice(this.effectivePrice);
  }

  getBillingCycleText(): string {
    // This would typically come from a billing cycle service
    // For now, return a default based on common patterns
    return 'month';
  }

  selectPlan(): void {
    // Emit event or navigate to subscription creation
    console.log('Selected plan:', this.plan.name);
  }
}
