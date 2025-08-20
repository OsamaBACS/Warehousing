import { Product } from "./product";
import { Role } from "./role";

export interface RoleProduct {
    id: number;
    role: Role;
    roleId: number;
    product: Product;
    productId: number;
}