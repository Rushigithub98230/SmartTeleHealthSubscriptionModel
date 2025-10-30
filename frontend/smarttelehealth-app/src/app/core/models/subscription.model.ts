/**
 * Subscription DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/SubscriptionDto.cs
 * and backend/SmartTelehealth.Core/Entities/Subscription.cs
 */

/**
 * Subscription Status Enum
 * Matches Subscription.SubscriptionStatuses from backend
 */
export enum SubscriptionStatus {
  Pending = 'Pending',
  Active = 'Active',
  Paused = 'Paused',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  PaymentFailed = 'PaymentFailed',
  TrialActive = 'TrialActive',
  TrialExpired = 'TrialExpired',
  Suspended = 'Suspended'
}

/**
 * Subscription DTO (Response)
 */
export interface SubscriptionDto {
  id: string;
  userId: number;
  userName: string;
  planId: string;
  planName: string;
  planDescription: string;
  status: SubscriptionStatus;
  statusReason?: string;
  currentPrice: number;
  autoRenew: boolean;
  notes?: string;
  startDate: Date;
  endDate?: Date;
  nextBillingDate: Date;
  pausedDate?: Date;
  resumedDate?: Date;
  cancelledDate?: Date;
  expirationDate?: Date;
  cancellationReason?: string;
  pauseReason?: string;
  
  // Stripe Integration
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  paymentMethodId?: string;
  lastPaymentDate?: Date;
  lastPaymentFailedDate?: Date;
  lastPaymentError?: string;
  failedPaymentAttempts: number;
  
  // Trial Properties
  isTrialSubscription: boolean;
  trialStartDate?: Date;
  trialEndDate?: Date;
  trialDurationInDays: number;
  
  // Usage Tracking
  lastUsedDate?: Date;
  totalUsageCount: number;
  
  // Billing Cycle
  billingCycleId: string;
  currencyId: string;
  
  // Scheduled Plan Changes (No Proration)
  pendingPlanChangeId?: string;
  pendingPlanName?: string;
  planChangeEffectiveDate?: Date;
  pendingChangeType?: 'Upgrade' | 'Downgrade';
  
  // Computed Properties (from backend)
  isActive: boolean;
  isPaused: boolean;
  isCancelled: boolean;
  isExpired: boolean;
  hasPaymentIssues: boolean;
  isInTrial: boolean;
  daysUntilNextBilling: number;
  isNearExpiration: boolean;
  
  // Business Logic Properties
  canPause: boolean;
  canResume: boolean;
  canCancel: boolean;
  canRenew: boolean;
  
  // Navigation Properties
  statusHistory: SubscriptionStatusHistoryDto[];
  payments: SubscriptionPaymentDto[];
  
  // Audit
  createdDate: Date;
  updatedDate: Date;
}

/**
 * Create Subscription DTO (Request)
 * Updated to match backend DTO structure
 */
export interface CreateSubscriptionDto {
  userId: number;
  subscriptionId?: string;
  planId: string;                // GUID
  name?: string;
  description?: string;
  price: number;
  // REMOVED: billingCycleId - comes from plan (fixed billing cycle)
  currencyId: string;            // GUID
  isActive: boolean;
  startDate?: Date;
  startImmediately: boolean;     // Default: true
  paymentMethodId?: string;      // Stripe payment method ID
  autoRenew: boolean;            // Default: true
}

/**
 * Upgrade Subscription DTO (Request)
 */
export interface UpgradeSubscriptionDto {
  subscriptionId: string;
  userId: number;
  newPlanId: string;             // Required
  paymentMethodId: string;       // Required
  prorate: boolean;              // Default: true
}

/**
 * Downgrade Subscription DTO (Request)
 */
export interface DowngradeSubscriptionDto {
  subscriptionId: string;
  userId: number;
  newPlanId: string;
  paymentMethodId: string;
  prorate: boolean;
}

/**
 * Pause Subscription DTO (Request)
 */
export interface PauseSubscriptionDto {
  reason: string;                // Required
  resumeDate?: Date;
  pauseDate?: Date;
}

/**
 * Subscription Status History DTO
 */
export interface SubscriptionStatusHistoryDto {
  id: string;
  subscriptionId: string;
  fromStatus: string;
  toStatus: string;
  reason?: string;
  changedByUserId?: string;
  changedAt: Date;
  metadata?: string;
  createdDate?: Date;
  updatedDate?: Date;
}

/**
 * Subscription Payment DTO
 */
export interface SubscriptionPaymentDto {
  id: string;
  subscriptionId: string;
  amount: number;
  taxAmount: number;
  netAmount: number;
  description: string;
  status: string;
  type: string;
  failureReason?: string;
  dueDate: Date;
  paidAt?: Date;
  failedAt?: Date;
  billingPeriodStart: Date;
  billingPeriodEnd: Date;
  stripePaymentIntentId?: string;
  stripeInvoiceId?: string;
  receiptUrl?: string;
  paymentIntentId?: string;
  invoiceId?: string;
  attemptCount: number;
  nextRetryAt?: Date;
  refundedAmount: number;
  refunds: PaymentRefundDto[];
  isPaid: boolean;
  isFailed: boolean;
  isRefunded: boolean;
  isOverdue: boolean;
  remainingAmount: number;
  createdDate: Date;
  updatedDate: Date;
}

/**
 * Payment Refund DTO
 */
export interface PaymentRefundDto {
  id: string;
  subscriptionPaymentId: string;
  amount: number;
  reason: string;
  stripeRefundId?: string;
  refundedAt: Date;
  processedByUserId?: string;
}


