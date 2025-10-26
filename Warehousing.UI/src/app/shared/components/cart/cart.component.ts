import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CartService } from '../../services/cart.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Product } from '../../../admin/models/product';
import { environment } from '../../../../environments/environment';
import { LanguageService } from '../../../core/services/language.service';
import { FormGroup } from '@angular/forms';
import { Supplier } from '../../../admin/models/supplier';
import { Store } from '../../../admin/models/store';
import { Status } from '../../../admin/models/status';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmModalComponent } from '../confirm-modal.component/confirm-modal.component';
import { Customer } from '../../../admin/models/customer';
import { OrderService } from '../../../admin/services/order.service';
import { PermissionsEnum } from '../../../admin/constants/enums/permissions.enum';
import { AuthService } from '../../../core/services/auth.service';
import { SubCategory } from '../../../admin/models/SubCategory';
import { Unit } from '../../../admin/models/unit';
import { PrintService } from '../../services/print.service';
import { OrderItemDto } from '../../../admin/models/OrderItemDto';
import { Subject } from 'rxjs';
import { CustomerFormPopupComponent } from '../customer-form-popup/customer-form-popup.component';
import { SupplierFormPopupComponent } from '../supplier-form-popup/supplier-form-popup.component';
import { CustomersService } from '../../../admin/services/customers.service';
import { SupplierService } from '../../../admin/services/supplier.service';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit, OnDestroy {
  cartItems: OrderItemDto[] = [];
  orderTypeId: number = 1; // Default to Purchase

  constructor(
    public cartService: CartService,
    private orderService: OrderService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService,
    private dialog: MatDialog,
    private authService: AuthService,
    private printService: PrintService,
    private customerService: CustomersService,
    private supplierService: SupplierService,
  ) {
    this.products = this.route.snapshot.data['productsResolver'];
    this.suppliers = this.route.snapshot.data['suppliersResolver'];
    this.customers = this.route.snapshot.data['customersRsolver'];
    this.stores = this.route.snapshot.data['StoresResolver'];
    this.statuses = this.route.snapshot.data['statusesResolver'];
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
    this.units = this.route.snapshot.data['unitsResolver'];

    // Title is now handled by translation in the template
  }

  ngOnInit(): void {
    // Show loading indicator
    this.isLoading = true;
    
    // Simulate loading time to ensure all data is loaded
    setTimeout(() => {
      this.orderTypeId = this.cartService.orderTypeId;

      this.calculateAndSetTotalAmount();

      // Optional: Also recalculate if cart items change dynamically
      this.cartService.cartItems.valueChanges.subscribe(() => {
        this.calculateAndSetTotalAmount();
      });

      // Hide loading indicator
      this.isLoading = false;
    }, 500); // 500ms delay to show loading indicator
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private destroy$ = new Subject<void>();
  title: string = '';
  isLoading: boolean = true; // Loading indicator
  products: Product[] = [];
  suppliers: Supplier[] = [];
  customers: Customer[] = [];
  subCategories!: SubCategory[];
  units: Unit[] = [];
  stores: Store[] = [];
  statuses: Status[] = [];
  resourceUrl: string = environment.resourcesUrl;
  status: {
    nameAr: string | null,
    nameEn: string | null,
    statusColor: string | null,
    statusCode: string | null
  } = { nameAr: null, nameEn: null, statusColor: null, statusCode: null };
  // Removed BsModalRef - using Angular Material dialog
  permissionsEnum = PermissionsEnum;
  @ViewChild('printSection') printSection!: ElementRef;

  hasAnyPermission(...perms: string[]): boolean {
    return perms.some(p => this.authService.hasPermission(p));
  }

  getProductInfo(productId: number) {
    return this.products.find(p => p.id === productId);
  }

  getStatusInfo(statusId: number) {
    const status = this.statuses.find(s => s.id === statusId);
    return status;
  }

  getCartItemFormGroups(): FormGroup[] {
    return this.cartService.cartItems.controls as FormGroup[];
  }

  async increaseQuantity(productId: number): Promise<void> {
    const cartItem = this.cartService.cartItems?.controls.find(i => i.get('productId')?.value == productId);
    if (cartItem) {
      const currentQuantity = cartItem.get('quantity')?.value || 0;
      const storeId = cartItem.get('storeId')?.value;
      const newQuantity = currentQuantity + 1;
      
      // Validate stock before increasing
      const isValid = await this.cartService.validateStockForCartItem(productId, storeId, newQuantity);
      if (isValid) {
        this.cartService.addToCart({ id: productId } as any, 1, storeId);
      }
    }
  }

  decreaseQuantity(productId: number): void {
    const cartItem = this.cartService.cartItems?.controls.find(i => i.get('productId')?.value == productId);
    if (cartItem) {
      const storeId = cartItem.get('storeId')?.value;
      const variantId = cartItem.get('variantId')?.value;
      
      // Check if we can decrease (quantity > 0)
      const currentQuantity = cartItem.get('quantity')?.value || 0;
      if (currentQuantity > 0) {
        this.cartService.addToCart({ id: productId } as any, -1, storeId, variantId);
      }
    }
  }

  // Get maximum quantity available for a cart item
  getMaxQuantityForItem(item: FormGroup): number {
    const productId = item.get('productId')?.value;
    const storeId = item.get('storeId')?.value;
    const variantId = item.get('variantId')?.value;
    
    if (variantId) {
      // For variants, get variant stock
      return this.getVariantStockForItem(productId, variantId, storeId);
    } else {
      // For main products, get main product stock
      return this.getProductStockForItem(productId, storeId);
    }
  }

  // Get stock for main product
  getProductStockForItem(productId: number, storeId: number): number {
    // This would need to be implemented based on your stock data
    // For now, return a high number to allow any quantity
    // You should implement this to get actual stock from your inventory
    return 999999; // Placeholder - implement actual stock check
  }

  // Get stock for variant
  getVariantStockForItem(productId: number, variantId: number, storeId: number): number {
    // This would need to be implemented based on your variant stock data
    // For now, return a high number to allow any quantity
    // You should implement this to get actual variant stock from your inventory
    return 999999; // Placeholder - implement actual variant stock check
  }

  // Handle manual quantity change with validation
  onQuantityChange(item: FormGroup): void {
    const newQuantity = parseInt(item.get('quantity')?.value) || 0;
    const maxQuantity = this.getMaxQuantityForItem(item);
    
    console.log('Cart quantity change:', newQuantity, 'max:', maxQuantity);
    
    // Validate quantity
    if (newQuantity < 0) {
      item.get('quantity')?.setValue(0);
      return;
    }
    
    if (newQuantity > maxQuantity) {
      item.get('quantity')?.setValue(maxQuantity);
      return;
    }
    
    // Update the cart with the new quantity
    const productId = item.get('productId')?.value;
    const storeId = item.get('storeId')?.value;
    const variantId = item.get('variantId')?.value;
    const currentQuantity = this.cartService.getCartItemQuantity(productId, storeId, variantId);
    
    if (newQuantity === 0) {
      // Remove from cart if quantity is 0
      if (currentQuantity > 0) {
        this.cartService.removeFromCartWithDetails(productId, storeId, currentQuantity, variantId);
      }
    } else if (newQuantity !== currentQuantity) {
      // Update quantity
      const difference = newQuantity - currentQuantity;
      if (difference > 0) {
        this.cartService.addToCart({ id: productId } as any, difference, storeId, variantId);
      } else {
        this.cartService.removeFromCartWithDetails(productId, storeId, Math.abs(difference), variantId);
      }
    }
  }


  onUnitCostChange() {
    this.cartService.calculateTotal();
  }

  removeFromCart(productId: number): void {
    this.cartService.removeFromCart(productId);
  }

  onSave() {
    // Store the original statusId to restore it if save fails
    const originalStatusId = this.cartService.cartForm.get('statusId')?.value;
    
    this.cartService.cartForm.get('statusId')?.setValue(11); //DRAFT
    this.cartService.cartForm.get('orderTypeId')?.setValue(this.cartService.orderTypeId);
    this.orderService.SaveOrder(this.cartService.cartForm.value).subscribe({
      next: (response) => {
        if (response) {
          console.log(response);
          this.toastr.success('تم حفظ الطلب بنجاح');
          this.cartService.clearCart();
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        console.error('Error saving order:', err);
        // Restore the original statusId so the button remains visible
        this.cartService.cartForm.get('statusId')?.setValue(originalStatusId);
        this.toastr.error('فشل في حفظ الطلب. يرجى المحاولة مرة أخرى.');
        // Don't clear cart on error - keep the data so user can try again
      }
    });
  }

  onCancel() {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'يرجى الاختيار',
        cancelBtn: 'إلغاء الطلب',
        confirmBtn: 'إفراغ السلة'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result === false) {
        this.cartService.cartForm.get('statusId')?.setValue(6);
        this.orderService.SaveOrder(this.cartService.cartForm.value).subscribe({
          next: (response) => {
            if (response) {
              console.log(response);
              this.toastr.success('تم إلغاء الطلب بنجاح');
              this.cartService.clearCart();
              this.router.navigate(['/']);
            }
          },
          error: (err) => {
            console.error('Error saving order:', err);
            this.toastr.error('فشل في إلغاء الطلب. يرجى المحاولة مرة أخرى.');
            // Don't clear cart on error - keep the data so user can try again
          }
        });
      }
      else if (result === true) {
        //Just clear cart
        this.cartService.clearCart();
      }
    });
  }

  onSubmit(): void {
    // Store the original statusId to restore it if submit fails
    const originalStatusId = this.cartService.cartForm.get('statusId')?.value;
    
    this.cartService.cartForm.get('statusId')?.setValue(1);
    this.cartService.cartForm.get('orderTypeId')?.setValue(this.cartService.orderTypeId);
    this.orderService.SaveOrder(this.cartService.cartForm.value).subscribe({
      next: (response) => {
        if (response) {
          console.log(response);
          this.toastr.success('تم إرسال الطلب بنجاح');
          this.cartService.clearCart();
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        console.error('Error saving order:', err);
        // Restore the original statusId so the button remains visible
        this.cartService.cartForm.get('statusId')?.setValue(originalStatusId);
        this.toastr.error('فشل في إرسال الطلب. يرجى المحاولة مرة أخرى.');
        // Don't clear cart on error - keep the data so user can try again
      }
    });
  }

  onConfirm(orderId: number) {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'هل انت متاكد من هذا الجراء؟',
        cancelBtn: 'لا',
        confirmBtn: 'نعم'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        console.log(result)
        if (orderId) {
          this.changeOrderStatus(+orderId);
        }
      }
    });
  }

  changeOrderStatus(orderId: number) {
    this.orderService.UpdateOrderStatusToComplete(orderId).subscribe({
      next: (res) => {
        if (res.success) {
          this.cartService.clearCart();
          this.toastr.success('تم الإعتماد');
          // this.router.navigate(['/home']);
          this.printOrder();
        } else {
          this.toastr.warning(res.message, 'Purchase Order');
        }
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err.error?.message || err.error || 'حدث خطأ أثناء إتمام الطلب';
        this.toastr.error(errorMessage, 'خطأ في الطلب');
      }
    });
  }

  onEdit(orderId: number) {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'هل انت متاكد من هذا الجراء؟',
        cancelBtn: 'لا',
        confirmBtn: 'نعم'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        console.log(result)
        if (orderId) {
          this.EditApprovedOrder(+orderId);
        }
      }
    });
  }

  EditApprovedOrder(orderId: number) {
    this.orderService.UpdateApprovedOrder(orderId, this.cartService.cartForm.value).subscribe({
      next: (res) => {
        if (res.success) {
          this.cartService.clearCart();
          this.toastr.success('تم التعديل');
          this.printOrder();
        } else {
          var errors = res.insufficientItems.join('\n');
          this.toastr.warning(errors, res.message);
        }
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err.error?.message || err.error || 'حدث خطأ أثناء تعديل الطلب';
        this.toastr.error(errorMessage, 'خطأ في التعديل');
      }
    });
  }

  onCancelApprovedOrder(orderId: number) {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'هل انت متاكد من هذا الجراء؟',
        cancelBtn: 'لا',
        confirmBtn: 'نعم'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        console.log(result)
        if (orderId) {
          this.CancelApprovedOrder(+orderId);
        }
      }
    });
  }

  CancelApprovedOrder(orderId: number) {
    this.orderService.CancelApprovedOrder(orderId).subscribe({
      next: (res) => {
        if (res.success) {
          this.cartService.clearCart();
          this.toastr.success('تم الإلغاء');
          this.printOrder();
        } else {
          var errors = res.insufficientItems.join('\n');
          this.toastr.warning(errors, res.message);
        }
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err.error?.message || err.error || 'حدث خطأ أثناء إلغاء الطلب';
        this.toastr.error(errorMessage, 'خطأ في الإلغاء');
      }
    });
  }

  onDelete(index: number) {
    const array = this.cartService.cartItems;
    array.removeAt(index);
    this.cartService.updateItemInLocalStorage(index);
  }

  getCustomerName(id: number): string {
    const customer = this.customers.find(c => c.id === id);
    return customer ? (this.lang.currentLang === 'ar' ? customer.nameAr : customer.nameEn) : '';
  }

  getSupplierName(id: number): string {
    const supplier = this.suppliers.find(c => c.id === id);
    return supplier ? (supplier.name) : '';
  }

  getCategoryName(id: number): string {
    const cat = this.subCategories.find(c => c.id === id);
    return cat ? (this.lang.currentLang === 'ar' ? cat.nameAr! : cat.nameEn!) : '';
  }

  getProductDescription(id: number): string {
    const product = this.products.find(p => p.id === id);
    return product ? (product.description!) : '';
  }

  getProductName(id: number): string {
    const product = this.products.find(p => p.id === id);
    return product ? (this.lang.currentLang === 'ar' ? product.nameAr : product.nameEn!) : '';
  }

  getUnitName(id: number): string {
    const unit = this.units.find(p => p.id === id);
    return unit ? (this.lang.currentLang === 'ar' ? unit.nameAr : unit.nameEn) : '';
  }

  getVariantName(productId: number, variantId: number): string {
    const product = this.products.find(p => p.id === productId);
    if (product && product.variants) {
      const variant = product.variants.find(v => v.id === variantId);
      return variant ? variant.name : '';
    }
    return '';
  }

  getStoreName(storeId: number): string {
    const store = this.stores.find(s => s.id === storeId);
    return store ? store.nameAr : '';
  }

  getModifierName(productId: number, modifierId: number): string {
    const product = this.products.find(p => p.id === productId);
    if (product && product.modifierGroups) {
      const modifierGroup = product.modifierGroups.find((mg: any) => mg.modifierId === modifierId);
      return modifierGroup ? (modifierGroup.modifierName || modifierGroup.modifier?.name || '') : '';
    }
    return '';
  }

  getModifierOptionName(productId: number, modifierId: number, optionId: number): string {
    const product = this.products.find(p => p.id === productId);
    if (product && product.modifierGroups) {
      const modifierGroup = product.modifierGroups.find((mg: any) => mg.modifierId === modifierId);
      if (modifierGroup && modifierGroup.modifier?.options) {
        const option = modifierGroup.modifier.options.find((o: any) => o.id === optionId);
        return option ? option.name : '';
      }
    }
    return '';
  }

  getSelectedModifiers(productId: number, selectedModifiers: { [modifierId: number]: number[] }): string[] {
    const modifierNames: string[] = [];
    
    Object.keys(selectedModifiers).forEach(modifierIdStr => {
      const modifierId = Number(modifierIdStr);
      const optionIds = selectedModifiers[modifierId];
      
      if (optionIds && optionIds.length > 0) {
        const modifierName = this.getModifierName(productId, modifierId);
        const optionNames = optionIds.map(optionId => 
          this.getModifierOptionName(productId, modifierId, optionId)
        ).filter(name => name);
        
        if (modifierName && optionNames.length > 0) {
          modifierNames.push(`${modifierName}: ${optionNames.join(', ')}`);
        }
      }
    });
    
    return modifierNames;
  }

  calculateAndSetTotalAmount() {
    const items = this.cartService.cartItems.controls as FormGroup[];
    let total = 0;

    for (const item of items) {
      const quantity = item.get('quantity')?.value || 0;
      const price = this.cartService.orderTypeId === 1
        ? item.get('unitCost')?.value || 0
        : item.get('unitPrice')?.value || 0;

      total += quantity * price;
    }

    // Update the form control
    this.cartService.cartForm.patchValue({
      totalAmount: total
    });
  }

  printOrder() {
    if (this.printSection) {
      this.printService.printHtml(this.printSection.nativeElement.innerHTML);
    } else {
      console.error("Print section not found");
    }
  }

  openCustomerForm(): void {
    const dialogRef = this.dialog.open(CustomerFormPopupComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      disableClose: true,
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Add the new customer to the customers list
        this.customers.push(result);
        // Set the new customer as selected
        this.cartService.cartForm.patchValue({
          customerId: result.id
        });
        this.toastr.success('تم إضافة العميل الجديد واختياره', 'نجح');
      }
    });
  }

  openSupplierForm(): void {
    const dialogRef = this.dialog.open(SupplierFormPopupComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      disableClose: true,
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Add the new supplier to the suppliers list
        this.suppliers.push(result);
        // Set the new supplier as selected
        this.cartService.cartForm.patchValue({
          supplierId: result.id
        });
        this.toastr.success('تم إضافة المورد الجديد واختياره', 'نجح');
      }
    });
  }
}
