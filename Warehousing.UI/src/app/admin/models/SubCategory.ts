import { Category } from "./category";
import { Product } from "./product";

export interface SubCategory {
    id: number;
    nameEn: string | null;
    nameAr: string | null;
    description: string | null;
    isActive: boolean;
    categoryId: number;
    category: Category;
    products?: Product[];
}