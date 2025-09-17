using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartTelehealth.DatabaseFix
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Connection string - adjust as needed
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=SmartTelehealthDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;";
            
            Console.WriteLine("Fixing database schema - Adding missing columns...");
            
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                // Read and execute the SQL script
                string sqlScript = await File.ReadAllTextAsync("AddMissingColumns.sql");
                
                using var command = new SqlCommand(sqlScript, connection);
                await command.ExecuteNonQueryAsync();
                
                Console.WriteLine("✅ Database schema fixed successfully!");
                Console.WriteLine("✅ Missing columns have been added:");
                Console.WriteLine("   - PlanType column added to SubscriptionPlans table");
                Console.WriteLine("   - UnitCost column added to SubscriptionPlanPrivileges table");
                Console.WriteLine("   - PrivilegeTypeId column verified/added to Privileges table");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fixing database schema: {ex.Message}");
                Console.WriteLine("Please run the AddMissingColumns.sql script manually in SQL Server Management Studio.");
            }
        }
    }
}
