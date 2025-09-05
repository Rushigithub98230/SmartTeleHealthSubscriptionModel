using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core user response entity that manages all user responses in the system.
    /// This entity handles user response creation, management, and validation for questionnaires.
    /// It serves as the central hub for user response management, providing response creation,
    /// status tracking, and validation capabilities.
    /// </summary>
    public class UserResponse : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the user response.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each user response in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key reference to the User who created this response.
        /// Links this response to the specific user account.
        /// Required for user-response relationship management.
        /// </summary>
        [Required]
        public int UserId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the Category that this response belongs to.
        /// Links this response to the specific category.
        /// Required for category-response relationship management.
        /// </summary>
        [Required]
        public Guid CategoryId { get; set; }
        
        /// <summary>
        /// Foreign key reference to the QuestionnaireTemplate that this response uses.
        /// Links this response to the specific questionnaire template.
        /// Required for template-response relationship management.
        /// </summary>
        [Required]
        public Guid TemplateId { get; set; }
        
        /// <summary>
        /// Current status of the user response.
        /// Used for response status tracking and management.
        /// Defaults to Draft when response is created.
        /// </summary>
        public ResponseStatus Status { get; set; } = ResponseStatus.Draft;
        
        // Navigation Properties
        /// <summary>
        /// Navigation property to the User who created this response.
        /// Provides access to user information for response management.
        /// Used for user-response relationship operations.
        /// </summary>
        public virtual User User { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the Category that this response belongs to.
        /// Provides access to category information for response management.
        /// Used for category-response relationship operations.
        /// </summary>
        public virtual Category Category { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the QuestionnaireTemplate that this response uses.
        /// Provides access to template information for response management.
        /// Used for template-response relationship operations.
        /// </summary>
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the UserAnswers that belong to this response.
        /// Provides access to answer information for response management.
        /// Used for response-answer relationship operations.
        /// </summary>
        public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
        
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
        
        // Helper methods
        /// <summary>
        /// Indicates whether this response is completed or submitted.
        /// Used for response completion checking and validation.
        /// Returns true if Status is Completed or Submitted.
        /// </summary>
        [NotMapped]
        public bool IsCompleted => Status == ResponseStatus.Completed || Status == ResponseStatus.Submitted;
        
        /// <summary>
        /// Indicates whether this response is in draft status.
        /// Used for response status checking and validation.
        /// Returns true if Status is Draft.
        /// </summary>
        [NotMapped]
        public bool IsDraft => Status == ResponseStatus.Draft;
    }
} 