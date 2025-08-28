using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreateNotificationDto
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";
    
    public bool IsRead { get; set; } = false;
    
    public string? ActionUrl { get; set; }
    
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    public DateTime? ExpiryDate { get; set; }
    
    public DateTime? ScheduledAt { get; set; }
} 