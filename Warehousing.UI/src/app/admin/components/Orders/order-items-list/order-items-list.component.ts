import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { OrderService } from '../../../services/order.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { PrintService } from '../../../../shared/services/print.service';
import { PdfPrintService } from '../../../../shared/services/pdf-print.service';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { Status } from '../../../models/status';
import { Customer } from '../../../models/customer';
import { Product } from '../../../models/product';
import { SubCategory } from '../../../models/SubCategory';
import { Statuses } from '../../../constants/enums/statuses.enum';
import { Unit } from '../../../models/unit';
import { OrderDto } from '../../../models/OrderDto';
import { Supplier } from '../../../models/supplier';

@Component({
  selector: 'app-order-items-list',
  standalone: false,
  templateUrl: './order-items-list.component.html',
  styleUrl: './order-items-list.component.scss'
})
export class OrderItemsListComponent implements OnInit {

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router,
    public lang: LanguageService,
    private printService: PrintService,
    private pdfPrintService: PdfPrintService,
    private toastr: ToastrService
  ) {
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
    this.products = this.route.snapshot.data['productsResolver'];
    this.statuses = this.route.snapshot.data['statusesResolver'];
    this.customers = this.route.snapshot.data['suppliersResolver'];
    this.suppliers = this.route.snapshot.data['customersRsolver'];
    this.units = this.route.snapshot.data['unitsResolver'];
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const orderId = +params['orderId'];
      if (orderId) {
        this.orderId = orderId;
        this.order$ = this.orderService.GetOrderById(orderId);
      }
    });
  }

  //#region Variables
  @ViewChild('printSection', { static: false }) printSection!: ElementRef;
  order$!: Observable<OrderDto>;
  orderId: number = 0;
  statuses: Status[] = []
  customers: Customer[] = [];
  suppliers: Supplier[] = [];
  products: Product[] = []
  subCategories!: SubCategory[];
  statusEnum = Statuses;
  units: Unit[] = [];
  currentDate = new Date();
  //#endregion

  //#region Methods
  getCustomerName(id: number | null): string {
    if (!id) return '';
    const customer = this.customers.find(c => c.id === id);
    return customer ? (this.lang.currentLang === 'ar' ? customer.nameAr : customer.nameEn!) : '';
  }

  getSupplierName(id: number | null): string {
    if (!id) return '';
    const supplier = this.suppliers.find(s => s.id === id);
    return supplier ? supplier.name : '';
  }

  getCategoryName(id: number | null | undefined): string {
    if (!id) return '';
    const subCategory = this.subCategories.find(c => c.id === id);
    return subCategory ? (this.lang.currentLang === 'ar' ? subCategory.nameAr! : subCategory.nameEn!) : '';
  }

  getProductName(id: number | null | undefined): string {
    if (!id) return '';
    const product = this.products.find(p => p.id === id);
    return product ? (this.lang.currentLang === 'ar' ? product.nameAr : product.nameEn!) : '';
  }

  getUnitName(id: number | null | undefined): string {
    if (!id) return '';
    const unit = this.units.find(p => p.id === id);
    return unit ? (this.lang.currentLang === 'ar' ? unit.nameAr : unit.nameEn) : '';
  }

  goBack() {
    this.router.navigate(['../order-list'], { relativeTo: this.route });
  }

  printOrder() {
    if (this.printSection) {
      const documentTitle = `طلب رقم ${this.orderId}`;
      this.printService.printHtml(this.printSection.nativeElement.innerHTML, documentTitle);
    } else {
      console.error("Print section not found");
      this.toastr.error('خطأ في الطباعة', 'خطأ');
    }
  }

  async printOrderPDF() {
    try {
      if (!this.printSection) {
        console.error("Print section not found");
        this.toastr.error('خطأ في الطباعة', 'خطأ');
        return;
      }

      const documentTitle = `طلب رقم ${this.orderId}`;
      
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
      console.error('Error printing PDF:', error);
      this.toastr.error('خطأ في طباعة PDF', 'خطأ');
      // Fallback to regular print service
      this.printOrder();
    }
  }

  async downloadOrderPDF() {
    try {
      if (!this.printSection) {
        console.error("Print section not found");
        this.toastr.error('خطأ في الطباعة', 'خطأ');
        return;
      }

      const documentTitle = `طلب رقم ${this.orderId}`;
      
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
      console.error('Error downloading PDF:', error);
      this.toastr.error('خطأ في تحميل PDF', 'خطأ');
    }
  }
  //#endregion
}