import { OrderDto } from "./OrderDto";
import { Product } from "./product";
import { Store } from "./store";

export interface OrderItemDto {
    id: number;
    quantity: number;
    costPrice: number | null;
    sellingPrice: number;
    product: Product | null;
    productId: number | null;
    store: Store | null;
    storeId: number | null;
    order: OrderDto | null;
    orderId: number | null;
}