using System;

namespace SmartTelehealth.Application.DTOs;

public class StripePriceDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public int IntervalCount { get; set; }
    public bool Active { get; set; }
    public string Type { get; set; } = string.Empty; // "one_time" or "recurring"
    public DateTime CreatedAt { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 