import { Component, OnInit } from '@angular/core';
import { SubCategoryService } from '../../admin/services/sub-category.service';
import { LanguageService } from '../../core/services/language.service';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { AuthService } from '../../core/services/auth.service';
import { Observable, map } from 'rxjs';
import { SubCategory } from '../../admin/models/SubCategory';
import { Category } from '../../admin/models/category';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-order-sub-categories',
  standalone: false,
  templateUrl: './order-sub-categories.component.html',
  styleUrl: './order-sub-categories.component.scss'
})
export class OrderSubCategoriesComponent implements OnInit {

  constructor(
    private subCategoryService: SubCategoryService,
    public lang: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    private breadcrumbService: BreadcrumbService,
    public authService: AuthService
  ) {
    this.categories = this.route.snapshot.data['categoriesResolver'];
  }
  ngOnInit(): void {
    this.route.parent?.paramMap.subscribe(params => {
      const orderTypeIdParam = params.get('orderTypeId');
      this.orderTypeId = orderTypeIdParam !== null ? Number(orderTypeIdParam) : 1; // default 1 if null
    });

    this.route.paramMap.subscribe(params => {
      const categoryIdParam = params.get('categoryId');
      this.categoryId = categoryIdParam !== null ? Number(categoryIdParam) : 1;
      const categoryName = this.getCategoryName(this.categoryId);

      this.breadcrumbService.setFrom([
        { label: 'الرئيسية', route: '/home' },
        { label: 'التصنيفات', route: `/order/${this.orderTypeId}/categories` },
        { label: categoryName, route: null }
      ]);

      this.loadSubCategories(this.categoryId);
    });

  }

  subCategories$!: Observable<SubCategory[]>;
  orderTypeId: number = 1;
  categoryId: number = 1;
  categories!: Category[];
  serverUrl: string = environment.resourcesUrl;


  loadSubCategories(categoryId: number) {
    this.subCategories$ = this.subCategoryService.GetSubCategoryByCategoryId(categoryId).pipe(
      map(subCategories => {
        // Filter sub-categories based on user permissions
        return subCategories.filter(subCategory => this.authService.hasSubCategory(subCategory.id!));
      })
    );
  }

  getCategoryName(categoryId: number): string {
    return this.categories.find(c => c.id === categoryId)?.nameAr || 'التصنيفات الفرعية';
  }

  onCardClick(subCategoryId: number) {
    this.router.navigate([
      '/order',
      this.orderTypeId,
      'categories',
      this.categoryId,      // you need to have this available
      'sub-categories',
      subCategoryId,
      'products'
    ]);

  }
}