import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OrderDto, OrderFilters, OrderPagination } from '../models/OrderDto';
import { ApiResponse } from '../models/ApiResponse';
import { ProductVariant } from '../models/ProductVariant';
import { ProductModifierGroup } from '../models/ProductModifier';
import { OrderWithVariantsAndModifiers } from '../models/OrderItemModifier';

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

  // New methods for variants and modifiers
  GetProductVariants(productId: number) {
    return this.http.get<ProductVariant[]>(`${this.url}GetProductVariants/${productId}`);
  }

  GetProductModifiers(productId: number) {
    return this.http.get<ProductModifierGroup[]>(`${this.url}GetProductModifiers/${productId}`);
  }

  SaveOrderWithVariantsAndModifiers(order: OrderWithVariantsAndModifiers) {
    return this.http.post<ApiResponse>(`${this.url}SaveOrderWithVariantsAndModifiers`, order);
  }
}
