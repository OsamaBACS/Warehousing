# Variant Stock Management - Shared Stock Approach

## Overview

This document explains how variant stock management works in the shared stock approach, where all variants of a product share the same inventory pool.

## How It Works

### 1. **Shared Stock Concept**
- **One inventory record** per product per store
- **All variants share** the same stock quantity
- When any variant is sold, it reduces the main product's inventory
- **Simpler management** and more common in retail

### 2. **Example Scenario**
```
Product: T-Shirt (ID: 6)
- Main Stock: 200 units in Store 1, 10 units in Store 3
- Variants: Small, Medium, Large, XL

When a customer buys:
- 1x Small T-Shirt → Main stock becomes 199 (Store 1) or 9 (Store 3)
- 1x Large T-Shirt → Main stock becomes 198 (Store 1) or 8 (Store 3)
```

### 3. **Database Structure**
```sql
-- Main inventory table (shared by all variants)
Inventories:
- ProductId: 6, StoreId: 1, Quantity: 200
- ProductId: 6, StoreId: 3, Quantity: 10

-- Variants table (no stock quantity field)
ProductVariants:
- Id: 1, ProductId: 6, Name: "Small", PriceAdjustment: 0
- Id: 2, ProductId: 6, Name: "Medium", PriceAdjustment: 0
- Id: 3, ProductId: 6, Name: "Large", PriceAdjustment: 5.00
- Id: 4, ProductId: 6, Name: "XL", PriceAdjustment: 10.00
```

## Implementation Details

### 1. **Frontend Changes**
- Removed `stockQuantity` field from variant forms
- Added informational message about shared stock
- Updated variant list to show "Shared Stock" instead of individual quantities

### 2. **Backend API Endpoints**
- `GET /api/products/{productId}/variant-stock` - Get stock info for a variant
- `GET /api/products/{productId}/variants-stock` - Get stock info for all variants
- `POST /api/products/{productId}/variant-stock` - Update stock (affects main product)
- `GET /api/products/{productId}/low-stock-variants` - Get low stock variants

### 3. **Stock Validation**
```typescript
// Validate stock before selling a variant
validateVariantStock(productId: 6, variantId: 2, storeId: 1, quantity: 5)
// This checks the main product's inventory, not the variant's
```

### 4. **Stock Updates**
```typescript
// When a variant is sold, update main product stock
updateVariantStock(productId: 6, variantId: 2, storeId: 1, quantityChange: -1)
// This reduces the main product's inventory by 1
```

## Benefits of Shared Stock Approach

### ✅ **Advantages**
1. **Simpler Management**: One stock level to track per product
2. **Real-time Accuracy**: All variants reflect the same available quantity
3. **Easier Reporting**: Single inventory record per product per store
4. **Common in Retail**: Most e-commerce platforms use this approach
5. **Automatic Sync**: No need to sync stock between variants

### ❌ **Limitations**
1. **No Variant-Specific Stock**: Can't have different stock levels per variant
2. **Less Granular Control**: Can't track which variant is selling more
3. **Reorder Level Complexity**: All variants use the same reorder level

## Alternative: Separate Stock per Variant

If you need variant-specific stock levels, you would:

1. **Create separate inventory records** for each variant
2. **Use VariantId** in the inventory table
3. **Track stock independently** for each variant
4. **More complex** but allows variant-specific stock levels

## Usage Examples

### 1. **Creating a Variant**
```typescript
// Create variant without stock quantity
const variantData = {
  productId: 6,
  name: "Large",
  priceAdjustment: 5.00,
  storeId: 1  // Required for inventory tracking
};
```

### 2. **Checking Stock**
```typescript
// Check if variant is available
const stockInfo = await variantStockService.getVariantStockInfo(6, 2, 1);
console.log(`Available: ${stockInfo.availableQuantity}`);
console.log(`Low Stock: ${stockInfo.isLowStock}`);
```

### 3. **Selling a Variant**
```typescript
// Validate stock before sale
const isValid = await variantStockService.validateVariantStock(6, 2, 1, 1);
if (isValid) {
  // Process sale and update stock
  await variantStockService.updateVariantStock(6, 2, 1, -1, "Sale of Large T-Shirt");
}
```

## Migration from Separate Stock

If you're migrating from separate stock per variant:

1. **Remove** `StockQuantity` from ProductVariant entity
2. **Update** frontend to remove stock quantity fields
3. **Consolidate** existing variant stock into main product stock
4. **Update** all stock validation logic to use main product inventory

## Best Practices

1. **Always validate stock** before processing orders
2. **Use reorder levels** to trigger low stock alerts
3. **Track inventory transactions** for audit purposes
4. **Implement stock reservations** for pending orders
5. **Regular stock audits** to ensure accuracy

## Conclusion

The shared stock approach is recommended for most use cases because:
- It's simpler to implement and maintain
- It's more common in retail environments
- It provides real-time stock accuracy
- It reduces complexity in inventory management

For specialized cases where you need variant-specific stock levels, consider the separate stock approach, but be prepared for increased complexity.
