import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { StoreTransfer, StoreTransferDto } from '../models/storeTransfer';

@Injectable({
  providedIn: 'root'
})
export class StoreTransferService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/StoreTransfers/';

  GetTransfers() {
    return this.http.get<StoreTransfer[]>(`${this.url}GetTransfers`);
  }

  GetTransferById(transferId: number) {
    return this.http.get<StoreTransfer>(`${this.url}GetTransferById?Id=${transferId}`);
  }

  GetTransferWithItems(transferId: number) {
    return this.http.get<StoreTransfer>(`${this.url}GetTransferWithItems?Id=${transferId}`);
  }

  GetTransfersByStore(storeId: number) {
    return this.http.get<StoreTransfer[]>(`${this.url}GetTransfersByStore?storeId=${storeId}`);
  }

  CreateTransfer(transfer: StoreTransferDto) {
    return this.http.post<any>(`${this.url}CreateTransfer`, transfer);
  }

  UpdateTransfer(transferId: number, transfer: StoreTransferDto) {
    return this.http.put<any>(`${this.url}UpdateTransfer?Id=${transferId}`, transfer);
  }

  DeleteTransfer(transferId: number) {
    return this.http.delete<any>(`${this.url}DeleteTransfer?Id=${transferId}`);
  }
}

