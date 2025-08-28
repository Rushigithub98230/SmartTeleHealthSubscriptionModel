using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class QuestionnaireTemplate : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public Guid CategoryId { get; set; }
        
        public int Version { get; set; } = 1;
        
        // Alias properties for backward compatibility
        public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
        
        // Navigation Properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
    }
} 