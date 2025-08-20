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
}
