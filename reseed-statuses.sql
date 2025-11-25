-- Script to truncate and reseed Statuses table with correct data
-- This will delete all existing statuses and insert the correct ones

-- Step 1: Set all Order.StatusId to NULL temporarily to avoid foreign key constraint issues
UPDATE Orders SET StatusId = NULL WHERE StatusId IS NOT NULL;

-- Step 2: Delete all existing statuses
DELETE FROM Statuses;

-- Step 3: Reset the identity seed (if using IDENTITY column)
DBCC CHECKIDENT ('Statuses', RESEED, 0);

-- Step 4: Insert the correct statuses with fixed data
SET IDENTITY_INSERT Statuses ON;

INSERT INTO Statuses (Id, Code, NameEn, NameAr, Description) VALUES
(1, 'PENDING', 'Pending', 'قيد الانتظار', 'Order is created but not processed yet'),
(2, 'PROCESSING', 'Processing', 'جاري المعالجة', 'Order is being prepared or reviewed'),
(3, 'CONFIRMED', 'Confirmed', 'مؤكد', 'Order has been confirmed by the supplier/customer'),
(4, 'SHIPPED', 'Shipped', 'تم الشحن', 'Goods have been dispatched'),
(5, 'DELIVERED', 'Delivered', 'تم التسليم', 'Goods have been successfully delivered'),
(6, 'CANCELLED', 'Cancelled', 'تم الإلغاء', 'Order was cancelled'),
(7, 'RETURNED', 'Returned', 'تم الإرجاع', 'Goods were returned after delivery'),
(8, 'COMPLETED', 'Completed', 'مكتمل', 'Order completed successfully'),
(9, 'ONHOLD', 'On Hold', 'معلق', 'Order temporarily paused'),
(10, 'FAILED', 'Failed', 'فشل', 'Order failed due to payment or stock issue'),
(11, 'DRAFT', 'Save as draft', 'حفظ كمسودة', 'Order is saved but not submitted');

SET IDENTITY_INSERT Statuses OFF;

-- Step 5: Optionally restore StatusId in Orders table if you want to keep existing order-status relationships
-- Note: You may want to update existing orders manually after this
-- UPDATE Orders SET StatusId = 1 WHERE StatusId IS NULL AND ... (your conditions)

-- Verify the data
SELECT * FROM Statuses ORDER BY Id;

