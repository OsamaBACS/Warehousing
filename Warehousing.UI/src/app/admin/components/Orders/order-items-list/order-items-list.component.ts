import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { OrderService } from '../../../services/order.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ToastrService } from 'ngx-toastr';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { PrintService } from '../../../../shared/services/print.service';
import { CompaniesService } from '../../../services/companies.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Observable, tap } from 'rxjs';
import { Status } from '../../../models/status';
import { Customer } from '../../../models/customer';
import { Product } from '../../../models/product';
import { SubCategory } from '../../../models/SubCategory';
import { Statuses } from '../../../constants/enums/statuses.enum';
import { Unit } from '../../../models/unit';
import { Company } from '../../../models/Company';
import { minArrayLength } from '../../../validators/minArrayLength';
import { OrderItemDto } from '../../../models/OrderItemDto';
import { STATUS_COLORS } from '../../../constants/status-color';
import { Supplier } from '../../../models/supplier';
import { OrderDto } from '../../../models/OrderDto';

@Component({
  selector: 'app-order-items-list',
  standalone: false,
  templateUrl: './order-items-list.component.html',
  styleUrl: './order-items-list.component.scss'
})
export class OrderItemsListComponent implements OnInit {

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router,
    public lang: LanguageService,
    private notification: NotificationService,
    private toastr: ToastrService,
    private modalService: BsModalService,
    private cdRef: ChangeDetectorRef,
    private printService: PrintService,
    private companiesService: CompaniesService,
    private authService: AuthService
  ) {
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
    this.products = this.route.snapshot.data['productsResolver'];
    this.statuses = this.route.snapshot.data['statusesResolver'];
    this.customers = this.route.snapshot.data['suppliersResolver'];
    this.suppliers = this.route.snapshot.data['customersRsolver'];
    this.units = this.route.snapshot.data['unitsResolver'];
  }
  ngOnInit(): void {
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const orderId = +params['orderId'];
      if (orderId) {
        this.orderId = orderId;
        this.order$ = this.orderService.GetOrderById(orderId)
          .pipe(
            tap(res => {
              this.status.statusCode = res.status?.code!;
              this.status.nameAr = res.status?.nameAr!;
              this.status.nameEn = res.status?.nameEn!;
              this.status.statusColor = this.getStatusColor(res.statusId! - 1);
              this.cdRef.detectChanges();
              this.initializingForm(res);
              const istrue = this.status.statusCode == this.statusEnum[this.statusEnum.COMPLETED]
            })
          );
      }
    });
  }

  //#region Variables
  order$!: Observable<OrderDto>;
  storeId: number = 0;
  private subscribedControls = new WeakSet<AbstractControl>();
  orderForm!: FormGroup;
  status: {
    nameAr: string | null,
    nameEn: string | null,
    statusColor: string | null,
    statusCode: string | null
  } = { nameAr: null, nameEn: null, statusColor: null, statusCode: null };
  statuses: Status[] = []
  customers: Customer[] = [];
  suppliers: Supplier[] = [];
  products: Product[] = []
  subCategories!: SubCategory[];
  orderId: number = 0;
  statusEnum = Statuses;
  units: Unit[] = [];
  bsModalRef?: BsModalRef;
  company!: Company;
  @ViewChild('printSection') printSection!: ElementRef;
  //#endregion

  //#region Method
  initializingForm(order: OrderDto | null) {
    const formattedOrderDate = order?.orderDate ? this.formatDate(order.orderDate) : this.formatDate(new Date().toString());
    this.orderForm = this.fb.group({
      id: [order?.id || 0],
      orderDate: [{ value: formattedOrderDate, disabled: this.status.statusCode == this.statusEnum[this.statusEnum.COMPLETED] }, [Validators.required]],
      totalAmount: [order?.totalAmount || 0],
      customerId: [{ value: order?.customerId || null, disabled: this.status.statusCode == this.statusEnum[this.statusEnum.COMPLETED] }, [Validators.required]],
      statusId: [order?.statusId || 1],
      items: this.fb.array([], minArrayLength(1))
    });

    // Populate items if available
    if (order?.id && order.id > 0) {
      const itemsFormArray = this.orderForm.get('items') as FormArray;

      if(order.items?.length > 0) this.storeId = order.items[0].storeId!;

      order.items.forEach((item: any) => {
        itemsFormArray.push(this.createItemGroup(item));
      });
    }
  }

  createItemGroup(item?: OrderItemDto): FormGroup {
    const fg = this.fb.group({
      id: [item?.id || null],
      subCategoryId: [{ value: item?.product?.subCategoryId || null, disabled: true }],
      code: [{ value: item?.product?.code || null, disabled: true }],
      productId: [
        { value: item?.productId || null, disabled: true },
        [Validators.required, this.emptyStringValidator()]
      ],
      unitId: [{ value: item?.product?.unitId || null, disabled: true }],
      quantity: [{ value: item?.quantity || 0, disabled: true }],
      unitPrice: [{ value: item?.unitPrice || 0, disabled: true}],
    });

    return fg;
  }

  emptyStringValidator() {
    return (control: FormControl) => {
      if (control.value === null || control.value === '') {
        return { required: true };
      }
      return null;
    };
  }

  addItem(): void {
    this.items.push(this.createItemGroup());
    this.calculatingTotal();
  }

  removeItem(index: number): void {
    this.items.removeAt(index);
    this.calculatingTotal();
  }

  calculatingTotal() {
    var sum = 0;
    this.items.controls.forEach(control => {
      const quantity = +control.get('quantity')?.value;
      const sellingPrice = +control.get('sellingPrice')?.value;
      sum += (quantity * sellingPrice);
    });
    this.orderForm.get('totalAmount')?.setValue(sum);
  }

  get id() {
    return this.orderForm.get('id') as FormControl;
  }

  get orderDate() {
    return this.orderForm.get('orderDate') as FormControl;
  }

  get totalAmount() {
    return this.orderForm.get('totalAmount') as FormControl;
  }

  get supplierId() {
    return this.orderForm.get('supplierId') as FormControl;
  }

  get statusId() {
    return this.orderForm.get('statusId') as FormControl;
  }

  get items(): FormArray {
    return this.orderForm.get('items') as FormArray;
  }

  save() {
    if (this.orderForm.valid) {
      this.calculatingTotal();
      console.log(this.orderForm.value);
      this.orderService.SaveOrder(this.orderForm.value).subscribe({
        next: (res) => {
          console.log(res);
          if (res) {
            this.notification.success('Successfully saved', 'Sale Order');
            this.router.navigate(['../order-list'], { relativeTo: this.route });
          }
          else {
            this.notification.error('Error while saving', 'Sale Order')
          }
        },
        error: (err) => {
          console.log(err.error);
          this.notification.error(err.error, 'Sale Order')
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['../order-list'], { relativeTo: this.route });
  }

  private formatDate(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are zero-based
    const day = ('0' + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }

  getStatusColor(status: Statuses): string {
    return STATUS_COLORS[status] || '#9E9E9E';
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

  printOrder() {
    if (this.printSection) {
      this.printService.printHtml(this.printSection.nativeElement.innerHTML);
    } else {
      console.error("Print section not found");
    }
  }

  //#endregion
}