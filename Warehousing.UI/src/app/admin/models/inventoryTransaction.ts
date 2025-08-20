import { OrderDto } from "./OrderDto";
import { Product } from "./product";
import { TransactionType } from "./transactionType";

export interface InventoryTransaction {
    id: number;
    quantityChanged: number;
    transactionDate: string;
    notes: string;
    product: Product;
    productId: number;
    transactionType: TransactionType;
    transactionTypeId: number;
    order: OrderDto;
    orderId: number;
}

export interface InventoryTransactionPagination {
    transactions: InventoryTransaction[],
    totals: number
}