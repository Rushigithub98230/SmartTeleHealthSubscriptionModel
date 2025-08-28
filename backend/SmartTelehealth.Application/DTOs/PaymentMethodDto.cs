namespace SmartTelehealth.Application.DTOs
{
    public class PaymentMethodDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public CardDto? Card { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
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