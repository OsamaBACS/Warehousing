import { ResolveFn } from '@angular/router';
import { SubCategory } from '../models/SubCategory';
import { SubCategoryService } from '../services/sub-category.service';
import { inject } from '@angular/core';

export const SubCategoriesResolver: ResolveFn<SubCategory[]> = 
(
  route, 
  state,
  subCategoryService: SubCategoryService = inject(SubCategoryService)
) => {
  return subCategoryService.GetSubCategories();
};
