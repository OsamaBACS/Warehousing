import { ResolveFn } from '@angular/router';
import { ProductsService } from '../services/products.service';
import { inject } from '@angular/core';
import { Product } from '../models/product';

export const ProductsResolver: ResolveFn<Product[]> = 
(
  route, 
  state,
  productsService: ProductsService = inject(ProductsService)
) => {
  return productsService.GetProducts();
};
