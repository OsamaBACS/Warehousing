import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../../admin/services/order.service';
import { map, Observable, tap } from 'rxjs';
import { OrderDto, OrderPagination } from '../../../admin/models/OrderDto';
import { Statuses } from '../../../admin/constants/enums/statuses.enum';
import { STATUS_COLORS } from '../../../admin/constants/status-color';
import { environment } from '../../../../environments/environment';
import { CartService } from '../../services/cart.service';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-order-pending-list',
  standalone: false,
  templateUrl: './order-pending-list.component.html',
  styleUrl: './order-pending-list.component.scss'
})
export class OrderPendingListComponent implements OnInit {

  constructor(
    private orderService: OrderService,
    public cartService: CartService,
    private toatsr: ToastrService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.initializingFilterForm();
  }
  ngOnInit(): void {
    this.loadOrders();
  }

  resourcesUrl = environment.resourcesUrl;
  orders$!: Observable<OrderPagination>;
  filterForm!: FormGroup;
  totalPages = 1;
  totalPagesArray: number[] = [];

  loadOrders(): void {
    this.orders$ = this.orderService.GetOrdersByUserId(this.filterForm.value).pipe(
      tap(res => {
        this.totalPages = Math.ceil(res.totals / +this.PageSize.value);
        this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
      }),
      map(response => ({
        ...response,
        orders: response.orders.map((order: any) => ({
          ...order,
          statusColor: this.getStatusColor(order.statusId - 1)
        }))
      }))
    );
  }

  initializingFilterForm() {
    this.filterForm = this.fb.group({
      pageIndex: [0],
      pageSize: [12],
      orderDate: [null],
      orderTypeId: [null],
      customerId: [null],
      supplierId: [null],
      statusId: [null],
    });
  }

  openInCart(order: OrderDto) {
    if (this.cartService.cartItems.length > 0) {
      this.toatsr.error('لا يمكن فتح الطلب، يرجى افراغ السلة اولا!');
      return;
    }

    this.cartService.loadOrder(order);
    this.toatsr.success('تم فتح الطلب بنجاح في السلة');
    this.router.navigate(['/cart'])
  }

  getStatusColor(status: Statuses): string {
    return STATUS_COLORS[status] || '#9E9E9E';
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.PageIndex.setValue(page);
    this.loadOrders();
  }

  getPageRange(): number[] {
    const delta = 2; // how many pages to show before/after current
    const range: number[] = [];

    const left = Math.max(2, +this.PageIndex.value - delta);
    const right = Math.min(this.totalPages - 1, +this.PageIndex.value + delta);

    for (let i = left; i <= right; i++) {
      range.push(i);
    }

    return range;
  }

  get PageIndex(): FormControl {
    return this.filterForm.get('pageIndex') as FormControl;
  }

  get PageSize(): FormControl {
    return this.filterForm.get('pageSize') as FormControl;
  }
}
