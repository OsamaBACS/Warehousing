import { Category } from "./category";
import { Role } from "./role";

export interface RoleCategory {
    id: number;
    role: Role;
    roleId: number;
    category: Category;
    categoryId: number;
}