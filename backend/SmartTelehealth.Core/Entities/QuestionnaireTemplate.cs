using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Core questionnaire template entity that manages all questionnaire templates in the system.
    /// This entity handles questionnaire template creation, versioning, and management.
    /// It serves as the central hub for questionnaire template management, providing template
    /// creation, versioning, and question organization capabilities.
    /// </summary>
    public class QuestionnaireTemplate : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the questionnaire template.
        /// Uses Guid for better scalability and security in distributed systems.
        /// Unique identifier for each questionnaire template in the system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the questionnaire template.
        /// Used for template identification and display.
        /// Required for template management and user experience.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the questionnaire template.
        /// Used for template documentation and user communication.
        /// Optional - used for enhanced template management and documentation.
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Foreign key reference to the Category that this template belongs to.
        /// Links this template to the specific category.
        /// Required for template-category relationship management.
        /// </summary>
        [Required]
        public Guid CategoryId { get; set; }
        
        /// <summary>
        /// Version number of the questionnaire template.
        /// Used for template versioning and management.
        /// Defaults to 1 when template is created.
        /// </summary>
        public int Version { get; set; } = 1;
        
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
        
        // Navigation Properties
        /// <summary>
        /// Navigation property to the Category that this template belongs to.
        /// Provides access to category information for template management.
        /// Used for template-category relationship operations.
        /// </summary>
        public virtual Category Category { get; set; } = null!;
        
        /// <summary>
        /// Navigation property to the Questions that belong to this template.
        /// Provides access to question information for template management.
        /// Used for template-question relationship operations.
        /// </summary>
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        
        /// <summary>
        /// Navigation property to the UserResponses that use this template.
        /// Provides access to user response information for template management.
        /// Used for template-response relationship operations.
        /// </summary>
        public virtual ICollection<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
    }
} 