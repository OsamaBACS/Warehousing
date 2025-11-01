import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Inventory } from '../models/Inventory';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Inventory';

  GetAllInventory() {
    return this.http.get<Inventory[]>(`${this.url}`);
  }

  GetInventoryByStore(storeId: number) {
    return this.http.get<Inventory[]>(`${this.url}/by-store/${storeId}`);
  }

  GetInventoryByProduct(productId: number) {
    return this.http.get<Inventory[]>(`${this.url}/by-product/${productId}`);
  }

  GetInventorySummary() {
    return this.http.get<any>(`${this.url}/summary`);
  }

  GetLowStockItems() {
    return this.http.get<Inventory[]>(`${this.url}/low-stock`);
  }

  AdjustInventory(inventoryId: number, newQuantity: number, notes: string) {
    return this.http.post<any>(`${this.url}/AdjustInventory`, {
      inventoryId,
      newQuantity,
      notes
    });
  }

  BulkAdjustInventory(adjustments: any[]) {
    return this.http.post<any>(`${this.url}/BulkAdjustInventory`, adjustments);
  }

  InitialStockSetup(data: any) {
    return this.http.post<any>(`${this.url}/initial-stock-setup`, data);
  }

  BulkInitialStockSetup(data: any) {
    return this.http.post<any>(`${this.url}/bulk-initial-stock-setup`, data);
  }

  SingleInitialStockSetup(data: any) {
    return this.http.post<any>(`${this.url}/single-initial-stock-setup`, data);
  }
}



