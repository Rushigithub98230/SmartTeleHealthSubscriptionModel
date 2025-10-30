/**
 * Invoice Models and DTOs
 * Complete type definitions for invoice management
 * 
 * Enhancement #4-6: Invoice Components Implementation
 * Created: October 28, 2025
 */

/**
 * Invoice Data Transfer Object
 * Represents an invoice in the system with complete details
 */
export interface InvoiceDto {
  // Core Identification
  invoiceNumber: string;
  billingRecordId: string;
  userId: number;
  userName?: string;
  userEmail?: string;
  
  // Amounts and Currency
  amount: number;
  taxAmount: number;
  totalAmount: number;
  currency: string;
  
  // Important Dates
  invoiceDate: Date;
  dueDate: Date;
  paidAt?: Date;
  createdDate: Date;
  updatedDate?: Date;
  
  // Status Information
  status: InvoiceStatus | string;
  paymentStatus: string;
  
  // Description and Classification
  description: string;
  type: string;
  notes?: string;
  
  // Stripe Integration
  stripeInvoiceId?: string;
  stripeInvoiceUrl?: string;
  stripePaymentIntentId?: string;
  stripeCustomerId?: string;
  
  // Subscription References
  subscriptionId?: string;
  subscriptionPlanName?: string;
  subscriptionPlanId?: string;
  billingCycleName?: string;
  
  // Invoice Content
  invoiceContent?: string;
  
  // Metadata
  generatedAt?: Date;
  generatedBy?: number;
  sentAt?: Date;
  sentTo?: string;
}

/**
 * Invoice Status Enumeration
 * All possible states of an invoice
 */
export enum InvoiceStatus {
  Draft = 'Draft',
  Sent = 'Sent',
  Paid = 'Paid',
  Pending = 'Pending',
  Overdue = 'Overdue',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded',
  Failed = 'Failed'
}

/**
 * Invoice Statistics DTO
 * Used for dashboard analytics and reporting
 */
export interface InvoiceStatsDto {
  // Counts
  totalSent: number;
  totalPaid: number;
  totalPending: number;
  totalOverdue: number;
  totalFailed?: number;
  totalCancelled?: number;
  
  // Amounts
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  overdueAmount?: number;
  
  // Additional Metrics
  averageInvoiceAmount?: number;
  paidPercentage?: number;
  overduePercentage?: number;
}

/**
 * Invoice Filter DTO
 * Used for filtering and searching invoices
 */
export interface InvoiceFilterDto {
  // Pagination
  page: number;
  pageSize: number;
  
  // Filters
  status?: InvoiceStatus | string;
  startDate?: Date;
  endDate?: Date;
  searchTerm?: string;
  userId?: number;
  subscriptionId?: string;
  
  // Sorting
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Invoice Line Item
 * Represents individual items/charges on an invoice
 */
export interface InvoiceLineItem {
  id?: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  taxRate?: number;
  taxAmount?: number;
  totalAmount: number;
}

/**
 * Bulk Send Invoices Request DTO
 * Used for sending multiple invoices at once
 */
export interface BulkSendInvoicesRequestDto {
  invoiceNumbers: string[];
  delayBetweenEmailsMs?: number;
  continueOnError?: boolean;
}

/**
 * Bulk Send Invoices Response DTO
 * Response from bulk send operation
 */
export interface BulkSendInvoicesResponseDto {
  totalProcessed: number;
  successCount: number;
  failureCount: number;
  results: BulkSendResult[];
}

/**
 * Individual result from bulk send operation
 */
export interface BulkSendResult {
  invoiceNumber: string;
  success: boolean;
  message: string;
  email?: string;
}

/**
 * Invoice Download Response
 * Response when downloading an invoice
 */
export interface InvoiceDownloadResponse {
  fileContent: string; // Base64 encoded
  fileName: string;
  contentType: string;
  fileSize: number;
}

/**
 * Invoice Generation Response
 * Response when generating a new invoice
 */
export interface InvoiceGenerationResponse {
  invoiceNumber: string;
  billingRecordId: string;
  userId: number;
  amount: number;
  generatedAt: Date;
  generatedBy: number;
}

/**
 * Helper function to check if invoice is paid
 */
export function isInvoicePaid(invoice: InvoiceDto): boolean {
  return invoice.status === InvoiceStatus.Paid || 
         invoice.status === 'Paid' ||
         invoice.paymentStatus?.toLowerCase() === 'paid';
}

/**
 * Helper function to check if invoice is overdue
 */
export function isInvoiceOverdue(invoice: InvoiceDto): boolean {
  if (isInvoicePaid(invoice)) return false;
  
  const dueDate = new Date(invoice.dueDate);
  const now = new Date();
  
  return dueDate < now;
}

/**
 * Helper function to get status badge class for styling
 */
export function getInvoiceStatusBadgeClass(status: InvoiceStatus | string): string {
  const statusMap: { [key: string]: string } = {
    'Paid': 'bg-success',
    'Pending': 'bg-warning text-dark',
    'Overdue': 'bg-danger',
    'Failed': 'bg-danger',
    'Refunded': 'bg-secondary',
    'Cancelled': 'bg-secondary',
    'Draft': 'bg-light text-dark',
    'Sent': 'bg-info'
  };
  
  return statusMap[status] || 'bg-secondary';
}

/**
 * Helper function to format invoice number for display
 */
export function formatInvoiceNumber(invoiceNumber: string): string {
  return invoiceNumber || 'N/A';
}

/**
 * Helper function to calculate days until due
 */
export function getDaysUntilDue(invoice: InvoiceDto): number {
  if (isInvoicePaid(invoice)) return 0;
  
  const dueDate = new Date(invoice.dueDate);
  const now = new Date();
  
  const diffTime = dueDate.getTime() - now.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  
  return diffDays;
}

/**
 * Helper function to get status display text
 */
export function getInvoiceStatusDisplay(invoice: InvoiceDto): string {
  if (isInvoiceOverdue(invoice)) {
    const daysOverdue = Math.abs(getDaysUntilDue(invoice));
    return `Overdue (${daysOverdue} days)`;
  }
  
  if (invoice.status === InvoiceStatus.Pending || invoice.status === 'Pending') {
    const daysUntilDue = getDaysUntilDue(invoice);
    if (daysUntilDue > 0) {
      return `Due in ${daysUntilDue} days`;
    }
  }
  
  return invoice.status;
}




