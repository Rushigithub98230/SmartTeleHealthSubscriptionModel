using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Enumeration defining the possible types of notifications.
/// Used for notification type classification and delivery method management.
/// </summary>
public enum NotificationType
{
    /// <summary>In-app notification displayed within the application interface.</summary>
    InApp,
    /// <summary>Email notification sent via email service.</summary>
    Email,
    /// <summary>SMS notification sent via SMS service.</summary>
    Sms
}

/// <summary>
/// Enumeration defining the possible statuses of notifications.
/// Used for notification status tracking and management.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Notification has not been read by the user.</summary>
    Unread,
    /// <summary>Notification has been read by the user.</summary>
    Read,
    /// <summary>Notification has been archived by the user.</summary>
    Archived
}

/// <summary>
/// Core notification entity that manages all notifications in the system.
/// This entity handles notification creation, delivery, and status tracking.
/// It serves as the central hub for notification management, providing notification
/// delivery, status tracking, and user communication capabilities.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the notification.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each notification in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the User who will receive this notification.
    /// Links this notification to the specific user account.
    /// Required for user-notification relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who will receive this notification.
    /// Provides access to user information for notification management.
    /// Used for user-notification relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Title or subject of the notification.
    /// Used for notification display and user communication.
    /// Required for notification management and user experience.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Content or body of the notification.
    /// Used for notification display and user communication.
    /// Required for notification management and user experience.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of notification delivery method.
    /// Used for notification type classification and delivery management.
    /// Defaults to InApp for standard in-application notifications.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.InApp;
    
    /// <summary>
    /// Current status of the notification.
    /// Used for notification status tracking and management.
    /// Defaults to Unread when notification is created.
    /// </summary>
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    
    /// <summary>
    /// Indicates whether the notification has been read by the user.
    /// Used for notification read status tracking and management.
    /// Defaults to false when notification is created.
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// Date and time when the notification was read by the user.
    /// Used for notification read tracking and management.
    /// Set when the notification is read by the user.
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// Date and time when the notification is scheduled to be sent.
    /// Used for notification scheduling and delivery management.
    /// Optional - used for delayed or scheduled notifications.
    /// </summary>
    public DateTime? ScheduledAt { get; set; }
} 