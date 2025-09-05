using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.DataUpdater;

/// <summary>
/// Console application to update existing SubscriptionPlans with valid CategoryId values.
/// This should be run before applying the migration that adds the CategoryId foreign key.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting SubscriptionPlans CategoryId update and test process...");
        Console.WriteLine("This script will update existing SubscriptionPlans and test the CategoryId relationship.");
        Console.WriteLine();
        
        try
        {
            await UpdateSubscriptionPlansAsync();
            Console.WriteLine();
            Console.WriteLine("✅ SubscriptionPlans update completed successfully!");
            
            // Test the category functionality
            Console.WriteLine();
            Console.WriteLine("Testing Category-SubscriptionPlan relationship...");
            await TestCategoryRelationshipAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
    
    /// <summary>
    /// Updates existing SubscriptionPlans with valid CategoryId values based on plan names or descriptions.
    /// </summary>
    private static async Task UpdateSubscriptionPlansAsync()
    {
        // You may need to update this connection string based on your database configuration
        var connectionString = "Server=SDN-153\\SQLEXPRESS2022;Database=SmartTelehealthDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
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
    
    /// <summary>
    /// Tests the category functionality by creating test subscription plans and verifying the relationship.
    /// </summary>
    private static async Task TestCategoryRelationshipAsync()
    {
        var connectionString = "Server=SDN-153\\SQLEXPRESS2022;Database=SmartTelehealthDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        try
        {
            Console.WriteLine("Testing Category-SubscriptionPlan relationship...");
            
            // Get categories
            var categories = await context.Categories.ToListAsync();
            Console.WriteLine($"Found {categories.Count} categories:");
            foreach (var category in categories)
            {
                Console.WriteLine($"- {category.Name} (ID: {category.Id})");
            }
            
            if (categories.Count == 0)
            {
                Console.WriteLine("No categories found. Please ensure categories are seeded first.");
                return;
            }
            
            var primaryCareCategory = categories.FirstOrDefault(c => c.Name == "Primary Care");
            var mentalHealthCategory = categories.FirstOrDefault(c => c.Name == "Mental Health");
            
            if (primaryCareCategory == null || mentalHealthCategory == null)
            {
                Console.WriteLine("Required categories not found.");
                return;
            }
            
            // Get existing subscription plans
            var existingPlans = await context.SubscriptionPlans.ToListAsync();
            Console.WriteLine($"Found {existingPlans.Count} existing subscription plans in database.");
            
            // Test category-based plan retrieval
            var primaryCarePlans = await context.SubscriptionPlans
                .Include(sp => sp.Category)
                .Where(sp => sp.CategoryId == primaryCareCategory.Id)
                .ToListAsync();
            
            var mentalHealthPlans = await context.SubscriptionPlans
                .Include(sp => sp.Category)
                .Where(sp => sp.CategoryId == mentalHealthCategory.Id)
                .ToListAsync();
            
            Console.WriteLine($"\nPrimary Care Plans ({primaryCarePlans.Count}):");
            foreach (var plan in primaryCarePlans)
            {
                Console.WriteLine($"- {plan.Name} (Category: {plan.Category?.Name})");
            }
            
            Console.WriteLine($"\nMental Health Plans ({mentalHealthPlans.Count}):");
            foreach (var plan in mentalHealthPlans)
            {
                Console.WriteLine($"- {plan.Name} (Category: {plan.Category?.Name})");
            }
            
            // Test navigation property
            var allPlansWithCategories = await context.SubscriptionPlans
                .Include(sp => sp.Category)
                .ToListAsync();
            
            Console.WriteLine($"\nAll Plans with Categories ({allPlansWithCategories.Count}):");
            foreach (var plan in allPlansWithCategories)
            {
                Console.WriteLine($"- {plan.Name} -> Category: {plan.Category?.Name ?? "No Category"}");
            }
            
            Console.WriteLine("\n✅ Category-SubscriptionPlan relationship test completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error testing category functionality: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
