import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { StoreTransfer, StoreTransferDto } from '../models/storeTransfer';

@Injectable({
  providedIn: 'root'
})
export class StoreTransferService {

  private readonly baseUrl = `${environment.baseUrl}/StoreTransfer`;

  constructor(private http: HttpClient) { }

  getTransfers() {
    return this.http.get<StoreTransfer[]>(this.baseUrl);
  }

  getTransferById(transferId: number) {
    return this.http.get<StoreTransfer>(`${this.baseUrl}/${transferId}`);
  }

  getTransferWithItems(transferId: number) {
    return this.http.get<StoreTransfer>(`${this.baseUrl}/with-items/${transferId}`);
  }

  getTransfersByStore(storeId: number, isFromStore: boolean = true) {
    return this.http.get<StoreTransfer[]>(`${this.baseUrl}/by-store/${storeId}?isFromStore=${isFromStore}`);
  }

  getTransfersByStatus(statusId: number) {
    return this.http.get<StoreTransfer[]>(`${this.baseUrl}/by-status/${statusId}`);
  }

  createTransfer(transfer: StoreTransferDto) {
    return this.http.post<StoreTransfer>(this.baseUrl, transfer);
  }

  updateTransfer(transferId: number, transfer: StoreTransferDto) {
    return this.http.put<StoreTransfer>(`${this.baseUrl}/${transferId}`, transfer);
  }

  deleteTransfer(transferId: number) {
    return this.http.delete<void>(`${this.baseUrl}/${transferId}`);
  }

  completeTransfer(transferId: number) {
    return this.http.post(`${this.baseUrl}/${transferId}/complete`, {});
  }

  cancelTransfer(transferId: number) {
    return this.http.post(`${this.baseUrl}/${transferId}/cancel`, {});
  }
}