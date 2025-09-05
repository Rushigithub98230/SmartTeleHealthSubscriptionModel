using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core user answer option entity that manages all user answer options in the system.
    /// This entity handles user answer option selection, management, and validation for questions.
    /// It serves as the central hub for user answer option management, providing option selection,
    /// value management, and validation capabilities.
    /// </summary>
    public class UserAnswerOption : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the user answer option.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each user answer option in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key reference to the UserAnswer that this option selection belongs to.
        /// Links this option selection to the specific user answer.
        /// Required for answer-option relationship management.
        /// </summary>
        [Required]
        public Guid AnswerId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the QuestionOption that was selected.
        /// Links this option selection to the specific question option.
        /// Required for option-answer relationship management.
        /// </summary>
        [Required]
        public Guid OptionId { get; set; }
        
        // Navigation Properties
        /// <summary>
        /// Navigation property to the UserAnswer that this option selection belongs to.
        /// Provides access to answer information for option selection management.
        /// Used for answer-option relationship operations.
        /// </summary>
        public virtual UserAnswer Answer { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the QuestionOption that was selected.
        /// Provides access to option information for option selection management.
        /// Used for option-answer relationship operations.
        /// </summary>
        public virtual QuestionOption Option { get; set; } = null!;
    }
} 