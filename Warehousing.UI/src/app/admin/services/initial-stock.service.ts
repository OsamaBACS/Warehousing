import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface InitialStockItem {
  productId: number;
  storeId: number;
  quantity: number;
}

export interface InitialStockSetupRequest {
  items: InitialStockItem[];
  notes?: string;
}

export interface InitialStockSetupResponse {
  success: boolean;
  results: Array<{
    productId: number;
    storeId: number;
    quantity: number;
    action: string;
    success: boolean;
  }>;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class InitialStockService {
  private url = `${environment.baseUrl}/Inventory/`;

  constructor(private http: HttpClient) { }

  setupInitialStock(request: InitialStockSetupRequest): Observable<InitialStockSetupResponse> {
    return this.http.post<InitialStockSetupResponse>(`${this.url}initial-stock-setup`, request);
  }
}












