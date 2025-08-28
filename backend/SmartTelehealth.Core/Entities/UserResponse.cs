using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class UserResponse : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int UserId { get; set; }
        
        [Required]
        public Guid CategoryId { get; set; }
        
        [Required]
        public Guid TemplateId { get; set; }
        
        public ResponseStatus Status { get; set; } = ResponseStatus.Draft;
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
        
        // Alias properties for backward compatibility
        public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
        
        // Helper methods
        [NotMapped]
        public bool IsCompleted => Status == ResponseStatus.Completed || Status == ResponseStatus.Submitted;
        
        [NotMapped]
        public bool IsDraft => Status == ResponseStatus.Submitted;
    }
} 