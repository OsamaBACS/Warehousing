import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Category } from '../models/category';

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Categories/';

  GetCategories() {
    return this.http.get<Category[]>(`${this.url}GetCategories`);
  }

  GetActiveCategories() {
    return this.http.get<Category[]>(`${this.url}GetActiveCategories`);
  }

  GetCategoryById(categoryId: number) {
    return this.http.get<Category>(`${this.url}GetCategoryById?Id=${categoryId}`);
  }

  GetCategoriesWithSubCategories() {
    return this.http.get<Category[]>(`${this.url}GetCategoriesWithSubCategories`);
  }

  SearchCategories(keyword: string) {
    return this.http.get<Category[]>(`${this.url}SearchCategories?keyword=${keyword}`);
  }

  SaveCategory(category: any) {
    // If category is FormData, don't set content-type header
    if (category instanceof FormData) {
      return this.http.post<any>(`${this.url}SaveCategory`, category, {
        headers: {
          // Don't set Content-Type header - let browser set it with boundary
        }
      });
    } else {
      // For regular JSON data
      return this.http.post<any>(`${this.url}SaveCategory`, category);
    }
  }

  DeleteCategory(categoryId: number) {
    return this.http.delete<any>(`${this.url}DeleteCategory?Id=${categoryId}`);
  }
}
