import { Injectable } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../../admin/models/product';
import { OrderDto } from '../../admin/models/OrderDto';
import { ProductsService } from '../../admin/services/products.service';
import { NotificationService } from '../../core/services/notification.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  constructor(
    private fb: FormBuilder,
    private productsService: ProductsService,
    private notification: NotificationService
  ) {
    this.loadCartFromLocalStorage();
  }

  orderTypeId: number = 0;
  public cartForm!: FormGroup;
  cartCount$ = new BehaviorSubject<number>(0);
  orderObject!: OrderDto;

  // Load both carts from localStorage
  private loadCartFromLocalStorage(): void {

    this.loadCart();
    this.calculateTotal();
  }

  private loadCart(): void {
    const savedcartForm = localStorage.getItem('cartForm');
    if (savedcartForm) {
      const parsed = JSON.parse(savedcartForm);
      this.cartForm = this.fb.group({
        id: [parsed.id || 0],
        orderDate: [parsed.orderDate || this.formatDate(new Date().toString())],
        totalAmount: [parsed.totalAmount || 0],
        orderTypeId: [this.orderTypeId],
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
    if (savedcartForm) {
      this.cartCount$.next(1);
    }
  }

  loadOrder(order: OrderDto): void {
    this.clearCart(); // make sure fresh start
    this.orderObject = order;
    console.log(this.orderObject)
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
    const cart = this.cartForm.value;
    localStorage.setItem('cartForm', JSON.stringify(cart));
  }

  // Add or update product in cart
  addToCart(product: Product, quantity: number = 1, storeId?: number): void {
    const itemsArray = this.cartItems;

    // Use the provided storeId, fallback to product.storeId, or null
    const targetStoreId = storeId || product.storeId || null;

    const index = itemsArray.controls.findIndex(
      ctrl => ctrl.value.productId === product.id && ctrl.value.storeId === targetStoreId
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
      // Validate stock before adding to cart
      this.validateStockBeforeAdd(product, targetStoreId, quantity).then(isValid => {
        if (isValid) {
          const itemGroup = this.createCartItemGroup({
            productId: product.id,
            unitCost: product.costPrice || 0,
            unitPrice: product.sellingPrice || 0,
            storeId: targetStoreId,
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
      console.error('Stock validation error:', error);
      this.notification.error('خطأ في التحقق من المخزون', 'خطأ');
      return false;
    }
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
      console.error('Stock validation error:', error);
      this.notification.error('خطأ في التحقق من المخزون', 'خطأ');
      return false;
    }
  }

  // Calculate total
  calculateTotal(): void {
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
      quantity: [item?.quantity || 0],
      unitCost: [item?.unitCost || item?.costPrice || null],
      unitPrice: [item?.unitPrice || item?.sellingPrice || null],
      discount: [item?.discount || 0],
      notes: [item?.notes || '']
    });
  }

  // Remove item
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
        this.orderTypeId = 0; // Reset orderTypeId when cart becomes empty
      }
    }
  }

  // Clear cart
  clearCart(): void {
    this.cartItems.clear();
    this.orderTypeId = 0; // Reset orderTypeId first
    this.cartForm.reset({
      id: 0,
      orderDate: this.formatDate(new Date().toString()),
      totalAmount: 0,
      orderTypeId: 0, // Use 0 instead of this.orderTypeId
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
    return this.cartForm.get('items') as FormArray;
  }

  private formatDate(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are zero-based
    const day = ('0' + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }
}
