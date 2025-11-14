import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ProductVariant } from '../models/ProductVariant';

@Injectable({
  providedIn: 'root'
})
export class ProductVariantService {
  private apiUrl = `${environment.baseUrl}/ProductVariants`;

  constructor(private http: HttpClient) { }

  getProductVariantsByProductId(productId: number) {
    return this.http.get<ProductVariant[]>(`${this.apiUrl}/by-product/${productId}`);
  }
}
