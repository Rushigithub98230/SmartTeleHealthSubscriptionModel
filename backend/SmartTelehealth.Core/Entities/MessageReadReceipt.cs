using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing message read receipts and delivery tracking.
/// This entity handles read receipt tracking, delivery confirmation, and user interaction for messages.
/// It serves as the central hub for message read receipt management, providing delivery tracking,
/// read confirmation, and audit trail capabilities.
/// </summary>
public class MessageReadReceipt : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the message read receipt.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each message read receipt in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Message that this read receipt belongs to.
    /// Links this read receipt to the specific message.
    /// Required for message-read receipt relationship management.
    /// </summary>
    public Guid MessageId { get; set; }
    
    /// <summary>
    /// Navigation property to the Message that this read receipt belongs to.
    /// Provides access to message information for read receipt management.
    /// Used for message-read receipt relationship operations.
    /// </summary>
    public virtual Message Message { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who read this message.
    /// Links this read receipt to the specific user account.
    /// Required for user-read receipt relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who read this message.
    /// Provides access to user information for read receipt management.
    /// Used for user-read receipt relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Provider who read this message.
    /// Links this read receipt to the specific healthcare provider.
    /// Optional - used for provider-read receipt relationship management.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who read this message.
    /// Provides access to provider information for read receipt management.
    /// Used for provider-read receipt relationship operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Date and time when the message was read.
    /// Used for read receipt tracking and delivery confirmation.
    /// Set when the message is read by the recipient.
    /// </summary>
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Information about the device used to read the message.
    /// Used for audit trail and security tracking.
    /// Optional - used for enhanced security and audit capabilities.
    /// </summary>
    [MaxLength(100)]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP address of the device used to read the message.
    /// Used for audit trail and security tracking.
    /// Optional - used for enhanced security and audit capabilities.
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }
} 