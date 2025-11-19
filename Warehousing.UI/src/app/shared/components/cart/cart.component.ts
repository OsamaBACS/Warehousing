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
import { PdfPrintService, OrderPrintRequest, OrderItemDto as PdfOrderItemDto } from '../../services/pdf-print.service';
import { OrderItemDto } from '../../../admin/models/OrderItemDto';
import { Subject } from 'rxjs';
import { CustomerFormPopupComponent } from '../customer-form-popup/customer-form-popup.component';
import { SupplierFormPopupComponent } from '../supplier-form-popup/supplier-form-popup.component';
import { CustomersService } from '../../../admin/services/customers.service';
import { SupplierService } from '../../../admin/services/supplier.service';
import { CompaniesService } from '../../../admin/services/companies.service';
import { Company } from '../../../admin/models/Company';
import { UsersService } from '../../../admin/services/users.service';
import { User } from '../../../admin/models/users';

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
    private pdfPrintService: PdfPrintService,
    private customerService: CustomersService,
    private supplierService: SupplierService,
    private companiesService: CompaniesService,
    private usersService: UsersService,
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
    
    // Load company data from API
    this.companiesService.GetCompanies().subscribe({
      next: (company) => {
        this.company = company;
        // Store in localStorage for quick access (optional, but helps if needed elsewhere)
        if (company) {
          localStorage.setItem('companyInfo', JSON.stringify(company));
        }
      },
      error: (err) => {
        // Try to get from localStorage as fallback
        const cached = localStorage.getItem('companyInfo');
        if (cached) {
          try {
            this.company = JSON.parse(cached);
          } catch (e) {
          }
        }
      }
    });

    // Load current user data from API (using userId from auth service)
    const userId = this.authService.userId;
    if (userId) {
      this.usersService.GetUserById(+userId).subscribe({
        next: (user) => {
          this.currentUser = user;
          // Store in localStorage for quick access
          if (user) {
            localStorage.setItem('userInfo', JSON.stringify(user));
          }
        },
        error: (err) => {
          // Try to get from localStorage as fallback
          const cached = localStorage.getItem('userInfo');
          if (cached) {
            try {
              this.currentUser = JSON.parse(cached);
            } catch (e) {
            }
          }
        }
      });
    }
    
    // Simulate loading time to ensure all data is loaded
    setTimeout(() => {
      // Get orderTypeId from cart service, but ensure it's valid (1 or 2)
      // If it's 0 or invalid, default to 1 (Purchase)
      const serviceOrderTypeId = this.cartService.orderTypeId;
      this.orderTypeId = (serviceOrderTypeId === 1 || serviceOrderTypeId === 2) ? serviceOrderTypeId : 1;
      
      // Also ensure cart service has valid orderTypeId
      if (this.cartService.orderTypeId !== 1 && this.cartService.orderTypeId !== 2) {
        this.cartService.setOrderTypeId(this.orderTypeId);
      }

      // Load cart items from the service
      if (this.cartService.cartForm && this.cartService.cartItems) {
        this.cartItems = this.cartService.cartItems.controls as unknown as OrderItemDto[];
      }

      this.calculateAndSetTotalAmount();

      // Optional: Also recalculate if cart items change dynamically
      this.cartService.cartItems.valueChanges.subscribe(() => {
        this.calculateAndSetTotalAmount();
        // Update cart items array when form changes
        this.cartItems = this.cartService.cartItems.controls as unknown as OrderItemDto[];
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
  company: Company | null = null; // Store company data
  currentUser: User | null = null; // Store current user data
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
      const variantId = cartItem.get('variantId')?.value;
      const newQuantity = currentQuantity + 1;
      
      // Check max quantity limit
      const maxQuantity = variantId 
        ? this.getVariantStockForItem(productId, variantId, storeId)
        : this.getProductStockForItem(productId, storeId);
      
      if (newQuantity > maxQuantity) {
        this.toastr.warning(`الكمية القصوى المتاحة: ${maxQuantity}`, 'تحذير');
        return;
      }
      
      // Validate stock before increasing (only for sale orders)
      if (this.cartService.orderTypeId === 2) {
        const isValid = await this.cartService.validateStockForCartItem(productId, storeId, newQuantity);
        if (!isValid) {
          return;
        }
      }
      
      this.cartService.addToCart({ id: productId } as any, 1, storeId, variantId || undefined);
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
    // For purchase orders, allow unlimited quantity
    if (this.cartService.orderTypeId === 1) {
      return 999999;
    }

    // For sale orders, try to get stock from products array first
    const product = this.products.find(p => p.id === productId);
    
    if (product && product.inventories && product.inventories.length > 0) {
      const inventory = product.inventories.find(inv => inv.storeId === storeId);
      const quantity = inventory?.quantity || 0;
      return quantity;
    }
    
    // If not found in products array, return 0
    // In a real implementation, you might want to make an API call here
    return 0;
  }

  // Get stock for variant
  getVariantStockForItem(productId: number, variantId: number, storeId: number): number {
    // For purchase orders, allow unlimited quantity
    if (this.cartService.orderTypeId === 1) {
      return 999999;
    }

    // For sale orders, try to get variant stock from products array first
    const product = this.products.find(p => p.id === productId);
    
    if (product && product.variants && product.variants.length > 0) {
      const variant = product.variants.find(v => v.id === variantId);
      
      if (variant && variant.inventories && variant.inventories.length > 0) {
        const inventory = variant.inventories.find(inv => inv.storeId === storeId);
        const quantity = inventory?.quantity || 0;
        return quantity;
      }
    }
    
    // If not found in products array, return 0
    return 0;
  }

  // Handle real-time input validation
  onQuantityInput(item: FormGroup, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = parseInt(input.value) || 0;
    const maxQuantity = this.getMaxQuantityForItem(item);
    
    // Prevent negative values
    if (value < 0) {
      input.value = '0';
      item.get('quantity')?.setValue(0, { emitEvent: false });
      return;
    }
    
    // Cap at max quantity
    if (value > maxQuantity) {
      input.value = maxQuantity.toString();
      item.get('quantity')?.setValue(maxQuantity, { emitEvent: false });
    }
  }

  // Handle blur event to validate and correct input
  onQuantityBlur(item: FormGroup): void {
    const currentValue = parseInt(item.get('quantity')?.value) || 0;
    const maxQuantity = this.getMaxQuantityForItem(item);
    
    if (currentValue < 0) {
      item.get('quantity')?.setValue(0);
    } else if (currentValue > maxQuantity) {
      item.get('quantity')?.setValue(maxQuantity);
      this.toastr.warning(`الكمية القصوى المتاحة: ${maxQuantity}`, 'تحذير');
    }
  }

  // Handle manual quantity change with validation
  async onQuantityChange(item: FormGroup): Promise<void> {
    const newQuantity = parseInt(item.get('quantity')?.value) || 0;
    const maxQuantity = this.getMaxQuantityForItem(item);
    
    // Validate quantity
    if (newQuantity < 0) {
      item.get('quantity')?.setValue(0);
      return;
    }
    
    if (newQuantity > maxQuantity) {
      item.get('quantity')?.setValue(maxQuantity);
      this.toastr.warning(`الكمية القصوى المتاحة: ${maxQuantity}`, 'تحذير');
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
      // Validate stock before updating quantity (only for sale orders)
      const difference = newQuantity - currentQuantity;
      if (difference > 0) {
        // For sale orders, validate stock before adding
        if (this.cartService.orderTypeId === 2) {
          const isValid = await this.cartService.validateStockForCartItem(productId, storeId, newQuantity);
          if (!isValid) {
            // Reset to previous quantity if validation fails
            item.get('quantity')?.setValue(currentQuantity);
            return;
          }
        }
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
          this.toastr.success('تم حفظ الطلب بنجاح');
          this.cartService.clearCart();
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        // Restore the original statusId so the button remains visible
        this.cartService.cartForm.get('statusId')?.setValue(originalStatusId);
        const errorMessage = this.extractErrorMessage(err);
        this.toastr.error(errorMessage);
        // Don't clear cart on error - keep the data so user can try again
      }
    });
  }

  clearCartWithConfirm() {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'هل أنت متأكد من إفراغ السلة؟ سيتم حذف جميع العناصر من السلة.',
        cancelBtn: 'إلغاء',
        confirmBtn: 'إفراغ السلة'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.cartService.clearCart();
        this.toastr.success('تم إفراغ السلة بنجاح');
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
              this.toastr.success('تم إلغاء الطلب بنجاح');
              this.cartService.clearCart();
              this.router.navigate(['/']);
            }
          },
          error: (err) => {
            const errorMessage = this.extractErrorMessage(err);
            this.toastr.error(errorMessage);
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
          this.toastr.success('تم إرسال الطلب بنجاح');
          this.cartService.clearCart();
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        // Restore the original statusId so the button remains visible
        this.cartService.cartForm.get('statusId')?.setValue(originalStatusId);
        const errorMessage = this.extractErrorMessage(err);
        this.toastr.error(errorMessage);
        // Don't clear cart on error - keep the data so user can try again
      }
    });
  }

  onConfirm(orderId: number) {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'هل أنت متأكد من اعتماد هذا الطلب؟ سيتم تغيير حالة الطلب إلى معتمد.',
        cancelBtn: 'إلغاء',
        confirmBtn: 'اعتماد الطلب'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
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
        const errorMessage = this.extractErrorMessage(err);
        this.toastr.error(errorMessage, 'خطأ في الطلب');
      }
    });
  }

  onEdit(orderId: number) {
    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      data: {
        message: 'هل أنت متأكد من إلغاء هذا الطلب؟ سيتم تغيير حالة الطلب إلى ملغي.',
        cancelBtn: 'إلغاء',
        confirmBtn: 'تأكيد الإلغاء'
      }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
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
        const errorMessage = this.extractErrorMessage(err);
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
        const errorMessage = this.extractErrorMessage(err);
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

  getProductDescriptionById(id: number): string {
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
      const documentTitle = this.cartService.orderTypeId === 1 ? 
        'وثيقة الشراء' : 'وثيقة البيع';
      this.printService.printHtml(this.printSection.nativeElement.innerHTML, documentTitle);
    } else {
    }
  }

  // Enhanced PDF printing methods
  async printOrderPDF() {
    try {
      if (!this.printSection) {
        return;
      }

      const documentTitle = this.cartService.orderTypeId === 1 ? 
        'وثيقة الشراء' : 'وثيقة البيع';
      
      // Check if PDF service is available
      const isAvailable = await this.pdfPrintService.isServiceAvailable();
      
      if (isAvailable) {
        // Use PDF service for better quality
        await this.pdfPrintService.printPDF(
          this.printSection.nativeElement.innerHTML, 
          documentTitle, 
          'order'
        );
      } else {
        // Fallback to regular print service
        this.printOrder();
      }
    } catch (error) {
      // Fallback to regular print service
      this.printOrder();
    }
  }

  async downloadOrderPDF() {
    try {
      if (!this.printSection) {
        return;
      }

      const documentTitle = this.cartService.orderTypeId === 1 ? 
        'وثيقة الشراء' : 'وثيقة البيع';
      
      const isAvailable = await this.pdfPrintService.isServiceAvailable();
      
      if (isAvailable) {
        await this.pdfPrintService.downloadPDF(
          this.printSection.nativeElement.innerHTML, 
          documentTitle, 
          'order'
        );
      } else {
        this.toastr.warning('خدمة PDF غير متاحة حالياً', 'تحذير');
      }
    } catch (error) {
      this.toastr.error('خطأ في تحميل PDF', 'خطأ');
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

  // Company Information Methods
  private getCompanyInfo(): Company | null {
    // Return cached company data if available
    if (this.company) {
      return this.company;
    }
    
    // Fallback to localStorage if API data not loaded yet
    const companyInfo = localStorage.getItem('companyInfo');
    if (companyInfo) {
      try {
        return JSON.parse(companyInfo);
      } catch (e) {
      }
    }
    
    return null;
  }

  getCompanyName(): string {
    const company = this.getCompanyInfo();
    return company?.nameAr || company?.nameEn || '';
  }

  getCompanyAddress(): string {
    const company = this.getCompanyInfo();
    return company?.addressAr || company?.addressEn || '';
  }

  getCompanyPhone(): string {
    const company = this.getCompanyInfo();
    return company?.phone || '';
  }

  getCompanyFax(): string {
    const company = this.getCompanyInfo();
    return company?.fax || '';
  }

  getCompanyEmail(): string {
    const company = this.getCompanyInfo();
    return company?.email || '';
  }

  getCompanyRegistrationNumber(): string {
    const company = this.getCompanyInfo();
    return company?.registrationNumber || '';
  }

  getCompanyCapital(): string {
    const company = this.getCompanyInfo();
    if (company?.capital != null && company.capital > 0) {
      return `${company.capital} ${company.currencyCode || 'د.أ'}`;
    }
    return '';
  }

  getCompanyTaxNumber(): string {
    const company = this.getCompanyInfo();
    return company?.taxNumber || '';
  }

  getCompanySlogan(): string {
    const company = this.getCompanyInfo();
    return company?.sloganAr || company?.sloganEn || '';
  }

  getCompanyLogoUrl(): string {
    const company = this.getCompanyInfo();
    const logoUrl = company?.logoUrl || '';
    if (logoUrl && !logoUrl.startsWith('http')) {
      return environment.resourcesUrl + logoUrl;
    }
    return logoUrl || '';
  }

  getCompanyTerms(): string {
    const company = this.getCompanyInfo();
    return company?.termsAr || company?.termsEn || '';
  }

  // Sales Order Code Generator
  getSalesOrderCode(): string {
    const orderId = this.cartService.cartForm?.get('id')?.value;
    const year = new Date().getFullYear();
    if (orderId) {
      return `SO-${year}-${orderId}`;
    }
    return `SO-${year}-NEW`;
  }

  // Get Sales Representative Info (from current user or order creator)
  private getCurrentUserInfo(): User | null {
    // Return cached user data if available
    if (this.currentUser) {
      return this.currentUser;
    }
    
    // Fallback to localStorage if API data not loaded yet
    const userInfo = localStorage.getItem('userInfo');
    if (userInfo) {
      try {
        return JSON.parse(userInfo);
      } catch (e) {
      }
    }
    
    // Final fallback: construct from auth service data
    if (this.authService.nameAr || this.authService.nameEn) {
      return {
        id: +this.authService.userId,
        nameAr: this.authService.nameAr,
        nameEn: this.authService.nameEn,
        phone: '',
        email: '',
        username: this.authService.username
      } as User;
    }
    
    return null;
  }

  getSalesRepName(): string {
    const user = this.getCurrentUserInfo();
    return user?.nameAr || user?.nameEn || '';
  }

  getSalesRepPhone(): string {
    const user = this.getCurrentUserInfo();
    return user?.phone || '';
  }

  // Get Store/Inventory name for print
  getStoreNameForPrint(storeId: number): string {
    const store = this.stores?.find(s => s.id === storeId);
    return store?.nameAr || store?.nameEn || 'غير محدد';
  }

  // Get first store name from cart items (for print header)
  getPrimaryStoreName(): string {
    if (this.cartService.cartItems && this.cartService.cartItems.length > 0) {
      const firstItem = this.cartService.cartItems.at(0);
      const storeId = firstItem?.get('storeId')?.value;
      if (storeId) {
        return this.getStoreNameForPrint(storeId);
      }
    }
    return 'غير محدد';
  }

  // Calculate totals for print
  getTotalBeforeDiscount(): number {
    return this.cartService.cartForm?.get('totalAmount')?.value || 0;
  }

  getTotalDiscount(): number {
    // Sum all item discounts
    if (this.cartService.cartItems) {
      let totalDiscount = 0;
      for (let i = 0; i < this.cartService.cartItems.length; i++) {
        const item = this.cartService.cartItems.at(i);
        const discount = item?.get('discount')?.value || 0;
        totalDiscount += discount;
      }
      return totalDiscount;
    }
    return 0;
  }

  getNetTotal(): number {
    return this.getTotalBeforeDiscount() - this.getTotalDiscount();
  }

  // Get product description/code for print table
  getProductDescription(productId: number): string {
    const product = this.products?.find(p => p.id === productId);
    return product?.code || '';
  }

  // User Print Settings Methods
  getUserPrintHeader(): string {
    const user = this.getCurrentUserInfo();
    const printHeader = user?.printHeader || '';
    
    if (!printHeader) return '';
    
    // If it's JSON, extract customText
    if (printHeader.trim().startsWith('{')) {
      try {
        const parsed = JSON.parse(printHeader);
        return parsed.customText || '';
      } catch (e) {
        return printHeader;
      }
    }
    
    return printHeader;
  }

  getUserPrintFooter(): string {
    const user = this.getCurrentUserInfo();
    const printFooter = user?.printFooter || '';
    
    if (!printFooter) return '';
    
    // If it's JSON, extract customText
    if (printFooter.trim().startsWith('{')) {
      try {
        const parsed = JSON.parse(printFooter);
        return parsed.customText || '';
      } catch (e) {
        return printFooter;
      }
    }
    
    return printFooter;
  }

  // Get print visibility settings
  getPrintHeaderVisibility(): any {
    const user = this.getCurrentUserInfo();
    const printHeader = user?.printHeader || '';
    
    if (!printHeader || !printHeader.trim().startsWith('{')) {
      // Return all visible by default
      return {
        showCompanyName: true,
        showCompanyLogo: true,
        showCompanyAddress: true,
        showCompanyPhone: true,
        showCompanyFax: true,
        showCompanyEmail: true,
        showRegistrationNumber: true,
        showCapital: true,
        showTaxNumber: true,
        showSlogan: true,
        showDocumentTitle: true
      };
    }
    
    if (printHeader.trim().startsWith('{')) {
      try {
        const parsed = JSON.parse(printHeader);
        return parsed.visibility || {
          showCompanyName: true,
          showCompanyLogo: true,
          showCompanyAddress: true,
          showCompanyPhone: true,
          showCompanyFax: true,
          showCompanyEmail: true,
          showRegistrationNumber: true,
          showCapital: true,
          showTaxNumber: true,
          showSlogan: true,
          showDocumentTitle: true
        };
      } catch (e) {
        // Return defaults (already returned above)
      }
    }
    
    // This should not be reached due to early return above, but keeping for safety
    return {
      showCompanyName: true,
      showCompanyLogo: true,
      showCompanyAddress: true,
      showCompanyPhone: true,
      showCompanyFax: true,
      showCompanyEmail: true,
      showRegistrationNumber: true,
      showCapital: true,
      showTaxNumber: true,
      showSlogan: true,
      showDocumentTitle: true
    };
  }

  getPrintFooterVisibility(): any {
    const user = this.getCurrentUserInfo();
    const printFooter = user?.printFooter || '';
    
    if (!printFooter || !printFooter.trim().startsWith('{')) {
      // Return all visible by default
      return {
        showTerms: true,
        showNotes: true,
        showCustomerSignature: true,
        showAuthorizedSignature: true,
        showCompanyFooterNote: true,
        showDocumentGenerationDate: true
      };
    }
    
    if (printFooter.trim().startsWith('{')) {
      try {
        const parsed = JSON.parse(printFooter);
        return parsed.visibility || {
          showTerms: true,
          showNotes: true,
          showCustomerSignature: true,
          showAuthorizedSignature: true,
          showCompanyFooterNote: true,
          showDocumentGenerationDate: true
        };
      } catch (e) {
        // Return defaults (already returned above)
      }
    }
    
    // This should not be reached due to early return above, but keeping for safety
    return {
      showTerms: true,
      showNotes: true,
      showCustomerSignature: true,
      showAuthorizedSignature: true,
      showCompanyFooterNote: true,
      showDocumentGenerationDate: true
    };
  }

  getCustomTerms(): string {
    const user = this.getCurrentUserInfo();
    const printFooter = user?.printFooter || '';
    
    if (!printFooter || !printFooter.trim().startsWith('{')) {
      return '';
    }
    
    if (printFooter.trim().startsWith('{')) {
      try {
        const parsed = JSON.parse(printFooter);
        return parsed.customTerms || '';
      } catch (e) {
        return '';
      }
    }
    
    return '';
  }

  getCustomNotes(): string {
    const user = this.getCurrentUserInfo();
    const printFooter = user?.printFooter || '';
    
    if (!printFooter || !printFooter.trim().startsWith('{')) {
      return '';
    }
    
    if (printFooter.trim().startsWith('{')) {
      try {
        const parsed = JSON.parse(printFooter);
        return parsed.customNotes || '';
      } catch (e) {
        return '';
      }
    }
    
    return '';
  }

  // Error Message Extraction
  private extractErrorMessage(err: any): string {
    // Try to get error message from different possible locations
    let errorMessage = '';
    
    if (err?.error?.errorMessage) {
      errorMessage = err.error.errorMessage;
    } else if (err?.error?.message) {
      errorMessage = err.error.message;
    } else if (err?.error) {
      // If error.error is a string
      if (typeof err.error === 'string') {
        errorMessage = err.error;
      } else if (err.error?.error) {
        errorMessage = err.error.error;
      }
    } else if (err?.message) {
      errorMessage = err.message;
    }
    
    // Check if it's a working hours error and translate it
    if (errorMessage && (errorMessage.toLowerCase().includes('working hours') || 
        errorMessage.toLowerCase().includes('outside working hours') ||
        errorMessage.toLowerCase().includes('restricted outside'))) {
      // Return translated message based on current language
      if (this.lang.currentLang === 'ar') {
        return 'العمليات مقيدة خارج ساعات العمل. يرجى المحاولة خلال ساعات العمل.';
      } else {
        return errorMessage; // Already in English
      }
    }
    
    // If no specific error message found, return generic message
    if (!errorMessage || errorMessage.trim() === '') {
      if (this.lang.currentLang === 'ar') {
        return 'فشل في إرسال الطلب. يرجى المحاولة مرة أخرى.';
      } else {
        return 'Failed to send order. Please try again.';
      }
    }
    
    return errorMessage;
  }

  // Utility Methods
  getCurrentDateTime(): string {
    return new Date().toLocaleString('ar-SA', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Product Information Methods for Print
  getProductSubCategory(productId: number): string {
    const product = this.products?.find(p => p.id === productId);
    return product?.subCategory?.nameAr || 'غير محدد';
  }
}
