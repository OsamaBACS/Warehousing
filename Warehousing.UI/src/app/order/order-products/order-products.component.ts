import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductsService } from '../../admin/services/products.service';
import { CartService } from '../../shared/services/cart.service';
import { AbstractControl, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { OrderBreadcrumbService } from '../services/order-breadcrumb.service';
import { Product } from '../../admin/models/product';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SubCategory } from '../../admin/models/SubCategory';
import { StoreSimple } from '../../admin/models/StoreSimple';
import { OrderDto } from '../../admin/models/OrderDto';
import { OrderItemDto } from '../../admin/models/OrderItemDto';
import { AuthService } from '../../core/services/auth.service';
import { ImageUrlService } from '../../shared/services/image-url.service';
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
    private orderBreadcrumbService: OrderBreadcrumbService,
    private authService: AuthService,
    public imageUrlService: ImageUrlService,
    // Removed unnecessary services since we navigate to detail page
  ) {
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
  }

  ngOnInit(): void {
    this.serverUrl = this.productsService.url.substring(0, this.productsService.url.indexOf('api'));
    
    this.route.parent?.paramMap.subscribe(params => {
      const orderTypeIdParam = params.get('orderTypeId');
      this.orderTypeId = orderTypeIdParam !== null ? Number(orderTypeIdParam) : 1; // default 1 if null
      
      // Always update the cart service orderTypeId to match the current route
      // This ensures the correct order type is used regardless of cart state
      this.cartService.setOrderTypeId(this.orderTypeId);
    });

    this.route.paramMap.subscribe(params => {
      const categoryIdParam = params.get('categoryId');
      const subCategoryIdParam = params.get('subCategoryId');

      this.categoryId = categoryIdParam !== null ? Number(categoryIdParam) : 1;
      this.subCategoryId = subCategoryIdParam !== null ? Number(subCategoryIdParam) : 1;

      const subCategory = this.subCategories.find(sc => sc.id === this.subCategoryId);
      const categoryName = subCategory?.category?.nameAr || 'التصنيف';
      const subCategoryName = subCategory?.nameAr || 'المنتجات';

      // Set breadcrumbs using the order breadcrumb service
      this.orderBreadcrumbService.setOrderProductsBreadcrumbs(
        this.orderTypeId, 
        this.categoryId, 
        categoryName, 
        this.subCategoryId, 
        subCategoryName
      );

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
  
  // Variant stock data for unified display
  variantStockData: { [key: string]: number } = {};
  // No longer needed since we navigate to detail page

  loadProducts(subCategoryId: number) {
    this.products$ = this.productsService.GetProductsBySubCategoryId(subCategoryId).pipe(
      tap(products => {
        console.log('Products from API:', products);
        console.log('User product permissions:', this.authService.getProductIds());
        // Load variant stock data for all products
        this.loadVariantStockData(products);
      }),
      map(products => {
        // Filter products based on user permissions
        const filteredProducts = products.filter(product => {
          const hasPermission = this.authService.hasProduct(product.id!);
          console.log(`Product ${product.id} (${product.nameEn}): hasPermission = ${hasPermission}`);
          return hasPermission;
        });
        console.log('Filtered products:', filteredProducts);
        return filteredProducts;
      })
    );
    // Set the selected subcategory for display
    this.selectedSubCategory = this.subCategories.find(sc => sc.id === subCategoryId)!;
  }

  loadVariantStockData(products: Product[]): void {
    // Process the actual inventory data from the API response
    console.log('🔍 Loading variant stock data for products:', products.length);
    
    products.forEach(product => {
      console.log(`📦 Product ${product.id} (${product.nameAr}):`, {
        inventories: product.inventories?.length || 0,
        variants: product.variants?.length || 0
      });
      
      if (product.inventories && product.inventories.length > 0) {
        product.inventories.forEach(inventory => {
          console.log(`📊 Inventory record:`, {
            productId: inventory.productId,
            storeId: inventory.storeId,
            variantId: inventory.variantId,
            quantity: inventory.quantity,
            storeName: inventory.store?.nameAr
          });
          
          // Check if this inventory record is for a variant (has variantId)
          if (inventory.variantId) {
            const key = `${product.id}-${inventory.variantId}-${inventory.storeId}`;
            this.variantStockData[key] = inventory.quantity;
            console.log(`✅ Added variant stock: ${key} = ${inventory.quantity}`);
          }
        });
      }
    });
    
    console.log('📈 Final variant stock data:', this.variantStockData);
  }

  getVariantStock(productId: number, variantId: number, storeId: number): number {
    // This will be populated when we load variant stock data
    return this.variantStockData[`${productId}-${variantId}-${storeId}`] || 0;
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
      return this.getStoresForVariant(product, variant.id!).length > 0;
    });
  }

  getMainProductInventories(product: Product): any[] {
    // Return only main product inventories (not variant-specific)
    return product.inventories?.filter(inv => !inv.variantId) || [];
  }

  getTotalVariantsWithStock(product: Product): number {
    if (!product.variants) return 0;
    
    return product.variants.filter(variant => {
      return this.getStoresForVariant(product, variant.id!).length > 0;
    }).length;
  }

  getStoresForVariant(product: Product, variantId: number): StoreSimple[] {
    // Get stores that have stock for this specific variant
    const stores: StoreSimple[] = [];
    
    console.log(`🔍 Getting stores for variant ${variantId} of product ${product.id}:`, {
      productInventories: product.inventories?.length || 0,
      productVariants: product.variants?.length || 0
    });
    
    if (product && product.inventories) {
      // Get stores that have stock for this variant
      const variantInventories = product.inventories.filter(inv => 
        inv.variantId === variantId && inv.quantity > 0
      );
      
      console.log(`📊 Found ${variantInventories.length} inventory records for variant ${variantId}:`, variantInventories);
      
      variantInventories.forEach(inventory => {
        console.log(`🏪 Processing inventory:`, {
          storeId: inventory.storeId,
          storeName: inventory.store?.nameAr,
          quantity: inventory.quantity
        });
        
        if (inventory.store) {
          const storeSimple = {
            id: inventory.store.id,
            name: inventory.store.nameAr || `مستودع ${inventory.store.id}`,
            code: inventory.store.code || `S${inventory.store.id}`,
            isActive: true
          };
          stores.push(storeSimple);
          console.log(`✅ Added store:`, storeSimple);
        }
      });
    }
    
    console.log(`🎯 Final stores for variant ${variantId}:`, stores);
    return stores;
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