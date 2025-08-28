using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public enum NotificationType
{
    InApp,
    Email,
    Sms
}

public enum NotificationStatus
{
    Unread,
    Read,
    Archived
}

public class Notification : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.InApp;
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
} 