namespace SmartTelehealth.Application.DTOs
{
    public class PaymentMethodDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public CardDto? Card { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AddPaymentMethodDto
    {
        public string PaymentMethodId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public bool IsDefault { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Additional properties for Stripe integration
        public string Type { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
    }

    public class UpdatePaymentMethodDto
    {
        public string PaymentMethodId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public bool IsDefault { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        
        // Additional properties for Stripe integration
        public string Type { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        
        // Billing address information
        public BillingAddressDto? BillingAddress { get; set; }
    }

    public class BillingAddressDto
    {
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }

    public class CardDto
    {
        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string? Fingerprint { get; set; }
    }
} 