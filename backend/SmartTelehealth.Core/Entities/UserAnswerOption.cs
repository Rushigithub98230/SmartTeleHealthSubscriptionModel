using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class UserAnswerOption : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AnswerId { get; set; }
        
        [Required]
        public Guid OptionId { get; set; }
        
        // Navigation Properties
        public virtual UserAnswer Answer { get; set; } = null!;
        public virtual QuestionOption Option { get; set; } = null!;
    }
} 