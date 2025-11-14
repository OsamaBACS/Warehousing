import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProductVariant, ProductVariantCreateRequest, ProductVariantUpdateRequest } from '../models/ProductVariant';

@Injectable({
  providedIn: 'root'
})
export class ProductVariantService {
  private apiUrl = `${environment.baseUrl}/ProductVariants`;

  constructor(private http: HttpClient) { }

  // Get variants by product ID
  getVariantsByProduct(productId: number): Observable<ProductVariant[]> {
    return this.http.get<ProductVariant[]>(`${this.apiUrl}/by-product/${productId}`);
  }

  // Get single variant
  getVariant(id: number): Observable<ProductVariant> {
    return this.http.get<ProductVariant>(`${this.apiUrl}/${id}`);
  }

  // Create new variant
  createVariant(variant: ProductVariantCreateRequest): Observable<ProductVariant> {
    return this.http.post<ProductVariant>(this.apiUrl, variant);
  }

  // Update variant
  updateVariant(id: number, variant: ProductVariantUpdateRequest): Observable<ProductVariant> {
    return this.http.put<ProductVariant>(`${this.apiUrl}/${id}`, variant);
  }

  // Delete variant
  deleteVariant(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
