// Dashboard Overview Interface
export interface DashboardOverview {
  totalProducts: number;
  totalStores: number;
  totalInventoryQuantity: number;
  lowStockProducts: number;
  zeroStockProducts: number;
  recentOrders: number;
  recentTransfers: number;
}

// Recent Transaction Interface
export interface RecentTransaction {
  id: number;
  productNameAr: string;
  productNameEn: string;
  storeNameAr: string;
  storeNameEn: string;
  transactionTypeNameAr: string;
  transactionTypeNameEn: string;
  quantityChanged: number;
  transactionDate: string;
  notes: string;
}

// Top Product Interface
export interface TopProduct {
  productId: number;
  productNameAr: string;
  productNameEn: string;
  productCode: string;
  totalQuantity: number;
  storeCount: number;
}

// Store Performance Interface
export interface StorePerformance {
  storeId: number;
  storeNameAr: string;
  storeNameEn: string;
  storeCode: string;
  isMainWarehouse: boolean;
  totalProducts: number;
  totalQuantity: number;
  lowStockProducts: number;
  zeroStockProducts: number;
}

// Monthly Transaction Interface
export interface MonthlyTransaction {
  year: number;
  month: number;
  transactionCount: number;
  totalQuantityChanged: number;
  purchaseTransactions: number;
  saleTransactions: number;
  adjustmentTransactions: number;
}

// Alert Interface
export interface Alert {
  type: string;
  severity: 'High' | 'Medium' | 'Low';
  message: string;
  productId?: number;
  storeId?: number;
  currentQuantity?: number;
}

// Dashboard Card Interface
export interface DashboardCard {
  title: string;
  value: number;
  icon: string;
  color: string;
  subtitle?: string;
  trend?: 'up' | 'down' | 'stable';
  trendValue?: number;
}

