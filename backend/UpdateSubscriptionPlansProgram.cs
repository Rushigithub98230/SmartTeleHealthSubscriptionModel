using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Data;

/// <summary>
/// Console application to update existing SubscriptionPlans with valid CategoryId values.
/// Run this before applying the migration that adds the CategoryId foreign key.
/// </summary>
public class UpdateSubscriptionPlansProgram
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting SubscriptionPlans CategoryId update process...");
        Console.WriteLine("This script will update existing SubscriptionPlans with valid CategoryId values.");
        Console.WriteLine();
        
        try
        {
            await UpdateSubscriptionPlansWithCategories.UpdateSubscriptionPlansAsync();
            Console.WriteLine();
            Console.WriteLine("✅ SubscriptionPlans update completed successfully!");
            Console.WriteLine("You can now apply the migration to add the CategoryId foreign key.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ Error updating SubscriptionPlans: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}


