using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core user answer entity that manages all user answers in the system.
    /// This entity handles user answer creation, management, and validation for questions.
    /// It serves as the central hub for user answer management, providing answer creation,
    /// value management, and validation capabilities.
    /// </summary>
    public class UserAnswer : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the user answer.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each user answer in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key reference to the UserResponse that this answer belongs to.
        /// Links this answer to the specific user response.
        /// Required for response-answer relationship management.
        /// </summary>
        [Required]
        public Guid ResponseId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the Question that this answer responds to.
        /// Links this answer to the specific question.
        /// Required for question-answer relationship management.
        /// </summary>
        [Required]
        public Guid QuestionId { get; set; }
        
        /// <summary>
        /// Text content of the user's answer.
        /// Used for text-based question answers and user communication.
        /// Optional - used for text and textarea question types.
        /// </summary>
        [MaxLength(4000)]
        public string? AnswerText { get; set; }
        
        /// <summary>
        /// Numeric value of the user's answer.
        /// Used for range and numeric question answers.
        /// Optional - used for range and numeric question types.
        /// </summary>
        public decimal? NumericValue { get; set; }
        
        /// <summary>
        /// Date/time value of the user's answer.
        /// Used for date, datetime, and time question answers.
        /// Optional - used for date, datetime, and time question types.
        /// </summary>
        public DateTime? DateTimeValue { get; set; }
        
        // Navigation Properties
        /// <summary>
        /// Navigation property to the UserResponse that this answer belongs to.
        /// Provides access to response information for answer management.
        /// Used for response-answer relationship operations.
        /// </summary>
        public virtual UserResponse Response { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the Question that this answer responds to.
        /// Provides access to question information for answer management.
        /// Used for question-answer relationship operations.
        /// </summary>
        public virtual Question Question { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the UserAnswerOptions that are selected for this answer.
        /// Provides access to selected option information for answer management.
        /// Used for answer-option relationship operations.
        /// </summary>
        public virtual ICollection<UserAnswerOption> SelectedOptions { get; set; } = new List<UserAnswerOption>();
        
        // Helper methods
        /// <summary>
        /// Indicates whether this answer has text content.
        /// Used for answer type checking and validation.
        /// Returns true if AnswerText is not null or empty.
        /// </summary>
        [NotMapped]
        public bool HasTextAnswer => !string.IsNullOrEmpty(AnswerText);
        
        /// <summary>
        /// Indicates whether this answer has a numeric value.
        /// Used for answer type checking and validation.
        /// Returns true if NumericValue has a value.
        /// </summary>
        [NotMapped]
        public bool HasNumericAnswer => NumericValue.HasValue;
        
        /// <summary>
        /// Indicates whether this answer has a date/time value.
        /// Used for answer type checking and validation.
        /// Returns true if DateTimeValue has a value.
        /// </summary>
        [NotMapped]
        public bool HasDateTimeAnswer => DateTimeValue.HasValue;
        
        /// <summary>
        /// Indicates whether this answer has selected options.
        /// Used for answer type checking and validation.
        /// Returns true if SelectedOptions has one or more items.
        /// </summary>
        [NotMapped]
        public bool HasSelectedOptions => SelectedOptions.Count > 0;
        
        // Alias properties for backward compatibility
        /// <summary>
        /// Alias property for CreatedDate from BaseEntity.
        /// Used for backward compatibility and legacy system integration.
        /// </summary>
        public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
        
        /// <summary>
        /// Alias property for UpdatedDate from BaseEntity.
        /// Used for backward compatibility and legacy system integration.
        /// </summary>
        public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
        
        /// <summary>
        /// Indicates whether this answer has been answered by the user.
        /// Used for answer completion checking and validation.
        /// Returns true if any answer type has been provided.
        /// </summary>
        [NotMapped]
        public bool IsAnswered => HasTextAnswer || HasNumericAnswer || HasDateTimeAnswer || HasSelectedOptions;
    }
} 