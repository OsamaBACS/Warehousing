# Separate Stock per Variant - Implementation Guide

## Overview
This document explains the separate stock approach where each product variant has its own independent stock quantity. This provides granular control over inventory management for each variant.

## How It Works

### 1. **Separate Stock Concept**
- **Each variant** has its own `StockQuantity` field
- **Independent tracking** of stock per variant
- **Granular control** over inventory management
- **Better reporting** and analytics per variant

### 2. **Example Scenario**
```
Product: T-Shirt (ID: 6)
├── Variant: Small (Stock: 50 units)
├── Variant: Medium (Stock: 75 units)  
├── Variant: Large (Stock: 60 units)
└── Variant: XL (Stock: 15 units)

When a customer buys:
- 1x Small T-Shirt → Small stock becomes 49
- 1x Large T-Shirt → Large stock becomes 59
- Other variants remain unchanged
```

### 3. **Database Structure**
```sql
-- ProductVariants table with individual stock
ProductVariants:
- Id: 1, ProductId: 6, Name: "Small", StockQuantity: 50
- Id: 2, ProductId: 6, Name: "Medium", StockQuantity: 75
- Id: 3, ProductId: 6, Name: "Large", StockQuantity: 60
- Id: 4, ProductId: 6, Name: "XL", StockQuantity: 15

-- Inventory table for main product (optional)
Inventories:
- ProductId: 6, StoreId: 1, VariantId: NULL, Quantity: 0 (not used)
```

## Implementation Details

### 1. **Entity Structure**
```csharp
public class ProductVariant : BaseClass
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    
    // Separate stock per variant
    public decimal? StockQuantity { get; set; }
    public decimal? ReorderLevel { get; set; }
    
    // Other properties...
}
```

### 2. **API Endpoints**
- `GET /api/product-variants/{id}/validate-stock` - Validate variant stock
- `GET /api/product-variants/{id}/stock-info` - Get variant stock info
- `POST /api/product-variants/{id}/update-stock` - Update variant stock

### 3. **Frontend Implementation**
- Stock quantity field in variant forms
- Individual stock display in variant lists
- Separate stock validation per variant
- Low stock alerts per variant

## Benefits of Separate Stock Approach

### ✅ **Advantages**
1. **Granular Control**: Track stock for each variant independently
2. **Better Analytics**: Understand which variants sell more
3. **Flexible Management**: Different reorder levels per variant
4. **Accurate Reporting**: Precise stock levels per variant
5. **Business Intelligence**: Identify popular variants and sizes
6. **Inventory Optimization**: Stock variants based on demand

### ⚠️ **Considerations**
1. **More Complex**: Requires managing multiple stock levels
2. **Data Entry**: Need to set stock for each variant
3. **Reporting**: More complex inventory reports
4. **Transfers**: Need to handle variant-specific transfers

## Usage Examples

### 1. **Creating a Variant with Stock**
```typescript
const variantData = {
  productId: 6,
  name: "Large",
  priceAdjustment: 5.00,
  stockQuantity: 100,  // Individual stock quantity
  reorderLevel: 10,
  storeId: 1
};
```

### 2. **Checking Variant Stock**
```typescript
// Check if variant has enough stock
const stockInfo = await variantStockService.getVariantStockInfo(6, 2, 1);
console.log(`Available: ${stockInfo.availableQuantity}`);
console.log(`Low Stock: ${stockInfo.isLowStock}`);
```

### 3. **Selling a Variant**
```typescript
// Validate stock before sale
const isValid = await variantStockService.validateVariantStock(6, 2, 1, 1);
if (isValid) {
  // Process sale and update variant stock
  await variantStockService.updateVariantStock(6, 2, 1, -1, "Sale of Large T-Shirt");
}
```

### 4. **Stock Adjustment**
```typescript
// Add stock to a variant
await variantStockService.updateVariantStock(6, 2, 1, 50, "Stock replenishment");
```

## Stock Management Workflow

### 1. **Initial Stock Setup**
1. Create product variants
2. Set initial stock quantity for each variant
3. Set reorder levels per variant
4. Configure low stock alerts

### 2. **Daily Operations**
1. **Sales**: Reduce variant stock when sold
2. **Receipts**: Increase variant stock when received
3. **Adjustments**: Correct stock discrepancies
4. **Transfers**: Move stock between variants

### 3. **Reporting & Analytics**
1. **Stock Levels**: Current stock per variant
2. **Sales Analysis**: Which variants sell most
3. **Low Stock**: Variants below reorder level
4. **Inventory Turnover**: How fast variants sell

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
  "variantId": 2,
  "productId": 6,
  "storeId": 1,
  "availableQuantity": 50,
  "reorderLevel": 10,
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

## Frontend Implementation

### 1. **Variant Form**
```html
<div class="form-group">
  <label for="stockQuantity">Stock Quantity</label>
  <input type="number" id="stockQuantity" formControlName="stockQuantity" class="form-control">
</div>
```

### 2. **Variant List**
```html
<tr *ngFor="let variant of variants">
  <td>{{ variant.name }}</td>
  <td>{{ variant.stockQuantity || 0 }}</td>
  <td>
    <span class="badge badge-warning" *ngIf="variant.stockQuantity <= variant.reorderLevel">
      Low Stock
    </span>
  </td>
</tr>
```

### 3. **Stock Validation**
```typescript
async validateStock(variantId: number, quantity: number): Promise<boolean> {
  const result = await this.variantStockService.validateVariantStock(
    this.productId, variantId, this.storeId, quantity
  ).toPromise();
  
  return result?.isValid || false;
}
```

## Best Practices

### 1. **Stock Management**
- Set realistic reorder levels per variant
- Monitor low stock alerts regularly
- Implement automated reorder processes
- Regular stock audits and adjustments

### 2. **Data Entry**
- Always validate stock quantities
- Use consistent units of measurement
- Document stock adjustments with notes
- Regular backup of inventory data

### 3. **Reporting**
- Generate daily stock reports
- Track variant performance metrics
- Monitor inventory turnover rates
- Analyze seasonal demand patterns

### 4. **Integration**
- Sync with POS systems
- Integrate with accounting software
- Connect with supplier systems
- Automate reorder processes

## Migration from Shared Stock

If migrating from shared stock approach:

1. **Data Migration**:
   - Distribute main product stock among variants
   - Set appropriate stock levels per variant
   - Update reorder levels

2. **Code Updates**:
   - Update stock validation logic
   - Modify inventory reports
   - Update order processing

3. **Testing**:
   - Test variant stock validation
   - Verify stock updates
   - Check reporting accuracy

## Conclusion

The separate stock approach provides:
- **Granular control** over inventory
- **Better business intelligence**
- **Accurate variant tracking**
- **Flexible inventory management**

This approach is ideal for businesses that need detailed control over variant inventory and want to optimize stock levels based on actual demand patterns.
