using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class QuestionOption : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid QuestionId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Value { get; set; } = string.Empty;
        
        [Required]
        public int Order { get; set; }
        
        [MaxLength(500)]
        public string? MediaUrl { get; set; }
        
        public bool IsCorrect { get; set; } = false; // For scoring/validation
        
        // Navigation Properties
        public virtual Question Question { get; set; } = null!;
        public virtual ICollection<UserAnswerOption> UserAnswerOptions { get; set; } = new List<UserAnswerOption>();
    }
} 