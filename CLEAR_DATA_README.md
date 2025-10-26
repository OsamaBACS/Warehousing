# Clear Inventory, Variants, and Modifiers Scripts

This directory contains scripts to safely clear inventory, product variants, and modifiers while preserving your 500 products.

## 🎯 What These Scripts Do

- ✅ **Preserves**: All your 500 products and their basic information
- 🗑️ **Clears**: Inventory, variants, modifiers, and related data
- 🔒 **Safe**: Respects foreign key constraints and data integrity

## 📁 Available Scripts

### 1. SQL Script (Direct Database)
**File**: `clear_inventory_variants_modifiers.sql`
- Run directly in your database management tool (SSMS, MySQL Workbench, etc.)
- Most reliable method
- Shows summary of cleared data

### 2. .NET API Controller
**File**: `Warehousing.Api/Controllers/ClearDataController.cs`
- Programmatic approach through your API
- Built into your application
- Provides detailed response with counts

### 3. PowerShell Script (Windows)
**File**: `clear_data.ps1`
- Windows PowerShell script
- Requires SQL Server PowerShell module
- Update connection parameters before running

## 🚀 How to Use

### Option 1: SQL Script (Recommended)
1. Open your database management tool
2. Connect to your Warehousing database
3. Run the `clear_inventory_variants_modifiers.sql` script
4. Check the summary output

### Option 2: .NET API (Programmatic)
1. Start your API server
2. Make a POST request to: `http://localhost:5036/api/ClearData/clear-inventory-variants-modifiers`
3. Check the response for confirmation

### Option 3: PowerShell (Windows)
1. Update connection parameters in `clear_data.ps1`
2. Run: `.\clear_data.ps1`
3. Check the output for success confirmation

## 📊 What Gets Cleared

| Table | Purpose | Status |
|-------|---------|--------|
| `Inventories` | Product stock per store | ✅ Cleared |
| `InventoryTransactions` | Stock movement history | ✅ Cleared |
| `ProductVariants` | Product variations | ✅ Cleared |
| `ProductModifiers` | Product modifiers | ✅ Cleared |
| `ProductModifierOptions` | Modifier options | ✅ Cleared |
| `ProductModifierGroups` | Modifier groups | ✅ Cleared |
| `OrderItemModifiers` | Order item modifiers | ✅ Cleared |
| `Products` | Your 500 products | 🔒 **PRESERVED** |

## ⚠️ Important Notes

1. **Backup First**: Always backup your database before running these scripts
2. **Test Environment**: Test on a copy of your database first
3. **Foreign Keys**: Scripts handle foreign key constraints properly
4. **Irreversible**: This action cannot be undone

## 🔍 Verification

After running any script, verify the results:

```sql
-- Check that products are preserved
SELECT COUNT(*) as ProductCount FROM Products;

-- Check that inventory/variants are cleared
SELECT COUNT(*) as InventoryCount FROM Inventories;
SELECT COUNT(*) as VariantCount FROM ProductVariants;
SELECT COUNT(*) as ModifierCount FROM ProductModifiers;
```

## 🎯 Next Steps

After clearing the data:
1. Your 500 products will remain intact
2. You can start fresh with inventory management
3. Create new variants and modifiers as needed
4. Set up stock quantities using the new inventory-based system

## 🆘 Troubleshooting

If you encounter issues:
1. Check foreign key constraints
2. Ensure you have proper database permissions
3. Verify connection parameters
4. Check database logs for errors

## 📞 Support

If you need help with these scripts, check:
- Database connection settings
- User permissions
- Foreign key relationships
- Table structure integrity
