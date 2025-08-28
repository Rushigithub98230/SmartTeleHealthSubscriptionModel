using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace SmartTelehealth.Infrastructure.Data;

public static class SeedData
{
    public static void SeedMasterTables(ApplicationDbContext context)
    {
        if (!context.UserRoles.Any())
        {
            var userRoles = new List<UserRole>
            {
                new UserRole { Name = "Client", Description = "Patient/Client users", SortOrder = 1 },
                new UserRole { Name = "Provider", Description = "Healthcare providers", SortOrder = 2 },
                new UserRole { Name = "Admin", Description = "System administrators", SortOrder = 3 }
            };
            context.UserRoles.AddRange(userRoles);
            context.SaveChanges();
        }

        if (!context.AppointmentStatuses.Any())
        {
            var appointmentStatuses = new List<AppointmentStatus>
            {
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending", Description = "Appointment created, waiting for provider approval", SortOrder = 1, Color = "#FFA500", Icon = "clock" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Approved", Description = "Provider approved the appointment", SortOrder = 2, Color = "#008000", Icon = "check" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Rejected", Description = "Provider rejected the appointment", SortOrder = 3, Color = "#FF0000", Icon = "x" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Scheduled", Description = "Appointment is scheduled and confirmed", SortOrder = 4, Color = "#0000FF", Icon = "calendar" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "InMeeting", Description = "Appointment is currently in progress", SortOrder = 5, Color = "#800080", Icon = "video" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "AwaitingCompletion", Description = "Meeting ended, waiting for provider completion", SortOrder = 6, Color = "#FFD700", Icon = "hourglass" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Completed", Description = "Appointment completed successfully", SortOrder = 7, Color = "#008000", Icon = "check-circle" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Expired", Description = "Appointment expired without action", SortOrder = 8, Color = "#808080", Icon = "clock-x" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Cancelled", Description = "Appointment was cancelled", SortOrder = 9, Color = "#FF0000", Icon = "x-circle" },
                new AppointmentStatus { Id = Guid.NewGuid(), Name = "Refunded", Description = "Payment was refunded", SortOrder = 10, Color = "#FFA500", Icon = "refresh" }
            };
            context.AppointmentStatuses.AddRange(appointmentStatuses);
        }

        if (!context.PaymentStatuses.Any())
        {
            var paymentStatuses = new List<PaymentStatus>
            {
                new PaymentStatus { Id = Guid.NewGuid(), Name = "Pending", Description = "Payment is pending", SortOrder = 1, Color = "#FFA500" },
                new PaymentStatus { Id = Guid.NewGuid(), Name = "Processing", Description = "Payment is being processed", SortOrder = 2, Color = "#0000FF" },
                new PaymentStatus { Id = Guid.NewGuid(), Name = "Completed", Description = "Payment completed successfully", SortOrder = 3, Color = "#008000" },
                new PaymentStatus { Id = Guid.NewGuid(), Name = "Failed", Description = "Payment failed", SortOrder = 4, Color = "#FF0000" },
                new PaymentStatus { Id = Guid.NewGuid(), Name = "Cancelled", Description = "Payment was cancelled", SortOrder = 5, Color = "#808080" },
                new PaymentStatus { Id = Guid.NewGuid(), Name = "Refunded", Description = "Payment was refunded", SortOrder = 6, Color = "#FFA500" },
                new PaymentStatus { Id = Guid.NewGuid(), Name = "PartiallyRefunded", Description = "Payment was partially refunded", SortOrder = 7, Color = "#FFD700" }
            };
            context.PaymentStatuses.AddRange(paymentStatuses);
        }

        if (!context.RefundStatuses.Any())
        {
            var refundStatuses = new List<RefundStatus>
            {
                new RefundStatus { Id = Guid.NewGuid(), Name = "None", Description = "No refund requested", SortOrder = 1, Color = "#808080" },
                new RefundStatus { Id = Guid.NewGuid(), Name = "Requested", Description = "Refund has been requested", SortOrder = 2, Color = "#FFA500" },
                new RefundStatus { Id = Guid.NewGuid(), Name = "Processing", Description = "Refund is being processed", SortOrder = 3, Color = "#0000FF" },
                new RefundStatus { Id = Guid.NewGuid(), Name = "Completed", Description = "Refund completed successfully", SortOrder = 4, Color = "#008000" },
                new RefundStatus { Id = Guid.NewGuid(), Name = "Failed", Description = "Refund failed", SortOrder = 5, Color = "#FF0000" }
            };
            context.RefundStatuses.AddRange(refundStatuses);
        }

        if (!context.ParticipantStatuses.Any())
        {
            var participantStatuses = new List<ParticipantStatus>
            {
                new ParticipantStatus { Id = Guid.NewGuid(), Name = "Invited", Description = "Participant has been invited", SortOrder = 1, Color = "#FFA500" },
                new ParticipantStatus { Id = Guid.NewGuid(), Name = "Joined", Description = "Participant has joined the appointment", SortOrder = 2, Color = "#008000" },
                new ParticipantStatus { Id = Guid.NewGuid(), Name = "Left", Description = "Participant has left the appointment", SortOrder = 3, Color = "#FF0000" },
                new ParticipantStatus { Id = Guid.NewGuid(), Name = "Declined", Description = "Participant declined the invitation", SortOrder = 4, Color = "#808080" },
                new ParticipantStatus { Id = Guid.NewGuid(), Name = "Removed", Description = "Participant was removed from the appointment", SortOrder = 5, Color = "#FF0000" }
            };
            context.ParticipantStatuses.AddRange(participantStatuses);
        }

        if (!context.ParticipantRoles.Any())
        {
            var participantRoles = new List<ParticipantRole>
            {
                new ParticipantRole { Id = Guid.NewGuid(), Name = "Patient", Description = "Primary patient", SortOrder = 1, Color = "#0000FF" },
                new ParticipantRole { Id = Guid.NewGuid(), Name = "Provider", Description = "Healthcare provider", SortOrder = 2, Color = "#008000" },
                new ParticipantRole { Id = Guid.NewGuid(), Name = "External", Description = "External participant (family member, caregiver, etc.)", SortOrder = 3, Color = "#FFA500" }
            };
            context.ParticipantRoles.AddRange(participantRoles);
        }

        if (!context.InvitationStatuses.Any())
        {
            var invitationStatuses = new List<InvitationStatus>
            {
                new InvitationStatus { Id = Guid.NewGuid(), Name = "Pending", Description = "Invitation sent, waiting for response", SortOrder = 1, Color = "#FFA500" },
                new InvitationStatus { Id = Guid.NewGuid(), Name = "Accepted", Description = "Invitation accepted", SortOrder = 2, Color = "#008000" },
                new InvitationStatus { Id = Guid.NewGuid(), Name = "Declined", Description = "Invitation declined", SortOrder = 3, Color = "#FF0000" },
                new InvitationStatus { Id = Guid.NewGuid(), Name = "Expired", Description = "Invitation expired", SortOrder = 4, Color = "#808080" }
            };
            context.InvitationStatuses.AddRange(invitationStatuses);
        }

        if (!context.AppointmentTypes.Any())
        {
            var appointmentTypes = new List<AppointmentType>
            {
                new AppointmentType { Id = Guid.NewGuid(), Name = "Regular", Description = "Regular consultation", SortOrder = 1, Color = "#0000FF" },
                new AppointmentType { Id = Guid.NewGuid(), Name = "Urgent", Description = "Urgent consultation", SortOrder = 2, Color = "#FF0000" },
                new AppointmentType { Id = Guid.NewGuid(), Name = "FollowUp", Description = "Follow-up consultation", SortOrder = 3, Color = "#008000" },
                new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation", Description = "General consultation", SortOrder = 4, Color = "#FFA500" },
                new AppointmentType { Id = Guid.NewGuid(), Name = "Emergency", Description = "Emergency consultation", SortOrder = 5, Color = "#FF0000" }
            };
            context.AppointmentTypes.AddRange(appointmentTypes);
        }

        if (!context.ConsultationModes.Any())
        {
            var consultationModes = new List<ConsultationMode>
            {
                new ConsultationMode { Id = Guid.NewGuid(), Name = "Video", Description = "Video consultation", SortOrder = 1, Color = "#0000FF" },
                new ConsultationMode { Id = Guid.NewGuid(), Name = "InPerson", Description = "In-person consultation", SortOrder = 2, Color = "#008000" },
                new ConsultationMode { Id = Guid.NewGuid(), Name = "Phone", Description = "Phone consultation", SortOrder = 3, Color = "#FFA500" }
            };
            context.ConsultationModes.AddRange(consultationModes);
        }

        // Create a system user for seeding purposes
        var systemUserId = 1; // System user ID as int
        var adminRole = context.UserRoles.FirstOrDefault(r => r.Name == "Admin");
        if (adminRole == null)
        {
            throw new InvalidOperationException("Admin role not found. Please ensure UserRoles are seeded first.");
        }

        if (!context.Users.Any())
        {
            var systemUser = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "system@smarttelehealth.com",
                UserName = "system@smarttelehealth.com",
                PhoneNumber = "0000000000",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "Other",
                Address = "System Address",
                City = "System City",
                State = "System State",
                ZipCode = "00000",
                IsActive = true,
                UserRoleId = adminRole.Id
            };
            context.Users.Add(systemUser);
            context.SaveChanges();
        }
        else
        {
            systemUserId = context.Users.First().Id;
        }

        // Temporarily disabled DocumentTypes seeding due to foreign key constraint issues
        // TODO: Fix DocumentTypes seeding after backend is running
        /*
        if (!context.DocumentTypes.Any())
        {
            var documentTypes = new List<DocumentType>
            {
                new DocumentType { Id = Guid.NewGuid(), Name = "Prescription", Description = "Medical prescription", DisplayOrder = 1, Icon = "file-text", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "LabReport", Description = "Laboratory report", DisplayOrder = 2, Icon = "file-text", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "Invoice", Description = "Payment invoice", DisplayOrder = 3, Icon = "file-text", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "MedicalRecord", Description = "Medical record", DisplayOrder = 4, Icon = "file-text", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "ConsentForm", Description = "Consent form", DisplayOrder = 5, Icon = "file-text", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "Referral", Description = "Medical referral", DisplayOrder = 6, Icon = "file-text", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "Image", Description = "Medical image", DisplayOrder = 7, Icon = "image", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "Video", Description = "Medical video", DisplayOrder = 8, Icon = "video", CreatedById = systemUserId },
                new DocumentType { Id = Guid.NewGuid(), Name = "Audio", Description = "Medical audio", DisplayOrder = 9, Icon = "volume-2", CreatedById = systemUserId }
            };
            context.DocumentTypes.AddRange(documentTypes);
        }
        */

        if (!context.ReminderTypes.Any())
        {
            var reminderTypes = new List<ReminderType>
            {
                new ReminderType { Id = Guid.NewGuid(), Name = "Email", Description = "Email reminder", SortOrder = 1 },
                new ReminderType { Id = Guid.NewGuid(), Name = "SMS", Description = "SMS reminder", SortOrder = 2 },
                new ReminderType { Id = Guid.NewGuid(), Name = "PushNotification", Description = "Push notification", SortOrder = 3 },
                new ReminderType { Id = Guid.NewGuid(), Name = "InApp", Description = "In-app notification", SortOrder = 4 }
            };
            context.ReminderTypes.AddRange(reminderTypes);
        }

        if (!context.ReminderTimings.Any())
        {
            var reminderTimings = new List<ReminderTiming>
            {
                new ReminderTiming { Id = Guid.NewGuid(), Name = "Immediate", Description = "Send immediately", SortOrder = 1, MinutesBeforeAppointment = 0 },
                new ReminderTiming { Id = Guid.NewGuid(), Name = "OneHourBefore", Description = "Send 1 hour before appointment", SortOrder = 2, MinutesBeforeAppointment = 60 },
                new ReminderTiming { Id = Guid.NewGuid(), Name = "TwentyFourHoursBefore", Description = "Send 24 hours before appointment", SortOrder = 3, MinutesBeforeAppointment = 1440 },
                new ReminderTiming { Id = Guid.NewGuid(), Name = "OneWeekBefore", Description = "Send 1 week before appointment", SortOrder = 4, MinutesBeforeAppointment = 10080 }
            };
            context.ReminderTimings.AddRange(reminderTimings);
        }

        if (!context.EventTypes.Any())
        {
            var eventTypes = new List<EventType>
            {
                new EventType { Id = Guid.NewGuid(), Name = "AppointmentCreated", Description = "Appointment was created", SortOrder = 1 },
                new EventType { Id = Guid.NewGuid(), Name = "PaymentProcessed", Description = "Payment was processed", SortOrder = 2 },
                new EventType { Id = Guid.NewGuid(), Name = "ProviderNotified", Description = "Provider was notified", SortOrder = 3 },
                new EventType { Id = Guid.NewGuid(), Name = "ProviderAccepted", Description = "Provider accepted appointment", SortOrder = 4 },
                new EventType { Id = Guid.NewGuid(), Name = "ProviderRejected", Description = "Provider rejected appointment", SortOrder = 5 },
                new EventType { Id = Guid.NewGuid(), Name = "MeetingScheduled", Description = "Meeting was scheduled", SortOrder = 6 },
                new EventType { Id = Guid.NewGuid(), Name = "MeetingStarted", Description = "Meeting started", SortOrder = 7 },
                new EventType { Id = Guid.NewGuid(), Name = "MeetingEnded", Description = "Meeting ended", SortOrder = 8 },
                new EventType { Id = Guid.NewGuid(), Name = "AppointmentCompleted", Description = "Appointment completed", SortOrder = 9 },
                new EventType { Id = Guid.NewGuid(), Name = "AppointmentCancelled", Description = "Appointment cancelled", SortOrder = 10 },
                new EventType { Id = Guid.NewGuid(), Name = "PaymentRefunded", Description = "Payment refunded", SortOrder = 11 },
                new EventType { Id = Guid.NewGuid(), Name = "ReminderSent", Description = "Reminder sent", SortOrder = 12 },
                new EventType { Id = Guid.NewGuid(), Name = "DocumentUploaded", Description = "Document uploaded", SortOrder = 13 },
                new EventType { Id = Guid.NewGuid(), Name = "StatusChanged", Description = "Status changed", SortOrder = 14 }
            };
            context.EventTypes.AddRange(eventTypes);
        }

        context.SaveChanges();
    }

    public static void SeedTestData(ApplicationDbContext context)
    {
        // Get the seeded master data IDs
        var clientRole = context.UserRoles.FirstOrDefault(r => r.Name == "Client");
        var providerRole = context.UserRoles.FirstOrDefault(r => r.Name == "Provider");
        var adminRole = context.UserRoles.FirstOrDefault(r => r.Name == "Admin");

        if (clientRole == null || providerRole == null || adminRole == null)
        {
            throw new InvalidOperationException("Master data not seeded. Please run SeedMasterTables first.");
        }

        // Seed test users if they don't exist
        if (!context.Users.Any())
        {
            var testUsers = new List<User>
            {
                                 new User
                 {
                     FirstName = "John",
                     LastName = "Doe",
                     Email = "john.doe@example.com",
                     UserName = "john.doe@example.com",
                     PhoneNumber = "1234567890",
                     DateOfBirth = DateTime.UtcNow.AddYears(-30),
                     Gender = "Male",
                     Address = "123 Main St",
                     City = "New York",
                     State = "NY",
                     ZipCode = "10001",
                     IsActive = true,
                     UserRoleId = clientRole.Id
                 },
                 new User
                 {
                     FirstName = "Dr. Jane",
                     LastName = "Smith",
                     Email = "dr.jane.smith@example.com",
                     UserName = "dr.jane.smith@example.com",
                     PhoneNumber = "0987654321",
                     DateOfBirth = DateTime.UtcNow.AddYears(-35),
                     Gender = "Female",
                     Address = "456 Oak Ave",
                     City = "Los Angeles",
                     State = "CA",
                     ZipCode = "90210",
                     IsActive = true,
                     UserRoleId = providerRole.Id
                 },
                 new User
                 {
                     FirstName = "Admin",
                     LastName = "User",
                     Email = "admin@example.com",
                     UserName = "admin@example.com",
                     PhoneNumber = "5555555555",
                     DateOfBirth = DateTime.UtcNow.AddYears(-25),
                     Gender = "Other",
                     Address = "789 Admin Blvd",
                     City = "Chicago",
                     State = "IL",
                     ZipCode = "60601",
                     IsActive = true,
                     UserRoleId = adminRole.Id
                 }
            };

            context.Users.AddRange(testUsers);
            context.SaveChanges();
        }

        // Seed test categories if they don't exist
        if (!context.Categories.Any())
        {
            var testCategories = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Primary Care",
                    Description = "General health consultations",
                    BasePrice = 100.00m,
                    ConsultationFee = 100.00m,
                    OneTimeConsultationFee = 150.00m,
                    IsActive = true,
                    RequiresHealthAssessment = true,
                    AllowsMedicationDelivery = true,
                    AllowsFollowUpMessaging = true
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Mental Health",
                    Description = "Mental health and therapy services",
                    BasePrice = 150.00m,
                    ConsultationFee = 150.00m,
                    OneTimeConsultationFee = 200.00m,
                    IsActive = true,
                    RequiresHealthAssessment = true,
                    AllowsMedicationDelivery = true,
                    AllowsFollowUpMessaging = true
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Dermatology",
                    Description = "Skin and dermatological consultations",
                    BasePrice = 120.00m,
                    ConsultationFee = 120.00m,
                    OneTimeConsultationFee = 180.00m,
                    IsActive = true,
                    RequiresHealthAssessment = false,
                    AllowsMedicationDelivery = true,
                    AllowsFollowUpMessaging = true
                }
            };

            context.Categories.AddRange(testCategories);
            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        // Enable seeding to create default admin user
        // Seed master tables
        SeedMasterTables(context);
        
        // Create Identity roles if they don't exist
        var adminRoleName = "Admin";
        var clientRoleName = "Client";
        var providerRoleName = "Provider";
        
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new Role(adminRoleName) { Description = "System administrators" });
        }
        
        if (!await roleManager.RoleExistsAsync(clientRoleName))
        {
            await roleManager.CreateAsync(new Role(clientRoleName) { Description = "Regular users" });
        }
        
        if (!await roleManager.RoleExistsAsync(providerRoleName))
        {
            await roleManager.CreateAsync(new Role(providerRoleName) { Description = "Healthcare providers" });
        }
        
        // Create default admin user with proper Identity role assignment
        var adminEmail = "admin@test.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        
        if (existingAdmin == null)
        {
            var adminRole = context.UserRoles.FirstOrDefault(r => r.Name == "Admin");
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail,
                UserName = adminEmail,
                PhoneNumber = "1234567890",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "Other",
                Address = "Admin Address",
                City = "Admin City",
                State = "Admin State",
                ZipCode = "12345",
                IsActive = true,
                UserRoleId = adminRole?.Id ?? 1,
                UserType = "Admin"
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
        else
        {
            // Ensure existing admin has the role assigned
            var isInRole = await userManager.IsInRoleAsync(existingAdmin, adminRoleName);
            if (!isInRole)
            {
                await userManager.AddToRoleAsync(existingAdmin, adminRoleName);
            }
            
            // Update the UserType property for backward compatibility
            if (string.IsNullOrEmpty(existingAdmin.UserType) || existingAdmin.UserType != "Admin")
            {
                existingAdmin.UserType = "Admin";
                await userManager.UpdateAsync(existingAdmin);
            }
        }
    }
} 