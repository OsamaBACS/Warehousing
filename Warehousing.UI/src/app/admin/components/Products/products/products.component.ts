import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ProductsService } from '../../../services/products.service';
import { CategoriesService } from '../../../services/categories.service';
import { UnitsService } from '../../../services/units.service';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../../core/services/auth.service';
import { Category } from '../../../models/category';
import { Unit } from '../../../models/unit';
import { Observable, tap } from 'rxjs';
import { Product, ProductPagination } from '../../../models/product';
import { PermissionsEnum } from '../../../constants/enums/permissions.enum';
import { Store } from '../../../models/store';

@Component({
  selector: 'app-products',
  standalone: false,
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {

  constructor(
    private productService: ProductsService,
    private categoriesService: CategoriesService,
    private unitsService: UnitsService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService,
    private authService: AuthService
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
    this.productForm = this.fb.group({
      id: [null],
      name: ['', Validators.required],
      price: [0, Validators.required]
    });
    this.searchForm = this.fb.group({
      keyword: [null, Validators.required],
    });
  }
  ngOnInit(): void {
    // this.products$ = this.productService.GetProductsPagination(this.pageIndex, this.pageSize);
    this.loadProducts();

    this.categoriesService.GetCategories().subscribe({
      next: (res) => {
        this.categories = res;
      },
      error: (err) => {
        console.log(err.error)
      }
    });
    this.unitsService.GetUnits().subscribe({
      next: (res) => {
        this.units = res;
      },
      error: (err) => {
        console.log(err.error)
      }
    });
    this.serverUrl = this.productService.url.substring(0, this.productService.url.indexOf('api'));
  }

  categories!: Category[];
  units!: Unit[];
  products$!: Observable<ProductPagination>;
  pageIndex: number = 1;
  pageSize: number = 8;
  totalPages = 1;
  totalPagesArray: number[] = [];
  productForm!: FormGroup;
  searchForm!: FormGroup;
  editingProduct: Product | null = null;
  @ViewChild('productModal') productModalRef!: ElementRef;
  serverUrl = '';
  permissionsEnum = PermissionsEnum;
  stores: Store[] = [];
  storeId: number = 0;

  hasAnyPermission(...perms: string[]): boolean {
    return perms.some(p => this.authService.hasPermission(p));
  }

  openForm(productId: number | null) {
    if (productId) {
      this.router.navigate(['../product-form'], { relativeTo: this.route, queryParams: { productId: productId } });
    }
    else {
      this.router.navigate(['../product-form'], { relativeTo: this.route });
    }
  }

  loadProducts(loadType: string = 'GET'): void {
    if (loadType === 'GET') {
      this.searchForm.reset();
      this.products$ = this.productService.GetProductsPagination(this.pageIndex, this.pageSize).pipe(
        tap(res => {
          this.totalPages = Math.ceil(res.totals / this.pageSize);
          this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
        })
      );
    }
    else {
      var keyword = '';
      if (this.keyword?.value) {
        keyword = this.keyword.value?.toString();
      }

        this.products$ = this.productService.SearchProductsPagination(this.pageIndex, this.pageSize, keyword, this.storeId).pipe(
          tap(res => {
            this.totalPages = Math.ceil(res.totals / this.pageSize);
            this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
          })
        );
    }
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

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex = page;

    if (this.keyword.value || this.storeId > 0) {
      this.loadProducts('SEARCH');
    }
    else {
      this.loadProducts();
    }
  }

  get keyword(): FormControl {
    return this.searchForm.get('keyword') as FormControl;
  }
}
