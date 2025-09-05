using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core role entity that manages all roles in the system.
/// This entity handles role creation, management, and access control for users.
/// It serves as the central hub for role management, providing role creation,
/// access control, and user permission management capabilities.
/// </summary>
public class Role : IdentityRole<int>
{
    /// <summary>
    /// Primary key identifier for the role.
    /// Uses integer for role identification and management.
    /// Unique identifier for each role in the system.
    /// </summary>
    [Key]
    public override int Id { get; set; }

    /// <summary>
    /// Default constructor for the Role entity.
    /// Used for role creation and initialization.
    /// </summary>
    public Role() : base()
    {
    }
    
    /// <summary>
    /// Constructor for the Role entity with role name.
    /// Used for role creation with specific role name.
    /// </summary>
    /// <param name="roleName">The name of the role to create</param>
    public Role(string roleName) : base(roleName)
    {
    }
    
    /// <summary>
    /// Description of the role.
    /// Used for role documentation and user communication.
    /// Optional - used for enhanced role management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
} 