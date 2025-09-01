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
  currencyId: string;
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
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateSubscriptionPlanDto {
  name: string;
  description?: string;
  price: number;
  billingCycleId: string;
  currencyId: string;
  messagingCount: number;
  includesMedicationDelivery: boolean;
  includesFollowUpCare: boolean;
  deliveryFrequencyDays: number;
  maxPauseDurationDays: number;
  isActive: boolean;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  features?: string;
}

export interface UpdateSubscriptionPlanDto {
  id: string;
  name: string;
  description?: string;
  price: number;
  billingCycleId: string;
  currencyId: string;
  isActive: boolean;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder?: number;
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
  createdAt: Date;
  updatedAt?: Date;
}

export interface MasterCurrency {
  id: string;
  code: string;
  name: string;
  symbol?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface MasterPrivilegeType {
  id: string;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface Privilege {
  id: string;
  name: string;
  description?: string;
  privilegeTypeId: string;
  privilegeTypeName?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}
