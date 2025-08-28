using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class VideoCallParticipant : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid VideoCallId { get; set; }
        public int UserId { get; set; }
        public int? ProviderId { get; set; }
        public bool IsInitiator { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public int DurationSeconds { get; set; } = 0;
        public bool IsVideoEnabled { get; set; } = true;
        public bool IsAudioEnabled { get; set; } = true;
        public bool IsScreenSharingEnabled { get; set; } = false;
        public int? AudioQuality { get; set; } // 1-5 scale
        public int? VideoQuality { get; set; } // 1-5 scale
        public int? NetworkQuality { get; set; } // 1-5 scale
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Navigation properties
        public virtual VideoCall VideoCall { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Provider? Provider { get; set; }
    }
} 