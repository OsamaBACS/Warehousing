import { Category } from "./category";
import { Inventory } from "./Inventory";
import { Store } from "./store";
import { SubCategory } from "./SubCategory";
import { Unit } from "./unit";
import { InventoryTransaction } from "./inventoryTransaction";
import { OrderItemDto } from "./OrderItemDto";
import { ProductVariant } from "./ProductVariant";

export interface Product {
    id: number;
    code: string;
    nameEn: string | null;
    nameAr: string;
    description: string | null;
    openingBalance: number | null;
    reorderLevel: number | null;
    imagePath: string | null;
    isActive: boolean;
    quantityInStock: number;
    costPrice: number;
    sellingPrice: number;
    lastStockUpdateDate: string;
    subCategory: SubCategory | null;
    subCategoryId: number | null;
    unitId: number | null;
    unit: Unit | null;
    storeId: number | null;
    store: Store | null;
    inventories: Inventory[];
    transactions: InventoryTransaction[];
    orderItems: OrderItemDto[];
    variants: ProductVariant[]; // Product variants
    modifierGroups?: any[]; // Product modifier groups
    totalQuantity?: number; // Total quantity across all stores
    variantStockData?: { [key: string]: any }; // Variant stock data from API
}
export interface ProductDto {
    id: number | null;
    code: string | null;
    nameEn: string | null;
    nameAr: string;
    description: string | null;
    openingBalance: number | null;
    reorderLevel: number | null;
    costPrice: number;
    sellingPrice: number;
    isActive: boolean;
    subCategory: SubCategory | null;
    subCategoryId: number | null;
    unitId: number | null;
    unit: Unit | null;
    storeId: number | null;
    store: Store | null;
    image: File | null;
    imagePath: string | null;
    inventories: Inventory[]
}

export interface ProductPagination {
    products: Product[],
    totals: number
}