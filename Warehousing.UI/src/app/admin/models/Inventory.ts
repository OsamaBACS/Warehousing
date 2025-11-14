import { Product } from "./product";
import { Store } from "./store";

export interface Inventory {
    id: number;
    productId: number;
    product: Product | null;
    storeId: number;
    store: Store | null;
    variantId?: number | null; // Added to support variant-specific inventory
    quantity: number;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
}