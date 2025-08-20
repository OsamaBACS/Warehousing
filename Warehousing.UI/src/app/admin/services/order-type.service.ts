import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OrderTypeDto } from '../models/OrderTypeDto';

@Injectable({
  providedIn: 'root'
})
export class OrderTypeService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/OrderType/';

  GetOrderTypes() {
    return this.http.get<OrderTypeDto[]>(`${this.url}GetOrderTypes`);
  }

  GetActiveOrderTypes() {
    return this.http.get<OrderTypeDto[]>(`${this.url}GetActiveOrderTypes`);
  }

  GetOrderTypeById(orderTypeId: number) {
    return this.http.get<OrderTypeDto>(`${this.url}GetOrderTypeById?Id=${orderTypeId}`);
  }

  SaveOrderType(category: any) {
    return this.http.post<any>(`${this.url}SaveOrderType`, category);
  }
}
