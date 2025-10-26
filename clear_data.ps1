# PowerShell script to clear inventory, variants, and modifiers
# This script will help you clear the data while preserving your 500 products

Write-Host "=== Clearing Inventory, Variants, and Modifiers ===" -ForegroundColor Yellow
Write-Host "This will preserve your products but clear inventory, variants, and modifiers" -ForegroundColor Cyan

# Database connection parameters (update these with your actual values)
$server = "localhost"  # Update with your server name
$database = "Warehousing"  # Update with your database name
$username = "your_username"  # Update with your username
$password = "your_password"  # Update with your password

# SQL commands to execute
$sqlCommands = @"
-- Disable foreign key checks temporarily
SET FOREIGN_KEY_CHECKS = 0;

-- Clear inventory-related tables
DELETE FROM InventoryTransactions;
DELETE FROM Inventories;

-- Clear product variants and related tables
DELETE FROM OrderItemModifiers;
DELETE FROM ProductModifierGroups;
DELETE FROM ProductModifierOptions;
DELETE FROM ProductModifiers;
DELETE FROM ProductVariants;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Show summary
SELECT 'Data cleared successfully' as Status;
"@

try {
    # Create connection string
    $connectionString = "Server=$server;Database=$database;User Id=$username;Password=$password;"
    
    # Execute SQL commands
    Write-Host "Executing SQL commands..." -ForegroundColor Green
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlCommands
    
    Write-Host "✅ Successfully cleared inventory, variants, and modifiers!" -ForegroundColor Green
    Write-Host "✅ Your 500 products have been preserved." -ForegroundColor Green
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check your database connection parameters." -ForegroundColor Yellow
}

Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
