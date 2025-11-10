export interface ProductModifier {
  id: number;
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
  options: ProductModifierOption[];
}

export interface ProductModifierOption {
  id: number;
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

export interface ProductModifierGroup {
  id: number;
  productId: number;
  modifierId: number;
  modifier?: ProductModifier;
  modifierName?: string;
  isRequired: boolean;
  maxSelections: number;
  displayOrder: number;
  isActive: boolean;
}











