import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductsService } from '../../admin/services/products.service';
import { StoreService } from '../../admin/services/store.service';
import { ProductVariantService } from '../services/product-variant.service';
import { ProductModifierService } from '../services/product-modifier.service';
import { CartService } from '../../shared/services/cart.service';
import { OrderBreadcrumbService } from '../services/order-breadcrumb.service';
import { Product } from '../../admin/models/product';
import { StoreSimple } from '../../admin/models/StoreSimple';
import { ProductVariant } from '../models/ProductVariant';
import { ProductModifierGroup } from '../models/ProductModifier';
import { environment } from '../../../environments/environment';
import { MatDialog } from '@angular/material/dialog';
import { SplitGeneralQuantityDialogComponent } from '../split-general-quantity-dialog/split-general-quantity-dialog.component';

@Component({
  selector: 'app-product-detail',
  standalone: false,
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  productId!: number;
  orderTypeId!: number;
  serverUrl = environment.resourcesUrl;
  
  // Navigation parameters
  categoryId!: number;
  subCategoryId!: number;
  
  // Stores
  allStores: StoreSimple[] = [];
  selectedStoreId: number | null = null;
  
  // Variants and Modifiers
  productVariants: ProductVariant[] = [];
  productModifiers: ProductModifierGroup[] = [];
  selectedVariantId: number | null = null;
  selectedModifiers: { [modifierId: number]: number[] } = {};
  
  // Form
  productForm!: FormGroup;
  quantity = 1;
  
  // Loading states
  isLoading = true;
  isAddingToCart = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService,
    private storeService: StoreService,
    private productVariantService: ProductVariantService,
    private productModifierService: ProductModifierService,
    private cartService: CartService,
    private orderBreadcrumbService: OrderBreadcrumbService,
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {
    this.productForm = this.fb.group({
      storeId: [null, Validators.required],
      variantId: [null],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    // First, get orderTypeId from parent route (it's in the parent route path: /order/:orderTypeId/...)
    this.route.parent?.paramMap.subscribe(params => {
      const orderTypeIdParam = params.get('orderTypeId');
      const newOrderTypeId = orderTypeIdParam ? Number(orderTypeIdParam) : 1;
      const orderTypeChanged = this.orderTypeId !== newOrderTypeId;
      this.orderTypeId = newOrderTypeId;
      console.log('ProductDetailComponent - orderTypeId from PARENT route:', this.orderTypeId, 'type:', typeof this.orderTypeId);
      
      this.categoryId = Number(params.get('categoryId'));
      this.subCategoryId = Number(params.get('subCategoryId'));
      
      // If product is already loaded and orderTypeId changed, reload stores
      if (this.product && orderTypeChanged) {
        console.log('OrderTypeId changed, reloading stores...');
        this.loadStores();
      }
    });
    
    // Then get productId from current route
    this.route.paramMap.subscribe(params => {
      this.productId = Number(params.get('productId'));
      console.log('ProductDetailComponent - productId:', this.productId);
      
      if (this.productId) {
        this.loadProduct();
        this.loadProductVariants();
        this.loadProductModifiers();
      }
    });
  }

  loadProduct(): void {
    this.isLoading = true;
    this.productsService.GetProductById(this.productId).subscribe({
      next: (product) => {
        this.product = product;
        console.log('Loaded product:', product);
        console.log('Product inventories:', product.inventories);
        console.log('Current orderTypeId when loading product:', this.orderTypeId);
        
        // Log each inventory record
        if (product.inventories) {
          product.inventories.forEach((inventory, index) => {
            console.log(`Inventory ${index}:`, {
              id: inventory.id,
              productId: inventory.productId,
              storeId: inventory.storeId,
              variantId: inventory.variantId,
              quantity: inventory.quantity
            });
          });
        }
        
        // Load variant stock data
        this.loadVariantStockData();
        // Load stores after product is loaded (so we can filter by inventory)
        // Ensure orderTypeId is set (should be from parent route subscription)
        if (!this.orderTypeId) {
          console.warn('⚠️ orderTypeId not set yet, waiting...');
          // If not set, try to get it synchronously from route snapshot
          const parentParams = this.route.parent?.snapshot?.paramMap;
          if (parentParams?.get('orderTypeId')) {
            this.orderTypeId = Number(parentParams.get('orderTypeId'));
            console.log('Set orderTypeId from snapshot:', this.orderTypeId);
          }
        }
        this.loadStores();
        
        // Set breadcrumbs for product detail
        this.setProductDetailBreadcrumbs();
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading product:', err);
        this.isLoading = false;
      }
    });
  }

  loadStores(): void {
    console.log('Loading stores...');
    console.log('Order Type ID:', this.orderTypeId);
    console.log('Product available:', !!this.product);
    console.log('Product inventories:', this.product?.inventories);
    
    this.storeService.GetActiveStores().subscribe({
      next: (stores) => {
        console.log('Raw stores loaded:', stores);
        // For purchases (orderTypeId === 1), show all stores so admin can choose
        // For sales (orderTypeId === 2), only show stores with inventory
        // Ensure orderTypeId is a number for comparison
        const orderType = Number(this.orderTypeId);
        console.log('Comparing orderType:', orderType, '=== 1?', orderType === 1);
        
        if (orderType === 1) {
          console.log('PURCHASE MODE: Showing all stores');
          this.allStores = stores;
        } else {
          console.log('SALE MODE: Filtering stores with inventory > 0');
          // Filter stores to only show those that have the product in inventory
          const filtered = this.filterStoresWithProductInventory(stores);
          this.allStores = filtered;
          console.log(`Filtered ${stores.length} stores down to ${filtered.length} stores with stock`);
          // Safety check: never show all stores for sales
          if (filtered.length !== stores.length && filtered.length < stores.length) {
            console.log('✅ Correctly filtered stores for sale');
          } else {
            console.warn('⚠️ WARNING: All stores are being shown for sale! This should not happen.');
          }
        }
        console.log('Final allStores:', this.allStores);
      },
      error: (err) => {
        console.error('Error loading stores:', err);
      }
    });
  }

  // Filter stores to only show those that have the product in inventory
  private filterStoresWithProductInventory(allStores: StoreSimple[]): StoreSimple[] {
    console.log('Filtering stores for product:', this.product);
    console.log('Product inventories:', this.product?.inventories);
    console.log('Order Type ID:', this.orderTypeId);
    
    if (!this.product || !this.product.inventories || this.product.inventories.length === 0) {
      console.log('No product or no inventories found');
      return [];
    }

    // If a variant is selected, filter by variant inventory
    if (this.selectedVariantId) {
      console.log('Filtering by variant:', this.selectedVariantId);
      return this.filterStoresWithVariantInventory(allStores);
    }

    // Get store IDs that have the product (general, no variant) with quantity > 0
    // Only consider general inventory where variantId is null or 0
    const validStoreIds = this.product.inventories
      .filter(inv => {
        const isGeneral = !inv.variantId || inv.variantId === 0;
        const qty = Number(inv.quantity) || 0;
        const hasStock = qty > 0;
        console.log(`Store ${inv.storeId}: isGeneral=${isGeneral}, quantity=${qty}, hasStock=${hasStock}`);
        return isGeneral && hasStock;
      })
      .map(inv => inv.storeId);

    console.log('Valid store IDs (with stock > 0):', validStoreIds);
    console.log('All stores:', allStores);

    // Return only stores that have the product in inventory with quantity > 0
    const filteredStores = allStores.filter(store => validStoreIds.includes(store.id));
    console.log('Filtered stores:', filteredStores);
    
    return filteredStores;
  }

  // Filter stores to only show those that have the selected variant in inventory
  private filterStoresWithVariantInventory(allStores: StoreSimple[]): StoreSimple[] {
    if (!this.product || !this.selectedVariantId) {
      return [];
    }

    // Check both variant.inventories and product.inventories with matching variantId
    let validStoreIds: number[] = [];

    // Method 1: Check variant inventories directly (if available)
    const selectedVariant = this.product.variants?.find(v => v.id === this.selectedVariantId);
    if (selectedVariant?.inventories && selectedVariant.inventories.length > 0) {
      validStoreIds = selectedVariant.inventories
        .filter((inv: any) => {
          const qty = Number(inv.quantity) || 0;
          return qty > 0;
        })
        .map((inv: any) => inv.storeId);
    }

    // Method 2: Check product inventories with matching variantId (fallback or supplement)
    if (this.product.inventories && this.product.inventories.length > 0) {
      const variantStoreIds = this.product.inventories
        .filter(inv => {
          const matchesVariant = inv.variantId === this.selectedVariantId;
          const qty = Number(inv.quantity) || 0;
          return matchesVariant && qty > 0;
        })
        .map(inv => inv.storeId);
      
      // Combine both sources and remove duplicates
      validStoreIds = [...new Set([...validStoreIds, ...variantStoreIds])];
    }

    console.log('Valid store IDs for variant (with stock > 0):', validStoreIds);

    // Return only stores that have the variant in inventory with quantity > 0
    return allStores.filter(store => validStoreIds.includes(store.id));
  }

  loadProductVariants(): void {
    this.productVariantService.getProductVariantsByProductId(this.productId).subscribe({
      next: (variants) => {
        this.productVariants = variants;
        // Set default variant if available
        const defaultVariant = variants.find(v => v.isDefault);
        if (defaultVariant) {
          this.selectedVariantId = defaultVariant.id;
          this.productForm.patchValue({ variantId: defaultVariant.id });
        }
      },
      error: (err) => {
        console.error('Error loading variants:', err);
      }
    });
  }

  loadProductModifiers(): void {
    this.productModifierService.getProductModifiersByProductId(this.productId).subscribe({
      next: (modifiers) => {
        this.productModifiers = modifiers;
        // Initialize selected modifiers
        modifiers.forEach(modifier => {
          this.selectedModifiers[modifier.modifierId] = [];
        });
      },
      error: (err) => {
        console.error('Error loading modifiers:', err);
      }
    });
  }

  onStoreChange(storeId: number | null): void {
    this.selectedStoreId = storeId;
    this.productForm.patchValue({ storeId });
  }

  onVariantChange(variantId: number | null): void {
    this.selectedVariantId = variantId;
    this.productForm.patchValue({ variantId });
    
    // Refresh store list based on variant selection
    this.refreshStoreList();
  }

  // Refresh store list based on current variant selection
  private refreshStoreList(): void {
    this.storeService.GetActiveStores().subscribe({
      next: (stores) => {
        // For purchases (orderTypeId === 1), show all stores so admin can choose
        // For sales (orderTypeId === 2), only show stores with inventory
        if (this.orderTypeId === 1) {
          this.allStores = stores;
        } else {
          // Filter stores to only show those that have the product/variant in inventory
          this.allStores = this.filterStoresWithProductInventory(stores);
        }
        
        // Clear store selection if current store is no longer valid (only for sales)
        if (this.orderTypeId === 2 && this.selectedStoreId && !this.allStores.find(s => s.id === this.selectedStoreId)) {
          this.selectedStoreId = null;
          this.productForm.patchValue({ storeId: null });
        }
      },
      error: (err) => {
        console.error('Error refreshing stores:', err);
      }
    });
  }

  onModifierChange(modifierId: number, optionId: number, isChecked: boolean): void {
    if (!this.selectedModifiers[modifierId]) {
      this.selectedModifiers[modifierId] = [];
    }

    if (isChecked) {
      this.selectedModifiers[modifierId].push(optionId);
    } else {
      const index = this.selectedModifiers[modifierId].indexOf(optionId);
      if (index > -1) {
        this.selectedModifiers[modifierId].splice(index, 1);
      }
    }
  }

  isModifierOptionSelected(modifierId: number, optionId: number): boolean {
    return this.selectedModifiers[modifierId]?.includes(optionId) || false;
  }

  onQuantityChange(quantity: number): void {
    if (quantity < 1) {
      this.quantity = 1;
    } else {
      this.quantity = quantity;
    }
    this.productForm.patchValue({ quantity: this.quantity });
  }

  async addToCart(): Promise<void> {
    if (!this.product || !this.selectedStoreId) {
      alert('يرجى اختيار المستودع أولاً');
      return;
    }

    if (this.cartService.hasActiveOrder(this.orderTypeId)) {
      const message = this.orderTypeId === 1 ? 'يرجى إنهاء طلب الشراء أولاً' : 'يرجى إنهاء طلب البيع أولاً';
      alert(message);
      return;
    }

    // Validate required modifiers
    for (const modifier of this.productModifiers) {
      if (modifier.isRequired && (!this.selectedModifiers[modifier.modifierId] || this.selectedModifiers[modifier.modifierId].length === 0)) {
        alert(`يرجى اختيار ${modifier.modifierName || modifier.modifier?.name}`);
        return;
      }
    }

    this.isAddingToCart = true;

    try {
      // Validate stock
      const isValid = await this.cartService.validateStockForCartItem(this.product.id, this.selectedStoreId, this.quantity);
      if (!isValid) {
        return;
      }

      // Add to cart with variant and modifier information
      this.cartService.addToCart(this.product, this.quantity, this.selectedStoreId, this.selectedVariantId || undefined, this.selectedModifiers);
      
      // Show success message
      alert('تم إضافة المنتج إلى السلة بنجاح');
      
      // Navigate back to products list
      this.router.navigate(['/order', this.orderTypeId, 'categories', this.product.subCategory?.categoryId, 'sub-categories', this.product.subCategoryId, 'products']);
      
    } catch (error) {
      console.error('Error adding to cart:', error);
      alert('حدث خطأ أثناء إضافة المنتج إلى السلة');
    } finally {
      this.isAddingToCart = false;
    }
  }

  setProductDetailBreadcrumbs(): void {
    if (this.product && this.categoryId && this.subCategoryId) {
      // Get category and subcategory names from the product
      const categoryName = this.product.subCategory?.category?.nameAr || 'التصنيف';
      const subCategoryName = this.product.subCategory?.nameAr || 'التصنيف الفرعي';
      const productName = this.product.nameAr || 'المنتج';
      
      this.orderBreadcrumbService.setProductDetailBreadcrumbs(
        this.orderTypeId,
        this.categoryId,
        categoryName,
        this.subCategoryId,
        subCategoryName,
        productName
      );
    }
  }


  goBack(): void {
    // Try to get category and subcategory from product data first
    if (this.product?.subCategory?.categoryId && this.product?.subCategoryId) {
      // Navigate back to order products using product data
      this.router.navigate(['/order', this.orderTypeId, 'categories', this.product.subCategory.categoryId, 'sub-categories', this.product.subCategoryId, 'products']);
    } else if (this.categoryId && this.subCategoryId) {
      // Fallback to route parameters
      this.router.navigate(['/order', this.orderTypeId, 'categories', this.categoryId, 'sub-categories', this.subCategoryId, 'products']);
    } else {
      // Final fallback to categories
      this.router.navigate(['/order', this.orderTypeId, 'categories']);
    }
  }

  goToProductForm(): void {
    if (this.product?.id) {
      // Navigate to admin product form to add variants
      this.router.navigate(['/admin/product-form'], { queryParams: { productId: this.product.id } });
    } else {
      console.error('Product ID not available');
    }
  }

  getProductPrice(): number {
    if (!this.product) return 0;
    
    let price = this.product.sellingPrice;
    
    // Add variant price adjustment
    if (this.selectedVariantId) {
      const variant = this.productVariants.find(v => v.id === this.selectedVariantId);
      if (variant?.priceAdjustment) {
        price += variant.priceAdjustment;
      }
    }
    
    // Add modifier price adjustments
    for (const modifierId in this.selectedModifiers) {
      const modifier = this.productModifiers.find(m => m.modifierId === Number(modifierId));
      if (modifier?.modifier?.options) {
        for (const optionId of this.selectedModifiers[modifierId]) {
          const option = modifier.modifier.options.find(o => o.id === optionId);
          if (option?.priceAdjustment) {
            price += option.priceAdjustment;
          }
        }
      }
    }
    
    return price;
  }

  // Get quantity for a specific store
  getStoreQuantity(storeId: number): number {
    if (!this.product?.inventories) return 0;
    
    const inventory = this.product.inventories.find(inv => inv.storeId === storeId);
    return inventory?.quantity || 0;
  }

  // Variant stock data for unified display
  variantStockData: { [key: string]: number } = {};

  // Load variant stock data when product is loaded
  loadVariantStockData(): void {
    console.log('Loading variant stock data...');
    console.log('Product:', this.product);
    console.log('Product variants:', this.product?.variants);
    console.log('Product variantStockData:', this.product?.variantStockData);
    
    if (this.product?.variants && this.product.variants.length > 0 && this.product.variantStockData) {
      // Process the variant stock data that's already included in the API response
      const variantStockData = this.product.variantStockData;
      console.log('Variant stock data:', variantStockData);
      
      if (variantStockData) {
        Object.keys(variantStockData).forEach(storeKey => {
          const stockData = variantStockData[storeKey] as any[];
          console.log(`Store ${storeKey} stock data:`, stockData);
          
          if (stockData) {
            const storeId = storeKey.replace('store_', '');
            
            stockData.forEach(variantStock => {
              const key = `${this.product!.id}-${variantStock.variantId}-${storeId}`;
              this.variantStockData[key] = variantStock.availableQuantity;
              console.log(`Set variant stock: ${key} = ${variantStock.availableQuantity}`);
            });
          }
        });
      }
    } else {
      console.log('No variant stock data available, checking for variant-specific inventory');
      // Check if inventories have variant information
      if (this.product?.inventories) {
        this.product.inventories.forEach(inventory => {
          console.log('Processing inventory:', inventory);
          
          // Check if this inventory record has a variant ID
          if (inventory.variantId) {
            const key = `${this.product!.id}-${inventory.variantId}-${inventory.storeId}`;
            this.variantStockData[key] = inventory.quantity;
            console.log(`Set variant stock from inventory: ${key} = ${inventory.quantity}`);
          } else {
            console.log('Inventory record has no variant ID, skipping');
          }
        });
      }
    }
    
    console.log('Final variant stock data:', this.variantStockData);
  }

  getVariantStock(productId: number, variantId: number, storeId: number): number {
    // This will be populated when we load variant stock data
    return this.variantStockData[`${productId}-${variantId}-${storeId}`] || 0;
  }

  // Enhanced cart management methods
  getStoresForVariant(variantId: number): StoreSimple[] {
    console.log('Getting stores for variant:', variantId);
    console.log('All stores:', this.allStores);
    console.log('Order Type ID:', this.orderTypeId);
    
    // For purchases (orderTypeId === 1), show all stores so admin can choose
    // For sales (orderTypeId === 2), only show stores with variant stock
    if (this.orderTypeId === 1) {
      return this.allStores;
    } else {
      // Filter stores that have this variant in stock
      const stores = this.allStores.filter(store => {
        const stock = this.getVariantStock(this.product!.id, variantId, store.id);
        console.log(`Variant ${variantId} stock in store ${store.id}:`, stock);
        return stock > 0;
      });
      
      console.log('Filtered stores for variant:', stores);
      
      // If no stores have stock, return empty array (don't show variant if no stock)
      if (stores.length === 0) {
        console.log('No stores with stock found for variant:', variantId);
        return [];
      }
      
      return stores;
    }
  }

  getVariantStockForStore(variantId: number, storeId: number): number {
    return this.getVariantStock(this.product!.id, variantId, storeId);
  }

  // Cart quantity management for main product (no variants)
  getCartQuantityForProductStore(productId: number, storeId: number): number {
    try {
      const quantity = this.cartService.getCartItemQuantity(productId, storeId);
      console.log(`Cart quantity for product ${productId}, store ${storeId}:`, quantity);
      return quantity;
    } catch (error) {
      console.error('Error getting cart quantity:', error);
      return 0;
    }
  }

  increaseQuantity(productId: number, storeId: number): void {
    console.log('Increase quantity called for product:', productId, 'store:', storeId);
    if (!this.product) {
      console.log('No product available');
      return;
    }
    
    const currentQuantity = this.getCartQuantityForProductStore(productId, storeId);
    const availableStock = this.getStoreQuantity(storeId);
    
    console.log('Current quantity:', currentQuantity, 'Available stock:', availableStock);
    
    // For purchase orders, no stock limit check
    if (this.orderTypeId === 1 || currentQuantity < availableStock) {
      console.log('Adding to cart...');
      this.cartService.addToCart(this.product, 1, storeId, undefined); // Explicitly pass undefined for products without variants
    } else {
      console.log('Cannot add more - stock limit reached');
    }
  }

  decreaseQuantity(productId: number, storeId: number): void {
    console.log('Decrease quantity called for product:', productId, 'store:', storeId);
    if (!this.product) {
      console.log('No product available');
      return;
    }
    
    const currentQuantity = this.getCartQuantityForProductStore(productId, storeId);
    console.log('Current quantity:', currentQuantity);
    
    if (currentQuantity > 0) {
      console.log('Removing from cart...');
      this.cartService.removeFromCartWithDetails(productId, storeId, 1);
    } else {
      console.log('Cannot decrease - quantity is already 0');
    }
  }

  // Cart quantity management for variants
  getCartQuantityForVariantStore(variantId: number, storeId: number): number {
    const quantity = this.cartService.getCartItemQuantity(this.product!.id, storeId, variantId);
    console.log(`Cart quantity for variant ${variantId}, store ${storeId}:`, quantity);
    return quantity;
  }

  increaseVariantQuantity(variantId: number, storeId: number): void {
    console.log('Increase variant quantity called for variant:', variantId, 'store:', storeId);
    if (!this.product) {
      console.log('No product available');
      return;
    }
    
    const currentQuantity = this.getCartQuantityForVariantStore(variantId, storeId);
    const availableStock = this.getVariantStockForStore(variantId, storeId);
    
    console.log('Current quantity:', currentQuantity, 'Available stock:', availableStock);
    
    // For purchase orders, no stock limit check
    if (this.orderTypeId === 1 || currentQuantity < availableStock) {
      console.log('Adding variant to cart...');
      this.cartService.addToCart(this.product, 1, storeId, variantId);
    } else {
      console.log('Cannot add more - stock limit reached');
    }
  }

  decreaseVariantQuantity(variantId: number, storeId: number): void {
    console.log('Decrease variant quantity called for variant:', variantId, 'store:', storeId);
    if (!this.product) {
      console.log('No product available');
      return;
    }
    
    const currentQuantity = this.getCartQuantityForVariantStore(variantId, storeId);
    console.log('Current quantity:', currentQuantity);
    
    if (currentQuantity > 0) {
      console.log('Removing variant from cart...');
      this.cartService.removeFromCartWithDetails(this.product.id, storeId, 1, variantId);
    } else {
      console.log('Cannot decrease - quantity is already 0');
    }
  }

  // Get total quantity in cart for this product
  getTotalCartQuantity(): number {
    if (!this.product) return 0;
    
    let total = 0;
    
    // Add quantities for main product (no variants)
    if (this.productVariants.length === 0) {
      this.allStores.forEach(store => {
        total += this.getCartQuantityForProductStore(this.product!.id, store.id);
      });
    } else {
      // Add quantities for all variants
      this.productVariants.forEach(variant => {
        this.getStoresForVariant(variant.id!).forEach(store => {
          total += this.getCartQuantityForVariantStore(variant.id!, store.id);
        });
      });
    }
    
    return total;
  }

  // Manual quantity setting methods
  setProductQuantity(productId: number, storeId: number, event: any): void {
    let newQuantity = parseInt(event.target.value) || 0;
    const maxQuantity = this.getStoreQuantity(storeId);
    
    console.log('Setting product quantity:', productId, storeId, newQuantity, 'max:', maxQuantity);
    
    // Validate quantity
    if (newQuantity < 0) {
      event.target.value = 0;
      return;
    }
    
    // Only check max quantity for sale orders (orderTypeId === 2)
    // For purchase orders (orderTypeId === 1), allow any quantity
    if (this.orderTypeId === 2 && newQuantity > maxQuantity) {
      event.target.value = maxQuantity;
      newQuantity = maxQuantity;
    }
    
    // Get current quantity
    const currentQuantity = this.getCartQuantityForProductStore(productId, storeId);
    
    if (newQuantity === 0) {
      // Remove from cart if quantity is 0
      if (currentQuantity > 0) {
        this.cartService.removeFromCartWithDetails(productId, storeId, currentQuantity);
      }
    } else if (newQuantity > currentQuantity) {
      // Add to cart
      const quantityToAdd = newQuantity - currentQuantity;
      this.cartService.addToCart(this.product!, quantityToAdd, storeId, undefined); // Explicitly pass undefined for products without variants
    } else if (newQuantity < currentQuantity) {
      // Remove from cart
      const quantityToRemove = currentQuantity - newQuantity;
      this.cartService.removeFromCartWithDetails(productId, storeId, quantityToRemove);
    }
  }

  setVariantQuantity(variantId: number, storeId: number, event: any): void {
    let newQuantity = parseInt(event.target.value) || 0;
    const maxQuantity = this.getVariantStockForStore(variantId, storeId);
    
    console.log('Setting variant quantity:', variantId, storeId, newQuantity, 'max:', maxQuantity);
    
    // Validate quantity
    if (newQuantity < 0) {
      event.target.value = 0;
      return;
    }
    
    // Only check max quantity for sale orders (orderTypeId === 2)
    // For purchase orders (orderTypeId === 1), allow any quantity
    if (this.orderTypeId === 2 && newQuantity > maxQuantity) {
      event.target.value = maxQuantity;
      newQuantity = maxQuantity;
    }
    
    // Get current quantity
    const currentQuantity = this.getCartQuantityForVariantStore(variantId, storeId);
    
    if (newQuantity === 0) {
      // Remove from cart if quantity is 0
      if (currentQuantity > 0) {
        this.cartService.removeFromCartWithDetails(this.product!.id, storeId, currentQuantity, variantId);
      }
    } else if (newQuantity > currentQuantity) {
      // Add to cart
      const quantityToAdd = newQuantity - currentQuantity;
      this.cartService.addToCart(this.product!, quantityToAdd, storeId, variantId);
    } else if (newQuantity < currentQuantity) {
      // Remove from cart
      const quantityToRemove = currentQuantity - newQuantity;
      this.cartService.removeFromCartWithDetails(this.product!.id, storeId, quantityToRemove, variantId);
    }
  }

  hasGeneralStock(): boolean {
    return this.product?.inventories?.some(inv => !inv.variantId && inv.quantity > 0) ?? false;
  }

  openSplitQuantityDialog(): void {
    if (!this.product || !this.productVariants.length) return;
    const generalInventory = this.product.inventories.find(inv => !inv.variantId && inv.quantity > 0);
    if (!generalInventory) return;
    this.dialog.open(SplitGeneralQuantityDialogComponent, {
      width: '450px',
      maxWidth: '95vw',
      data: {
        product: this.product,
        variants: this.productVariants,
        generalInventory: generalInventory
      },
      disableClose: false
    }).afterClosed().subscribe(result => {
      if (result) {
        // TODO: Call the inventoryService.splitGeneralToVariants(result)
        // and refresh inventories afterwards.
        this.loadProduct();
      }
    });
  }

}
