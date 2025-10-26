import { Inventory } from './Inventory';

export interface ProductVariant {
  id?: number;
  productId: number;
  name: string;
  code?: string;
  description?: string;
  priceAdjustment?: number;
  costAdjustment?: number;
  // StockQuantity is calculated from Inventory table per variant per store
  reorderLevel?: number;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  createdAt?: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
  // Navigation properties
  inventories?: Inventory[];
}

export interface ProductVariantCreateRequest {
  productId: number;
  name: string;
  code?: string;
  description?: string;
  priceAdjustment?: number;
  costAdjustment?: number;
  // StockQuantity is managed through Inventory table per variant per store
  reorderLevel?: number;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  storeId?: number; // Add storeId for inventory tracking
}

export interface ProductVariantUpdateRequest extends ProductVariantCreateRequest {
  id: number;
}


