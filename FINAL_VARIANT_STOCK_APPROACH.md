# Final Variant Stock Management Approach

## Overview
This document explains the final implementation of variant stock management using the **Inventory table** to track stock per variant per store. This approach provides the best of both worlds: granular control and consistency with your existing inventory system.

## How It Works

### 1. **Database Structure**
```sql
-- Inventory table tracks stock per variant per store
Inventories:
- Id: 1, ProductId: 6, StoreId: 1, VariantId: 1, Quantity: 50  (Small T-Shirt in Store 1)
- Id: 2, ProductId: 6, StoreId: 1, VariantId: 2, Quantity: 75  (Medium T-Shirt in Store 1)
- Id: 3, ProductId: 6, StoreId: 1, VariantId: 3, Quantity: 60  (Large T-Shirt in Store 1)
- Id: 4, ProductId: 6, StoreId: 3, VariantId: 1, Quantity: 10  (Small T-Shirt in Store 3)

-- ProductVariants table (no StockQuantity field)
ProductVariants:
- Id: 1, ProductId: 6, Name: "Small", ReorderLevel: 10
- Id: 2, ProductId: 6, Name: "Medium", ReorderLevel: 15
- Id: 3, ProductId: 6, Name: "Large", ReorderLevel: 12
```

### 2. **Key Benefits**
- ✅ **Granular Control**: Each variant has separate stock per store
- ✅ **Consistent System**: Uses existing Inventory table
- ✅ **Audit Trail**: All stock changes tracked in InventoryTransaction
- ✅ **Flexible**: Can track stock across multiple stores
- ✅ **Scalable**: Easy to add new stores or variants

## Implementation Details

### 1. **Entity Structure**
```csharp
// ProductVariant - No StockQuantity field
public class ProductVariant : BaseClass
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal? ReorderLevel { get; set; }  // Only reorder level
    // Stock is managed through Inventory table
}

// Inventory - Tracks stock per variant per store
public class Inventory : BaseClass
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int StoreId { get; set; }
    public int? VariantId { get; set; }  // NULL for main product, ID for variant
    public decimal Quantity { get; set; }
}
```

### 2. **Stock Management Flow**
1. **Create Variant**: No stock quantity set initially
2. **Add Stock**: Creates/updates Inventory record with VariantId
3. **Check Stock**: Queries Inventory table for variant + store
4. **Update Stock**: Modifies Inventory record + creates transaction
5. **Report Stock**: Aggregates from Inventory table

### 3. **API Endpoints**
- `GET /api/product-variants/{id}/validate-stock` - Check variant stock
- `GET /api/product-variants/{id}/stock-info` - Get variant stock info
- `POST /api/product-variants/{id}/update-stock` - Update variant stock

## Usage Examples

### 1. **Creating a Variant**
```typescript
const variantData = {
  productId: 6,
  name: "Large",
  priceAdjustment: 5.00,
  reorderLevel: 12,
  storeId: 1  // Required for inventory tracking
};
// No stockQuantity - managed through Inventory table
```

### 2. **Adding Stock to Variant**
```typescript
// Add 50 units to Large variant in Store 1
await variantStockService.updateVariantStock(3, 1, 50, "Initial stock");
// Creates: Inventory(ProductId: 6, StoreId: 1, VariantId: 3, Quantity: 50)
```

### 3. **Checking Variant Stock**
```typescript
// Check if Large variant has enough stock
const stockInfo = await variantStockService.getVariantStockInfo(6, 3, 1);
console.log(`Available: ${stockInfo.availableQuantity}`); // 50
console.log(`Low Stock: ${stockInfo.isLowStock}`); // false
```

### 4. **Selling a Variant**
```typescript
// Validate stock before sale
const isValid = await variantStockService.validateVariantStock(6, 3, 1, 1);
if (isValid) {
  // Process sale and reduce stock
  await variantStockService.updateVariantStock(3, 1, -1, "Sale of Large T-Shirt");
  // Updates: Inventory(ProductId: 6, StoreId: 1, VariantId: 3, Quantity: 49)
}
```

## Database Queries

### 1. **Get Variant Stock**
```sql
SELECT Quantity 
FROM Inventories 
WHERE ProductId = 6 AND StoreId = 1 AND VariantId = 3;
```

### 2. **Get All Variants Stock for Product**
```sql
SELECT v.Name, i.Quantity, s.NameAr as StoreName
FROM ProductVariants v
LEFT JOIN Inventories i ON v.Id = i.VariantId AND i.StoreId = 1
LEFT JOIN Stores s ON i.StoreId = s.Id
WHERE v.ProductId = 6;
```

### 3. **Low Stock Variants**
```sql
SELECT v.Name, i.Quantity, v.ReorderLevel
FROM ProductVariants v
INNER JOIN Inventories i ON v.Id = i.VariantId
WHERE i.Quantity <= v.ReorderLevel AND i.StoreId = 1;
```

## Frontend Implementation

### 1. **Variant Form (No Stock Field)**
```html
<!-- No stock quantity field - managed through Inventory -->
<div class="form-group">
  <label for="reorderLevel">Reorder Level</label>
  <input type="number" id="reorderLevel" formControlName="reorderLevel">
</div>
```

### 2. **Stock Display**
```html
<td>
  <span class="badge badge-info">Per Store</span>
  <small class="text-muted d-block">Managed via Inventory</small>
</td>
```

### 3. **Stock Management Interface**
```typescript
// Add stock to variant
async addStockToVariant(variantId: number, storeId: number, quantity: number) {
  await this.variantStockService.updateVariantStock(
    variantId, storeId, quantity, "Stock replenishment"
  );
}

// Check variant stock
async checkVariantStock(variantId: number, storeId: number) {
  const stockInfo = await this.variantStockService.getVariantStockInfo(
    this.productId, variantId, storeId
  );
  return stockInfo;
}
```

## Stock Management Workflow

### 1. **Initial Setup**
1. Create product variants (no stock)
2. Set reorder levels per variant
3. Add initial stock via Inventory table
4. Configure low stock alerts

### 2. **Daily Operations**
1. **Sales**: Reduce variant stock in Inventory table
2. **Receipts**: Increase variant stock in Inventory table
3. **Adjustments**: Correct stock in Inventory table
4. **Transfers**: Move stock between stores/variants

### 3. **Reporting**
1. **Current Stock**: Query Inventory table per variant
2. **Low Stock**: Compare Inventory.Quantity with ProductVariant.ReorderLevel
3. **Sales Analysis**: Track InventoryTransaction per variant
4. **Inventory Turnover**: Calculate from InventoryTransaction

## Migration from Previous Approaches

### From Shared Stock:
1. **Distribute main product stock** among variants
2. **Create Inventory records** for each variant
3. **Update stock validation** to use Inventory table
4. **Remove shared stock logic**

### From Separate Stock (ProductVariant.StockQuantity):
1. **Move stock data** from ProductVariant to Inventory table
2. **Create Inventory records** for each variant per store
3. **Update all stock queries** to use Inventory table
4. **Remove StockQuantity field** from ProductVariant

## Best Practices

### 1. **Stock Management**
- Always use Inventory table for stock operations
- Set appropriate reorder levels per variant
- Monitor low stock alerts regularly
- Implement automated reorder processes

### 2. **Data Consistency**
- Always validate stock before operations
- Use transactions for stock updates
- Maintain audit trail in InventoryTransaction
- Regular stock reconciliation

### 3. **Performance**
- Index Inventory table on (ProductId, StoreId, VariantId)
- Cache frequently accessed stock data
- Use efficient queries for stock reports
- Consider materialized views for complex reports

### 4. **Integration**
- Sync with POS systems
- Integrate with accounting software
- Connect with supplier systems
- Automate reorder processes

## API Reference

### Validate Variant Stock
```http
GET /api/product-variants/{variantId}/validate-stock?storeId={storeId}&quantity={quantity}
```

**Response:**
```json
{
  "isValid": true,
  "availableQuantity": 50,
  "requestedQuantity": 1,
  "shortage": 0,
  "message": "Stock available"
}
```

### Get Variant Stock Info
```http
GET /api/product-variants/{variantId}/stock-info?storeId={storeId}
```

**Response:**
```json
{
  "variantId": 3,
  "productId": 6,
  "storeId": 1,
  "availableQuantity": 50,
  "reorderLevel": 12,
  "isLowStock": false
}
```

### Update Variant Stock
```http
POST /api/product-variants/{variantId}/update-stock
Content-Type: application/json

{
  "storeId": 1,
  "quantityChange": -1,
  "notes": "Sale of Large T-Shirt"
}
```

**Response:**
```json
{
  "success": true,
  "newQuantity": 49
}
```

## Conclusion

This approach provides:
- **Granular control** over variant inventory
- **Consistency** with existing inventory system
- **Flexibility** for multi-store operations
- **Audit trail** for all stock changes
- **Scalability** for future growth

The Inventory table serves as the single source of truth for all stock levels, providing a robust and maintainable solution for variant stock management.
