-- SAFE Script to truncate and reseed Statuses table with correct data
-- This script handles foreign key constraints properly

USE [WarehousingDB];  -- Change this to your database name if different
GO

-- Step 1: Temporarily disable foreign key constraints
ALTER TABLE Orders NOCHECK CONSTRAINT ALL;

-- Step 2: Set all Order.StatusId to NULL to avoid foreign key constraint issues
UPDATE Orders SET StatusId = NULL WHERE StatusId IS NOT NULL;

-- Step 3: Delete all existing statuses
DELETE FROM Statuses;

-- Step 4: Reset the identity seed (if using IDENTITY column)
DBCC CHECKIDENT ('Statuses', RESEED, 0);
GO

-- Step 5: Insert the correct statuses
-- Note: Since Id is likely an IDENTITY column, we'll let SQL Server auto-generate the IDs
-- But if you need specific IDs, uncomment the SET IDENTITY_INSERT lines below

-- Option A: Let SQL Server auto-generate IDs (RECOMMENDED)
INSERT INTO Statuses (Code, NameEn, NameAr, Description) VALUES
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
('DRAFT', 'Save as draft', 'حفظ كمسودة', 'Order is saved but not submitted');
GO

-- Option B: If you need specific IDs (uncomment below and comment Option A above)
/*
SET IDENTITY_INSERT Statuses ON;
GO

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
GO
*/

-- Step 6: Re-enable foreign key constraints
ALTER TABLE Orders CHECK CONSTRAINT ALL;
GO

-- Step 7: Verify the data
SELECT * FROM Statuses ORDER BY Id;
GO

-- Note: After running this script, you may want to update existing Orders.StatusId 
-- to point to the new status IDs if needed

