import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { InventoryTransaction, InventoryTransactionPagination } from '../models/inventoryTransaction';

@Injectable({
  providedIn: 'root'
})
export class InventoryTransactionsService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/InventoryTransaction/';

  GetTransactionByProductId(pageIndex: number, pageSize: number, id: number, storeId: number) {
    return this.http.get<InventoryTransactionPagination>(
      this.url +
      `GetTransactionByProductId?pageIndex=${pageIndex}&pageSize=${pageSize}&id=${id}&storeId=${storeId}`
    );
  }

  GetAllTransactions(pageIndex: number, pageSize: number, storeId?: number, productId?: number, fromDate?: Date, toDate?: Date) {
    let params = `pageIndex=${pageIndex}&pageSize=${pageSize}`;
    if (storeId) params += `&storeId=${storeId}`;
    if (productId) params += `&productId=${productId}`;
    if (fromDate) params += `&fromDate=${fromDate.toISOString()}`;
    if (toDate) params += `&toDate=${toDate.toISOString()}`;
    
    return this.http.get<InventoryTransactionPagination>(`${this.url}GetAllTransactions?${params}`);
  }

  GetStockMovementReport(storeId?: number, fromDate?: Date, toDate?: Date) {
    let params = '';
    if (storeId) params += `storeId=${storeId}`;
    if (fromDate) params += `${params ? '&' : ''}fromDate=${fromDate.toISOString()}`;
    if (toDate) params += `${params ? '&' : ''}toDate=${toDate.toISOString()}`;
    
    return this.http.get<any[]>(`${this.url}GetStockMovementReport?${params}`);
  }

  GetInventoryValuationReport(storeId?: number) {
    const params = storeId ? `?storeId=${storeId}` : '';
    return this.http.get<any>(`${this.url}GetInventoryValuationReport${params}`);
  }

  GetLowStockReport(threshold: number = 10, storeId?: number) {
    let params = `threshold=${threshold}`;
    if (storeId) params += `&storeId=${storeId}`;
    
    return this.http.get<any>(`${this.url}GetLowStockReport?${params}`);
  }

  GetTransactionTrends(storeId?: number, productId?: number, months: number = 6) {
    let params = `months=${months}`;
    if (storeId) params += `&storeId=${storeId}`;
    if (productId) params += `&productId=${productId}`;
    
    return this.http.get<any[]>(`${this.url}GetTransactionTrends?${params}`);
  }

  GetTopMovingProducts(storeId?: number, fromDate?: Date, toDate?: Date, topCount: number = 10) {
    let params = `topCount=${topCount}`;
    if (storeId) params += `&storeId=${storeId}`;
    if (fromDate) params += `&fromDate=${fromDate.toISOString()}`;
    if (toDate) params += `&toDate=${toDate.toISOString()}`;
    
    return this.http.get<any[]>(`${this.url}GetTopMovingProducts?${params}`);
  }
}
