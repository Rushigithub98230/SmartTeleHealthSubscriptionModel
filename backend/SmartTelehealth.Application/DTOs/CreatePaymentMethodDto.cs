public class CreatePaymentMethodDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
} 