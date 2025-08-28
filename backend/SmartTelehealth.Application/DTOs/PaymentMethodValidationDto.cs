namespace SmartTelehealth.Application.DTOs
{
    public class PaymentMethodValidationDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CardType { get; set; }
        public string? Last4Digits { get; set; }
        public string? ValidationMessage { get; set; }
        public string? CardBrand { get; set; }
        public string? Last4 { get; set; }
        public int? ExpMonth { get; set; }
        public int? ExpYear { get; set; }
        public bool IsExpired { get; set; }
    }
} 