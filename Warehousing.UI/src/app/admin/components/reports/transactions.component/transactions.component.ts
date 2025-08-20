import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { InventoryTransactionPagination } from '../../../models/inventoryTransaction';
import { Store } from '../../../models/store';
import { InventoryTransactionsService } from '../../../services/inventory-transactions.service';
import { LanguageService } from '../../../../core/services/language.service';
import { PrintService } from '../../../../shared/services/print.service';
import { Product } from '../../../models/product';

@Component({
  selector: 'app-transactions.component',
  standalone: false,
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss'
})
export class TransactionsComponent implements OnInit {
  productId!: number;
  transactions$!: Observable<InventoryTransactionPagination>;
  pageIndex: number = 1;
  pageSize: number = 8;
  totalPages = 1;
  totalPagesArray: number[] = [];
  @ViewChild('printSection') printSection!: ElementRef;
  finalBalance: number = 0;
  stores: Store[] = [];
  storeId: number = 1;

  constructor(
    private route: ActivatedRoute,
    private inventoryTransactionService: InventoryTransactionsService,
    public lang: LanguageService,
    private printService: PrintService,
    private router: Router
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
  }

  ngOnInit(): void {
    this.productId = +this.route.snapshot.paramMap.get('id')!;
    console.log('Product ID:', this.productId);

    this.loadProductDetails();
    // You can now call a service like:
    // this.productService.getProductById(this.productId).subscribe(...)
  }

  loadProductDetails() {
    this.transactions$ = this.inventoryTransactionService.GetTransactionByProductId(this.pageIndex, this.pageSize, this.productId, this.storeId).pipe(
      tap(res => {
        this.totalPages = Math.ceil(res.totals / this.pageSize);
        this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);

        // Get opening balance from the first transaction's product
        const openingBalance = res.transactions.length > 0 ? res.transactions[0].product.openingBalance : 0;

        // Calculate total quantity change
        const totalChange = res.transactions.reduce((acc, curr) => acc + curr.quantityChanged, 0);

        // Final balance = Opening + Total Change
        this.finalBalance = openingBalance! + totalChange;
      })
    );
    this.inventoryTransactionService.GetTransactionByProductId(this.pageIndex, this.pageSize, this.productId, this.storeId).subscribe(
      data => { console.log(data) }
    )
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex = page;
    this.loadProductDetails();
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

  goBack() {
    this.router.navigate(['/admin/inventory-report']);
  }

  printTransactions(): void {
    if (!this.printSection) return;

    this.printService.printHtml(this.printSection.nativeElement.innerHTML);
  }
}