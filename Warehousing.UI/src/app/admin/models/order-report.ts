export interface OrderReportFilter {
  orderTypeId?: number;
  dateFrom?: string;
  dateTo?: string;
  storeId?: number;
  customerId?: number;
  supplierId?: number;
  statusId?: number;
  maxRecords?: number;
}

export interface OrderReportSummary {
  totalOrders: number;
  totalAmount: number;
  totalQuantity: number;
  averageOrderValue: number;
  dateFrom: string;
  dateTo: string;
}

export interface OrderReportDaily {
  date: string;
  orderCount: number;
  totalAmount: number;
}

export interface OrderReportStoreBreakdown {
  storeId: number;
  storeNameAr: string;
  storeNameEn?: string;
  quantity: number;
  totalAmount: number;
}

export interface OrderReportDetail {
  orderId: number;
  orderDate: string;
  orderTypeId?: number;
  orderTypeNameAr?: string;
  orderTypeNameEn?: string;
  customerName?: string;
  supplierName?: string;
  statusNameAr?: string;
  statusNameEn?: string;
  totalAmount: number;
  totalQuantity: number;
  stores: OrderReportStoreBreakdown[];
}

export interface OrderReportResponse {
  summary: OrderReportSummary;
  dailyBreakdown: OrderReportDaily[];
  orders: OrderReportDetail[];
}

