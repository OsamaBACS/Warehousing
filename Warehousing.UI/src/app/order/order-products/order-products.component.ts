import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
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
import { StoreService } from '../../admin/services/store.service';
import { Store } from '../../admin/models/store';

@Component({
  selector: 'app-order-products',
  standalone: false,
  templateUrl: './order-products.component.html',
  styleUrl: './order-products.component.scss'
})
export class OrderProductsComponent implements OnInit {

  constructor(
    private route: ActivatedRoute,
    private productsService: ProductsService,
    public cartService: CartService,
    private fb: FormBuilder,
    private breadcrumbService: BreadcrumbService,
    private storeService: StoreService
  ) {
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
  }

  ngOnInit(): void {
    this.serverUrl = this.productsService.url.substring(0, this.productsService.url.indexOf('api'));
    
    // Load all stores for buying scenarios
    this.storeService.GetActiveStores().subscribe({
      next: (stores) => {
        this.allStores = stores;
      },
      error: (err) => {
        console.error('Error loading stores:', err);
      }
    });
    
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
  selectedStores: { [productId: number]: number | null } = {}; // Track selected store for each product
  allStores: Store[] = []; // All available stores for buying scenarios

  loadProducts(subCategoryId: number) {
    this.products$ = this.productsService.GetProductsBySubCategoryId(subCategoryId);
    // Set the selected subcategory for display
    this.selectedSubCategory = this.subCategories.find(sc => sc.id === subCategoryId)!;
  }

  initializingForm(order: OrderDto | null) {
    const formattedOrderDate = order?.orderDate ? this.formatDate(order.orderDate) : this.formatDate(new Date().toString());

    this.cartService.cartForm = this.fb.group({
      id: [order?.id || 0],
      orderDate: [formattedOrderDate],
      totalAmount: [order?.totalAmount || 0],
      orderTypeId: [this.orderTypeId],
      customerId: [order?.customerId || null],
      supplierId: [order?.supplierId || null],
      statusId: [order?.statusId || 1],
      items: this.fb.array([])
    });

    if (order?.id && order.id > 0) {
      order.items?.forEach((item: any) => {
        this.cartService.cartItems.push(this.createItemGroup(item));
      });
    }
  }

  createItemGroup(item?: OrderItemDto): FormGroup {
    const fg = this.fb.group({
      id: [item?.id || null],
      storeId: [item?.storeId || null],
      productId: [item?.productId || null],
      quantity: [item?.quantity || 0],
      unitCost: [item?.unitCost || 0],
      unitPrice: [item?.unitPrice || 0],
    });

    return fg;
  }


  removeFromCart(product: Product): void {
    if (this.cartService.hasActiveOrder(this.orderTypeId)) {
      var message = this.cartService.orderTypeId == 1 ? 'يرجى إنهاء طلب الشراء اولا' : 'يرجى إنهاء طلب البيع اولا'
      alert(message);
      return;
    }

    const currentQuantity = this.getQuantity(product.id);
    if (currentQuantity <= 0) {
      if (this.cartService.cartItems.length <= 0) {
        this.cartService.clearCart();
      }
      return;
    }

    // Get the selected store for this product
    const selectedStoreId = this.selectedStores[product.id];
    if (!selectedStoreId) {
      alert('يرجى اختيار المستودع أولاً');
      return;
    }

    // Only allow removal if quantity will remain >= 0
    if (currentQuantity - 1 >= 0) {
      this.cartService.addToCart(product, -1, selectedStoreId);
    }
  }

  getQuantity(productId: number): number {
    const selectedStoreId = this.selectedStores[productId];
    
    // If no store is selected, return 0
    if (!selectedStoreId) {
      return 0;
    }

    const index = this.cartService.cartItems.controls.findIndex(
      ctrl => ctrl.value.productId === productId && ctrl.value.storeId === selectedStoreId
    );

    return index > -1 ? this.cartService.cartItems.at(index).get('quantity')?.value || 0 : 0;
  }

  getCartItemIndexByProductId(productId: number): number | null {
    const selectedStoreId = this.selectedStores[productId];
    
    // If no store is selected, return null
    if (!selectedStoreId) {
      return null;
    }

    const index = this.cartService.cartItems.controls.findIndex(
      group => group.get('productId')?.value === productId && group.get('storeId')?.value === selectedStoreId
    );
    return index >= 0 ? index : null;
  }

  getQuantityFormControl(productId: number): FormControl | null {
    const index = this.getCartItemIndexByProductId(productId);
    if (index === null) return null;

    const control = this.cartService.cartItems.at(index).get('quantity');
    return control as FormControl; // ✅ Safe cast if you're sure it's a FormControl
  }

  // Handle quantity change in order-products component
  async onQuantityChangeInOrderProducts(productId: number, event: any): Promise<void> {
    const newQuantity = parseInt(event.target.value) || 0;
    
    if (newQuantity < 0) {
      event.target.value = 0;
      return;
    }

    const cartItem = this.cartService.cartItems?.controls.find(i => i.get('productId')?.value == productId);
    
    if (!cartItem) {
      // Item not in cart yet - check if store is selected and add to cart
      const selectedStoreId = this.selectedStores[productId];
      if (!selectedStoreId) {
        alert('يرجى اختيار المستودع أولاً');
        event.target.value = 0;
        return;
      }

      if (newQuantity > 0) {
        // Add item to cart with the specified quantity
        this.cartService.addToCart({ id: productId } as any, newQuantity, selectedStoreId);
      }
      return;
    }

    // Item is in cart - update existing quantity
    const storeId = cartItem.get('storeId')?.value;
    
    // Validate stock before updating quantity
    const isValid = await this.cartService.validateStockForCartItem(productId, storeId, newQuantity);
    if (!isValid) {
      // Clear the input field and revert to previous value if validation fails
      const currentQuantity = cartItem.get('quantity')?.value || 0;
      event.target.value = currentQuantity;
      cartItem.get('quantity')?.setValue(currentQuantity);
      return;
    }
    
    // Update the quantity if validation passes
    cartItem.get('quantity')?.setValue(newQuantity);
    
    // Force update the input value to ensure synchronization
    setTimeout(() => {
      event.target.value = newQuantity;
    }, 0);
  }

  // Calculate total quantity across all stores for a product
  getTotalQuantity(product: Product): number {
    if (!product.inventories || product.inventories.length === 0) {
      return 0;
    }
    return product.inventories.reduce((total, inv) => total + (inv.quantity || 0), 0);
  }

  // Get available stores for a product based on order type
  getAvailableStores(product: Product): any[] {
    if (this.orderTypeId === 1) {
      // Buying scenario: show all stores
      return this.allStores.map(store => ({
        storeId: store.id,
        store: store,
        quantity: 0 // For buying, quantity is 0 since we're purchasing
      }));
    } else {
      // Selling scenario: show only stores where product has inventory
      return product.inventories || [];
    }
  }

  // Check if store selection is required
  isStoreSelectionRequired(): boolean {
    return true; // Always require store selection for both buying and selling
  }

  // Get store selection label
  getStoreSelectionLabel(): string {
    return this.orderTypeId === 1 ? 'اختر المستودع الوجهة:' : 'اختر المستودع:';
  }

  // Get selected store ID for a product
  getSelectedStoreId(productId: number): number | null {
    return this.selectedStores[productId] ?? null;
  }

  // Handle store selection change
  onStoreChange(productId: number, storeId: string | null): void {
    const selectedStoreId = storeId ? Number(storeId) : null;
    const previousStoreId = this.selectedStores[productId];
    
    // Update the selected store
    this.selectedStores[productId] = selectedStoreId;
    
    // If there was a previous store selection and it's different from the new one
    if (previousStoreId && previousStoreId !== selectedStoreId) {
      // Check if there's a cart item for the previous store
      const previousCartItemIndex = this.cartService.cartItems.controls.findIndex(
        ctrl => ctrl.value.productId === productId && ctrl.value.storeId === previousStoreId
      );
      
      if (previousCartItemIndex !== -1) {
        // Remove the cart item from the previous store
        this.cartService.cartItems.removeAt(previousCartItemIndex);
      }
    }
    
    // Update cart item with selected store (if any)
    const cartItemIndex = this.getCartItemIndexByProductId(productId);
    if (cartItemIndex !== null) {
      this.cartService.cartItems.at(cartItemIndex).patchValue({ storeId: selectedStoreId });
    }
  }

  // Override addToCart to include store selection
  async addToCart(product: Product): Promise<void> {
    if (this.cartService.hasActiveOrder(this.orderTypeId)) {
      var message = this.cartService.orderTypeId == 1 ? 'يرجى إنهاء طلب الشراء اولا' : 'يرجى إنهاء طلب البيع اولا'
      alert(message);
      return;
    }

    // Check if store is selected
    const selectedStoreId = this.selectedStores[product.id];
    if (!selectedStoreId) {
      alert('يرجى اختيار المستودع أولاً');
      return;
    }

    // Calculate what the new quantity will be after adding 1
    const currentQuantity = this.getQuantity(product.id);
    const newQuantity = currentQuantity + 1;

    // Validate stock before adding to cart
    const isValid = await this.cartService.validateStockForCartItem(product.id, selectedStoreId, newQuantity);
    if (!isValid) {
      // Validation failed, don't add to cart
      return;
    }

    // Use the cart service's enhanced validation (which includes total quantity validation)
    this.cartService.addToCart(product, 1, selectedStoreId);
    
    // Trigger change detection to update the input field
    setTimeout(() => {
      // Force Angular to detect changes and update the input field
      const inputElement = document.querySelector(`input[data-product-id="${product.id}"]`) as HTMLInputElement;
      if (inputElement) {
        inputElement.value = this.getQuantity(product.id).toString();
      }
    }, 100);
  }

  private formatDate(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are zero-based
    const day = ('0' + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }

}