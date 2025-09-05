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
    private breadcrumbService: BreadcrumbService
  ) {
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
  }

  ngOnInit(): void {
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
      this.subCategoryId = categoryIdParam !== null ? Number(subCategoryIdParam) : 1;

      const categoryName = this.subCategories.find(c => c.id === this.subCategoryId)?.nameAr;

      this.breadcrumbService.setFrom([
        { label: 'الرئيسية', route: '/home' },
        { label: 'التصنيفات', route: `/order/${this.orderTypeId}/categories` },
        { label: categoryName || 'Products', route: null }
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

  loadProducts(subCategoryId: number) {
    this.products$ = this.productsService.GetProductsBySubCategoryId(subCategoryId);
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
      costPrice: [item?.costPrice || 0],
      sellingPrice: [item?.sellingPrice || 0],
    });

    return fg;
  }

  addToCart(product: Product): void {
    if (this.cartService.hasActiveOrder(this.orderTypeId)) {
      var message = this.cartService.orderTypeId == 1 ? 'يرجى إنهاء طلب الشراء اولا' : 'يرجى إنهاء طلب البيع اولا'
      alert(message);
      return;
    }
    this.cartService.addToCart(product, 1);
  }

  removeFromCart(product: Product): void {
    if (this.cartService.hasActiveOrder(this.orderTypeId)) {
      var message = this.cartService.orderTypeId == 1 ? 'يرجى إنهاء طلب الشراء اولا' : 'يرجى إنهاء طلب البيع اولا'
      alert(message);
      return;
    }

    if (this.getQuantity(product.id) <= 0) {
      if (this.cartService.cartItems.length <= 0) {
        this.cartService.clearCart();
      }
      return;
    }

    this.cartService.addToCart(product, -1);
  }

  getQuantity(productId: number): number {
    const index = this.cartService.cartItems.controls.findIndex(
      ctrl => ctrl.value.productId === productId
    );

    return index > -1 ? this.cartService.cartItems.at(index).get('quantity')?.value || 0 : 0;
  }

  getCartItemIndexByProductId(productId: number): number | null {
    const index = this.cartService.cartItems.controls.findIndex(
      group => group.get('productId')?.value === productId
    );
    return index >= 0 ? index : null;
  }

  getQuantityFormControl(productId: number): FormControl | null {
    const index = this.getCartItemIndexByProductId(productId);
    if (index === null) return null;

    const control = this.cartService.cartItems.at(index).get('quantity');
    return control as FormControl; // ✅ Safe cast if you're sure it's a FormControl
  }

  private formatDate(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are zero-based
    const day = ('0' + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }
}