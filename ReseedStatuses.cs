using System;
using System.Data;
using Microsoft.Data.SqlClient;

class ReseedStatuses
{
    static void Main(string[] args)
    {
        string connectionString = "Server=tcp:warehousing.database.windows.net,1433;Initial Catalog=WarehousingDB;Persist Security Info=False;User ID=noor;Password=RoseParis@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully.");
                
                var commands = new[]
                {
                    // Step 1: Disable foreign key constraints
                    "ALTER TABLE Orders NOCHECK CONSTRAINT ALL",
                    
                    // Step 2: Set StatusId to NULL
                    "UPDATE Orders SET StatusId = NULL WHERE StatusId IS NOT NULL",
                    
                    // Step 3: Delete all statuses
                    "DELETE FROM Statuses",
                    
                    // Step 4: Reset identity seed
                    "DBCC CHECKIDENT ('Statuses', RESEED, 0)",
                    
                    // Step 5: Insert new statuses
                    @"INSERT INTO Statuses (Code, NameEn, NameAr, Description) VALUES
('PENDING', 'Pending', 'قيد الانتظار', 'Order is created but not processed yet'),
('PROCESSING', 'Processing', 'جاري المعالجة', 'Order is being prepared or reviewed'),
('CONFIRMED', 'Confirmed', 'مؤكد', 'Order has been confirmed by the supplier/customer'),
('SHIPPED', 'Shipped', 'تم الشحن', 'Goods have been dispatched'),
('DELIVERED', 'Delivered', 'تم التسليم', 'Goods have been successfully delivered'),
('CANCELLED', 'Cancelled', 'تم الإلغاء', 'Order was cancelled'),
('RETURNED', 'Returned', 'تم الإرجاع', 'Goods were returned after delivery'),
('COMPLETED', 'Completed', 'مكتمل', 'Order completed successfully'),
('ONHOLD', 'On Hold', 'معلق', 'Order temporarily paused'),
('FAILED', 'Failed', 'فشل', 'Order failed due to payment or stock issue'),
('DRAFT', 'Save as draft', 'حفظ كمسودة', 'Order is saved but not submitted')",
                    
                    // Step 6: Re-enable constraints
                    "ALTER TABLE Orders CHECK CONSTRAINT ALL"
                };
                
                foreach (var cmd in commands)
                {
                    using (var command = new SqlCommand(cmd, connection))
                    {
                        try
                        {
                            if (cmd.StartsWith("DBCC"))
                            {
                                // DBCC commands might return results
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                int rowsAffected = command.ExecuteNonQuery();
                                Console.WriteLine($"Executed: {cmd.Substring(0, Math.Min(50, cmd.Length))}... (Rows affected: {rowsAffected})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing command: {ex.Message}");
                            Console.WriteLine($"Command: {cmd.Substring(0, Math.Min(100, cmd.Length))}...");
                        }
                    }
                }
                
                // Verify
                using (var verifyCmd = new SqlCommand("SELECT Id, Code, NameEn, NameAr, Description FROM Statuses ORDER BY Id", connection))
                {
                    using (var reader = verifyCmd.ExecuteReader())
                    {
                        Console.WriteLine("\n=== Statuses after reseed ===");
                        Console.WriteLine("Id\tCode\t\tNameEn\t\t\tNameAr");
                        Console.WriteLine("--\t----\t\t------\t\t\t------");
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader["Id"]}\t{reader["Code"]}\t\t{reader["NameEn"]}\t\t{reader["NameAr"]}");
                        }
                    }
                }
                
                Console.WriteLine("\nStatus reseed completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}

