import { SubCategory } from "./SubCategory";

export interface Category {
    id: number;
    nameEn: string | null;
    nameAr: string;
    description: string | null;
    imagePath: string | null;
    isActive: boolean;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
    subCategories: SubCategory[];
}