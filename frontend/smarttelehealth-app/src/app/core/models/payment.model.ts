/**
 * Payment DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/PaymentMethodDto.cs
 */

/**
 * Payment Method DTO (Response)
 */
export interface PaymentMethodDto {
  id: string;
  customerId: string;
  type: string;
  card?: CardDto;
  isDefault: boolean;
  createdDate: Date;
}

/**
 * Card DTO (Nested in PaymentMethod)
 */
export interface CardDto {
  brand?: string;                // "visa", "mastercard", etc.
  last4?: string;                // Last 4 digits
  expMonth: number;
  expYear: number;
  fingerprint?: string;
}

/**
 * Add Payment Method DTO (Request)
 */
export interface AddPaymentMethodDto {
  token: string;
  paymentMethodId: string;
  isDefault: boolean;
  setAsDefault: boolean;
  type: string;
  last4: string;
  expiryMonth: number;
  expiryYear: number;
}

/**
 * Process Payment Request DTO
 */
export interface ProcessPaymentRequestDto {
  billingRecordId: string;
  paymentMethodId?: string;
}

/**
 * Validate Payment Method DTO
 */
export interface ValidatePaymentMethodDto {
  paymentMethodId: string;
}

/**
 * Payment Analytics DTO (Response - Admin)
 */
export interface PaymentAnalyticsDto {
  totalPayments: number;
  successfulPayments: number;
  failedPayments: number;
  totalAmount: number;
  averageAmount: number;
  successRate: number;
  period: string;
}

/**
 * Payment History DTO (Response)
 */
export interface PaymentHistoryDto {
  id: string;
  amount: number;
  status: string;
  type: string;
  paymentDate: Date;
  description: string;
  invoiceNumber?: string;
}


