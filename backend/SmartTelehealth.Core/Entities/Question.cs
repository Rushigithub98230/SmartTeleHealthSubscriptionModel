using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core question entity that manages all questions in the system.
    /// This entity handles question creation, type management, and validation for questionnaires.
    /// It serves as the central hub for question management, providing question creation,
    /// type classification, and validation capabilities.
    /// </summary>
    public class Question : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the question.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each question in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key reference to the QuestionnaireTemplate that this question belongs to.
        /// Links this question to the specific questionnaire template.
        /// Required for template-question relationship management.
        /// </summary>
        [Required]
        public Guid TemplateId { get; set; }
        
        /// <summary>
        /// Text content of the question.
        /// Used for question display and user communication.
        /// Required for question management and user experience.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of question (e.g., Text, Radio, Checkbox, Range).
        /// Used for question type classification and validation.
        /// Required for question type enforcement and management.
        /// </summary>
        [Required]
        public QuestionType Type { get; set; } = QuestionType.Text;
        
        /// <summary>
        /// Indicates whether this question is required to be answered.
        /// Used for question validation and user experience.
        /// Defaults to true for standard question requirements.
        /// </summary>
        public bool IsRequired { get; set; } = true;
        
        /// <summary>
        /// Display order of this question within the questionnaire template.
        /// Used for question ordering and user experience.
        /// Required for question ordering and management.
        /// </summary>
        [Required]
        public int Order { get; set; }
        
        /// <summary>
        /// Help text or additional guidance for this question.
        /// Used for question documentation and user assistance.
        /// Optional - used for enhanced user experience and guidance.
        /// </summary>
        [MaxLength(200)]
        public string? HelpText { get; set; }
        
        /// <summary>
        /// URL or path to media content associated with this question.
        /// Used for question media display and user experience.
        /// Optional - used for enhanced question presentation and media support.
        /// </summary>
        [MaxLength(500)]
        public string? MediaUrl { get; set; }
        
        // Range-specific properties
        /// <summary>
        /// Minimum value allowed for range-type questions.
        /// Used for range question validation and management.
        /// Optional - used for range question constraints and validation.
        /// </summary>
        public decimal? MinValue { get; set; }
        
        /// <summary>
        /// Maximum value allowed for range-type questions.
        /// Used for range question validation and management.
        /// Optional - used for range question constraints and validation.
        /// </summary>
        public decimal? MaxValue { get; set; }
        
        /// <summary>
        /// Step value for range-type questions.
        /// Used for range question validation and management.
        /// Optional - used for range question constraints and validation.
        /// </summary>
        public decimal? StepValue { get; set; }
        
        // Navigation Properties
        /// <summary>
        /// Navigation property to the QuestionnaireTemplate that this question belongs to.
        /// Provides access to template information for question management.
        /// Used for template-question relationship operations.
        /// </summary>
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the QuestionOptions that belong to this question.
        /// Provides access to option information for question management.
        /// Used for question-option relationship operations.
        /// </summary>
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        
        /// <summary>
        /// Navigation property to the UserAnswers that respond to this question.
        /// Provides access to user response information for question management.
        /// Used for question-answer relationship operations.
        /// </summary>
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        
        // Helper methods for validation
        /// <summary>
        /// Indicates whether this question is a multiple choice question.
        /// Used for question type checking and validation.
        /// Returns true if the question type is Radio, Checkbox, or Dropdown.
        /// </summary>
        [NotMapped]
        public bool IsMultipleChoice => Type == QuestionType.Radio || Type == QuestionType.Checkbox || Type == QuestionType.Dropdown;
        
        /// <summary>
        /// Indicates whether this question is a text-based question.
        /// Used for question type checking and validation.
        /// Returns true if the question type is Text or TextArea.
        /// </summary>
        [NotMapped]
        public bool IsTextBased => Type == QuestionType.Text || Type == QuestionType.TextArea;
        
        /// <summary>
        /// Indicates whether this question is a range question.
        /// Used for question type checking and validation.
        /// Returns true if the question type is Range.
        /// </summary>
        [NotMapped]
        public bool IsRange => Type == QuestionType.Range;
        
        /// <summary>
        /// Indicates whether this question is a date/time-based question.
        /// Used for question type checking and validation.
        /// Returns true if the question type is Date, DateTime, or Time.
        /// </summary>
        [NotMapped]
        public bool IsDateTimeBased => Type == QuestionType.Date || Type == QuestionType.DateTime || Type == QuestionType.Time;
        
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
        /// Indicates whether this question has options available.
        /// Used for question option checking and validation.
        /// Returns true if the question has one or more options.
        /// </summary>
        [NotMapped]
        public bool HasOptions => Options.Count > 0;
    }
} 