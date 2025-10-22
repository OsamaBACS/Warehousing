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
import { AuthService } from '../../../core/services/auth.service';
import { OrderTypeDto } from '../../../admin/models/OrderTypeDto';
import { Status } from '../../../admin/models/status';
import { Customer } from '../../../admin/models/customer';
import { Supplier } from '../../../admin/models/supplier';
import { OrderTypeService } from '../../../admin/services/order-type.service';
import { StatusService } from '../../../admin/services/status.service';
import { CustomersService } from '../../../admin/services/customers.service';
import { SupplierService } from '../../../admin/services/supplier.service';

@Component({
  selector: 'app-order-pending-list',
  standalone: false,
  templateUrl: './order-pending-list.component.html',
  styleUrl: './order-pending-list.component.scss'
})
export class OrderPendingListComponent implements OnInit {
  // Loading states
  isLoading = false;
  isFilterExpanded = false;

  // Data properties
  resourcesUrl = environment.resourcesUrl;
  orders$!: Observable<OrderPagination>;
  filterForm!: FormGroup;
  totalPages = 1;
  totalPagesArray: number[] = [];

  // Filter options
  orderTypes: OrderTypeDto[] = [];
  statuses: Status[] = [];
  customers: Customer[] = [];
  suppliers: Supplier[] = [];

  constructor(
    private orderService: OrderService,
    public cartService: CartService,
    private toatsr: ToastrService,
    private router: Router,
    private fb: FormBuilder,
    private authService: AuthService,
    private orderTypeService: OrderTypeService,
    private statusService: StatusService,
    private customersService: CustomersService,
    private supplierService: SupplierService
  ) {
    this.initializingFilterForm();
  }

  ngOnInit(): void {
    this.loadFilterOptions();
    this.loadOrders();
  }

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
      pageIndex: [1],
      pageSize: [12],
      searchTerm: [null],
      orderDate: [null],
      orderTypeId: [null],
      customerId: [null],
      supplierId: [null],
      statusId: [null],
      dateFrom: [null],
      dateTo: [null],
      minAmount: [null],
      maxAmount: [null],
    });
  }

  async loadFilterOptions(): Promise<void> {
    try {
      // Load filter options in parallel
      const [orderTypesRes, statusesRes, customersRes, suppliersRes] = await Promise.all([
        this.orderTypeService.GetOrderTypes().toPromise(),
        this.statusService.GetStatusList().toPromise(),
        this.customersService.GetCustomers().toPromise(),
        this.supplierService.GetSuppliers().toPromise()
      ]);

      this.orderTypes = orderTypesRes || [];
      this.statuses = statusesRes || [];
      this.customers = customersRes || [];
      this.suppliers = suppliersRes || [];
    } catch (error) {
      console.error('Error loading filter options:', error);
    }
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

  // Search and filter methods
  onSearch(): void {
    this.PageIndex.setValue(1); // Reset to first page when searching
    this.loadOrders();
  }

  onFilterChange(): void {
    this.PageIndex.setValue(1); // Reset to first page when filtering
    this.loadOrders();
  }

  clearFilters(): void {
    this.filterForm.reset({
      pageIndex: 1,
      pageSize: 12,
      searchTerm: null,
      orderDate: null,
      orderTypeId: null,
      customerId: null,
      supplierId: null,
      statusId: null,
      dateFrom: null,
      dateTo: null,
      minAmount: null,
      maxAmount: null,
    });
    this.loadOrders();
  }

  toggleFilterPanel(): void {
    this.isFilterExpanded = !this.isFilterExpanded;
  }

  // Getter methods for form controls
  get SearchTerm(): FormControl {
    return this.filterForm.get('searchTerm') as FormControl;
  }

  get OrderDate(): FormControl {
    return this.filterForm.get('orderDate') as FormControl;
  }

  get OrderTypeId(): FormControl {
    return this.filterForm.get('orderTypeId') as FormControl;
  }

  get CustomerId(): FormControl {
    return this.filterForm.get('customerId') as FormControl;
  }

  get SupplierId(): FormControl {
    return this.filterForm.get('supplierId') as FormControl;
  }

  get StatusId(): FormControl {
    return this.filterForm.get('statusId') as FormControl;
  }

  get DateFrom(): FormControl {
    return this.filterForm.get('dateFrom') as FormControl;
  }

  get DateTo(): FormControl {
    return this.filterForm.get('dateTo') as FormControl;
  }

  get MinAmount(): FormControl {
    return this.filterForm.get('minAmount') as FormControl;
  }

  get MaxAmount(): FormControl {
    return this.filterForm.get('maxAmount') as FormControl;
  }

  // Helper method to check if user is admin
  get isAdmin(): boolean {
    return this.authService.username === 'admin';
  }
}
