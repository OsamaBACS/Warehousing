import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OrderReportFilter, OrderReportResponse } from '../models/order-report';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {

  private readonly baseUrl = `${environment.baseUrl}/Reports`;

  constructor(private http: HttpClient) { }

  getOrderSummary(payload: OrderReportFilter) {
    return this.http.post<OrderReportResponse>(`${this.baseUrl}/order-summary`, payload);
  }
}

