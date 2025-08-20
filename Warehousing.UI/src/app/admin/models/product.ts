import { Category } from "./category";
import { Store } from "./store";
import { SubCategory } from "./SubCategory";
import { Unit } from "./unit";

export interface Product {
    id: number;
    code: string;
    nameEn: string | null;
    nameAr: string;
    description: string;
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
}
export interface ProductDto {
    id: number | null;
    code: string | null;
    nameEn: string | null;
    nameAr: string;
    description: string | null;
    openingBalance: number | null;
    reorderLevel: number | null;
    quantityInStock: number;
    costPrice: number;
    sellingPrice: number;
    lastStockUpdateDate: string;
    isActive: boolean;
    subCategory: SubCategory | null;
    subCategoryId: number | null;
    unitId: number | null;
    unit: Unit | null;
    storeId: number | null;
    store: Store;
    image: File | null;
    imagePath: string | null;
}

export interface ProductPagination {
    products: Product[],
    totals: number
}