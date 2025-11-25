-- Clean script to truncate and reseed Statuses table
-- This version removes GO statements for Azure CLI execution

-- Step 1: Temporarily disable foreign key constraints on Orders table
ALTER TABLE Orders NOCHECK CONSTRAINT ALL;

-- Step 2: Set all Order.StatusId to NULL to avoid foreign key constraint issues
UPDATE Orders SET StatusId = NULL WHERE StatusId IS NOT NULL;

-- Step 3: Delete all existing statuses
DELETE FROM Statuses;

-- Step 4: Reset the identity seed
DBCC CHECKIDENT ('Statuses', RESEED, 0);

-- Step 5: Insert the correct statuses (letting SQL auto-generate IDs)
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

-- Step 6: Re-enable foreign key constraints
ALTER TABLE Orders CHECK CONSTRAINT ALL;

-- Step 7: Verify the data
SELECT Id, Code, NameEn, NameAr, Description FROM Statuses ORDER BY Id;

