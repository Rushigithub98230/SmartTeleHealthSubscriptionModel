using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class Question : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TemplateId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        public QuestionType Type { get; set; } = QuestionType.Text;
        
        public bool IsRequired { get; set; } = true;
        
        [Required]
        public int Order { get; set; }
        
        [MaxLength(200)]
        public string? HelpText { get; set; }
        
        [MaxLength(500)]
        public string? MediaUrl { get; set; }
        
        // Range-specific properties
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? StepValue { get; set; }
        
        // Navigation Properties
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        
        // Helper methods for validation
        [NotMapped]
        public bool IsMultipleChoice => Type == QuestionType.Radio || Type == QuestionType.Checkbox || Type == QuestionType.Dropdown;
        
        [NotMapped]
        public bool IsTextBased => Type == QuestionType.Text || Type == QuestionType.TextArea;
        
        [NotMapped]
        public bool IsRange => Type == QuestionType.Range;
        
        [NotMapped]
        public bool IsDateTimeBased => Type == QuestionType.Date || Type == QuestionType.DateTime || Type == QuestionType.Time;
        
        // Alias properties for backward compatibility
        public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
        
        [NotMapped]
        public bool HasOptions => Options.Count > 0;
    }
} 