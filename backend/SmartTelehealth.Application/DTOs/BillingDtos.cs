namespace SmartTelehealth.Application.DTOs;

public class CreateRecurringBillingDto
{
    public int UserId { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public Guid BillingCycleId { get; set; } // Use FK instead of string
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime DueDate { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
    public bool AutoRenew { get; set; } = true;
    public int GracePeriodDays { get; set; } = 7;
    public decimal? LateFeeAmount { get; set; }
    public string? Description { get; set; }
}

public class CreateUpfrontPaymentDto
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsUrgent { get; set; } = false;
}

public class CreateBundlePaymentDto
{
    public int UserId { get; set; }
    public List<BundleItemDto> Items { get; set; } = new List<BundleItemDto>();
    public string PaymentMethodId { get; set; } = string.Empty;
    public bool IncludeShipping { get; set; } = true;
    public bool IsExpressShipping { get; set; } = false;
    public string? CouponCode { get; set; }
    public string? Description { get; set; }
}

public class BundleItemDto
{
    public Guid ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty; // consultation, medication, subscription
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Amount => UnitPrice * Quantity;
    public string? Description { get; set; }
}

public class BillingAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid BillingRecordId { get; set; }
    public decimal Amount { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? AppliedBy { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime CreatedAt => AppliedAt;
    public bool IsPercentage { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime BillingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Description { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInvoiceDto
{
    public int UserId { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Amount => TotalAmount;
    public string Description => Notes ?? string.Empty;
    public string Currency { get; set; } = "USD";
    public DateTime DueDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    public string? ItemType { get; set; }
    public Guid? ItemId { get; set; }
}

public class BillingSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalBilled { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalRefunded { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<BillingRecordDto> RecentTransactions { get; set; } = new List<BillingRecordDto>();
    
    // Added missing properties to fix build errors
    public int TotalBillingRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal FailedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
}

public class PaymentScheduleDto
{
    public Guid SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime NextPaymentDate { get; set; }
    public int TotalPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int RemainingPayments { get; set; }
    public bool AutoRenew { get; set; }
    public List<PaymentScheduleItemDto> PaymentHistory { get; set; } = new List<PaymentScheduleItemDto>();
    public List<PaymentScheduleItemDto> Payments => PaymentHistory;
    
    // Added missing properties to fix build errors
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
}

public class PaymentScheduleItemDto
{
    public Guid BillingRecordId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // scheduled, paid, failed, cancelled
    public string? PaymentMethodId { get; set; }
    public string? TransactionId { get; set; }
}

public class CreateBillingCycleDto
{
    /// <summary>
    /// UserId must always be an int. The client must send a valid integer in JSON (e.g., "userId": 123).
    /// </summary>
    public int UserId { get; set; }
    public CreateBillingCycleDto() { }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid BillingCycleId { get; set; } // Use FK if needed
    public List<Guid> SubscriptionIds { get; set; } = new List<Guid>();
    public bool AutoProcess { get; set; } = true;
    public int GracePeriodDays { get; set; } = 7;
    public decimal? LateFeeAmount { get; set; }
}

public class BillingCycleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid BillingCycleId { get; set; } // Use FK if needed
    public string Status { get; set; } = string.Empty; // active, completed, cancelled
    public bool AutoProcess { get; set; }
    public int GracePeriodDays { get; set; }
    public decimal? LateFeeAmount { get; set; }
    public int TotalSubscriptions { get; set; }
    public int ProcessedSubscriptions { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Added missing properties to fix build errors
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

// Added missing DTO to fix build errors
public class BillingCancellationDto
{
    public Guid Id { get; set; }
    public Guid BillingRecordId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
    public string CancelledBy { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public string RefundReason { get; set; } = string.Empty;
    public DateTime? RefundedAt { get; set; }
    public string? Notes { get; set; }
    
    // Added missing properties to fix build errors
    public Guid SubscriptionId { get; set; }
    public string Status { get; set; } = string.Empty;
}