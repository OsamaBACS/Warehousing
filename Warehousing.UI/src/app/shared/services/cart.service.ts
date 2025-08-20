import { Injectable } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../../admin/models/product';
import { OrderDto } from '../../admin/models/OrderDto';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  constructor(private fb: FormBuilder) {
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
  addToCart(product: Product, quantity: number = 1): void {
    const itemsArray = this.cartItems;

    const index = itemsArray.controls.findIndex(
      ctrl => ctrl.value.productId === product.id
    );

    if (index > -1) {
      const currentQty = this.cartItems.at(index).get('quantity')?.value;
      if (currentQty <= 1 && quantity === -1) {
        this.removeFromCart(product.id);
      }
      else {
        this.cartItems.at(index).patchValue({ quantity: currentQty + quantity });
      }
    } else {

      const itemGroup =
        this.createCartItemGroup({
          productId: product.id,
          costPrice: product.costPrice || 0,
          sellingPrice: product.sellingPrice || 0,
          storeId: product.storeId || null,
          quantity: quantity
        })

      if (product.quantityInStock) {
        itemGroup.get('quantity')?.setValidators(Validators.max(product.quantityInStock))
        itemsArray.push(itemGroup);
      }
      else {
        if (this.orderTypeId == 1) {
          itemsArray.push(itemGroup);
        }
        else {
          alert('لا يوجد رصيد كافي لهذه المادة!');
          return;
        }
      }
    }

    this.calculateTotal();
    this.saveCartToLocalStorage();

    this.cartCount$.next(1);
  }

  // Calculate total
  calculateTotal(): void {
    const itemsArray = this.cartItems;
    const totalAmountControl = this.cartForm.get('totalAmount');

    const sum = itemsArray.controls.reduce((acc, control) => {
      const price = (this.orderTypeId == 1)
        ? +control.get('costPrice')?.value
        : +control.get('sellingPrice')?.value;
      const qty = +control.get('quantity')?.value;
      return acc + (price * qty || 0);
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
      costPrice: [item?.costPrice || null],
      sellingPrice: [item?.sellingPrice || null],
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
      }
    }
  }

  // Clear cart
  clearCart(): void {
    this.cartItems.clear();
    this.cartForm.reset({
      id: 0,
      orderDate: this.formatDate(new Date().toString()),
      totalAmount: 0,
      orderTypeId: this.orderTypeId,
      customerId: null,
      supplierId: null,
      statusId: null
    });

    localStorage.removeItem('cartForm');

    localStorage.removeItem('orderId');
    this.orderTypeId = 0;
    this.cartCount$.next(0);
  }

  hasActiveOrder(orderTypeId: number): boolean {
    if (this.orderTypeId == orderTypeId) {
      return false;
    }
    else {
      return this.cartItems.length > 0;
    }
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
