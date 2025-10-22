import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { lastValueFrom, map, Observable, tap } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { ProductsService } from '../../../services/products.service';
import { InventoryTransactionsService } from '../../../services/inventory-transactions.service';
import { LanguageService } from '../../../../core/services/language.service';
import { PrintService } from '../../../../shared/services/print.service';
import { Product, ProductPagination } from '../../../models/product';
import { Store } from '../../../models/store';

@Component({
  selector: 'app-inventory-report.component',
  standalone: false,
  templateUrl: './inventory-report.component.html',
  styleUrl: './inventory-report.component.scss'
})
export class InventoryReportComponent implements OnInit {

  constructor(
    private productService: ProductsService,
    private inventoryTransactionService: InventoryTransactionsService,
    public lang: LanguageService,
    private printService: PrintService,
    private toastr: ToastrService,
    private route: ActivatedRoute,
  ) { 
    this.stores = this.route.snapshot.data['StoresResolver'];
  }
  ngOnInit(): void {
    this.loadProducts();
    
    // Check for tab query parameter
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.setActiveTab(params['tab']);
      }
    });
  }

  //#region Variables
  @ViewChild('printSection') printSection!: ElementRef;
  @ViewChild('printAllSection') printAllSection!: ElementRef;
  products$!: Observable<ProductPagination>;
  pageIndex: number = 1;
  pageSize: number = 8;
  totalPages = 1;
  totalPagesArray: number[] = [];
  today: Date = new Date();
  currentPrintData: any[] = [];
  isPrintingAll: boolean = false;
  stores: Store[] = [];
  storeId: number = 1;
  
  // New comprehensive reports data
  activeTab: string = 'inventory';
  stockMovementReport: any[] = [];
  inventoryValuationReport: any = null;
  lowStockReport: any = null;
  transactionTrends: any[] = [];
  topMovingProducts: any[] = [];
  
  // Loading states
  isLoadingStockMovement = false;
  isLoadingValuation = false;
  isLoadingLowStock = false;
  isLoadingTrends = false;
  isLoadingTopProducts = false;
  
  // Filter properties
  fromDate: Date | null = null;
  toDate: Date | null = null;
  threshold: number = 10;
  months: number = 6;
  topCount: number = 10;
  //#endregion

  //#region Functions
  loadProducts(): void {
    this.products$ = this.productService.GetProductsPagination(this.pageIndex, this.pageSize).pipe(
      tap(res => {
        this.totalPages = Math.ceil(res.totals / this.pageSize);
        this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
      })
    );
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex = page;
    this.loadProducts();
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

  printCurrentPage() {
    if (!this.printSection) return;
    this.printService.printHtml(this.printSection.nativeElement.innerHTML);
  }

  printAllPages() {
    // if (this.isPrintingAll) return;
    // this.isPrintingAll = true;

    // Load all products from API
    this.productService.GetProducts().subscribe({
      next: (allProducts) => {
        this.currentPrintData = allProducts;

        // Wait for DOM to update
        setTimeout(() => {
          if (this.printAllSection) {
            this.printService.printHtml(this.printAllSection.nativeElement.innerHTML);
          } else {
            console.error("Print section not found");
          }
        }, 500); // Small delay to allow view to refresh
      },
      error: (err) => {
        console.error("Failed to load all products", err);
        this.toastr.error("Failed to load all products", "Error");
        this.isPrintingAll = false;
      }
    });
  }

  //#endregion

  //#region Comprehensive Reports Methods
  setActiveTab(tab: string): void {
    this.activeTab = tab;
    
    // Set default date range when switching to comprehensive reports
    if (tab !== 'inventory' && (!this.fromDate || !this.toDate)) {
      this.toDate = new Date();
      this.fromDate = new Date();
      this.fromDate.setDate(this.fromDate.getDate() - 30);
    }
    
    // Load data for the selected tab
    switch (tab) {
      case 'stock-movement':
        this.loadStockMovementReport();
        break;
      case 'valuation':
        this.loadInventoryValuationReport();
        break;
      case 'low-stock':
        this.loadLowStockReport();
        break;
      case 'trends':
        this.loadTransactionTrends();
        break;
      case 'top-products':
        this.loadTopMovingProducts();
        break;
    }
  }

  async loadStockMovementReport(): Promise<void> {
    this.isLoadingStockMovement = true;
    try {
      // Convert string dates to Date objects if needed
      const fromDate = this.fromDate instanceof Date ? this.fromDate : (this.fromDate ? new Date(this.fromDate) : undefined);
      const toDate = this.toDate instanceof Date ? this.toDate : (this.toDate ? new Date(this.toDate) : undefined);
      
      this.stockMovementReport = await this.inventoryTransactionService
        .GetStockMovementReport(this.storeId > 0 ? this.storeId : undefined, fromDate, toDate)
        .toPromise() || [];
    } catch (error) {
      console.error('Error loading stock movement report:', error);
      this.toastr.error('خطأ في تحميل تقرير حركة المخزون', 'خطأ');
    } finally {
      this.isLoadingStockMovement = false;
    }
  }

  async loadInventoryValuationReport(): Promise<void> {
    this.isLoadingValuation = true;
    try {
      this.inventoryValuationReport = await this.inventoryTransactionService
        .GetInventoryValuationReport(this.storeId > 0 ? this.storeId : undefined)
        .toPromise();
    } catch (error) {
      console.error('Error loading inventory valuation report:', error);
      this.toastr.error('خطأ في تحميل تقرير تقييم المخزون', 'خطأ');
    } finally {
      this.isLoadingValuation = false;
    }
  }

  async loadLowStockReport(): Promise<void> {
    this.isLoadingLowStock = true;
    try {
      this.lowStockReport = await this.inventoryTransactionService
        .GetLowStockReport(this.threshold, this.storeId > 0 ? this.storeId : undefined)
        .toPromise();
    } catch (error) {
      console.error('Error loading low stock report:', error);
      this.toastr.error('خطأ في تحميل تقرير المخزون المنخفض', 'خطأ');
    } finally {
      this.isLoadingLowStock = false;
    }
  }

  async loadTransactionTrends(): Promise<void> {
    this.isLoadingTrends = true;
    try {
      this.transactionTrends = await this.inventoryTransactionService
        .GetTransactionTrends(this.storeId > 0 ? this.storeId : undefined, undefined, this.months)
        .toPromise() || [];
    } catch (error) {
      console.error('Error loading transaction trends:', error);
      this.toastr.error('خطأ في تحميل تقرير الاتجاهات', 'خطأ');
    } finally {
      this.isLoadingTrends = false;
    }
  }

  async loadTopMovingProducts(): Promise<void> {
    this.isLoadingTopProducts = true;
    try {
      // Convert string dates to Date objects if needed
      const fromDate = this.fromDate instanceof Date ? this.fromDate : (this.fromDate ? new Date(this.fromDate) : undefined);
      const toDate = this.toDate instanceof Date ? this.toDate : (this.toDate ? new Date(this.toDate) : undefined);
      
      this.topMovingProducts = await this.inventoryTransactionService
        .GetTopMovingProducts(this.storeId > 0 ? this.storeId : undefined, fromDate, toDate, this.topCount)
        .toPromise() || [];
    } catch (error) {
      console.error('Error loading top moving products:', error);
      this.toastr.error('خطأ في تحميل تقرير أكثر المنتجات حركة', 'خطأ');
    } finally {
      this.isLoadingTopProducts = false;
    }
  }

  onFilterChange(): void {
    // Reload the current active tab when filters change
    this.setActiveTab(this.activeTab);
  }

  onFromDateChange(dateString: string): void {
    this.fromDate = dateString ? new Date(dateString) : null;
    this.onFilterChange();
  }

  onToDateChange(dateString: string): void {
    this.toDate = dateString ? new Date(dateString) : null;
    this.onFilterChange();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-JO', {
      style: 'currency',
      currency: 'JOD',
      minimumFractionDigits: 2
    }).format(value);
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('en-JO').format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-JO', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      calendar: 'gregory'
    });
  }

  getSeverityClass(quantity: number, threshold: number): string {
    if (quantity === 0) return 'text-danger';
    if (quantity <= threshold) return 'text-warning';
    return 'text-success';
  }

  getMovementClass(movement: number): string {
    if (movement > 0) return 'text-success';
    if (movement < 0) return 'text-danger';
    return 'text-muted';
  }

  getMovementIcon(movement: number): string {
    if (movement > 0) return 'bi bi-arrow-up';
    if (movement < 0) return 'bi bi-arrow-down';
    return 'bi bi-minus';
  }
  //#endregion
}
