using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

#region User Roles Master Table
public class UserRole : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
#endregion

#region Appointment Status Master Table
public class AppointmentStatus : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
#endregion

#region Payment Status Master Table
public class PaymentStatus : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();
}
#endregion

#region Refund Status Master Table
public class RefundStatus : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();
}
#endregion

#region Participant Status Master Table
public class ParticipantStatus : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
}
#endregion

#region Participant Role Master Table
public class ParticipantRole : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
}
#endregion

#region Invitation Status Master Table
public class InvitationStatus : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentInvitation> Invitations { get; set; } = new List<AppointmentInvitation>();
}
#endregion

#region Appointment Type Master Table
public class AppointmentType : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
#endregion

#region Consultation Mode Master Table
public class ConsultationMode : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
#endregion

#region Document Type Master Table
// DocumentType class is defined in DocumentType.cs
#endregion

#region Reminder Type Master Table
public class ReminderType : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
}
#endregion

#region Reminder Timing Master Table
public class ReminderTiming : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    public int MinutesBeforeAppointment { get; set; } = 15;
    
    // Navigation properties
    public virtual ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
}
#endregion

#region Event Type Master Table
public class EventType : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
}
#endregion

#region Master Billing Cycle
public class MasterBillingCycle : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int DurationInDays { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
#endregion

#region Master Currency
public class MasterCurrency : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string? Symbol { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
}
#endregion

#region Master Privilege Type
public class MasterPrivilegeType : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<Privilege> Privileges { get; set; } = new List<Privilege>();
}
#endregion 