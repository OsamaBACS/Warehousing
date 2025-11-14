import { OrderDto } from "./OrderDto";
import { Product } from "./product";
import { Store } from "./store";

export interface OrderItemDto {
    id: number;
    quantity: number;
    unitCost: number;
    unitPrice: number;
    discount: number;
    notes: string | null;
    product: Product | null;
    productId: number;
    store: Store | null;
    storeId: number;
    order: OrderDto | null;
    orderId: number;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
}