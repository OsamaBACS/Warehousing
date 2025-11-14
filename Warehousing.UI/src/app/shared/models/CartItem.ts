export interface CartItem {
  productId: number;
  quantity: number;
  unitCost: number;
  unitPrice: number;
  discount: number;
  storeId: number;
}

export interface CartItemForm {
  productId: number;
  quantity: number;
  unitCost: number;
  unitPrice: number;
  discount: number;
  storeId: number;
  notes?: string;
}