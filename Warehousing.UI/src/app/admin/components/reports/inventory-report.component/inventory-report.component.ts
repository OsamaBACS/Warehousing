import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { lastValueFrom, map, Observable, tap } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { ProductsService } from '../../../services/products.service';
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
    public lang: LanguageService,
    private printService: PrintService,
    private toastr: ToastrService,
    private route: ActivatedRoute,
  ) { 
    this.stores = this.route.snapshot.data['StoresResolver'];
  }
  ngOnInit(): void {
    this.loadProducts();
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
}
