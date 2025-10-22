import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductsService } from '../../admin/services/products.service';
import { CartService } from '../../shared/services/cart.service';
import { AbstractControl, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { Product } from '../../admin/models/product';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SubCategory } from '../../admin/models/SubCategory';
import { OrderDto } from '../../admin/models/OrderDto';
import { OrderItemDto } from '../../admin/models/OrderItemDto';
// Removed unnecessary imports since we navigate to detail page

@Component({
  selector: 'app-order-products',
  standalone: false,
  templateUrl: './order-products.component.html',
  styleUrl: './order-products.component.scss'
})
export class OrderProductsComponent implements OnInit {

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService,
    public cartService: CartService,
    private fb: FormBuilder,
    private breadcrumbService: BreadcrumbService,
    // Removed unnecessary services since we navigate to detail page
  ) {
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
  }

  ngOnInit(): void {
    this.serverUrl = this.productsService.url.substring(0, this.productsService.url.indexOf('api'));
    
    this.route.parent?.paramMap.subscribe(params => {
      const orderTypeIdParam = params.get('orderTypeId');
      this.orderTypeId = orderTypeIdParam !== null ? Number(orderTypeIdParam) : 1; // default 1 if null
      if (this.cartService.cartItems.length <= 0) {
        this.cartService.orderTypeId = this.orderTypeId;
        if (this.cartService.cartForm) {
          this.cartService.cartForm.patchValue({ orderTypeId: this.orderTypeId });
        }
      }
    });

    this.route.paramMap.subscribe(params => {
      const categoryIdParam = params.get('categoryId');
      const subCategoryIdParam = params.get('subCategoryId');

      this.categoryId = categoryIdParam !== null ? Number(categoryIdParam) : 1;
      this.subCategoryId = subCategoryIdParam !== null ? Number(subCategoryIdParam) : 1;

      const subCategory = this.subCategories.find(sc => sc.id === this.subCategoryId);
      const categoryName = subCategory?.category?.nameAr || 'التصنيف';
      const subCategoryName = subCategory?.nameAr || 'المنتجات';

      this.breadcrumbService.setFrom([
        { label: 'الرئيسية', route: '/home' },
        { label: 'التصنيفات', route: `/order/${this.orderTypeId}/categories` },
        { label: categoryName, route: `/order/${this.orderTypeId}/categories/${this.categoryId}/sub-categories` },
        { label: subCategoryName, route: null }
      ]);

      this.loadProducts(this.subCategoryId);
    });
  }

  orderTypeId!: number;
  categoryId!: number;
  subCategoryId!: number;
  products$!: Observable<Product[]>;
  resourcesUrl = environment.resourcesUrl;
  subCategories!: SubCategory[];
  selectedSubCategory!: SubCategory;
  serverUrl = '';
  // No longer needed since we navigate to detail page

  loadProducts(subCategoryId: number) {
    this.products$ = this.productsService.GetProductsBySubCategoryId(subCategoryId);
    // Set the selected subcategory for display
    this.selectedSubCategory = this.subCategories.find(sc => sc.id === subCategoryId)!;
  }

  // Removed loadProductVariants and loadProductModifiers - now handled in detail page

  // Removed cart-related methods - now handled in detail page


  // Removed cart-related methods - now handled in detail page

  // Removed all cart-related methods - now handled in detail page

  // Removed getTotalQuantity method - now using product.totalQuantity from API

  // Removed all variant/modifier methods - now handled in detail page

  // Navigation method
  viewProductDetail(product: Product): void {
    this.router.navigate(['/order', this.orderTypeId, 'product', product.id]);
  }

}