import { UserDevice } from "./UserDevice";
import { UserRole } from "./userRole";

export interface User {
    id: number;
    nameEn: string;
    nameAr: string;
    phone: string;
    email: string;
    address: string;
    username: string;
    passwordHash: string;
    printHeader: string;
    printFooter: string;
    isActive: boolean;
    userRoles: UserRole[];
    devices: UserDevice[];
}

export interface UserPagination {
    users: User[];
    totals: number
}