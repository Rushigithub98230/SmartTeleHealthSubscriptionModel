using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core video call entity that manages all video calls in the system.
    /// This entity handles video call session management, participant tracking, and call recording.
    /// It serves as the central hub for video call functionality, integrating with appointments,
    /// participants, and call events. The entity includes comprehensive video call tracking,
    /// session management, and recording capabilities.
    /// </summary>
    public class VideoCall : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the video call.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each video call in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Foreign key reference to the Appointment that this video call is for.
        /// Links this video call to the specific appointment.
        /// Required for appointment-video call relationship management.
        /// </summary>
        public Guid AppointmentId { get; set; }
        
        /// <summary>
        /// Video call session ID for the video call platform (e.g., OpenTok, Zoom).
        /// Used for video call session identification and management.
        /// Set when video call session is created.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Authentication token for the video call session.
        /// Used for video call authentication and access control.
        /// Set when video call session is created.
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// Date and time when this video call started.
        /// Used for video call timing tracking and duration calculations.
        /// Set when video call begins.
        /// </summary>
        public DateTime StartedAt { get; set; }
        
        /// <summary>
        /// Date and time when this video call ended.
        /// Used for video call timing tracking and duration calculations.
        /// Set when video call concludes.
        /// </summary>
        public DateTime? EndedAt { get; set; }
        
        /// <summary>
        /// Current status of this video call.
        /// Used for video call status tracking and workflow management.
        /// Set when video call status changes.
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// URL to access the video call recording.
        /// Used for recording access and user interface.
        /// Generated when recording is available for this video call.
        /// </summary>
        public string? RecordingUrl { get; set; }

        // Alias properties for backward compatibility
        /// <summary>
        /// Alias property for CreatedDate from BaseEntity.
        /// Used for backward compatibility with existing code.
        /// </summary>
        public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
        
        /// <summary>
        /// Alias property for UpdatedDate from BaseEntity.
        /// Used for backward compatibility with existing code.
        /// </summary>
        public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
        
        // Navigation properties
        /// <summary>
        /// Navigation property to the Appointment that this video call is for.
        /// Provides access to appointment information for video call management.
        /// Used for appointment-video call relationship operations.
        /// </summary>
        public virtual Appointment Appointment { get; set; } = null!;
        
        /// <summary>
        /// Collection of participants in this video call.
        /// Used for participant management and video call access control.
        /// Includes all participants who joined this video call.
        /// </summary>
        public virtual ICollection<VideoCallParticipant> Participants { get; set; } = new List<VideoCallParticipant>();
        
        /// <summary>
        /// Collection of events for this video call.
        /// Used for event tracking and video call history.
        /// Includes all events and status changes for this video call.
        /// </summary>
        public virtual ICollection<VideoCallEvent> Events { get; set; } = new List<VideoCallEvent>();
    }
} 