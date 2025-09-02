using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class VideoCall : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid AppointmentId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RecordingUrl { get; set; }

        // Alias properties for backward compatibility
        public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
        
        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual ICollection<VideoCallParticipant> Participants { get; set; } = new List<VideoCallParticipant>();
        public virtual ICollection<VideoCallEvent> Events { get; set; } = new List<VideoCallEvent>();
    }
} 