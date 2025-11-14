import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  ProductModifier, 
  ProductModifierCreateRequest, 
  ProductModifierUpdateRequest,
  ProductModifierOption,
  ProductModifierOptionCreateRequest,
  ProductModifierOptionUpdateRequest,
  ProductModifierGroup,
  ProductModifierGroupCreateRequest,
  ProductModifierGroupUpdateRequest
} from '../models/ProductModifier';

@Injectable({
  providedIn: 'root'
})
export class ProductModifierService {
  private apiUrl = `${environment.baseUrl}/ProductModifiers`;

  constructor(private http: HttpClient) { }

  // Modifiers CRUD
  getAllModifiers(): Observable<ProductModifier[]> {
    return this.http.get<ProductModifier[]>(this.apiUrl);
  }

  getModifier(id: number): Observable<ProductModifier> {
    return this.http.get<ProductModifier>(`${this.apiUrl}/${id}`);
  }

  createModifier(modifier: ProductModifierCreateRequest): Observable<ProductModifier> {
    return this.http.post<ProductModifier>(this.apiUrl, modifier);
  }

  updateModifier(id: number, modifier: ProductModifierUpdateRequest): Observable<ProductModifier> {
    return this.http.put<ProductModifier>(`${this.apiUrl}/${id}`, modifier);
  }

  deleteModifier(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // Modifier Options CRUD
  getModifierOptions(modifierId: number): Observable<ProductModifierOption[]> {
    return this.http.get<ProductModifierOption[]>(`${this.apiUrl}/${modifierId}/options`);
  }

  createModifierOption(modifierId: number, option: ProductModifierOptionCreateRequest): Observable<ProductModifierOption> {
    return this.http.post<ProductModifierOption>(`${this.apiUrl}/${modifierId}/options`, option);
  }

  updateModifierOption(id: number, option: ProductModifierOptionUpdateRequest): Observable<ProductModifierOption> {
    return this.http.put<ProductModifierOption>(`${this.apiUrl}/options/${id}`, option);
  }

  deleteModifierOption(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/options/${id}`);
  }

  // Modifier Groups CRUD
  getModifierGroupsByProduct(productId: number): Observable<ProductModifierGroup[]> {
    return this.http.get<ProductModifierGroup[]>(`${this.apiUrl}/groups/by-product/${productId}`);
  }

  createModifierGroup(group: ProductModifierGroupCreateRequest): Observable<ProductModifierGroup> {
    return this.http.post<ProductModifierGroup>(`${this.apiUrl}/groups`, group);
  }

  updateModifierGroup(id: number, group: ProductModifierGroupUpdateRequest): Observable<ProductModifierGroup> {
    return this.http.put<ProductModifierGroup>(`${this.apiUrl}/groups/${id}`, group);
  }

  deleteModifierGroup(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/groups/${id}`);
  }
}
