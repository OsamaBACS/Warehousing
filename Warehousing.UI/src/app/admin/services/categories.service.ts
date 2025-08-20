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

  SaveCategory(category: any) {
    return this.http.post<any>(`${this.url}SaveCategory`, category);
  }
}
