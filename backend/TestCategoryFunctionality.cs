using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Infrastructure.Data;

/// <summary>
/// Test class to verify that the CategoryId relationship is working correctly.
/// </summary>
public class TestCategoryFunctionality
{
    /// <summary>
    /// Tests the category functionality by creating test subscription plans and verifying the relationship.
    /// </summary>
    public static async Task TestCategoryRelationshipAsync()
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
            
            // Create test subscription plans
            var testPlans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic Primary Care Plan",
                    Description = "Basic primary care subscription plan",
                    Price = 29.99m,
                    CategoryId = primaryCareCategory.Id,
                    BillingCycleId = Guid.NewGuid(), // This would normally be a valid billing cycle ID
                    CurrencyId = Guid.NewGuid(), // This would normally be a valid currency ID
                    IsActive = true,
                    DisplayOrder = 1
                },
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Premium Mental Health Plan",
                    Description = "Premium mental health and therapy subscription plan",
                    Price = 79.99m,
                    CategoryId = mentalHealthCategory.Id,
                    BillingCycleId = Guid.NewGuid(), // This would normally be a valid billing cycle ID
                    CurrencyId = Guid.NewGuid(), // This would normally be a valid currency ID
                    IsActive = true,
                    DisplayOrder = 2
                }
            };
            
            // Add test plans to database
            context.SubscriptionPlans.AddRange(testPlans);
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {testPlans.Count} test subscription plans.");
            
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


