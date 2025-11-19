import { Injectable } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../../admin/models/product';
import { OrderDto } from '../../admin/models/OrderDto';
import { ProductsService } from '../../admin/services/products.service';
import { NotificationService } from '../../core/services/notification.service';
import { StoreService } from '../../admin/services/store.service';
import { Store } from '../../admin/models/store';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  constructor(
    private fb: FormBuilder,
    private productsService: ProductsService,
    private notification: NotificationService,
    private storeService: StoreService
  ) {
    this.loadCartFromLocalStorage();
    this.loadStores();
  }

  orderTypeId: number = 1; // Default to Purchase (1) instead of 0
  public cartForm!: FormGroup;
  cartCount$ = new BehaviorSubject<number>(0);
  orderObject!: OrderDto;
  stores: Store[] = [];

  // Ensure cartForm is always initialized
  private ensureCartFormInitialized(): void {
    if (!this.cartForm) {
      this.cartForm = this.createCartForm();
    }
    // Always ensure orderTypeId is in sync
    this.syncOrderTypeId();
  }

  // Sync orderTypeId between service and form
  private syncOrderTypeId(): void {
    if (this.cartForm && this.cartForm.get('orderTypeId')) {
      const formOrderTypeId = this.cartForm.get('orderTypeId')?.value;
      if (formOrderTypeId !== this.orderTypeId) {
        this.cartForm.patchValue({ orderTypeId: this.orderTypeId });
      }
    }
  }

  // Method to set orderTypeId and update form
  setOrderTypeId(orderTypeId: number): void {
    this.orderTypeId = orderTypeId;
    this.ensureCartFormInitialized();
    this.cartForm.patchValue({ orderTypeId: orderTypeId });
    this.saveCartToLocalStorage();
  }

  // Load stores from service
  private loadStores(): void {
    this.storeService.GetStores().subscribe({
      next: (stores) => {
        this.stores = stores;
      },
      error: (error) => {
      }
    });
  }

  // Load both carts from localStorage
  private loadCartFromLocalStorage(): void {

    this.loadCart();
    this.calculateTotal();
  }

  private loadCart(): void {
    const savedcartForm = localStorage.getItem('cartForm');
    
    if (savedcartForm) {
      const parsed = JSON.parse(savedcartForm);
      
      // Handle orderTypeId: if it's null/undefined/0, use default (1)
      // Only use saved value if it's a valid order type (1 or 2)
      const savedOrderTypeId = parsed.orderTypeId;
      if (savedOrderTypeId === 1 || savedOrderTypeId === 2) {
        this.orderTypeId = savedOrderTypeId;
      } else {
        this.orderTypeId = 1; // Default to Purchase
      }
      
      this.cartForm = this.fb.group({
        id: [parsed.id || 0],
        orderDate: [parsed.orderDate || this.formatDate(new Date().toString())],
        totalAmount: [parsed.totalAmount || 0],
        orderTypeId: [this.orderTypeId], // Use the validated orderTypeId
        customerId: [parsed?.customerId || null],
        supplierId: [parsed?.supplierId || null],
        statusId: [parsed.statusId || null],
        items: this.fb.array(
          (parsed.items || []).map((item: any) => this.createCartItemGroup(item))
        )
      });
    } else {
      this.cartForm = this.createCartForm();
    }
    
    // Only set cart count to 1 if there are actual items in the cart
    // Check if cart has items before setting count
    const hasItems = this.cartItems && this.cartItems.length > 0;
    if (hasItems) {
      this.cartCount$.next(1);
    } else {
      this.cartCount$.next(0);
      // If cart form exists but has no items, clear it
      if (savedcartForm) {
        localStorage.removeItem('cartForm');
        this.cartForm = this.createCartForm();
      }
    }
  }

  loadOrder(order: OrderDto): void {
    this.clearCart(); // make sure fresh start
    this.orderObject = order;
    this.orderTypeId = order.orderTypeId!;

    this.cartForm.patchValue({
      id: order.id,
      orderDate: order.orderDate,
      totalAmount: order.totalAmount,
      orderTypeId: order.orderTypeId,
      customerId: order.customerId,
      supplierId: order.supplierId,
      statusId: order.statusId
    });

    order.items?.forEach((item: any) => {
      const itemGroup = this.createCartItemGroup(item);
      this.cartItems.push(itemGroup);
    });

    this.calculateTotal();
    this.saveCartToLocalStorage();

    if(this.cartItems.length > 0)
      this.cartCount$.next(1);
  }


  updateItemInLocalStorage(index: number): void {
    const savedCart = localStorage.getItem('cartForm');

    if (savedCart) {
      const parsed = JSON.parse(savedCart);

      if (Array.isArray(parsed.items) && index >= 0 && index < parsed.items.length) {
        if (parsed.items.length === 1) {
          this.clearCart();
        }
        else {
          parsed.items.splice(index, 1);
          localStorage.setItem('cartForm', JSON.stringify(parsed)); // Save back
        }
      }

      if (Array.isArray(parsed.items) && parsed.items.length <= 0) {
        this.cartCount$.next(0);
      }
    }
  }

  // Save cart to localStorage
  saveCartToLocalStorage(): void {
    this.ensureCartFormInitialized();
    const cart = this.cartForm.value;
    
    // Ensure orderTypeId is always valid (1 or 2) before saving
    if (!cart.orderTypeId || (cart.orderTypeId !== 1 && cart.orderTypeId !== 2)) {
      cart.orderTypeId = this.orderTypeId; // Use service's orderTypeId
    }
    // Also ensure service's orderTypeId matches form's orderTypeId
    if (cart.orderTypeId && (cart.orderTypeId === 1 || cart.orderTypeId === 2)) {
      this.orderTypeId = cart.orderTypeId;
    }
    
    localStorage.setItem('cartForm', JSON.stringify(cart));
  }

  // Add or update product in cart
  addToCart(product: Product, quantity: number = 1, storeId?: number, variantId?: number, selectedModifiers?: { [modifierId: number]: number[] }): void {
    const itemsArray = this.cartItems;

    // Validate required fields
    if (!storeId) {
      this.notification.warning('يجب اختيار المستودع', 'تحذير');
      return;
    }

    // Check if product has variants and variant is required
    if (product.variants && product.variants.length > 0 && !variantId) {
      this.notification.warning('يجب اختيار متغير للمنتج', 'تحذير');
      return;
    }

    // Ensure orderTypeId is valid before proceeding
    this.ensureCartFormInitialized();

    const index = itemsArray.controls.findIndex(
      ctrl => ctrl.value.productId === product.id && 
               ctrl.value.storeId === storeId && 
               (ctrl.value.variantId === variantId || 
                (ctrl.value.variantId === null && variantId === undefined) ||
                (ctrl.value.variantId === undefined && variantId === null))
    );

    if (index > -1) {
      const currentQty = this.cartItems.at(index).get('quantity')?.value;
      const newQuantity = currentQty + quantity;
      
      if (newQuantity <= 0) {
        this.removeFromCart(product.id);
      }
      else {
        this.cartItems.at(index).patchValue({ quantity: newQuantity });
        this.calculateTotal();
        this.saveCartToLocalStorage();
      }
    } else {
      // Validate store has product inventory and variant if applicable
      this.validateStoreAndVariantInventory(product, storeId, quantity, variantId).then(isValid => {
        if (isValid) {
          const itemGroup = this.createCartItemGroup({
            productId: product.id,
            variantId: variantId || null, // Include variant ID
            selectedModifiers: selectedModifiers || {}, // Include modifiers
            unitCost: product.costPrice || 0,
            unitPrice: product.sellingPrice || 0,
            storeId: storeId,
            quantity: quantity
          });

          itemsArray.push(itemGroup);
          this.calculateTotal();
          this.saveCartToLocalStorage();
          this.cartCount$.next(1);
        }
      });
    }
  }

  // Validate stock before adding to cart
  private async validateStockBeforeAdd(product: Product, storeId: number | null, quantity: number): Promise<boolean> {
    if (this.orderTypeId === 1) { // Purchase order - no stock validation needed
      return true;
    }

    if (!storeId) {
      this.notification.warning('يجب اختيار المستودع', 'تحذير');
      return false;
    }

    try {
      // Calculate total quantity that will be in cart after adding this quantity
      const existingCartItem = this.getCartItemByProductId(product.id);
      const currentQuantityInCart = existingCartItem ? existingCartItem.get('quantity')?.value || 0 : 0;
      const totalQuantityAfterAdd = currentQuantityInCart + quantity;

      const result = await this.productsService.ValidateStock(product.id, storeId, totalQuantityAfterAdd).toPromise();
      
      if (result && result.isValid) {
        return true;
      } else {
        const message = result?.message || `لا يوجد رصيد كافي! المتاح: ${result?.availableQuantity || 0}, المطلوب: ${totalQuantityAfterAdd}`;
        this.notification.error(message, 'خطأ في المخزون');
        return false;
      }
    } catch (error) {
      this.notification.error('خطأ في التحقق من المخزون', 'خطأ');
      return false;
    }
  }

  // Validate store has product inventory and variant if applicable
  private async validateStoreAndVariantInventory(product: Product, storeId: number, quantity: number, variantId?: number): Promise<boolean> {
    if (this.orderTypeId === 1) { // Purchase order - no stock validation needed
      return true;
    }

    try {
      // First check if the store has the product in inventory
      const storeHasProduct = product.inventories?.some(inv => inv.storeId === storeId && inv.quantity > 0);
      
      if (!storeHasProduct) {
        this.notification.error(`المنتج غير متوفر في المستودع المحدد`, 'خطأ في المخزون');
        return false;
      }

      // If product has variants, check variant-specific inventory
      if (product.variants && product.variants.length > 0 && variantId) {
        const variant = product.variants.find(v => v.id === variantId);
        if (!variant) {
          this.notification.error(`المتغير المحدد غير موجود`, 'خطأ في المتغير');
          return false;
        }

        // Check if variant has inventory in the selected store
        const variantHasInventory = variant.inventories?.some((inv: any) => inv.storeId === storeId && inv.quantity > 0);
        
        if (!variantHasInventory) {
          this.notification.error(`المتغير غير متوفر في المستودع المحدد`, 'خطأ في المخزون');
          return false;
        }

        // Validate variant stock quantity
        const variantInventory = variant.inventories?.find((inv: any) => inv.storeId === storeId);
        if (variantInventory && variantInventory.quantity < quantity) {
          this.notification.error(`الكمية المطلوبة (${quantity}) تتجاوز المخزون المتاح (${variantInventory.quantity}) للمتغير`, 'خطأ في المخزون');
          return false;
        }
      } else if (product.variants && product.variants.length > 0 && !variantId) {
        // Product has variants but no variant selected
        this.notification.error(`يجب اختيار متغير للمنتج`, 'خطأ في الاختيار');
        return false;
      } else {
        // No variants - validate main product stock
        const productInventory = product.inventories?.find(inv => inv.storeId === storeId);
        if (productInventory && productInventory.quantity < quantity) {
          this.notification.error(`الكمية المطلوبة (${quantity}) تتجاوز المخزون المتاح (${productInventory.quantity})`, 'خطأ في المخزون');
          return false;
        }
      }

      return true;
    } catch (error) {
      this.notification.error('خطأ في التحقق من المخزون', 'خطأ');
      return false;
    }
  }

  // Get valid stores for a product (stores that have the product in inventory)
  getValidStoresForProduct(product: Product): Store[] {
    if (!product.inventories || product.inventories.length === 0) {
      return [];
    }

    // Get stores that have the product with quantity > 0
    const validStoreIds = product.inventories
      .filter(inv => inv.quantity > 0)
      .map(inv => inv.storeId);

    // Return stores that have the product in inventory
    return this.stores.filter(store => validStoreIds.includes(store.id));
  }

  // Get valid stores for a product variant
  getValidStoresForVariant(product: Product, variantId: number): Store[] {
    const variant = product.variants?.find(v => v.id === variantId);
    if (!variant || !variant.inventories || variant.inventories.length === 0) {
      return [];
    }

    // Get stores that have the variant with quantity > 0
    const validStoreIds = variant.inventories
      .filter((inv: any) => inv.quantity > 0)
      .map((inv: any) => inv.storeId);

    // Return stores that have the variant in inventory
    return this.stores.filter(store => validStoreIds.includes(store.id));
  }

  // Get cart item by product ID
  getCartItemByProductId(productId: number): FormGroup | null {
    return this.cartItems.controls.find(item => item.get('productId')?.value === productId) as FormGroup || null;
  }

  // Validate stock when quantity changes in cart
  async validateStockForCartItem(productId: number, storeId: number, newQuantity: number): Promise<boolean> {
    if (this.orderTypeId === 1) { // Purchase order - no stock validation needed
      return true;
    }

    if (!storeId) {
      this.notification.warning('يجب اختيار المستودع', 'تحذير');
      return false;
    }

    try {
      const result = await this.productsService.ValidateStock(productId, storeId, newQuantity).toPromise();
      
      if (result && result.isValid) {
        return true;
      } else {
        const message = result?.message || `لا يوجد رصيد كافي! المتاح: ${result?.availableQuantity || 0}, المطلوب: ${newQuantity}`;
        this.notification.error(message, 'خطأ في المخزون');
        return false;
      }
    } catch (error) {
      this.notification.error('خطأ في التحقق من المخزون', 'خطأ');
      return false;
    }
  }

  // Calculate total
  calculateTotal(): void {
    this.ensureCartFormInitialized(); // Ensure form is initialized and orderTypeId is synced
    const itemsArray = this.cartItems;
    const totalAmountControl = this.cartForm.get('totalAmount');

    const sum = itemsArray.controls.reduce((acc, control) => {
      const price = (this.orderTypeId == 1)
        ? +control.get('unitCost')?.value || +control.get('costPrice')?.value
        : +control.get('unitPrice')?.value || +control.get('sellingPrice')?.value;
      const qty = +control.get('quantity')?.value;
      const discount = +control.get('discount')?.value || 0;
      const itemTotal = (price * qty) - discount;
      return acc + (itemTotal || 0);
    }, 0);

    totalAmountControl?.setValue(sum);
  }

  private createCartForm(): FormGroup {
    const formattedDate = this.formatDate(new Date().toString());
    return this.fb.group({
      id: [0],
      orderDate: [formattedDate],
      totalAmount: [0],
      orderTypeId: [this.orderTypeId],
      customerId: [null],
      supplierId: [null],
      statusId: [null],
      items: this.fb.array([])
    });
  }

  private createCartItemGroup(item?: any): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      storeId: [item?.storeId || null],
      productId: [item?.productId || null],
      variantId: [item?.variantId || null], // Added variant support
      selectedModifiers: [item?.selectedModifiers || {}], // Added modifier support
      quantity: [item?.quantity || 0],
      unitCost: [item?.unitCost || item?.costPrice || null],
      unitPrice: [item?.unitPrice || item?.sellingPrice || null],
      discount: [item?.discount || 0],
      notes: [item?.notes || '']
    });
  }

  // Remove item (legacy method - kept for backward compatibility)
  removeFromCart(productId: number): void {
    const itemsArray = this.cartItems;
    const index = itemsArray.controls.findIndex(
      ctrl => ctrl.value.productId === productId
    );
    if (index > -1) {
      itemsArray.removeAt(index);
      this.calculateTotal();
      this.saveCartToLocalStorage();

      if (itemsArray.length <= 0) {
        this.cartCount$.next(0);
        // Don't reset orderTypeId to 0 - keep the current order type
        // this.orderTypeId = 0; // This was causing the foreign key constraint error
      }
    }
  }

  // Clear cart
  clearCart(): void {
    this.cartItems.clear();
    // Don't reset orderTypeId to 0 - keep the current order type
    // this.orderTypeId = 0; // This was causing the foreign key constraint error
    this.cartForm.reset({
      id: 0,
      orderDate: this.formatDate(new Date().toString()),
      totalAmount: 0,
      orderTypeId: this.orderTypeId, // Use current orderTypeId instead of 0
      customerId: null,
      supplierId: null,
      statusId: null
    });

    localStorage.removeItem('cartForm');
    localStorage.removeItem('orderId');
    this.cartCount$.next(0);
  }

  hasActiveOrder(orderTypeId: number): boolean {
    // If cart is empty, no active order
    if (this.cartItems.length === 0) {
      return false;
    }
    
    // If there are items in cart and orderTypeId doesn't match, there's an active order of different type
    if (this.orderTypeId != 0 && this.orderTypeId != orderTypeId) {
      return true; // There's an active order of a different type
    }
    
    // Same order type or no conflicting order type, allow adding items
    return false;
  }

  getQuantity(productId: number): number {
    const index = this.cartItems.controls.findIndex(
      ctrl => ctrl.value.productId === productId
    );

    return index > -1 ? this.cartItems.at(index).get('quantity')?.value || 0 : 0;
  }

  // Helper methods
  get cartItems(): FormArray {
    if (!this.cartForm) {
      // Initialize cartForm if it's not initialized yet
      this.cartForm = this.createCartForm();
    }
    const items = this.cartForm?.get('items') as FormArray;
    return items || this.fb.array([]); // Return empty FormArray if items is null/undefined
  }

  private formatDate(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are zero-based
    const day = ('0' + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }

  // Enhanced cart management methods for direct quantity control
  getCartItemQuantity(productId: number, storeId: number, variantId?: number): number {
    const itemsArray = this.cartItems;
    const item = itemsArray.controls.find(ctrl => {
      const value = ctrl.value;
      return value.productId === productId && 
             value.storeId === storeId && 
             (variantId === undefined ? !value.variantId : value.variantId === variantId);
    });
    
    return item ? item.value.quantity : 0;
  }

  removeFromCartWithDetails(productId: number, storeId: number, quantity: number = 1, variantId?: number): void {
    const itemsArray = this.cartItems;
    const index = itemsArray.controls.findIndex(ctrl => {
      const value = ctrl.value;
      return value.productId === productId && 
             value.storeId === storeId && 
             (variantId === undefined ? !value.variantId : value.variantId === variantId);
    });
    
    if (index > -1) {
      const item = itemsArray.at(index);
      const currentQuantity = item.value.quantity;
      
      if (currentQuantity <= quantity) {
        // Remove the entire item
        itemsArray.removeAt(index);
      } else {
        // Decrease quantity
        item.patchValue({ quantity: currentQuantity - quantity });
      }
      
      this.calculateTotal();
      this.saveCartToLocalStorage();
      
      if (itemsArray.length <= 0) {
        this.cartCount$.next(0);
        // Don't reset orderTypeId to 0 - keep the current order type
        // this.orderTypeId = 0; // This was causing the foreign key constraint error
      }
    }
  }
}
