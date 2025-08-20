export interface RoleCreateUpdateDto {
    id: number;
    code: string;
    nameEn: string;
    nameAr: string;
    permissionCodes: string[];
}