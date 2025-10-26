-- Script to clear inventory, variants, and modifiers while keeping products
-- This will preserve your 500 products but reset inventory, variants, and modifiers

-- Disable foreign key checks temporarily
SET FOREIGN_KEY_CHECKS = 0;

-- Clear inventory-related tables
DELETE FROM InventoryTransactions;
DELETE FROM Inventories;

-- Clear product variants and related tables
DELETE FROM OrderItemModifiers; -- Clear order item modifiers first (foreign key dependency)
DELETE FROM ProductModifierGroups; -- Clear modifier groups first
DELETE FROM ProductModifierOptions; -- Clear modifier options
DELETE FROM ProductModifiers; -- Clear modifiers
DELETE FROM ProductVariants; -- Clear variants

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Optional: Reset auto-increment counters (uncomment if needed)
-- ALTER TABLE Inventories AUTO_INCREMENT = 1;
-- ALTER TABLE InventoryTransactions AUTO_INCREMENT = 1;
-- ALTER TABLE ProductVariants AUTO_INCREMENT = 1;
-- ALTER TABLE ProductModifiers AUTO_INCREMENT = 1;
-- ALTER TABLE ProductModifierOptions AUTO_INCREMENT = 1;
-- ALTER TABLE ProductModifierGroups AUTO_INCREMENT = 1;
-- ALTER TABLE OrderItemModifiers AUTO_INCREMENT = 1;

-- Show summary of what was cleared
SELECT 'Inventory cleared' as Status, COUNT(*) as RemainingRecords FROM Inventories
UNION ALL
SELECT 'InventoryTransactions cleared' as Status, COUNT(*) as RemainingRecords FROM InventoryTransactions
UNION ALL
SELECT 'ProductVariants cleared' as Status, COUNT(*) as RemainingRecords FROM ProductVariants
UNION ALL
SELECT 'ProductModifiers cleared' as Status, COUNT(*) as RemainingRecords FROM ProductModifiers
UNION ALL
SELECT 'ProductModifierOptions cleared' as Status, COUNT(*) as RemainingRecords FROM ProductModifierOptions
UNION ALL
SELECT 'ProductModifierGroups cleared' as Status, COUNT(*) as RemainingRecords FROM ProductModifierGroups
UNION ALL
SELECT 'OrderItemModifiers cleared' as Status, COUNT(*) as RemainingRecords FROM OrderItemModifiers
UNION ALL
SELECT 'Products preserved' as Status, COUNT(*) as RemainingRecords FROM Products;
