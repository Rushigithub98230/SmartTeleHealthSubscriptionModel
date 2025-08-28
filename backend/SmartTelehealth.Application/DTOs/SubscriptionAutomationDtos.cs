namespace SmartTelehealth.Application.DTOs;

// Request DTOs for Subscription Automation
public class StateTransitionRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class SuspensionRequest
{
    public string Reason { get; set; } = string.Empty;
}
