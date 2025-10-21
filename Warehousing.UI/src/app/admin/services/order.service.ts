import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OrderDto, OrderFilters, OrderPagination } from '../models/OrderDto';
import { ApiResponse } from '../models/ApiResponse';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Order/';

  GetOrdersPagination(pageIndex: number, pageSize: number, orderTypeId: number) {
    return this.http.get<OrderPagination>(`${this.url}GetOrdersPagination?pageIndex=${pageIndex}&pageSize=${pageSize}&orderTypeId=${orderTypeId}`);
  }

  FilterOrdersPagination(filters: OrderFilters) {
    return this.http.post<OrderPagination>(`${this.url}FilterOrdersPagination`, filters);
  }

  GetOrderById(id: number) {
    return this.http.get<OrderDto>(`${this.url}GetOrderById?id=${id}`);
  }

  GetOrdersByUserId(filters: OrderFilters) {
    return this.http.post<OrderPagination>(`${this.url}GetOrdersByUserId`, filters);
  }

  SaveOrder(orderDto: OrderDto) {
    return this.http.post<any>(`${this.url}SaveOrder`, orderDto);
  }

  UpdateOrderStatusToPending(id: number) {
    return this.http.post<ApiResponse>(`${this.url}UpdateOrderStatusToPending/${id}`, {});
  }

  UpdateOrderStatusToComplete(id: number) {
    return this.http.post<ApiResponse>(`${this.url}UpdateOrderStatusToComplete/${id}`, {});
  }

  UpdateApprovedOrder(id: number, orderDto: OrderDto) {
    return this.http.post<ApiResponse>(`${this.url}UpdateApprovedOrder/${id}`, orderDto);
  }

  CancelApprovedOrder(id: number) {
    return this.http.post<ApiResponse>(`${this.url}CancelApprovedOrder/${id}`, {});
  }
}
