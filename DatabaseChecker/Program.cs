using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=localhost,1433;Initial Catalog=AccountingDB;user id=sa;password=Bus1989@123;TrustServerCertificate=True;";
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully!");
                
                // Query to get all table names
                string query = @"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE' 
                    ORDER BY TABLE_NAME";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("\nTables in AccountingDB:");
                        Console.WriteLine("=====================");
                        while (reader.Read())
                        {
                            Console.WriteLine($"- {reader["TABLE_NAME"]}");
                        }
                    }
                }
                
                // Check specifically for our new tables
                Console.WriteLine("\nChecking for specific tables:");
                Console.WriteLine("=============================");
                
                string[] targetTables = { "UserActivityLogs", "WorkingHours", "WorkingHoursExceptions" };
                
                foreach (string tableName in targetTables)
                {
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = @tableName AND TABLE_TYPE = 'BASE TABLE'";
                    
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@tableName", tableName);
                        int count = (int)checkCommand.ExecuteScalar();
                        Console.WriteLine($"{tableName}: {(count > 0 ? "EXISTS" : "NOT FOUND")}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}