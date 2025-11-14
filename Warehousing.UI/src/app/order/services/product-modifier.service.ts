import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ProductModifierGroup } from '../models/ProductModifier';

@Injectable({
  providedIn: 'root'
})
export class ProductModifierService {
  private apiUrl = `${environment.baseUrl}/ProductModifiers`;

  constructor(private http: HttpClient) { }

  getProductModifiersByProductId(productId: number) {
    return this.http.get<ProductModifierGroup[]>(`${this.apiUrl}/groups/by-product/${productId}`);
  }

  getModifierOptions(modifierId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/${modifierId}/options`);
  }
}
