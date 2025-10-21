import { StoreTransfer } from "./storeTransfer";
import { Product } from "./product";

export interface StoreTransferItem {
    id: number;
    transferId: number;
    transfer: StoreTransfer;
    productId: number;
    product: Product;
    quantity: number;
    unitCost: number;
    notes: string;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
}

export interface StoreTransferItemDto {
    id: number | null;
    transferId: number | null;
    productId: number;
    quantity: number;
    unitCost: number;
    notes: string;
}



