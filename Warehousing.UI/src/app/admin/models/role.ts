import { RoleCategory } from "./RoleCategory";
import { RolePermission, RolePermissionDto } from "./rolePermission";
import { RoleProduct } from "./RoleProduct";
import { RoleSubCategory } from "./RoleSubCategory";

export interface Role {
    id: number;
    code: string;
    nameEn: string;
    nameAr: string;
    rolePermissions: RolePermission[];
    roleCategories : RoleCategory[],
    roleProducts : RoleProduct[],
    roleSubCategories : RoleSubCategory[]
}
export interface RoleDto {
    id: number;
    code: string;
    nameEn: string;
    nameAr: string;
    permissions: RolePermissionDto[];
    categoryIds : number[],
    productIds : number[],
    subCategoryIds : number[],
    printerConfigurationId?: number | null
}

export interface RoleDtoForAdd {
    id: number;
    code: string;
    nameEn: string;
    nameAr: string;
    rolePermissionIds: number[];
    categoryIds : number[],
    productIds : number[],
    subCategoryIds : number[],
    printerConfigurationId?: number | null
}