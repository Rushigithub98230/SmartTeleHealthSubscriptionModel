using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class UserAnswer : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ResponseId { get; set; }
        
        [Required]
        public Guid QuestionId { get; set; }
        
        [MaxLength(4000)] // For text/textarea answers
        public string? AnswerText { get; set; }
        
        public decimal? NumericValue { get; set; } // For range answers
        
        public DateTime? DateTimeValue { get; set; } // For date, datetime, time answers
        
        // Navigation Properties
        public virtual UserResponse Response { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        public virtual ICollection<UserAnswerOption> SelectedOptions { get; set; } = new List<UserAnswerOption>();
        
        // Helper methods
        [NotMapped]
        public bool HasTextAnswer => !string.IsNullOrEmpty(AnswerText);
        
        [NotMapped]
        public bool HasNumericAnswer => NumericValue.HasValue;
        
        [NotMapped]
        public bool HasDateTimeAnswer => DateTimeValue.HasValue;
        
        [NotMapped]
        public bool HasSelectedOptions => SelectedOptions.Count > 0;
        
        // Alias properties for backward compatibility
        public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
        
        [NotMapped]
        public bool IsAnswered => HasTextAnswer || HasNumericAnswer || HasDateTimeAnswer || HasSelectedOptions;
    }
} 