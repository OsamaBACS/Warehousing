import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Store } from '../models/store';
import { StoreSimple } from '../models/StoreSimple';
import { Inventory } from '../models/Inventory';

@Injectable({
  providedIn: 'root'
})
export class StoreService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Stores/';

  GetStores() {
    return this.http.get<Store[]>(`${this.url}GetStores`);
  }

  GetActiveStores() {
    return this.http.get<StoreSimple[]>(`${this.url}active`);
  }

  GetWarehouses() {
    return this.http.get<Store[]>(`${this.url}warehouses`);
  }

  GetStoreById(storeId: number) {
    return this.http.get<Store>(`${this.url}GetStoreById?Id=${storeId}`);
  }

  GetStoreByCode(code: string) {
    return this.http.get<Store>(`${this.url}by-code/${code}`);
  }

  GetStoreInventorySummary(storeId: number) {
    return this.http.get<any>(`${this.url}${storeId}/inventory-summary`);
  }

  GetStoreProducts(storeId: number) {
    return this.http.get<any[]>(`${this.url}${storeId}/products`);
  }

  SaveStore(Store: any) {
    return this.http.post<any>(`${this.url}SaveStore`, Store);
  }

  DeleteStore(storeId: number) {
    return this.http.delete<any>(`${this.url}DeleteStore?Id=${storeId}`);
  }
}
