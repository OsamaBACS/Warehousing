export interface ProductVariant {
  id: number;
  productId: number;
  name: string;
  code?: string;
  description?: string;
  priceAdjustment?: number;
  costAdjustment?: number;
  stockQuantity?: number;
  reorderLevel?: number;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
}







