export interface ProductModifier {
  id?: number;
  name: string;
  code?: string;
  description?: string;
  priceAdjustment: number;
  costAdjustment?: number;
  isRequired: boolean;
  isMultiple: boolean;
  maxSelections: number;
  isActive: boolean;
  displayOrder: number;
  createdAt?: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
  options?: ProductModifierOption[];
}

export interface ProductModifierOption {
  id?: number;
  modifierId: number;
  name: string;
  code?: string;
  description?: string;
  priceAdjustment: number;
  costAdjustment?: number;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  createdAt?: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface ProductModifierGroup {
  id?: number;
  productId: number;
  modifierId: number;
  modifier?: ProductModifier;
  modifierName?: string; // For display purposes when modifier object is not loaded
  isRequired: boolean;
  maxSelections: number;
  displayOrder: number;
  isActive: boolean;
  createdAt?: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface ProductModifierCreateRequest {
  name: string;
  code?: string;
  description?: string;
  priceAdjustment: number;
  costAdjustment?: number;
  isRequired: boolean;
  isMultiple: boolean;
  maxSelections: number;
  isActive: boolean;
  displayOrder: number;
}

export interface ProductModifierUpdateRequest extends ProductModifierCreateRequest {
  id: number;
}

export interface ProductModifierOptionCreateRequest {
  modifierId: number;
  name: string;
  code?: string;
  description?: string;
  priceAdjustment: number;
  costAdjustment?: number;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
}

export interface ProductModifierOptionUpdateRequest extends ProductModifierOptionCreateRequest {
  id: number;
}

export interface ProductModifierGroupCreateRequest {
  productId: number;
  modifierId: number;
  isRequired: boolean;
  maxSelections: number;
  displayOrder: number;
  isActive: boolean;
}

export interface ProductModifierGroupUpdateRequest extends ProductModifierGroupCreateRequest {
  id: number;
}
