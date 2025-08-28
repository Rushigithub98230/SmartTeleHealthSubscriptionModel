using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Role : IdentityRole<int>
{
    [Key]
    public override int Id { get; set; }

    public Role() : base()
    {
    }
    
    public Role(string roleName) : base(roleName)
    {
    }
    
    [MaxLength(500)]
    public string? Description { get; set; }
} 