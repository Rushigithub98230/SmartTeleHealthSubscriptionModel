using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Master table entity that defines user roles in the system.
/// This entity handles user role management including role definitions, descriptions, and access control.
/// It serves as the foundation for role-based access control, defining what roles are available
/// to users and how they can be assigned. The entity includes comprehensive role management
/// and integration with user accounts for authorization and access control.
/// </summary>
#region User Roles Master Table
public class UserRole : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the user role.
    /// Uses integer for better performance and simpler relationships.
    /// Unique identifier for each user role in the system.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the user role for display and identification purposes.
    /// Required field for user role management and user interface display.
    /// Used in role selection, authorization, and user interface.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the user role and its permissions.
    /// Used for role information display and user education.
    /// Provides comprehensive information about the role's capabilities.
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Sort order for displaying user roles in user interface.
    /// Used for controlling the order in which roles are displayed.
    /// Lower numbers are displayed first in the role selection interface.
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    /// <summary>
    /// Collection of all users assigned to this role.
    /// Represents the users who have this role assigned.
    /// Used for role-based user management and authorization.
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
#endregion

/// <summary>
/// Master table entity that defines appointment statuses in the system.
/// This entity handles appointment status management including status definitions, descriptions, and UI display properties.
/// It serves as the foundation for appointment status tracking, defining what statuses are available
/// for appointments and how they are displayed in the user interface. The entity includes comprehensive
/// status management and integration with appointments for status tracking and workflow management.
/// </summary>
#region Appointment Status Master Table
public class AppointmentStatus : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment status.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each appointment status in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the appointment status for display and identification purposes.
    /// Required field for appointment status management and user interface display.
    /// Used in status selection, appointment management, and user interface.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the appointment status and its meaning.
    /// Used for status information display and user education.
    /// Provides comprehensive information about the status's meaning and implications.
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Sort order for displaying appointment statuses in user interface.
    /// Used for controlling the order in which statuses are displayed.
    /// Lower numbers are displayed first in the status selection interface.
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Color code for displaying this appointment status in the user interface.
    /// Used for visual status identification and user interface customization.
    /// Can be hex color codes or CSS color names for consistent UI display.
    /// </summary>
    [MaxLength(50)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Icon identifier for displaying this appointment status in the user interface.
    /// Used for visual status identification and user interface customization.
    /// Can be icon class names or icon identifiers for consistent UI display.
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Collection of all appointments that use this status.
    /// Represents the appointments that have this status assigned.
    /// Used for appointment status tracking and management.
    /// </summary>
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

/// <summary>
/// Master table entity that defines billing cycles in the system.
/// This entity handles billing cycle management including cycle definitions, durations, and payment scheduling.
/// It serves as the foundation for subscription billing, defining what billing cycles are available
/// for subscriptions and how they are used for payment scheduling. The entity includes comprehensive
/// billing cycle management and integration with subscription plans and subscriptions for billing operations.
/// </summary>
#region Master Billing Cycle
public class MasterBillingCycle : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the billing cycle.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each billing cycle in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the billing cycle for display and identification purposes.
    /// Required field for billing cycle management and user interface display.
    /// Used in billing cycle selection, subscription management, and user interface.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the billing cycle and its characteristics.
    /// Used for billing cycle information display and user education.
    /// Provides comprehensive information about the billing cycle's features and usage.
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Duration of the billing cycle in days.
    /// Used for billing cycle calculations and payment scheduling.
    /// Determines how often users are billed for their subscriptions.
    /// </summary>
    public int DurationInDays { get; set; }
    
    /// <summary>
    /// Sort order for displaying billing cycles in user interface.
    /// Used for controlling the order in which billing cycles are displayed.
    /// Lower numbers are displayed first in the billing cycle selection interface.
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    /// <summary>
    /// Collection of all subscription plans that use this billing cycle.
    /// Represents the subscription plans that have this billing cycle assigned.
    /// Used for subscription plan billing cycle management and configuration.
    /// </summary>
    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    
    /// <summary>
    /// Collection of all subscriptions that use this billing cycle.
    /// Represents the subscriptions that have this billing cycle assigned.
    /// Used for subscription billing cycle management and payment scheduling.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
#endregion

/// <summary>
/// Master table entity that defines currencies in the system.
/// This entity handles currency management including currency codes, names, symbols, and international support.
/// It serves as the foundation for international billing and payments, defining what currencies are available
/// for subscriptions, billing, and payments. The entity includes comprehensive currency management
/// and integration with subscription plans, subscriptions, and billing records for international operations.
/// </summary>
#region Master Currency
public class MasterCurrency : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the currency.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each currency in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ISO currency code for this currency (e.g., USD, EUR, GBP).
    /// Required field for currency identification and international standards compliance.
    /// Used for currency selection, international payments, and currency management.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the currency for display and identification purposes.
    /// Required field for currency management and user interface display.
    /// Used in currency selection, billing, and user interface.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Currency symbol for display purposes (e.g., $, €, £).
    /// Used for currency display in user interface and billing.
    /// Can be used for formatting currency amounts in the UI.
    /// </summary>
    [MaxLength(10)]
    public string? Symbol { get; set; }
    
    /// <summary>
    /// Sort order for displaying currencies in user interface.
    /// Used for controlling the order in which currencies are displayed.
    /// Lower numbers are displayed first in the currency selection interface.
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    /// <summary>
    /// Collection of all subscription plans that use this currency.
    /// Represents the subscription plans that have this currency assigned.
    /// Used for subscription plan currency management and configuration.
    /// </summary>
    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    
    /// <summary>
    /// Collection of all subscriptions that use this currency.
    /// Represents the subscriptions that have this currency assigned.
    /// Used for subscription currency management and payment processing.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    /// <summary>
    /// Collection of all billing records that use this currency.
    /// Represents the billing records that have this currency assigned.
    /// Used for billing record currency management and payment processing.
    /// </summary>
    public virtual ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
}
#endregion

/// <summary>
/// Master table entity that defines privilege types in the system.
/// This entity handles privilege type management including type definitions, descriptions, and categorization.
/// It serves as the foundation for the privilege system, defining what privilege types are available
/// for privileges and how they are categorized. The entity includes comprehensive privilege type management
/// and integration with privileges for privilege categorization and access control.
/// </summary>
#region Master Privilege Type
public class MasterPrivilegeType : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the privilege type.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each privilege type in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the privilege type for display and identification purposes.
    /// Required field for privilege type management and user interface display.
    /// Used in privilege type selection, privilege management, and user interface.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the privilege type and its characteristics.
    /// Used for privilege type information display and user education.
    /// Provides comprehensive information about the privilege type's features and usage.
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Sort order for displaying privilege types in user interface.
    /// Used for controlling the order in which privilege types are displayed.
    /// Lower numbers are displayed first in the privilege type selection interface.
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    /// <summary>
    /// Collection of all privileges that use this privilege type.
    /// Represents the privileges that have this privilege type assigned.
    /// Used for privilege type management and privilege categorization.
    /// </summary>
    public virtual ICollection<Privilege> Privileges { get; set; } = new List<Privilege>();
}
#endregion 