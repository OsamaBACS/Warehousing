import { Product } from "./product";

export interface ProductRecipe {
    id: number;
    parentProductId: number;
    parentProduct: Product;
    componentProductId: number;
    componentProduct: Product;
    quantity: number;
    isActive: boolean;
    notes: string | null;
    createdAt: string | null;
    createdBy: string | null;
    updatedAt: string | null;
    updatedBy: string | null;
}

export interface ProductRecipeDto {
    id: number | null;
    parentProductId: number;
    componentProductId: number;
    quantity: number;
    isActive: boolean;
    notes: string | null;
}



