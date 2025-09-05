using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core question option entity that manages all question options in the system.
    /// This entity handles question option creation, management, and validation for questions.
    /// It serves as the central hub for question option management, providing option creation,
    /// value management, and validation capabilities.
    /// </summary>
    public class QuestionOption : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the question option.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each question option in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key reference to the Question that this option belongs to.
        /// Links this option to the specific question.
        /// Required for question-option relationship management.
        /// </summary>
        [Required]
        public Guid QuestionId { get; set; }
        
        /// <summary>
        /// Text content of the question option.
        /// Used for option display and user communication.
        /// Required for option management and user experience.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Value of the question option.
        /// Used for option value management and validation.
        /// Required for option value enforcement and management.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Display order of this option within the question.
        /// Used for option ordering and user experience.
        /// Required for option ordering and management.
        /// </summary>
        [Required]
        public int Order { get; set; }
        
        /// <summary>
        /// URL or path to media content associated with this option.
        /// Used for option media display and user experience.
        /// Optional - used for enhanced option presentation and media support.
        /// </summary>
        [MaxLength(500)]
        public string? MediaUrl { get; set; }
        
        /// <summary>
        /// Indicates whether this option is correct for scoring/validation purposes.
        /// Used for option correctness tracking and validation.
        /// Defaults to false for standard option correctness.
        /// </summary>
        public bool IsCorrect { get; set; } = false;
        
        // Navigation Properties
        /// <summary>
        /// Navigation property to the Question that this option belongs to.
        /// Provides access to question information for option management.
        /// Used for question-option relationship operations.
        /// </summary>
        public virtual Question Question { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the UserAnswerOptions that select this option.
        /// Provides access to user answer option information for option management.
        /// Used for option-answer relationship operations.
        /// </summary>
        public virtual ICollection<UserAnswerOption> UserAnswerOptions { get; set; } = new List<UserAnswerOption>();
    }
} 