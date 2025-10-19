import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SubscriptionDto } from '../../../core/models';

/**
 * Reusable Subscription Card Component
 * Display subscription summary in card format
 * 
 * Usage:
 * <app-subscription-card 
 *   [subscription]="sub" 
 *   (viewDetails)="onViewDetails($event)">
 * </app-subscription-card>
 */
@Component({
  selector: 'app-subscription-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription-card.component.html',
  styleUrls: ['./subscription-card.component.scss']
})
export class SubscriptionCardComponent {
  @Input() subscription!: SubscriptionDto;
  @Input() showActions = true;
  @Output() viewDetails = new EventEmitter<string>();
  @Output() pauseSubscription = new EventEmitter<string>();
  @Output() cancelSubscription = new EventEmitter<string>();

  getStatusBadgeClass(status: string): string {
    const map: { [key: string]: string } = {
      'Active': 'bg-success',
      'TrialActive': 'bg-info',
      'Pending': 'bg-warning text-dark',
      'Paused': 'bg-secondary',
      'Cancelled': 'bg-danger',
      'Expired': 'bg-dark'
    };
    return map[status] || 'bg-secondary';
  }

  getDaysUntilBilling(nextBillingDate: Date): number {
    const today = new Date();
    const billingDate = new Date(nextBillingDate);
    const diffTime = billingDate.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  onViewDetailsClick(): void {
    this.viewDetails.emit(this.subscription.id);
  }

  onPauseClick(): void {
    this.pauseSubscription.emit(this.subscription.id);
  }

  onCancelClick(): void {
    this.cancelSubscription.emit(this.subscription.id);
  }
}

