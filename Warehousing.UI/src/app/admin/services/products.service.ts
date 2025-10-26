import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product, ProductPagination } from '../models/product';
import { Inventory } from '../models/Inventory';

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

  SearchProducts(keyword: string) {
    return this.http.get<Product[]>(`${this.url}SearchProducts?keyword=${keyword}`);
  }

  GetProductsByCategory(categoryId: number) {
    return this.http.get<Product[]>(`${this.url}GetProductsByCategory?categoryId=${categoryId}`);
  }

  GetProductsBySubCategory(subCategoryId: number) {
    return this.http.get<Product[]>(`${this.url}GetProductsBySubCategory?subCategoryId=${subCategoryId}`);
  }

  GetLowStockProducts() {
    return this.http.get<Product[]>(`${this.url}GetLowStockProducts`);
  }

  GetProductInventory(productId: number) {
    return this.http.get<Inventory[]>(`${this.url}GetProductInventory?productId=${productId}`);
  }

  ValidateStock(productId: number, storeId: number, quantity: number) {
    return this.http.get<any>(`${this.url}ValidateStock?productId=${productId}&storeId=${storeId}&quantity=${quantity}`);
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

  GetProductVariantsStock(productId: number, storeId: number) {
    return this.http.get<any[]>(`${this.url}${productId}/variants-stock?storeId=${storeId}`);
  }

  DistributeStockToVariants(productId: number, request: any) {
    return this.http.post<any>(`${this.url}${productId}/distribute-stock-to-variants`, request);
  }

  SetVariantStock(productId: number, request: any) {
    return this.http.post<any>(`${this.url}${productId}/set-variant-stock`, request);
  }
}
