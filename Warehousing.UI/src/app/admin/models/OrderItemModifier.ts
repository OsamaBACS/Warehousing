import { ProductModifierOption } from './ProductModifier';
import { ProductVariant } from './ProductVariant';

export interface OrderItemModifier {
  id?: number;
  orderItemId: number;
  modifierOptionId: number;
  modifierOption?: ProductModifierOption;
  priceAdjustment: number;
  costAdjustment?: number;
  quantity: number;
  notes?: string;
  createdAt?: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface OrderItemWithVariantsAndModifiers {
  id?: number;
  orderId: number;
  productId: number;
  variantId?: number;
  variant?: ProductVariant;
  storeId: number;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  totalPrice: number;
  notes?: string;
  product?: any; // Product interface
  store?: any; // Store interface
  modifiers?: OrderItemModifier[];
}

export interface OrderWithVariantsAndModifiers {
  id?: number;
  orderTypeId: number;
  customerId?: number;
  supplierId?: number;
  statusId?: number;
  orderDate: Date;
  deliveryDate?: Date;
  notes?: string;
  totalAmount?: number;
  discountAmount?: number;
  taxAmount?: number;
  finalAmount?: number;
  orderType?: any;
  customer?: any;
  supplier?: any;
  status?: any;
  items: OrderItemWithVariantsAndModifiers[];
}
