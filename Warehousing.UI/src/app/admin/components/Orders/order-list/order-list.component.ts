import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../../services/order.service';
import { ProductsService } from '../../../services/products.service';
import { StatusService } from '../../../services/status.service';
import { CustomersService } from '../../../services/customers.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { OrderPagination } from '../../../models/OrderDto';
import { map, Observable, tap } from 'rxjs';
import { Status } from '../../../models/status';
import { Customer } from '../../../models/customer';
import { Product } from '../../../models/product';
import { Statuses } from '../../../constants/enums/statuses.enum';
import { STATUS_COLORS } from '../../../constants/status-color';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-order-list',
  standalone: false,
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit {

  constructor(
    private orderService: OrderService,
    private productService: ProductsService,
    private statusService: StatusService,
    private customerService: CustomersService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private fb: FormBuilder
  ) { 
    this.filter = this.fb.group({
      orderTypeId: [0]
    })
  }
  ngOnInit(): void {
    this.loadOrders();
    this.products = this.route.snapshot.data['productsResolver'];
    this.statuses = this.route.snapshot.data['statusesResolver'];
    this.customers = this.route.snapshot.data['customersRsolver'];
  }

  orders$!: Observable<OrderPagination>;
  pageIndex: number = 1;
  pageSize: number = 10;
  totalPages = 1;
  totalPagesArray: number[] = [];
  statuses: Status[] = []
  customers: Customer[] = []
  products: Product[] = [];
  filter!: FormGroup;

  openForm(orderId: number | null) {
    if (orderId) {
      this.router.navigate(['../order-items-list'], { relativeTo: this.route, queryParams: { orderId: orderId } });
    }
    else {
      this.router.navigate(['../order-items-list'], { relativeTo: this.route });
    }
  }

  loadOrders(): void {
    this.orders$ = this.orderService.GetOrdersPagination(this.pageIndex, this.pageSize, +this.OrderTypeId.value).pipe(
      tap(res => {
        this.totalPages = Math.ceil(res.totals / this.pageSize);
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

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex = page;
    this.loadOrders();
  }

  getPageRange(): number[] {
    const delta = 2; // how many pages to show before/after current
    const range: number[] = [];

    const left = Math.max(2, this.pageIndex - delta);
    const right = Math.min(this.totalPages - 1, this.pageIndex + delta);

    for (let i = left; i <= right; i++) {
      range.push(i);
    }

    return range;
  }

  getStatusColor(status: Statuses): string {
    return STATUS_COLORS[status] || '#9E9E9E';
  }

  get OrderTypeId(): FormControl {
    return this.filter.get('orderTypeId') as FormControl;
  }
}