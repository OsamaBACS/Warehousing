import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product, ProductPagination } from '../models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Products/';

  GetProductsPagination(pageIndex: number, pageSize: number) {
    return this.http.get<ProductPagination>(
      this.url +
      `GetProductsPagination?pageIndex=${pageIndex}&pageSize=${pageSize}`
    );
  }

  SearchProductsPagination(pageIndex: number, pageSize: number, keyword: string, storeId: number) {
    return this.http.get<ProductPagination>(
      this.url +
      `SearchProductsPagination?pageIndex=${pageIndex}&pageSize=${pageSize}&keyword=${keyword}&StoreId=${storeId}`
    );
  }

  SaveProduct(product: any) {
    return this.http.post<any>(`${this.url}SaveProduct`, product);
  }

  GetProducts() {
    return this.http.get<Product[]>(`${this.url}GetProducts`)
  }

  GetProductById(productId: number) {
    return this.http.get<Product>(`${this.url}GetProductById?Id=${productId}`)
  }

  GetProductsBySubCategoryId(subCategoryId: number) {
    return this.http.get<Product[]>(`${this.url}GetProductsBySubCategoryId?SubCategoryId=${subCategoryId}`)
  }

  GetTotalCount() {
    return this.http.get<number>(`${this.url}GetTotalCount`)
  }
}
