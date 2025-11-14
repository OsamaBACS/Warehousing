import { OrderDto } from "./OrderDto";
import { Product } from "./product";
import { TransactionType } from "./transactionType";
import { Store } from "./store";
import { OrderItemDto } from "./OrderItemDto";

export interface InventoryTransaction {
    id: number;
    quantityChanged: number;
    quantityBefore: number;
    quantityAfter: number;
    unitCost: number;
    transactionDate: string;
    notes: string;
    product: Product;
    productId: number;
    store: Store;
    storeId: number;
    transactionType: TransactionType;
    transactionTypeId: number;
    order: OrderDto | null;
    orderId: number | null;
    orderItem: OrderItemDto | null;
    orderItemId: number | null;
    transferId: number | null;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
}

export interface InventoryTransactionPagination {
    transactions: InventoryTransaction[],
    totals: number
}