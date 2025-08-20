import { ResolveFn } from '@angular/router';
import { CategoriesService } from '../services/categories.service';
import { inject } from '@angular/core';
import { Category } from '../models/category';

export const CategoriesResolver: ResolveFn<Category[]> = 
(
  route, 
  state,
  categoriesService: CategoriesService = inject(CategoriesService)
) => {
  return categoriesService.GetCategories();
};
