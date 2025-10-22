export interface ProductVariant {
  id?: number;
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
  createdAt?: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface ProductVariantCreateRequest {
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

export interface ProductVariantUpdateRequest extends ProductVariantCreateRequest {
  id: number;
}

