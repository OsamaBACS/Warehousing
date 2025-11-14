import { Store } from "./store";
import { Status } from "./status";

export interface StoreTransfer {
    id: number;
    transferDate: string;
    notes: string;
    fromStoreId: number;
    fromStore: Store;
    toStoreId: number;
    toStore: Store;
    statusId: number;
    status: Status;
    items: StoreTransferItem[];
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
}

export interface StoreTransferDto {
    id: number | null;
    transferDate: string;
    notes: string;
    fromStoreId: number;
    toStoreId: number;
    statusId: number;
    items: StoreTransferItemDto[];
}

export interface StoreTransferItem {
    id: number;
    transferId: number;
    transfer: StoreTransfer;
    productId: number;
    product: any; // Product interface
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
