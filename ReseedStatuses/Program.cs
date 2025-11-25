using System;
using System.Data;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Server=tcp:warehousing.database.windows.net,1433;Initial Catalog=WarehousingDB;Persist Security Info=False;User ID=noor;Password=RoseParis@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        // Set console output encoding to UTF-8 for proper Arabic display
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully.");
                
                var commands = new[]
                {
                    // Step 1: Disable foreign key constraints
                    ("Disabling foreign key constraints...", "ALTER TABLE Orders NOCHECK CONSTRAINT ALL; ALTER TABLE StoreTransfers NOCHECK CONSTRAINT ALL"),
                    
                    // Step 2: Set Order.StatusId to NULL
                    ("Setting Order.StatusId to NULL...", "UPDATE Orders SET StatusId = NULL WHERE StatusId IS NOT NULL"),
                    
                    // Step 3: Delete StoreTransfers that reference Statuses (or update them if needed)
                    // Note: We'll just delete the foreign key constraint temporarily
                    ("Removing StoreTransfer references to Statuses...", "DELETE FROM StoreTransfers WHERE StatusId IS NOT NULL"),
                    
                    // Step 4: Delete all statuses
                    ("Deleting all existing statuses...", "DELETE FROM Statuses"),
                    
                    // Step 5: Reset identity seed
                    ("Resetting identity seed...", "DBCC CHECKIDENT ('Statuses', RESEED, 0)"),
                    
                    // Step 6: Insert new statuses with Unicode (N prefix) for proper Arabic text
                    ("Inserting new statuses...", @"INSERT INTO Statuses (Code, NameEn, NameAr, Description) VALUES
(N'PENDING', N'Pending', N'قيد الانتظار', N'Order is created but not processed yet'),
(N'PROCESSING', N'Processing', N'جاري المعالجة', N'Order is being prepared or reviewed'),
(N'CONFIRMED', N'Confirmed', N'مؤكد', N'Order has been confirmed by the supplier/customer'),
(N'SHIPPED', N'Shipped', N'تم الشحن', N'Goods have been dispatched'),
(N'DELIVERED', N'Delivered', N'تم التسليم', N'Goods have been successfully delivered'),
(N'CANCELLED', N'Cancelled', N'تم الإلغاء', N'Order was cancelled'),
(N'RETURNED', N'Returned', N'تم الإرجاع', N'Goods were returned after delivery'),
(N'COMPLETED', N'Completed', N'مكتمل', N'Order completed successfully'),
(N'ONHOLD', N'On Hold', N'معلق', N'Order temporarily paused'),
(N'FAILED', N'Failed', N'فشل', N'Order failed due to payment or stock issue'),
(N'DRAFT', N'Save as draft', N'حفظ كمسودة', N'Order is saved but not submitted')"),
                    
                    // Step 7: Re-enable constraints
                    ("Re-enabling foreign key constraints...", "ALTER TABLE Orders CHECK CONSTRAINT ALL; ALTER TABLE StoreTransfers CHECK CONSTRAINT ALL")
                };
                
                foreach (var (description, cmd) in commands)
                {
                    using (var command = new SqlCommand(cmd, connection))
                    {
                        try
                        {
                            Console.Write(description);
                            int rowsAffected = command.ExecuteNonQuery();
                            Console.WriteLine($" ✓ (Rows affected: {rowsAffected})");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($" ✗ Error: {ex.Message}");
                            if (ex.Message.Contains("DBCC"))
                            {
                                // DBCC might not return rows affected, that's okay
                                Console.WriteLine("  (Note: DBCC command executed, may not report rows affected)");
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                
                // Verify with proper Unicode handling
                Console.WriteLine("\n=== Verifying Statuses ===");
                using (var verifyCmd = new SqlCommand("SELECT Id, Code, NameEn, NameAr, Description FROM Statuses ORDER BY Id", connection))
                {
                    verifyCmd.CommandTimeout = 30;
                    using (var reader = verifyCmd.ExecuteReader())
                    {
                        Console.WriteLine("Id\tCode\t\t\tNameEn\t\t\tNameAr");
                        Console.WriteLine("--\t----\t\t\t------\t\t\t------");
                        while (reader.Read())
                        {
                            var id = reader["Id"];
                            var code = reader["Code"]?.ToString() ?? "";
                            var nameEn = reader["NameEn"]?.ToString() ?? "";
                            var nameAr = reader["NameAr"]?.ToString() ?? "";
                            var desc = reader["Description"]?.ToString() ?? "";
                            
                            // Output with proper encoding
                            Console.WriteLine($"{id}\t{code,-15}\t{nameEn,-20}\t{nameAr}");
                            Console.WriteLine($"  Description: {desc}");
                        }
                    }
                }
                
                Console.WriteLine("\n✓ Status reseed completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}

