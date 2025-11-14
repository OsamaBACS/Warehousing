import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface StockValidationResult {
  isValid: boolean;
  availableQuantity: number;
  requestedQuantity: number;
  shortage: number;
  message: string;
}

export interface VariantStockInfo {
  productId: number;
  variantId?: number;
  storeId: number;
  availableQuantity: number;
  reorderLevel: number;
  isLowStock: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class VariantStockService {
  private apiUrl = environment.baseUrl;

  constructor(private http: HttpClient) { }

  /**
   * Validate stock for a variant (uses variant's own stock)
   */
  validateVariantStock(productId: number, variantId: number, storeId: number, requestedQuantity: number): Observable<StockValidationResult> {
    // For separate stock approach, we validate against the variant's own inventory
    return this.http.get<StockValidationResult>(`${this.apiUrl}/product-variants/${variantId}/validate-stock`, {
      params: {
        storeId: storeId.toString(),
        quantity: requestedQuantity.toString()
      }
    });
  }

  /**
   * Get stock information for a variant
   */
  getVariantStockInfo(productId: number, variantId: number, storeId: number): Observable<VariantStockInfo> {
    return this.http.get<VariantStockInfo>(`${this.apiUrl}/products/${productId}/variant-stock`, {
      params: {
        variantId: variantId.toString(),
        storeId: storeId.toString()
      }
    });
  }

  /**
   * Get stock information for all variants of a product
   */
  getProductVariantsStock(productId: number, storeId: number): Observable<VariantStockInfo[]> {
    return this.http.get<VariantStockInfo[]>(`${this.apiUrl}/products/${productId}/variants-stock`, {
      params: {
        storeId: storeId.toString()
      }
    });
  }

  /**
   * Update stock for a variant (affects main product stock)
   */
  updateVariantStock(productId: number, variantId: number, storeId: number, quantityChange: number, notes?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/products/${productId}/variant-stock`, {
      variantId,
      storeId,
      quantityChange,
      notes
    });
  }

  /**
   * Get low stock variants for a product
   */
  getLowStockVariants(productId: number, storeId: number): Observable<VariantStockInfo[]> {
    return this.http.get<VariantStockInfo[]>(`${this.apiUrl}/products/${productId}/low-stock-variants`, {
      params: {
        storeId: storeId.toString()
      }
    });
  }
}
