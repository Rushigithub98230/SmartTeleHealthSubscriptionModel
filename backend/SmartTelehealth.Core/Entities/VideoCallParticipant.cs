using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Entity for managing video call participants and their session details.
    /// This entity handles participant tracking, session management, and quality monitoring.
    /// It serves as the central hub for video call participant management, providing detailed
    /// tracking of participant behavior, quality metrics, and session information.
    /// </summary>
    public class VideoCallParticipant : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the video call participant.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each video call participant in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Foreign key reference to the VideoCall that this participant is part of.
        /// Links this participant to the specific video call session.
        /// Required for video call-participant relationship management.
        /// </summary>
        public Guid VideoCallId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the User who is this participant.
        /// Links this participant to the specific user account.
        /// Required for user-participant relationship management.
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the Provider who is this participant.
        /// Links this participant to the specific healthcare provider.
        /// Optional - used for provider-participant relationship management.
        /// </summary>
        public int? ProviderId { get; set; }
        
        /// <summary>
        /// Indicates whether this participant initiated the video call.
        /// Used for participant role management and call initiation tracking.
        /// Defaults to false for standard participants.
        /// </summary>
        public bool IsInitiator { get; set; } = false;
        
        /// <summary>
        /// Date and time when this participant joined the video call.
        /// Used for participant timing tracking and session management.
        /// Defaults to current UTC time when participant joins.
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Date and time when this participant left the video call.
        /// Used for participant timing tracking and session management.
        /// Set when participant leaves the video call.
        /// </summary>
        public DateTime? LeftAt { get; set; }
        
        /// <summary>
        /// Duration of this participant's session in seconds.
        /// Used for participant session tracking and duration calculations.
        /// Updated when participant leaves the video call.
        /// </summary>
        public int DurationSeconds { get; set; } = 0;
        
        /// <summary>
        /// Indicates whether this participant has video enabled.
        /// Used for participant feature management and call quality tracking.
        /// Defaults to true for standard video call functionality.
        /// </summary>
        public bool IsVideoEnabled { get; set; } = true;
        
        /// <summary>
        /// Indicates whether this participant has audio enabled.
        /// Used for participant feature management and call quality tracking.
        /// Defaults to true for standard video call functionality.
        /// </summary>
        public bool IsAudioEnabled { get; set; } = true;
        
        /// <summary>
        /// Indicates whether this participant has screen sharing enabled.
        /// Used for participant feature management and call quality tracking.
        /// Defaults to false for standard video call functionality.
        /// </summary>
        public bool IsScreenSharingEnabled { get; set; } = false;
        
        /// <summary>
        /// Audio quality rating for this participant (1-5 scale).
        /// Used for call quality monitoring and participant experience tracking.
        /// Set when audio quality is measured or reported.
        /// </summary>
        public int? AudioQuality { get; set; } // 1-5 scale
        
        /// <summary>
        /// Video quality rating for this participant (1-5 scale).
        /// Used for call quality monitoring and participant experience tracking.
        /// Set when video quality is measured or reported.
        /// </summary>
        public int? VideoQuality { get; set; } // 1-5 scale
        
        /// <summary>
        /// Network quality rating for this participant (1-5 scale).
        /// Used for call quality monitoring and participant experience tracking.
        /// Set when network quality is measured or reported.
        /// </summary>
        public int? NetworkQuality { get; set; } // 1-5 scale
        
        /// <summary>
        /// Device information for this participant.
        /// Used for participant device tracking and compatibility management.
        /// Set when participant device information is available.
        /// </summary>
        public string? DeviceInfo { get; set; }
        
        /// <summary>
        /// IP address of this participant.
        /// Used for participant network tracking and security management.
        /// Set when participant IP address is available.
        /// </summary>
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// User agent string for this participant.
        /// Used for participant browser tracking and compatibility management.
        /// Set when participant user agent information is available.
        /// </summary>
        public string? UserAgent { get; set; }

        // Navigation properties
        /// <summary>
        /// Navigation property to the VideoCall that this participant is part of.
        /// Provides access to video call information for participant management.
        /// Used for video call-participant relationship operations.
        /// </summary>
        public virtual VideoCall VideoCall { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the User who is this participant.
        /// Provides access to user information for participant management.
        /// Used for user-participant relationship operations.
        /// </summary>
        public virtual User User { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the Provider who is this participant.
        /// Provides access to provider information for participant management.
        /// Used for provider-participant relationship operations.
        /// </summary>
        public virtual Provider? Provider { get; set; }
    }
} 