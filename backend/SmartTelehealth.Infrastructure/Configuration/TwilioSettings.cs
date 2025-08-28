namespace SmartTelehealth.Infrastructure.Configuration;

public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SendGridApiKey { get; set; } = string.Empty;
    public string AppUrl { get; set; } = string.Empty;
    public bool EnableSms { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public int SmsRateLimitPerMinute { get; set; } = 10;
    public int EmailRateLimitPerMinute { get; set; } = 50;
}
