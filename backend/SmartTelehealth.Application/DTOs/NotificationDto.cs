using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReadAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
} 