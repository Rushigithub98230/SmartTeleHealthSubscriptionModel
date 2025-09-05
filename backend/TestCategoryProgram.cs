using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Data;

/// <summary>
/// Console application to test the Category-SubscriptionPlan relationship functionality.
/// </summary>
public class TestCategoryProgram
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Category-SubscriptionPlan relationship functionality...");
        Console.WriteLine();
        
        try
        {
            await TestCategoryFunctionality.TestCategoryRelationshipAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}


