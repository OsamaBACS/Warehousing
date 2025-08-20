import { Permission } from "./permission";
import { Role } from "./role";

export interface RolePermission {
    id: number;
    role: Role;
    roleId: number;
    permission: Permission;
    permissionId: number;
}

export interface RolePermissionDto {
    id: number;
    roleId: number;
    permissionId: number;
    nameEn: string;
    nameAr: string;
    code: string;
}