import { SubCategory } from "./SubCategory";

export interface Category {
    id: number;
    nameEn: string;
    nameAr: string;
    description: string;
    isActive: boolean;
    subCategories: SubCategory[];
}