using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing message reactions and emoji responses.
/// This entity handles reaction tracking, emoji management, and user interaction for messages.
/// It serves as the central hub for message reaction management, providing emoji-based
/// feedback and interaction tracking capabilities.
/// </summary>
public class MessageReaction : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the message reaction.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each message reaction in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Message that this reaction belongs to.
    /// Links this reaction to the specific message.
    /// Required for message-reaction relationship management.
    /// </summary>
    public Guid MessageId { get; set; }
    
    /// <summary>
    /// Navigation property to the Message that this reaction belongs to.
    /// Provides access to message information for reaction management.
    /// Used for message-reaction relationship operations.
    /// </summary>
    public virtual Message Message { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who created this reaction.
    /// Links this reaction to the specific user account.
    /// Required for user-reaction relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who created this reaction.
    /// Provides access to user information for reaction management.
    /// Used for user-reaction relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Provider who created this reaction.
    /// Links this reaction to the specific healthcare provider.
    /// Optional - used for provider-reaction relationship management.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who created this reaction.
    /// Provides access to provider information for reaction management.
    /// Used for provider-reaction relationship operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Emoji or reaction symbol used for this reaction.
    /// Used for reaction display and interaction tracking.
    /// Required for reaction management and user experience.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Emoji { get; set; } = string.Empty;
} 