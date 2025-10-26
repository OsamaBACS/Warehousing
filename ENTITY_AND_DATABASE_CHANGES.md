# Entity and Database Changes for Shared Stock Approach

## Overview
This document outlines the changes made to entities, DTOs, and database to implement the shared stock approach for product variants.

## Changes Made

### 1. **ProductVariant Entity Changes**
**File:** `Warehousing.Data/Entities/ProductVariant.cs`

**Removed:**
- `StockQuantity` property (line 29)

**Reason:** Variants now share the main product's stock through the Inventory table.

**Before:**
```csharp
// Inventory
public decimal? StockQuantity { get; set; }
public decimal? ReorderLevel { get; set; }
```

**After:**
```csharp
// Inventory - Shared stock approach (no individual stock quantity)
// Stock is managed through the main product's inventory
public decimal? ReorderLevel { get; set; }
```

### 2. **DTO Changes**

#### ProductVariantDto
**File:** `Warehousing.Repo/Dtos/ProductVariantDto.cs`

**Removed:**
- `StockQuantity` property

**Before:**
```csharp
public decimal? StockQuantity { get; set; }
```

**After:**
```csharp
// Removed StockQuantity - variants share main product stock
```

#### ProductVariantSimpleDto
**File:** `Warehousing.Repo/Dtos/ProductVariantSimpleDto.cs`

**Removed:**
- `StockQuantity` property

### 3. **Frontend Model Changes**

#### ProductVariant Interface
**File:** `Warehousing.UI/src/app/admin/models/ProductVariant.ts`

**Removed:**
- `stockQuantity` property from both `ProductVariant` and `ProductVariantCreateRequest` interfaces

**Before:**
```typescript
stockQuantity?: number;
```

**After:**
```typescript
// Removed stockQuantity - variants share main product stock
```

### 4. **Database Migration**

#### Migration File
**File:** `Warehousing.Data/Migrations/20250101000000_RemoveStockQuantityFromProductVariants.cs`

**Changes:**
- Removes `StockQuantity` column from `ProductVariants` table
- Provides rollback capability if needed

**SQL Generated:**
```sql
-- Up Migration
ALTER TABLE ProductVariants DROP COLUMN StockQuantity;

-- Down Migration (rollback)
ALTER TABLE ProductVariants ADD StockQuantity decimal(18,2) NULL;
```

### 5. **Inventory Entity (No Changes Needed)**
**File:** `Warehousing.Data/Entities/Inventory.cs`

The Inventory entity already supports variants through:
- `VariantId` property (nullable)
- `ProductId` property (required)
- `Quantity` property (shared stock)

This structure supports both approaches:
- **Shared Stock:** `VariantId` is null, stock is shared
- **Separate Stock:** `VariantId` has value, stock is variant-specific

## How Shared Stock Works

### 1. **Database Structure**
```sql
-- Main product inventory (shared by all variants)
Inventories:
- Id: 1, ProductId: 6, StoreId: 1, VariantId: NULL, Quantity: 200
- Id: 2, ProductId: 6, StoreId: 3, VariantId: NULL, Quantity: 10

-- Variants (no individual stock)
ProductVariants:
- Id: 1, ProductId: 6, Name: "Small", StockQuantity: NULL
- Id: 2, ProductId: 6, Name: "Medium", StockQuantity: NULL
- Id: 3, ProductId: 6, Name: "Large", StockQuantity: NULL
```

### 2. **Stock Management Flow**
1. **Stock Check:** Check main product's inventory (`VariantId = NULL`)
2. **Stock Update:** Update main product's inventory when any variant is sold
3. **Stock Display:** All variants show the same available quantity

### 3. **API Endpoints**
- `GET /api/products/{productId}/variant-stock` - Get stock info for a variant
- `GET /api/products/{productId}/variants-stock` - Get stock info for all variants
- `POST /api/products/{productId}/variant-stock` - Update stock (affects main product)

## Migration Steps

### 1. **Apply Database Migration**
```bash
cd Warehousing.Data
dotnet ef database update
```

### 2. **Update Existing Data**
If you have existing variants with stock quantities, you need to:
1. **Consolidate stock** into main product inventory
2. **Remove variant-specific** stock records
3. **Update any existing** stock validation logic

### 3. **Frontend Updates**
- Remove stock quantity fields from variant forms
- Update stock display to show "Shared Stock"
- Update stock validation to use main product inventory

## Benefits of These Changes

### ✅ **Advantages**
1. **Simplified Data Model:** No duplicate stock tracking
2. **Real-time Accuracy:** All variants reflect same stock level
3. **Easier Management:** Single inventory record per product per store
4. **Better Performance:** Fewer database queries for stock checks
5. **Consistent Reporting:** Single source of truth for stock levels

### ⚠️ **Considerations**
1. **Data Migration:** Need to consolidate existing variant stock
2. **Business Logic:** Update all stock validation to use main product
3. **Frontend Changes:** Remove stock quantity fields from variant forms
4. **Testing:** Thoroughly test stock validation and updates

## Rollback Plan

If you need to rollback to separate stock per variant:

1. **Revert Migration:**
   ```bash
   dotnet ef database update <previous-migration>
   ```

2. **Restore Entity:**
   - Add back `StockQuantity` property to `ProductVariant`
   - Update DTOs and frontend models
   - Update business logic for separate stock

3. **Data Recovery:**
   - Restore variant-specific stock quantities
   - Update inventory records to use `VariantId`

## Testing Checklist

- [ ] Variant creation without stock quantity field
- [ ] Stock validation using main product inventory
- [ ] Stock updates affecting main product
- [ ] Low stock alerts for variants
- [ ] Order processing with variant stock validation
- [ ] Inventory reporting accuracy
- [ ] Stock adjustments and transfers

## Conclusion

These changes implement a clean shared stock approach that:
- Simplifies inventory management
- Provides real-time stock accuracy
- Reduces data redundancy
- Improves system performance
- Maintains audit trails through inventory transactions

The migration is designed to be reversible if needed, but the shared stock approach is recommended for most retail scenarios.
