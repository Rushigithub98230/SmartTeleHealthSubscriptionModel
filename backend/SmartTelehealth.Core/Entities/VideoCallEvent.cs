using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Entity for tracking video call events and status changes.
    /// This entity handles event logging, audit trails, and status change tracking for video calls.
    /// It serves as a comprehensive event system that records all significant events and changes
    /// that occur during video call sessions, providing detailed audit trails and history.
    /// </summary>
    public class VideoCallEvent : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the video call event.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each video call event in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Foreign key reference to the VideoCall that this event is for.
        /// Links this event to the specific video call session.
        /// Required for video call-event relationship management.
        /// </summary>
        public Guid VideoCallId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the User who triggered this event.
        /// Links this event to the specific user who caused the event.
        /// Optional - used for user action tracking and audit trails.
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the Provider who triggered this event.
        /// Links this event to the specific provider who caused the event.
        /// Optional - used for provider action tracking and audit trails.
        /// </summary>
        public int? ProviderId { get; set; }
        
        /// <summary>
        /// Type of this video call event.
        /// Used for event type management and categorization.
        /// Set when the event is created.
        /// </summary>
        public VideoCallEventType Type { get; set; }
        
        /// <summary>
        /// Date and time when this video call event occurred.
        /// Used for event timing and chronological tracking.
        /// Defaults to current UTC time when event is created.
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Description or details about this video call event.
        /// Used for event documentation and human-readable event information.
        /// Can include event context, reasons, or additional details.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// JSON metadata containing additional event information.
        /// Used for storing structured event data and additional context.
        /// Can include event-specific data, parameters, or configuration.
        /// </summary>
        public string? Metadata { get; set; } // JSON data for additional event details

        // Navigation properties
        /// <summary>
        /// Navigation property to the VideoCall that this event is for.
        /// Provides access to video call information for event management.
        /// Used for video call-event relationship operations.
        /// </summary>
        public virtual VideoCall VideoCall { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the User who triggered this event.
        /// Provides access to user information for event management.
        /// Used for user action tracking and audit trails.
        /// </summary>
        public virtual User? User { get; set; }
        
        /// <summary>
        /// Navigation property to the Provider who triggered this event.
        /// Provides access to provider information for event management.
        /// Used for provider action tracking and audit trails.
        /// </summary>
        public virtual Provider? Provider { get; set; }
    }

    /// <summary>
    /// Enumeration defining the possible types of video call events.
    /// Used for video call event type management and categorization.
    /// </summary>
    public enum VideoCallEventType
    {
        /// <summary>Video call session was started.</summary>
        Started,
        /// <summary>Video call session was ended.</summary>
        Ended,
        /// <summary>A participant joined the video call.</summary>
        ParticipantJoined,
        /// <summary>A participant left the video call.</summary>
        ParticipantLeft,
        /// <summary>Recording of the video call was started.</summary>
        RecordingStarted,
        /// <summary>Recording of the video call was stopped.</summary>
        RecordingStopped,
        /// <summary>Video call was initiated.</summary>
        CallInitiated,
        /// <summary>Video call was disconnected.</summary>
        CallDisconnected,
        /// <summary>Video call was rejected.</summary>
        CallRejected,
        /// <summary>Video was enabled for a participant.</summary>
        VideoEnabled,
        /// <summary>Video was disabled for a participant.</summary>
        VideoDisabled,
        /// <summary>Audio was enabled for a participant.</summary>
        AudioEnabled,
        /// <summary>Audio was disabled for a participant.</summary>
        AudioDisabled,
        /// <summary>Screen sharing was started by a participant.</summary>
        ScreenSharingStarted,
        /// <summary>Screen sharing was stopped by a participant.</summary>
        ScreenSharingStopped,
        /// <summary>Call quality changed for a participant.</summary>
        QualityChanged
    }
} 