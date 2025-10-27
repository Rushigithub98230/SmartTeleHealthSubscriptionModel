import { Injectable } from '@angular/core';
import { SubscriptionPlanDto } from '../models/subscription-plan.model';

@Injectable({
  providedIn: 'root'
})
export class PlanPricingService {

  /**
   * Gets the effective price for a subscription plan, considering discounts and validity periods.
   * Returns the discounted price if valid, otherwise returns the base price.
   */
  getEffectivePrice(plan: SubscriptionPlanDto): number {
    try {
      // Use the new pricing architecture
      let price = plan.basePrice || plan.price || 0;
      
      // Apply promotional discount if valid
      if (plan.discountPercentage && plan.discountPercentage > 0 &&
          (!plan.discountValidUntil || new Date() <= new Date(plan.discountValidUntil))) {
        price = price * (1 - (plan.discountPercentage / 100));
      }
      
      // Apply billing discount
      if (plan.billingDiscountPercentage && plan.billingDiscountPercentage > 0) {
        price = price * (1 - (plan.billingDiscountPercentage / 100));
      }
      
      return Math.max(price, 0);
    } catch (error) {
      console.error('Error calculating effective price for plan', plan.name, error);
      return plan.basePrice || plan.price || 0;
    }
  }

  /**
   * Checks if a plan has an active discount
   */
  hasActiveDiscount(plan: SubscriptionPlanDto): boolean {
    try {
      // Check promotional discount
      const hasPromotionalDiscount = !!(plan.discountPercentage && plan.discountPercentage > 0 &&
        (!plan.discountValidUntil || new Date() <= new Date(plan.discountValidUntil)));
      
      // Check billing discount
      const hasBillingDiscount = !!(plan.billingDiscountPercentage && plan.billingDiscountPercentage > 0);
      
      return hasPromotionalDiscount || hasBillingDiscount;
    } catch (error) {
      console.error('Error checking active discount for plan', plan.name, error);
      return false;
    }
  }

  /**
   * Calculates the discount percentage
   */
  getDiscountPercentage(plan: SubscriptionPlanDto): number {
    try {
      if (!this.hasActiveDiscount(plan)) {
        return 0;
      }
      
      // Return the highest discount percentage
      const promotionalDiscount = plan.discountPercentage || 0;
      const billingDiscount = plan.billingDiscountPercentage || 0;
      
      return Math.max(promotionalDiscount, billingDiscount);
    } catch (error) {
      console.error('Error calculating discount percentage for plan', plan.name, error);
      return 0;
    }
  }

  /**
   * Calculates the discount amount in currency
   */
  getDiscountAmount(plan: SubscriptionPlanDto): number {
    try {
      if (!this.hasActiveDiscount(plan)) {
        return 0;
      }
      
      const basePrice = plan.basePrice || plan.price || 0;
      const effectivePrice = this.getEffectivePrice(plan);
      
      return basePrice - effectivePrice;
    } catch (error) {
      console.error('Error calculating discount amount for plan', plan.name, error);
      return 0;
    }
  }

  /**
   * Gets the number of days until discount expires
   */
  getDaysUntilDiscountExpires(plan: SubscriptionPlanDto): number | null {
    try {
      if (!this.hasActiveDiscount(plan)) {
        return null;
      }

      const now = new Date();
      const expiryDate = new Date(plan.discountValidUntil!);
      const diffTime = expiryDate.getTime() - now.getTime();
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      
      return diffDays > 0 ? diffDays : 0;
    } catch (error) {
      console.error('Error calculating days until discount expires for plan', plan.name, error);
      return null;
    }
  }

  /**
   * Formats price with currency symbol
   */
  formatPrice(price: number, currency: string = 'USD'): string {
    try {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
      }).format(price);
    } catch (error) {
      console.error('Error formatting price', price, error);
      return `$${price.toFixed(2)}`;
    }
  }

  /**
   * Gets display text for discount status
   */
  getDiscountStatusText(plan: SubscriptionPlanDto): string {
    try {
      if (!this.hasActiveDiscount(plan)) {
        return '';
      }

      const daysLeft = this.getDaysUntilDiscountExpires(plan);
      if (daysLeft === null) {
        return 'Limited Time Offer';
      }

      if (daysLeft === 0) {
        return 'Expires Today';
      } else if (daysLeft === 1) {
        return 'Expires Tomorrow';
      } else if (daysLeft <= 7) {
        return `Expires in ${daysLeft} days`;
      } else {
        return 'Limited Time Offer';
      }
    } catch (error) {
      console.error('Error getting discount status text for plan', plan.name, error);
      return '';
    }
  }
}

