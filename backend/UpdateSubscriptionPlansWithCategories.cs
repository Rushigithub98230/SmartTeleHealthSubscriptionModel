using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Infrastructure.Data;

/// <summary>
/// Script to update existing SubscriptionPlans with valid CategoryId values.
/// This script should be run before applying the migration that adds the CategoryId foreign key.
/// </summary>
public class UpdateSubscriptionPlansWithCategories
{
    /// <summary>
    /// Updates existing SubscriptionPlans with valid CategoryId values based on plan names or descriptions.
    /// </summary>
    public static async Task UpdateSubscriptionPlansAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SmartTelehealthDb;Trusted_Connection=true;MultipleActiveResultSets=true");
        
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        try
        {
            // Ensure categories exist
            await EnsureCategoriesExistAsync(context);
            
            // Get all categories
            var categories = await context.Categories.ToListAsync();
            var primaryCareCategory = categories.FirstOrDefault(c => c.Name == "Primary Care");
            var mentalHealthCategory = categories.FirstOrDefault(c => c.Name == "Mental Health");
            var dermatologyCategory = categories.FirstOrDefault(c => c.Name == "Dermatology");
            
            if (primaryCareCategory == null || mentalHealthCategory == null || dermatologyCategory == null)
            {
                throw new InvalidOperationException("Required categories not found. Please ensure categories are seeded first.");
            }
            
            // Get all existing subscription plans
            var subscriptionPlans = await context.SubscriptionPlans.ToListAsync();
            
            Console.WriteLine($"Found {subscriptionPlans.Count} subscription plans to update.");
            
            foreach (var plan in subscriptionPlans)
            {
                // Determine category based on plan name or description
                var categoryId = DetermineCategoryId(plan, primaryCareCategory.Id, mentalHealthCategory.Id, dermatologyCategory.Id);
                
                if (categoryId.HasValue)
                {
                    plan.CategoryId = categoryId.Value;
                    Console.WriteLine($"Updated plan '{plan.Name}' with category ID: {categoryId.Value}");
                }
                else
                {
                    // Default to Primary Care if no specific category can be determined
                    plan.CategoryId = primaryCareCategory.Id;
                    Console.WriteLine($"Updated plan '{plan.Name}' with default category (Primary Care)");
                }
            }
            
            // Save changes
            await context.SaveChangesAsync();
            Console.WriteLine($"Successfully updated {subscriptionPlans.Count} subscription plans with category IDs.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating subscription plans: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Ensures that the required categories exist in the database.
    /// </summary>
    private static async Task EnsureCategoriesExistAsync(ApplicationDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            Console.WriteLine("No categories found. Creating default categories...");
            
            var categories = new List<Category>
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
            
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Default categories created successfully.");
        }
    }
    
    /// <summary>
    /// Determines the appropriate category ID based on the subscription plan name or description.
    /// </summary>
    private static Guid? DetermineCategoryId(SubscriptionPlan plan, Guid primaryCareId, Guid mentalHealthId, Guid dermatologyId)
    {
        var planName = plan.Name?.ToLowerInvariant() ?? "";
        var planDescription = plan.Description?.ToLowerInvariant() ?? "";
        
        // Mental Health keywords
        if (planName.Contains("mental") || planName.Contains("therapy") || planName.Contains("psychology") ||
            planName.Contains("counseling") || planName.Contains("anxiety") || planName.Contains("depression") ||
            planDescription.Contains("mental") || planDescription.Contains("therapy") || planDescription.Contains("psychology") ||
            planDescription.Contains("counseling") || planDescription.Contains("anxiety") || planDescription.Contains("depression"))
        {
            return mentalHealthId;
        }
        
        // Dermatology keywords
        if (planName.Contains("dermatology") || planName.Contains("skin") || planName.Contains("dermatologist") ||
            planDescription.Contains("dermatology") || planDescription.Contains("skin") || planDescription.Contains("dermatologist"))
        {
            return dermatologyId;
        }
        
        // Primary Care keywords (or default)
        if (planName.Contains("primary") || planName.Contains("general") || planName.Contains("basic") ||
            planName.Contains("standard") || planName.Contains("premium") || planName.Contains("elite") ||
            planDescription.Contains("primary") || planDescription.Contains("general") || planDescription.Contains("basic") ||
            planDescription.Contains("standard") || planDescription.Contains("premium") || planDescription.Contains("elite"))
        {
            return primaryCareId;
        }
        
        // Default to Primary Care if no specific keywords are found
        return primaryCareId;
    }
}


