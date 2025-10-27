/**
 * Filter Models for Advanced Filtering
 * Comprehensive filter interfaces matching backend DTOs
 */

export interface SubscriptionFilter {
  page: number;
  pageSize: number;
  searchTerm?: string;
  subscriptionId?: string;
  planId?: string;
  planName?: string;
  userId?: number;
  userEmail?: string;
  status?: string;
  statuses?: string[];
  isActive?: boolean;
  isTrial?: boolean;
  isPaused?: boolean;
  isCancelled?: boolean;
  isExpired?: boolean;
  minAmount?: number;
  maxAmount?: number;
  exactAmount?: number;
  currencyId?: string;
  billingCycleId?: string;
  billingCycleName?: string;
  createdDateFrom?: Date;
  createdDateTo?: Date;
  updatedDateFrom?: Date;
  updatedDateTo?: Date;
  startDateFrom?: Date;
  startDateTo?: Date;
  endDateFrom?: Date;
  endDateTo?: Date;
  nextBillingDateFrom?: Date;
  nextBillingDateTo?: Date;
  lastBillingDateFrom?: Date;
  lastBillingDateTo?: Date;
  minTrialDays?: number;
  maxTrialDays?: number;
  minBillingInterval?: number;
  maxBillingInterval?: number;
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  hasStripeIntegration?: boolean;
  subscriptionIds?: string[];
  excludeSubscriptionIds?: string[];
  planIds?: string[];
  userIds?: number[];
  hasActivePayments?: boolean;
  hasFailedPayments?: boolean;
  hasPendingPayments?: boolean;
  hasRefunds?: boolean;
  paymentMethodType?: string;
  paymentStatus?: string;
  sortColumn: string;
  sortOrder: 'asc' | 'desc';
}

export interface BillingFilter {
  page: number;
  pageSize: number;
  searchTerm?: string;
  billingRecordId?: string;
  subscriptionId?: string;
  userId?: number;
  userEmail?: string;
  status?: string;
  statuses?: string[];
  type?: string;
  types?: string[];
  isActive?: boolean;
  isPaid?: boolean;
  isOverdue?: boolean;
  isPending?: boolean;
  isFailed?: boolean;
  isRefunded?: boolean;
  minAmount?: number;
  maxAmount?: number;
  exactAmount?: number;
  currencyId?: string;
  createdDateFrom?: Date;
  createdDateTo?: Date;
  updatedDateFrom?: Date;
  updatedDateTo?: Date;
  dueDateFrom?: Date;
  dueDateTo?: Date;
  paidDateFrom?: Date;
  paidDateTo?: Date;
  processedDateFrom?: Date;
  processedDateTo?: Date;
  paymentMethod?: string;
  paymentStatus?: string;
  paymentMethodType?: string;
  transactionId?: string;
  stripeInvoiceId?: string;
  stripePaymentIntentId?: string;
  stripeChargeId?: string;
  hasStripeIntegration?: boolean;
  hasPaymentMethod?: boolean;
  hasTransactionId?: boolean;
  billingRecordIds?: string[];
  excludeBillingRecordIds?: string[];
  subscriptionIds?: string[];
  userIds?: number[];
  minRetryCount?: number;
  maxRetryCount?: number;
  minFailureCount?: number;
  maxFailureCount?: number;
  failureReason?: string;
  notes?: string;
  description?: string;
  isRecurring?: boolean;
  isOneTime?: boolean;
  isAdjustment?: boolean;
  isRefund?: boolean;
  billingCycle?: string;
  billingCycleId?: string;
  lastRetryDateFrom?: Date;
  lastRetryDateTo?: Date;
  nextRetryDateFrom?: Date;
  nextRetryDateTo?: Date;
  sortColumn: string;
  sortOrder: 'asc' | 'desc';
}

export interface FilterPreset {
  id: string;
  name: string;
  description: string;
  filter: SubscriptionFilter | BillingFilter;
  isDefault?: boolean;
  createdAt: Date;
}

export interface FilterPresets {
  subscriptionPresets: FilterPreset[];
  billingPresets: FilterPreset[];
}

// Default filter presets
export const DEFAULT_SUBSCRIPTION_PRESETS: FilterPreset[] = [
  {
    id: 'all',
    name: 'All Subscriptions',
    description: 'Show all subscriptions',
    filter: {
      page: 1,
      pageSize: 20,
      sortColumn: 'CreatedDate',
      sortOrder: 'desc'
    },
    isDefault: true,
    createdAt: new Date()
  },
  {
    id: 'active',
    name: 'Active Subscriptions',
    description: 'Show only active subscriptions',
    filter: {
      page: 1,
      pageSize: 20,
      isActive: true,
      sortColumn: 'CreatedDate',
      sortOrder: 'desc'
    },
    createdAt: new Date()
  },
  {
    id: 'at-risk',
    name: 'At Risk Subscriptions',
    description: 'Subscriptions with payment issues or low usage',
    filter: {
      page: 1,
      pageSize: 20,
      hasFailedPayments: true,
      sortColumn: 'LastBillingDate',
      sortOrder: 'asc'
    },
    createdAt: new Date()
  },
  {
    id: 'high-value',
    name: 'High Value Subscriptions',
    description: 'Subscriptions with high revenue',
    filter: {
      page: 1,
      pageSize: 20,
      minAmount: 100,
      sortColumn: 'Amount',
      sortOrder: 'desc'
    },
    createdAt: new Date()
  },
  {
    id: 'trials-ending',
    name: 'Trials Ending Soon',
    description: 'Trial subscriptions ending in the next 7 days',
    filter: {
      page: 1,
      pageSize: 20,
      isTrial: true,
      endDateTo: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000),
      sortColumn: 'EndDate',
      sortOrder: 'asc'
    },
    createdAt: new Date()
  },
  {
    id: 'payment-failed',
    name: 'Payment Failed',
    description: 'Subscriptions with failed payments',
    filter: {
      page: 1,
      pageSize: 20,
      hasFailedPayments: true,
      sortColumn: 'LastBillingDate',
      sortOrder: 'desc'
    },
    createdAt: new Date()
  }
];

export const DEFAULT_BILLING_PRESETS: FilterPreset[] = [
  {
    id: 'all',
    name: 'All Billing Records',
    description: 'Show all billing records',
    filter: {
      page: 1,
      pageSize: 20,
      sortColumn: 'CreatedDate',
      sortOrder: 'desc'
    },
    isDefault: true,
    createdAt: new Date()
  },
  {
    id: 'paid',
    name: 'Paid Records',
    description: 'Show only paid billing records',
    filter: {
      page: 1,
      pageSize: 20,
      isPaid: true,
      sortColumn: 'PaidDate',
      sortOrder: 'desc'
    },
    createdAt: new Date()
  },
  {
    id: 'pending',
    name: 'Pending Records',
    description: 'Show pending billing records',
    filter: {
      page: 1,
      pageSize: 20,
      isPending: true,
      sortColumn: 'DueDate',
      sortOrder: 'asc'
    },
    createdAt: new Date()
  },
  {
    id: 'failed',
    name: 'Failed Records',
    description: 'Show failed billing records',
    filter: {
      page: 1,
      pageSize: 20,
      isFailed: true,
      sortColumn: 'LastRetryDate',
      sortOrder: 'desc'
    },
    createdAt: new Date()
  },
  {
    id: 'overdue',
    name: 'Overdue Records',
    description: 'Show overdue billing records',
    filter: {
      page: 1,
      pageSize: 20,
      isOverdue: true,
      sortColumn: 'DueDate',
      sortOrder: 'asc'
    },
    createdAt: new Date()
  }
];
