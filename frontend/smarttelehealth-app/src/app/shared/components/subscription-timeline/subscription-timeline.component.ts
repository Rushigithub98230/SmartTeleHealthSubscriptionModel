import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionDto } from '../../../core/models';

export interface TimelineEvent {
  id: string;
  type: 'created' | 'activated' | 'paused' | 'resumed' | 'upgraded' | 'downgraded' | 'cancelled' | 'expired' | 'payment_failed' | 'payment_success' | 'trial_started' | 'trial_ends' | 'next_billing';
  title: string;
  description: string;
  date: Date;
  status: 'success' | 'warning' | 'error' | 'info';
  icon: string;
  metadata?: any;
}

/**
 * Subscription Timeline Component
 * Visualizes subscription lifecycle events in a timeline format
 */
@Component({
  selector: 'app-subscription-timeline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription-timeline.component.html',
  styleUrls: ['./subscription-timeline.component.scss']
})
export class SubscriptionTimelineComponent implements OnInit {
  @Input() subscriptions: SubscriptionDto[] = [];
  @Input() showAllSubscriptions: boolean = false;
  @Input() maxEvents: number = 20;

  timelineEvents: TimelineEvent[] = [];
  isLoading = false;

  ngOnInit(): void {
    this.generateTimelineEvents();
  }

  ngOnChanges(): void {
    this.generateTimelineEvents();
  }

  /**
   * Generate timeline events from subscription data
   */
  private generateTimelineEvents(): void {
    this.timelineEvents = [];
    
    const subscriptionsToProcess = this.showAllSubscriptions 
      ? this.subscriptions 
      : this.subscriptions.slice(0, 1); // Only current subscription by default

    subscriptionsToProcess.forEach(subscription => {
      // Add subscription creation event
      this.addTimelineEvent({
        id: `created_${subscription.id}`,
        type: 'created',
        title: 'Subscription Created',
        description: `Subscription created for plan: ${subscription.planName}`,
        date: new Date(subscription.createdDate),
        status: 'info',
        icon: 'bi-plus-circle',
        metadata: { subscriptionId: subscription.id, planName: subscription.planName }
      });

      // Add activation event if subscription is active
      if (subscription.status === 'Active' && subscription.startDate) {
        this.addTimelineEvent({
          id: `activated_${subscription.id}`,
          type: 'activated',
          title: 'Subscription Activated',
          description: `Subscription activated and billing started`,
          date: new Date(subscription.startDate),
          status: 'success',
          icon: 'bi-play-circle',
          metadata: { subscriptionId: subscription.id }
        });
      }

      // Add trial events
      if (subscription.isInTrial) {
        this.addTimelineEvent({
          id: `trial_started_${subscription.id}`,
          type: 'trial_started',
          title: 'Trial Started',
          description: `Free trial period started`,
          date: new Date(subscription.startDate),
          status: 'info',
          icon: 'bi-clock',
          metadata: { subscriptionId: subscription.id, isTrial: true }
        });

        if (subscription.trialEndDate) {
          this.addTimelineEvent({
            id: `trial_ends_${subscription.id}`,
            type: 'trial_ends',
            title: 'Trial Ends',
            description: `Trial period ends on ${new Date(subscription.trialEndDate).toLocaleDateString()}`,
            date: new Date(subscription.trialEndDate),
            status: 'warning',
            icon: 'bi-exclamation-triangle',
            metadata: { subscriptionId: subscription.id, isTrialEnd: true }
          });
        }
      }

      // Add pause events
      if (subscription.status === 'Paused' && subscription.pausedDate) {
        this.addTimelineEvent({
          id: `paused_${subscription.id}`,
          type: 'paused',
          title: 'Subscription Paused',
          description: subscription.pauseReason || 'Subscription paused by user',
          date: new Date(subscription.pausedDate),
          status: 'warning',
          icon: 'bi-pause-circle',
          metadata: { subscriptionId: subscription.id, reason: subscription.pauseReason }
        });
      }

      // Add resume events
      if (subscription.status === 'Active' && subscription.resumedDate) {
        this.addTimelineEvent({
          id: `resumed_${subscription.id}`,
          type: 'resumed',
          title: 'Subscription Resumed',
          description: 'Subscription resumed and billing reactivated',
          date: new Date(subscription.resumedDate),
          status: 'success',
          icon: 'bi-play-circle',
          metadata: { subscriptionId: subscription.id }
        });
      }

      // Add cancellation events
      if (subscription.status === 'Cancelled' && subscription.cancelledDate) {
        this.addTimelineEvent({
          id: `cancelled_${subscription.id}`,
          type: 'cancelled',
          title: 'Subscription Cancelled',
          description: subscription.cancellationReason || 'Subscription cancelled by user',
          date: new Date(subscription.cancelledDate),
          status: 'error',
          icon: 'bi-x-circle',
          metadata: { subscriptionId: subscription.id, reason: subscription.cancellationReason }
        });
      }

      // Add expiry events
      if (subscription.status === 'Expired' && subscription.endDate) {
        this.addTimelineEvent({
          id: `expired_${subscription.id}`,
          type: 'expired',
          title: 'Subscription Expired',
          description: 'Subscription has expired',
          date: new Date(subscription.endDate),
          status: 'error',
          icon: 'bi-calendar-x',
          metadata: { subscriptionId: subscription.id }
        });
      }

      // Add payment failure events
      if (subscription.status === 'PaymentFailed' && subscription.lastPaymentFailedDate) {
        this.addTimelineEvent({
          id: `payment_failed_${subscription.id}`,
          type: 'payment_failed',
          title: 'Payment Failed',
          description: subscription.lastPaymentError || 'Payment processing failed',
          date: new Date(subscription.lastPaymentFailedDate),
          status: 'error',
          icon: 'bi-credit-card',
          metadata: { subscriptionId: subscription.id, error: subscription.lastPaymentError }
        });
      }

      // Add next billing date
      if (subscription.nextBillingDate && subscription.status === 'Active') {
        this.addTimelineEvent({
          id: `next_billing_${subscription.id}`,
          type: 'next_billing',
          title: 'Next Billing Date',
          description: `Next payment due: $${subscription.currentPrice} on ${new Date(subscription.nextBillingDate).toLocaleDateString()}`,
          date: new Date(subscription.nextBillingDate),
          status: 'info',
          icon: 'bi-calendar-check',
          metadata: { subscriptionId: subscription.id, amount: subscription.currentPrice }
        });
      }
    });

    // Sort events by date (newest first)
    this.timelineEvents.sort((a, b) => b.date.getTime() - a.date.getTime());

    // Limit events if specified
    if (this.maxEvents > 0) {
      this.timelineEvents = this.timelineEvents.slice(0, this.maxEvents);
    }
  }

  /**
   * Add timeline event
   */
  private addTimelineEvent(event: TimelineEvent): void {
    this.timelineEvents.push(event);
  }

  /**
   * Get status color class
   */
  getStatusClass(status: string): string {
    switch (status) {
      case 'success': return 'timeline-success';
      case 'warning': return 'timeline-warning';
      case 'error': return 'timeline-error';
      case 'info': return 'timeline-info';
      default: return 'timeline-info';
    }
  }

  /**
   * Format date for display
   */
  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Get relative time (e.g., "2 days ago")
   */
  getRelativeTime(date: Date): string {
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));

    if (diffInDays > 0) {
      return `${diffInDays} day${diffInDays > 1 ? 's' : ''} ago`;
    } else if (diffInHours > 0) {
      return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
    } else if (diffInMinutes > 0) {
      return `${diffInMinutes} minute${diffInMinutes > 1 ? 's' : ''} ago`;
    } else {
      return 'Just now';
    }
  }

  /**
   * Toggle between current subscription and all subscriptions
   */
  toggleSubscriptionView(): void {
    this.showAllSubscriptions = !this.showAllSubscriptions;
    this.generateTimelineEvents();
  }
}
