import { Component, OnDestroy, OnInit } from '@angular/core';
import { CategoriesService } from '../../admin/services/categories.service';
import { LanguageService } from '../../core/services/language.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { OrderBreadcrumbService } from '../services/order-breadcrumb.service';
import { AuthService } from '../../core/services/auth.service';
import { Observable, Subscription, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, tap, catchError, map } from 'rxjs/operators';
import { Category } from '../../admin/models/category';
import { environment } from '../../../environments/environment';
import { ProductsService } from '../../admin/services/products.service';
import { FormControl } from '@angular/forms';
import { Product } from '../../admin/models/product';
import { ImageUrlService } from '../../shared/services/image-url.service';

@Component({
  selector: 'app-order-categories',
  standalone: false,
  templateUrl: './order-categories.component.html',
  styleUrl: './order-categories.component.scss'
})
export class OrderCategoriesComponent implements OnInit, OnDestroy {

  constructor(
    private categoriesService: CategoriesService,
    public lang: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    private breadcrumbService: BreadcrumbService,
    private orderBreadcrumbService: OrderBreadcrumbService,
    private authService: AuthService,
    private productsService: ProductsService,
    public imageUrlService: ImageUrlService
  ) { }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  ngOnInit(): void {
    this.route.parent?.paramMap.subscribe(params => {
      const orderTypeIdParam = params.get('orderTypeId');
      this.orderTypeId = orderTypeIdParam !== null ? Number(orderTypeIdParam) : 1; // default 1 if null
      
      // Set breadcrumbs using the order breadcrumb service
      this.orderBreadcrumbService.setOrderCategoriesBreadcrumbs(this.orderTypeId);
      this.loadCategories();
    });

    this.initializeProductSearch();
  }

  categories$!: Observable<Category[]>;
  orderTypeId: number = 1;
  serverUrl: string = environment.resourcesUrl;
  searchControl: FormControl<string> = new FormControl<string>('', { nonNullable: true });
  searchResults: Product[] = [];
  searchLoading = false;
  searchError: string | null = null;
  private searchSubscription?: Subscription;

  loadCategories() {
    this.categories$ = this.categoriesService.GetActiveCategories().pipe(
      map(categories => {
        // Filter categories based on user permissions
        return categories.filter(category => this.authService.hasCategory(category.id!));
      })
    );
  }

  onCardClick(categoryId: number) {
    this.router.navigate([
      '/order',
      this.orderTypeId,
      'categories',
      categoryId,
      'sub-categories'
    ]);
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: true });
    this.searchResults = [];
    this.searchError = null;
    this.searchLoading = false;
  }

  onSelectProduct(product: Product): void {
    if (!product?.id) {
      return;
    }

    this.router.navigate(
      ['/order', this.orderTypeId, 'product', product.id],
      {
        queryParams: {
          categoryId: product.subCategory?.categoryId ?? undefined,
          subCategoryId: product.subCategoryId ?? undefined
        }
      }
    );

    this.searchResults = [];
    this.searchError = null;
    this.searchControl.setValue('', { emitEvent: false });
  }

  private initializeProductSearch(): void {
    this.searchSubscription = this.searchControl.valueChanges.pipe(
      map(value => value?.trim() ?? ''),
      tap(value => {
        if (!value) {
          this.searchResults = [];
          this.searchError = null;
        }
      }),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(keyword => {
        if (!keyword) {
          this.searchLoading = false;
          return of<Product[]>([]);
        }

        this.searchLoading = true;
        this.searchError = null;

        return this.productsService.SearchProducts(keyword).pipe(
          map(products => products.filter(product => this.authService.hasProduct(product.id!))),
          tap(() => this.searchLoading = false),
          catchError(error => {
            this.searchLoading = false;
            this.searchError = 'حدث خطأ أثناء البحث عن المنتجات. حاول مرة أخرى.';
            return of<Product[]>([]);
          })
        );
      })
    ).subscribe(results => {
      this.searchResults = results.slice(0, 10);
    });
  }
}