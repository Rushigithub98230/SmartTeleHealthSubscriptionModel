export interface SubscriptionDto {
  id: string;
  userId: number;
  userName: string;
  planId: string;
  planName: string;
  planDescription: string;
  status: string;
  customerId?: string;
  currentPeriodStart?: Date;
  currentPeriodEnd?: Date;
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
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  paymentMethodId?: string;
  isActive: boolean;
  isPaused: boolean;
  isCancelled: boolean;
  isExpired: boolean;
}

export interface CreateSubscriptionDto {
  userId: number;
  subscriptionId: string;
  planId: string;
  name?: string;
  description?: string;
  price: number;
  billingCycleId: string;
  currencyId: string;
  isActive: boolean;
  startDate?: Date;
  startImmediately: boolean;
  paymentMethodId?: string;
  autoRenew: boolean;
}

export interface SubscriptionPlanDto {
  id: string;
  name: string;
  description: string;
  shortDescription?: string;
  price: number;
  discountedPrice?: number;
  discountValidUntil?: Date;
  billingCycleId: string;
  billingCycleName?: string;
  currencyId: string;
  currencyName?: string;
  categoryId: string;
  categoryName?: string;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  features?: string;
  terms?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  effectivePrice: number;
  hasActiveDiscount: boolean;
  isCurrentlyAvailable: boolean;
  createdDate: Date;
  updatedDate?: Date;
  // Additional fields for plan configuration
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  maxConcurrentUsers: number;
  gracePeriodDays: number;
  // Stripe integration fields
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  // Privilege information
  privileges?: PlanPrivilegeDto[];
  totalActiveSubscriptions?: number;
}

export interface CreateSubscriptionPlanDto {
  name: string;
  description?: string;
  shortDescription?: string;
  price: number;
  discountedPrice?: number;
  discountValidUntil?: Date;
  billingCycleId: string;
  currencyId: string;
  categoryId: string;
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  maxConcurrentUsers: number;
  gracePeriodDays: number;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  features?: string;
  terms?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  // Stripe integration fields
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  // Privilege configuration
  privileges?: PlanPrivilegeDto[];
}

export interface UpdateSubscriptionPlanDto {
  id: string;
  name: string;
  description?: string;
  shortDescription?: string;
  price: number;
  discountedPrice?: number;
  discountValidUntil?: Date;
  billingCycleId: string;
  currencyId: string;
  categoryId: string;
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  maxConcurrentUsers: number;
  gracePeriodDays: number;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder?: number;
  features?: string;
  terms?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  // Stripe integration fields
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  // Privilege configuration
  privileges?: PlanPrivilegeDto[];
}

export interface ApiResponse<T> {
  data: any;
  message: string;
  statusCode: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Master Data Models
export interface MasterBillingCycle {
  id: string;
  name: string;
  description?: string;
  durationInDays: number;
  sortOrder: number;
  isActive: boolean;
  isDeleted: boolean;
  createdBy?: number;
  createdDate?: Date;
  updatedBy?: number;
  updatedDate?: Date;
  deletedBy?: number;
  deletedDate?: Date;
}

export interface MasterCurrency {
  id: string;
  code: string;
  name: string;
  symbol?: string;
  sortOrder: number;
  isActive: boolean;
  isDeleted: boolean;
  createdBy?: number;
  createdDate?: Date;
  updatedBy?: number;
  updatedDate?: Date;
  deletedBy?: number;
  deletedDate?: Date;
}

export interface MasterPrivilegeType {
  id: string;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  isDeleted: boolean;
  createdBy?: number;
  createdDate?: Date;
  updatedBy?: number;
  updatedDate?: Date;
  deletedBy?: number;
  deletedDate?: Date;
}

export interface Privilege {
  id: string;
  name: string;
  description?: string;
  privilegeTypeId: string;
  privilegeTypeName?: string;
  isActive: boolean;
  isDeleted: boolean;
  createdBy?: number;
  createdDate?: Date;
  updatedBy?: number;
  updatedDate?: Date;
  deletedBy?: number;
  deletedDate?: Date;
}

export interface PlanPrivilegeDto {
  privilegeId: string;
  privilegeName?: string;
  value: number; // -1 for unlimited, 0 for disabled, >0 for limited
  usagePeriodId: string;
  usagePeriodName?: string;
  durationMonths: number;
  description?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  // Time-based limits
  dailyLimit?: number;
  weeklyLimit?: number;
  monthlyLimit?: number;
}

// Additional interfaces for comprehensive subscription management
export interface SubscriptionStatusHistoryDto {
  id: string;
  subscriptionId: string;
  fromStatus: string;
  toStatus: string;
  reason?: string;
  changedBy?: number;
  changedDate: Date;
  notes?: string;
}

export interface BillingRecordDto {
  id: string;
  subscriptionId: string;
  amount: number;
  currency: string;
  billingDate: Date;
  dueDate: Date;
  paidDate?: Date;
  status: string;
  paymentMethodId?: string;
  stripeInvoiceId?: string;
  notes?: string;
}

export interface SubscriptionPaymentDto {
  id: string;
  subscriptionId: string;
  amount: number;
  currency: string;
  paymentDate: Date;
  paymentMethod: string;
  transactionId?: string;
  stripePaymentIntentId?: string;
  status: string;
  failureReason?: string;
}

export interface UserSubscriptionPrivilegeUsageDto {
  id: string;
  subscriptionId: string;
  privilegeId: string;
  privilegeName: string;
  usedCount: number;
  allowedCount: number;
  resetDate: Date;
  lastUsedDate?: Date;
}

export interface CategoryDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  sortOrder: number;
  createdDate: Date;
}

export interface SubscriptionAnalyticsDto {
  totalSubscriptions: number;
  activeSubscriptions: number;
  pausedSubscriptions: number;
  cancelledSubscriptions: number;
  trialSubscriptions: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  averageRevenuePerUser: number;
  churnRate: number;
  conversionRate: number;
  popularPlans: Array<{
    planId: string;
    planName: string;
    subscriptionCount: number;
    revenue: number;
  }>;
  revenueByPlan: Array<{
    planId: string;
    planName: string;
    revenue: number;
    subscriptionCount: number;
  }>;
  subscriptionsByStatus: Array<{
    status: string;
    count: number;
  }>;
}

export interface SubscriptionDetailsDto extends SubscriptionDto {
  planDetails: SubscriptionPlanDto;
  billingHistory: BillingRecordDto[];
  paymentHistory: SubscriptionPaymentDto[];
  statusHistory: SubscriptionStatusHistoryDto[];
  privilegeUsage: UserSubscriptionPrivilegeUsageDto[];
}
