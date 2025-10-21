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
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
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
    private modalService: BsModalService,
    private authService: AuthService,
    private printService: PrintService,
  ) {
    this.products = this.route.snapshot.data['productsResolver'];
    this.suppliers = this.route.snapshot.data['suppliersResolver'];
    this.customers = this.route.snapshot.data['customersRsolver'];
    this.stores = this.route.snapshot.data['StoresResolver'];
    this.statuses = this.route.snapshot.data['statusesResolver'];
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
    this.units = this.route.snapshot.data['unitsResolver'];

    this.title = cartService.orderTypeId == 1 ? 'سلة المشتريات' : 'سلة المبيعات';
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
  bsModalRef?: BsModalRef;
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
      this.cartService.addToCart({ id: productId } as any, -1, storeId);
    }
  }

  async onQuantityChange(productId: number, event: any): Promise<void> {
    const newQuantity = parseInt(event.target.value) || 0;
    
    const cartItem = this.cartService.cartItems?.controls.find(i => i.get('productId')?.value == productId);
    if (!cartItem) {
      return;
    }

    if (newQuantity < 0) {
      event.target.value = 0;
      cartItem.get('quantity')?.setValue(0);
      return;
    }

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

    const config = {
      initialState: {
        message: 'يرجى الاختيار',
        cancelBtn: 'إلغاء الطلب',
        confirmBtn: 'إفراغ السلة'
      }
    };

    this.bsModalRef = this.modalService.show(ConfirmModalComponent, config);
    this.bsModalRef.content?.onClose.subscribe((result: boolean) => {
      if (!result) {
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
      else {
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
    const config = {
      initialState: {
        message: 'هل انت متاكد من هذا الجراء؟',
        cancelBtn: 'لا',
        confirmBtn: 'نعم'
      }
    };

    this.bsModalRef = this.modalService.show(ConfirmModalComponent, config);
    this.bsModalRef.content?.onClose.subscribe((result: boolean) => {
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
        this.toastr.error(err.error, 'Purchase Order');
      }
    });
  }

  onEdit(orderId: number) {
    const config = {
      initialState: {
        message: 'هل انت متاكد من هذا الجراء؟',
        cancelBtn: 'لا',
        confirmBtn: 'نعم'
      }
    };

    this.bsModalRef = this.modalService.show(ConfirmModalComponent, config);
    this.bsModalRef.content?.onClose.subscribe((result: boolean) => {
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
        this.toastr.error(err.error, 'Order');
      }
    });
  }

  onCancelApprovedOrder(orderId: number) {
    const config = {
      initialState: {
        message: 'هل انت متاكد من هذا الجراء؟',
        cancelBtn: 'لا',
        confirmBtn: 'نعم'
      }
    };

    this.bsModalRef = this.modalService.show(ConfirmModalComponent, config);
    this.bsModalRef.content?.onClose.subscribe((result: boolean) => {
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
        this.toastr.error(err.error, 'Order');
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
}
