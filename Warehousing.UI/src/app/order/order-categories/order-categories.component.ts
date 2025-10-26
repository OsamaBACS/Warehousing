import { Component, OnInit } from '@angular/core';
import { CategoriesService } from '../../admin/services/categories.service';
import { LanguageService } from '../../core/services/language.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { OrderBreadcrumbService } from '../services/order-breadcrumb.service';
import { Observable } from 'rxjs';
import { Category } from '../../admin/models/category';

@Component({
  selector: 'app-order-categories',
  standalone: false,
  templateUrl: './order-categories.component.html',
  styleUrl: './order-categories.component.scss'
})
export class OrderCategoriesComponent implements OnInit {

  constructor(
    private categoriesService: CategoriesService,
    public lang: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    private breadcrumbService: BreadcrumbService,
    private orderBreadcrumbService: OrderBreadcrumbService
  ) { }
  ngOnInit(): void {
    this.route.parent?.paramMap.subscribe(params => {
      const orderTypeIdParam = params.get('orderTypeId');
      this.orderTypeId = orderTypeIdParam !== null ? Number(orderTypeIdParam) : 1; // default 1 if null
      // Set breadcrumbs using the order breadcrumb service
      this.orderBreadcrumbService.setOrderCategoriesBreadcrumbs(this.orderTypeId);
      this.loadCategories();
    });
  }

  categories$!: Observable<Category[]>;
  orderTypeId: number = 1;

  loadCategories() {
    this.categories$ = this.categoriesService.GetActiveCategories();
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

}