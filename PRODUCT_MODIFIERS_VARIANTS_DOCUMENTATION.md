# Product, Modifiers, and Variants - Complete Reference Guide

## Overview

This document explains the relationships between Products, Modifiers, and Variants in the Warehousing system, and how they affect pricing and cost calculations.

---

## 1. Architecture and Data Structure

### 1.1 Database Tables

#### **Products Table**
- **Purpose**: Base product information
- **Fields**: `Id`, `Code`, `NameAr`, `NameEn`, `Description`, `CostPrice`, `SellingPrice`, `ImagePath`, etc.
- **Relationships**:
  - Has many `ProductVariants` (one-to-many)
  - Has many `ProductModifierGroups` (through junction table)
  - Has many `Inventories` (for general stock)
  - Has many `OrderItems` (when product is ordered)

#### **ProductModifiers Table**
- **Purpose**: Global modifier templates (created in "Modifier Management" page)
- **Fields**: `Id`, `Name`, `Code`, `Description`, `PriceAdjustment`, `CostAdjustment`, `IsRequired`, `IsMultiple`, `MaxSelections`, etc.
- **Relationships**:
  - Has many `ProductModifierOptions` (one-to-many)
  - Has many `ProductModifierGroups` (through junction table - links to products)
- **Where it's saved**: `ProductModifiers` table
- **Example**: "Size" modifier, "Color" modifier, "Extra Cheese" modifier

#### **ProductModifierOptions Table**
- **Purpose**: Options within each modifier
- **Fields**: `Id`, `ModifierId`, `Name`, `Code`, `PriceAdjustment`, `CostAdjustment`, `IsDefault`, `IsActive`, etc.
- **Relationships**:
  - Belongs to `ProductModifier` (many-to-one)
  - Has many `OrderItemModifiers` (when selected in orders)
- **Example**: For "Size" modifier → "Small", "Medium", "Large" are options

#### **ProductModifierGroups Table (Junction Table)**
- **Purpose**: Links a specific Product to a Modifier
- **Fields**: `Id`, `ProductId`, `ModifierId`, `IsRequired`, `MaxSelections`, `DisplayOrder`, `IsActive`
- **Relationships**:
  - Belongs to `Product` (many-to-one)
  - Belongs to `ProductModifier` (many-to-one)
- **Where it's saved**: `ProductModifierGroups` table
- **When**: Created when you select a modifier for a product in the product form

#### **ProductVariants Table**
- **Purpose**: Different versions/SKUs of the same product
- **Fields**: `Id`, `ProductId`, `Name`, `Code`, `Description`, `PriceAdjustment`, `CostAdjustment`, `IsDefault`, `IsActive`, etc.
- **Relationships**:
  - Belongs to `Product` (many-to-one)
  - Has many `Inventories` (for variant-specific stock)
  - Has many `OrderItems` (when variant is ordered)
- **Where it's saved**: `ProductVariants` table
- **Example**: For "T-Shirt" product → "Blue T-Shirt", "Red T-Shirt" are variants

---

## 2. Relationships and Hierarchy

### 2.1 Product Hierarchy

```
Product (Base)
├── ProductVariants (Different SKUs/versions)
│   ├── Inventory (Stock per variant per store)
│   └── OrderItems (When variant is ordered)
│
└── ProductModifierGroups (Links to modifiers)
    └── ProductModifier (Global template)
        └── ProductModifierOptions (Options within modifier)
            └── OrderItemModifiers (When option is selected)
```

### 2.2 Key Relationships

**Product → Variants:**
- One product can have many variants
- Each variant has its own inventory and pricing adjustments
- Variants are product-specific (belong directly to the product)

**Product → Modifiers:**
- Products are linked to modifiers through `ProductModifierGroups` (junction table)
- Modifiers are global templates (created once, used by many products)
- Each product can have multiple modifiers assigned

**Modifier → Options:**
- Each modifier has multiple options (e.g., "Small", "Medium", "Large" for Size)
- Options are part of the modifier template (not product-specific)

---

## 3. Workflow and Usage

### 3.1 Creating Modifiers (Step 1: Modifier Management)

1. Navigate to: **Admin → إدارة المكونات (Modifier Management)**
2. Create modifier templates:
   - Enter name (e.g., "Size", "Color", "Extra Cheese")
   - Add description
   - Set `PriceAdjustment` (optional, affects base modifier)
   - Set `CostAdjustment` (optional, affects base modifier)
   - Configure `IsRequired`, `IsMultiple`, `MaxSelections`
3. Add options to each modifier:
   - For "Size" modifier: Add "Small", "Medium", "Large" as options
   - Each option can have its own `PriceAdjustment` and `CostAdjustment`
4. **Saved to**: `ProductModifiers` and `ProductModifierOptions` tables

### 3.2 Assigning Modifiers to Products (Step 2: Product Form)

1. Open/Edit a product in **Product Form**
2. Scroll to **"المكونات والمتغيرات" (Modifiers and Variants)** section
3. **Modifiers section appears first** (as requested)
4. You'll see all available modifiers as checkboxes
5. Check the modifiers you want for this product
6. Configure settings for each selected modifier:
   - `IsRequired`: Is this modifier mandatory for the product?
   - `MaxSelections`: Maximum number of options customer can select
   - `DisplayOrder`: Order in which modifier appears
7. **View price/cost adjustments** (read-only):
   - Shows modifier's base `PriceAdjustment` and `CostAdjustment`
   - Shows calculated price/cost after adjustment
   - Note: Final price depends on which options customer selects
8. Save the product
9. **Saved to**: `ProductModifierGroups` table (creates records linking ProductId to ModifierId)

### 3.3 Adding Variants (Step 3: Product Form)

1. After saving the product, **Variants section appears after Modifiers**
2. Click **"إدارة المتغيرات" (Manage Variants)** button
3. Add variants:
   - Enter variant name (e.g., "Blue T-Shirt", "Red T-Shirt")
   - Set `PriceAdjustment` (optional, adjusts product base price)
   - Set `CostAdjustment` (optional, adjusts product base cost)
   - Configure `IsDefault`, `IsActive`, `DisplayOrder`
4. **Saved to**: `ProductVariants` table

---

## 4. Price and Cost Calculation

### 4.1 Base Pricing

**Product Base Prices:**
- `Product.CostPrice`: Base cost price of the product
- `Product.SellingPrice`: Base selling price of the product

### 4.2 Price Adjustments

#### **Variant Price Adjustments:**
- **Field**: `ProductVariant.PriceAdjustment` and `ProductVariant.CostAdjustment`
- **Applied when**: Customer selects a variant
- **Formula**: 
  ```
  Final Price = Product.SellingPrice + Variant.PriceAdjustment
  Final Cost = Product.CostPrice + Variant.CostAdjustment
  ```

#### **Modifier Price Adjustments:**
- **Modifier Base**: `ProductModifier.PriceAdjustment` and `ProductModifier.CostAdjustment`
  - These are shown in the product form when modifier is selected (read-only)
  - Typically not used directly in calculations
  
- **Option Adjustments**: `ProductModifierOption.PriceAdjustment` and `ProductModifierOption.CostAdjustment`
  - **Applied when**: Customer selects specific modifier options
  - **Formula**:
    ```
    Final Price = Base Price + Sum of all selected Option.PriceAdjustment
    Final Cost = Base Cost + Sum of all selected Option.CostAdjustment
    ```

### 4.3 Complete Price Calculation Flow

**When a customer orders a product:**

1. **Start with base product price:**
   ```
   BasePrice = Product.SellingPrice
   BaseCost = Product.CostPrice
   ```

2. **Apply variant adjustment (if variant selected):**
   ```
   Price = BasePrice + Variant.PriceAdjustment (if variant selected)
   Cost = BaseCost + Variant.CostAdjustment (if variant selected)
   ```

3. **Apply modifier option adjustments (for each selected option):**
   ```
   FinalPrice = Price + Sum(All selected ModifierOption.PriceAdjustment)
   FinalCost = Cost + Sum(All selected ModifierOption.CostAdjustment)
   ```

4. **Calculate totals:**
   ```
   ItemTotalPrice = FinalPrice × Quantity
   ItemTotalCost = FinalCost × Quantity
   ```

### 4.4 Example Calculation

**Product: "Pizza"**
- Base Price: 10.00 JOD
- Base Cost: 5.00 JOD

**Variant Selected: "Large Pizza"**
- Variant Price Adjustment: +5.00 JOD
- Variant Cost Adjustment: +2.00 JOD
- Adjusted Price: 10.00 + 5.00 = **15.00 JOD**
- Adjusted Cost: 5.00 + 2.00 = **7.00 JOD**

**Modifiers Selected:**
- "Extra Cheese" option (Price Adjustment: +2.00 JOD, Cost Adjustment: +1.00 JOD)
- "Pepperoni" option (Price Adjustment: +3.00 JOD, Cost Adjustment: +1.50 JOD)

**Final Calculation:**
- Final Price: 15.00 + 2.00 + 3.00 = **20.00 JOD**
- Final Cost: 7.00 + 1.00 + 1.50 = **9.50 JOD**

**For Quantity = 2:**
- Total Price: 20.00 × 2 = **40.00 JOD**
- Total Cost: 9.50 × 2 = **19.00 JOD**
- Profit: 40.00 - 19.00 = **21.00 JOD**

---

## 5. Important Concepts

### 5.1 Modifiers vs Variants

| Aspect | Modifiers | Variants |
|--------|-----------|----------|
| **Purpose** | Add-ons/customizations | Different versions/SKUs |
| **Scope** | Global templates (reusable) | Product-specific |
| **Created in** | Modifier Management page | Product Form (variants manager) |
| **Assigned to** | Products (via checkbox selection) | Products (directly) |
| **Options** | Has options (Small/Medium/Large) | No options, just variants |
| **Inventory** | No inventory (options don't have stock) | Has inventory (per variant per store) |
| **Pricing** | Options have price adjustments | Variant has price adjustments |
| **Example** | "Toppings" modifier with "Cheese/Pepperoni" options (no separate inventory) | "Blue T-Shirt" (10 stock) vs "Red T-Shirt" (5 stock) - separate inventory per color |

### 5.2 Modifier Base Price Adjustments

**Question**: What is `ProductModifier.PriceAdjustment` used for?

**Answer**: 
- The modifier base `PriceAdjustment` is shown in the product form (read-only) for information
- **It's typically not used in final price calculation**
- The actual price adjustments come from the **Modifier Options** that customers select
- Example: "Size" modifier might have base adjustment of 0, but "Large" option has +5.00 adjustment

### 5.3 When to Use Variants vs Modifiers

**The Key Question**: Do you need to track inventory separately for each option?

#### **Use Variants when:**
- You need **separate inventory tracking** for each version
- Each version has its own stock quantity per store
- You need to know exactly how many "Blue T-Shirts" vs "Red T-Shirts" you have
- Examples:
  - **Color as Variant**: "Blue T-Shirt" (10 in stock) vs "Red T-Shirt" (5 in stock) - tracked separately
  - **Size as Variant**: "Large Pizza" (different SKU) vs "Small Pizza" (different SKU)
  - Different materials, different configurations that need separate stock tracking

#### **Use Modifiers when:**
- You **don't need separate inventory tracking**
- All options share the same stock pool
- Customer selects the option, but inventory is managed at the product level
- Examples:
  - **Color as Modifier**: You have 15 T-Shirts total, customer picks color at checkout, but all colors share inventory
  - **Size as Modifier**: Pizza size options (Small/Medium/Large) - inventory is tracked at pizza level, not per size
  - Toppings (Cheese, Pepperoni) - added to base product, no separate inventory
  - Extra services (Extra Cheese, Extra Sauce) - no inventory tracking needed

**Important**: The same attribute (like "Color" or "Size") can be either a variant OR a modifier, depending on your business needs:
- Need separate stock tracking? → **Use Variants**
- Share inventory across options? → **Use Modifiers**

---

## 6. Data Flow Summary

### 6.1 Modifier Flow

```
1. Create Modifier Template
   Modifier Management → ProductModifiers table
   
2. Add Options to Modifier
   Modifier Management → ProductModifierOptions table
   
3. Assign Modifier to Product
   Product Form → ProductModifierGroups table (links Product to Modifier)
   
4. Customer Selects Options
   Order Process → OrderItemModifiers table (links OrderItem to selected Options)
```

### 6.2 Variant Flow

```
1. Create Product
   Product Form → Products table
   
2. Add Variants to Product
   Product Form → Variants Manager → ProductVariants table
   
3. Manage Inventory
   Inventory Management → Inventory table (per variant per store)
   
4. Customer Selects Variant
   Order Process → OrderItem.VariantId (links OrderItem to Variant)
```

---

## 7. Terminology Reference

| English | Arabic | Context |
|---------|--------|---------|
| Modifier | المكون | Global template for add-ons |
| Modifier Option | خيار المكون | Option within a modifier (e.g., Small, Medium, Large) |
| Variant | المتغير | Different version/SKU of a product |
| Modifier Management | إدارة المكونات | Page to create modifier templates |
| Product Modifiers | مكونات المنتج | Modifiers assigned to a specific product |
| Price Adjustment | تعديل السعر | Additional price for variant or modifier option |
| Cost Adjustment | تعديل التكلفة | Additional cost for variant or modifier option |

---

## 8. Best Practices

1. **Modifiers**: Create reusable modifier templates in Modifier Management that can be used across multiple products
2. **Variants**: Create variants only when you need separate inventory tracking for different versions
3. **Price Adjustments**: Set price adjustments at the option level (not modifier base) for better control
4. **Order**: Assign modifiers first, then add variants (as reflected in the UI)
5. **Testing**: Always test price calculations with different combinations of variants and modifier options

---

## 9. Technical Implementation Details

### 9.1 Price Calculation in Order Processing

**Backend (C#):**
```csharp
// Base price
decimal finalPrice = product.SellingPrice;

// Add variant adjustment if variant selected
if (orderItem.VariantId.HasValue && variant != null)
{
    finalPrice += variant.PriceAdjustment ?? 0;
}

// Add modifier option adjustments
foreach (var orderItemModifier in orderItem.Modifiers)
{
    if (orderItemModifier.ModifierOption != null)
    {
        finalPrice += orderItemModifier.ModifierOption.PriceAdjustment * orderItemModifier.Quantity;
    }
}

orderItem.UnitPrice = finalPrice;
```

**Frontend (TypeScript/Angular):**
- Price is calculated dynamically in the cart/order components
- Uses `Product.SellingPrice` + `Variant.PriceAdjustment` + sum of `ModifierOption.PriceAdjustment`

### 9.2 Storage in Orders

**OrderItem:**
- `ProductId`: Links to base product
- `VariantId`: Optional, links to selected variant
- `UnitPrice`: Final calculated price (includes all adjustments)
- `UnitCost`: Final calculated cost (includes all adjustments)

**OrderItemModifier:**
- `OrderItemId`: Links to order item
- `ModifierOptionId`: Links to selected modifier option
- `PriceAdjustment`: Snapshot of option's price adjustment at time of order
- `CostAdjustment`: Snapshot of option's cost adjustment at time of order
- `Quantity`: How many times this modifier option was added

---

## 10. FAQ

**Q: Can a product have both variants and modifiers?**  
A: Yes! A product can have variants (e.g., Blue T-Shirt, Red T-Shirt) and modifiers (e.g., Size options).

**Q: Which price adjustment is used - modifier base or option?**  
A: The **option's price adjustment** is used in calculations. The modifier base adjustment is for reference only.

**Q: How are modifiers and variants related?**  
A: They are **independent**. Variants are product versions, modifiers are add-ons. A customer can select a variant AND modifiers.

**Q: Can modifiers have inventory?**  
A: No. Only variants and products have inventory. Modifier options don't need separate inventory tracking.

**Q: Where are modifier selections saved?**  
A: When assigning to a product → `ProductModifierGroups` table. When customer selects in order → `OrderItemModifiers` table.

---

## Conclusion

This system provides flexibility for products with multiple variations and customization options. The key is understanding:
- **Variants** = Different products (different SKUs)
- **Modifiers** = Add-ons/customizations (with options)
- **Price adjustments** are cumulative (base + variant + modifier options)

For questions or clarifications, refer to this documentation or check the database schema directly.

