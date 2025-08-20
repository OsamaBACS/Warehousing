import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { SubCategory } from '../models/SubCategory';

@Injectable({
  providedIn: 'root'
})
export class SubCategoryService {
  
  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/SubCategory/';

  GetSubCategories() {
    return this.http.get<SubCategory[]>(`${this.url}GetSubCategories`);
  }

  GetActiveSubCategories() {
    return this.http.get<SubCategory[]>(`${this.url}GetActiveSubCategories`);
  }

  GetSubCategoryById(subCategoryId: number) {
    return this.http.get<SubCategory>(`${this.url}GetSubCategoryById?Id=${subCategoryId}`);
  }

  GetSubCategoryByCategoryId(categoryId: number) {
    return this.http.get<SubCategory[]>(`${this.url}GetSubCategoryByCategoryId?CategoryId=${categoryId}`);
  }

  SaveSubCategory(subCategory: any) {
    return this.http.post<any>(`${this.url}SaveSubCategory`, subCategory);
  }
}
