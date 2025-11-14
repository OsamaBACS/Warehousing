import { Category } from "./category";

export interface SubCategory {
    id: number;
    nameEn: string | null;
    nameAr: string | null;
    description: string | null;
    imagePath: string | null;
    isActive: boolean;
    categoryId: number;
    category: Category;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
    // Removed products collection to match API changes (prevents circular reference)
}