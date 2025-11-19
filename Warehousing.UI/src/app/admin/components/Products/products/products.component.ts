import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ProductsService } from '../../../services/products.service';
import { CategoriesService } from '../../../services/categories.service';
import { UnitsService } from '../../../services/units.service';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../../core/services/auth.service';
import { Category } from '../../../models/category';
import { Unit } from '../../../models/unit';
import { Observable, tap, map } from 'rxjs';
import { Product, ProductPagination } from '../../../models/product';
import { PermissionsEnum } from '../../../constants/enums/permissions.enum';
import { Store } from '../../../models/store';
import { AdminBreadcrumbService } from '../../../services/admin-breadcrumb.service';
import { ImageUrlService } from '../../../../shared/services/image-url.service';

@Component({
  selector: 'app-products',
  standalone: false,
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {

  constructor(
    private productService: ProductsService,
    private categoriesService: CategoriesService,
    private unitsService: UnitsService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService,
    private authService: AuthService,
    private adminBreadcrumbService: AdminBreadcrumbService,
    public imageUrlService: ImageUrlService
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
    this.productForm = this.fb.group({
      id: [null],
      name: ['', Validators.required],
      price: [0, Validators.required]
    });
    this.searchForm = this.fb.group({
      keyword: [null, Validators.required],
    });
  }
  ngOnInit(): void {
    // Set breadcrumbs for products management
    this.adminBreadcrumbService.setProductsBreadcrumbs();
    
    // this.products$ = this.productService.GetProductsPagination(this.pageIndex, this.pageSize);
    this.loadProducts();

    this.categoriesService.GetCategories().subscribe({
      next: (res) => {
        this.categories = res;
      },
      error: (err) => {
      }
    });
    this.unitsService.GetUnits().subscribe({
      next: (res) => {
        this.units = res;
      },
      error: (err) => {
      }
    });
    this.serverUrl = this.productService.url.substring(0, this.productService.url.indexOf('api'));
  }

  categories!: Category[];
  units!: Unit[];
  products$!: Observable<ProductPagination>;
  pageIndex: number = 1;
  pageSize: number = 10;
  totalPages = 1;
  totalPagesArray: number[] = [];
  productForm!: FormGroup;
  searchForm!: FormGroup;
  editingProduct: Product | null = null;
  @ViewChild('productModal') productModalRef!: ElementRef;
  serverUrl = '';
  permissionsEnum = PermissionsEnum;
  stores: Store[] = [];
  storeId: number = 0;

  hasAnyPermission(...perms: string[]): boolean {
    return perms.some(p => this.authService.hasPermission(p));
  }

  openForm(productId: number | null) {
    if (productId) {
      this.router.navigate(['../product-form'], { relativeTo: this.route, queryParams: { productId: productId } });
    }
    else {
      this.router.navigate(['../product-form'], { relativeTo: this.route });
    }
  }

  manageVariantStock(productId: number, variantId: number) {
    // Navigate to variant stock management or open a modal
    // For now, we'll navigate to the product form with variant focus
    this.router.navigate(['../product-form'], { 
      relativeTo: this.route, 
      queryParams: { 
        productId: productId,
        variantId: variantId,
        tab: 'variants'
      } 
    });
  }

  getVariantStock(productId: number, variantId: number, storeId: number): number {
    // This will be populated when we load variant stock data
    return this.variantStockData[`${productId}-${variantId}-${storeId}`] || 0;
  }

  getTotalVariantStock(productId: number, variantId: number): number {
    // Calculate total stock across all stores for this variant
    let total = 0;
    this.stores.forEach(store => {
      total += this.getVariantStock(productId, variantId, store.id);
    });
    return total;
  }

  getVariantStockForStore(productId: number, variantId: number, storeId: number): number {
    // Alias for getVariantStock to make template more readable
    return this.getVariantStock(productId, variantId, storeId);
  }

  // Helper methods for unified display
  hasVariantStock(product: Product): boolean {
    if (!product.variants || product.variants.length === 0) return false;
    
    return product.variants.some(variant => {
      // Check if this variant has stock in any store
      return this.getStoresForVariant(product.id, variant.id!).length > 0;
    });
  }

  getMainProductInventories(product: Product): any[] {
    // Return only main product inventories (not variant-specific)
    return product.inventories?.filter(inv => !inv.variantId) || [];
  }

  getTotalVariantsWithStock(product: Product): number {
    if (!product.variants) return 0;
    
    return product.variants.filter(variant => {
      return this.getStoresForVariant(product.id, variant.id!).length > 0;
    }).length;
  }

  getStoresForVariant(productId: number, variantId: number): Store[] {
    // Get stores that have stock for this specific variant
    const stores: Store[] = [];
    
    // Get all stores from the variant stock data
    const variantStock = this.variantStockData;
    
    // Find all unique store IDs that have stock for this variant
    const storeIds = new Set<number>();
    
    Object.keys(variantStock).forEach(key => {
      const [pid, vid, sid] = key.split('-').map(Number);
      if (pid === productId && vid === variantId && variantStock[key] > 0) {
        storeIds.add(sid);
      }
    });
    
    // Convert store IDs to store objects using the stores array
    storeIds.forEach(storeId => {
      const store = this.stores.find(s => s.id === storeId);
      if (store) {
        stores.push(store);
      }
    });
    
    return stores;
  }

  variantStockData: { [key: string]: number } = {};

  loadVariantStockData(products: Product[]): void {
    // Variant stock data is now included in the main API response
    // No need for separate API calls - data is already available in product.variantStockData
    products.forEach(product => {
      if (product.variants && product.variants.length > 0 && product.variantStockData) {
        // Process the variant stock data that's already included in the API response
        Object.keys(product.variantStockData).forEach(storeKey => {
          const stockData = product.variantStockData![storeKey] as any[];
          const storeId = storeKey.replace('store_', '');
          
          stockData.forEach(variantStock => {
            const key = `${product.id}-${variantStock.variantId}-${storeId}`;
            this.variantStockData[key] = variantStock.availableQuantity;
          });
        });
      }
    });
  }

  loadProducts(loadType: string = 'GET'): void {
    if (loadType === 'GET') {
      this.searchForm.reset();
      this.products$ = this.productService.GetProductsPagination(this.pageIndex, this.pageSize).pipe(
        tap(res => {
          this.totalPages = Math.ceil(res.totals / this.pageSize);
          this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
          
          // Load variant stock data for all products
          this.loadVariantStockData(res.products);
        }),
        map(res => {
          // Filter products based on user permissions
          const filteredProducts = res.products.filter(product => this.authService.hasProduct(product.id!));
          return {
            ...res,
            products: filteredProducts,
            totals: filteredProducts.length
          };
        })
      );
    }
    else {
      var keyword = '';
      if (this.keyword?.value) {
        keyword = this.keyword.value?.toString();
      }

        this.products$ = this.productService.SearchProductsPagination(this.pageIndex, this.pageSize, keyword, this.storeId).pipe(
          tap(res => {
            this.totalPages = Math.ceil(res.totals / this.pageSize);
            this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
          }),
          map(res => {
            // Filter products based on user permissions
            const filteredProducts = res.products.filter(product => this.authService.hasProduct(product.id!));
            return {
              ...res,
              products: filteredProducts,
              totals: filteredProducts.length
            };
          })
        );
    }
  }

  getPageRange(): number[] {
    const delta = 2; // how many pages to show before/after current
    const range: number[] = [];

    const left = Math.max(2, this.pageIndex - delta);
    const right = Math.min(this.totalPages - 1, this.pageIndex + delta);

    for (let i = left; i <= right; i++) {
      range.push(i);
    }

    return range;
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex = page;

    if (this.keyword.value || this.storeId > 0) {
      this.loadProducts('SEARCH');
    }
    else {
      this.loadProducts();
    }
  }

  get keyword(): FormControl {
    return this.searchForm.get('keyword') as FormControl;
  }

  // Get total stock for a product across all stores and variants
  getTotalStock(product: any): number {
    let totalStock = 0;
    
    // Add main product inventory
    if (product.inventories && product.inventories.length > 0) {
      totalStock += product.inventories.reduce((sum: number, inv: any) => sum + (inv.quantity || 0), 0);
    }
    
    // Add variant inventories
    if (product.variants && product.variants.length > 0) {
      product.variants.forEach((variant: any) => {
        if (variant.inventories && variant.inventories.length > 0) {
          totalStock += variant.inventories.reduce((sum: number, inv: any) => sum + (inv.quantity || 0), 0);
        }
      });
    }
    
    return totalStock;
  }

  // Navigate to inventory management for a specific product
  manageInventory(productId: number): void {
    this.router.navigate(['/admin/inventory-management'], { 
      queryParams: { productId: productId } 
    });
  }
}
