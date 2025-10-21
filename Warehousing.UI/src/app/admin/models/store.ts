import { Inventory } from "./Inventory";

export interface Store {
    id: number;
    code: string | null;
    nameEn: string | null;
    nameAr: string;
    description: string | null;
    address: string | null;
    phone: string | null;
    isMainWarehouse: boolean;
    isActive: boolean;
    inventories?: Inventory[];
}