import { Customer } from "./customer";
import { OrderItemDto } from "./OrderItemDto";
import { OrderTypeDto } from "./OrderTypeDto";
import { Status } from "./status";
import { Supplier } from "./supplier";

export interface OrderDto {
    id: number;
    orderDate: string;
    totalAmount: number;
    orderType: OrderTypeDto | null;
    orderTypeId: number | null;
    customer: Customer | null;
    customerId: number | null;
    supplier: Supplier | null;
    supplierId: number | null;
    status: Status | null;
    statusId: number | null;
    items: OrderItemDto[];
    statusColor: string;
}

export interface OrderPagination {
    orders: OrderDto[],
    totals: number
}

export interface OrderFilters {
    pageIndex: number;
    pageSize: number;
    searchTerm: string | null;
    orderDate: string | null;
    orderTypeId: number | null;
    customerId: number | null;
    supplierId: number | null;
    statusId: number | null;
    dateFrom: string | null;
    dateTo: string | null;
    minAmount: number | null;
    maxAmount: number | null;
}