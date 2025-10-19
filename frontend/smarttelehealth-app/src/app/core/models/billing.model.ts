/**
 * Billing DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/BillingRecordDto.cs
 * and backend/SmartTelehealth.Core/Entities/BillingRecord.cs
 */

/**
 * Billing Type Enum
 */
export enum BillingType {
  Subscription = 'Subscription',
  Overage = 'Overage',
  Consultation = 'Consultation',
  Medication = 'Medication',
  LateFee = 'LateFee',
  Refund = 'Refund',
  Recurring = 'Recurring',
  Upfront = 'Upfront',
  Bundle = 'Bundle',
  Invoice = 'Invoice',
  Cycle = 'Cycle'
}

/**
 * Billing Status Enum
 */
export enum BillingStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded',
  Overdue = 'Overdue',
  Upcoming = 'Upcoming'
}

/**
 * Billing Record DTO (Response)
 */
export interface BillingRecordDto {
  id: string;
  userId: number;
  subscriptionId?: string;
  consultationId?: string;
  medicationDeliveryId?: string;
  
  // Amounts
  amount: number;
  taxAmount: number;
  shippingAmount: number;
  totalAmount: number;
  
  // Type and Status
  type: BillingType;
  status: BillingStatus;
  
  // Dates
  billingDate: Date;
  dueDate?: Date;
  paidAt?: Date;
  processedAt?: Date;
  
  // Payment Information
  invoiceNumber?: string;
  paymentMethod?: string;
  transactionId?: string;
  paymentIntentId?: string;
  
  // Stripe Integration
  stripeInvoiceId?: string;
  stripePaymentIntentId?: string;
  
  // Description
  description?: string;
  failureReason?: string;
  errorMessage?: string;
  
  // Recurring
  isRecurring: boolean;
  nextBillingDate?: Date;
  
  // Accrual
  accruedAmount?: number;
  accrualStartDate?: Date;
  accrualEndDate?: Date;
  
  // Computed Properties
  isPaid: boolean;
  isFailed: boolean;
  isRefunded: boolean;
  isOverdue: boolean;
  
  // User Information (for admin view)
  userName: string;
  userEmail: string;
  subscriptionName?: string;
  
  // Audit
  createdDate: Date;
  updatedDate?: Date;
}

/**
 * Create Billing Record DTO (Request)
 */
export interface CreateBillingRecordDto {
  userId: number;
  subscriptionId?: string;
  amount: number;
  type: BillingType;
  description?: string;
  dueDate?: Date;
  currencyId: string;
}

/**
 * Update Billing Record DTO (Request)
 */
export interface UpdateBillingRecordDto {
  status?: BillingStatus;
  paidAt?: Date;
  failureReason?: string;
}

/**
 * Billing Filter DTO (Request)
 * Matches backend/SmartTelehealth.Core/DTOs/BillingFilterDto.cs
 */
export interface BillingFilterDto {
  page: number;
  pageSize: number;
  searchTerm?: string;
  status?: BillingStatus[];
  type?: BillingType[];
  userId?: number[];
  subscriptionId?: string[];
  startDate?: Date;
  endDate?: Date;
  sortBy?: string;
  sortOrder?: string;
}

/**
 * Process Overage DTO (Request)
 */
export interface ProcessOverageDto {
  subscriptionId: string;
  privilegeName: string;
  amount: number;
}

/**
 * Payment Result DTO (Response)
 */
export interface PaymentResult {
  billingRecordId: string;
  amount: number;
  privilegeUpdated: boolean;
  newUsedValue: number;
  allowedValue: number;
  success: boolean;
  message?: string;
}

/**
 * Billing Adjustment DTO
 */
export interface BillingAdjustmentDto {
  id: string;
  billingRecordId: string;
  adjustmentType: string;        // "Credit" or "Debit"
  amount: number;
  reason: string;
  adjustedBy: number;
  adjustedDate: Date;
}


