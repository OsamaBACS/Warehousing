import { Injectable } from '@angular/core';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { Breadcrumb } from '../../shared/models/Breadcrumb';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class OrderBreadcrumbService {

  constructor(
    private breadcrumbService: BreadcrumbService,
    private translate: TranslateService,
    private authService: AuthService
  ) { }

  // Helper method to get the home route based on user role
  private getHomeRoute(orderTypeId?: number): string {
    if (this.authService.isAdmin) {
      return '/admin/main';
    }
    return orderTypeId ? `/order/${orderTypeId}/categories` : '/order/2/categories';
  }

  // Set breadcrumbs for order categories
  setOrderCategoriesBreadcrumbs(orderTypeId: number): void {
    const orderTypeName = this.getOrderTypeName(orderTypeId);
    this.breadcrumbService.setFrom([
      { label: this.translate.instant('BREADCRUMB.HOME'), route: this.getHomeRoute(orderTypeId) },
      { label: orderTypeName, route: `/order/${orderTypeId}/categories` },
      { label: this.translate.instant('BREADCRUMB.CATEGORIES'), route: null }
    ]);
  }

  // Set breadcrumbs for order subcategories
  setOrderSubCategoriesBreadcrumbs(orderTypeId: number, categoryId: number, categoryName: string): void {
    const orderTypeName = this.getOrderTypeName(orderTypeId);
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: this.getHomeRoute(orderTypeId) },
      { label: orderTypeName, route: `/order/${orderTypeId}/categories` },
      { label: 'التصنيفات', route: `/order/${orderTypeId}/categories` },
      { label: categoryName, route: `/order/${orderTypeId}/categories/${categoryId}/sub-categories` },
      { label: 'التصنيفات الفرعية', route: null }
    ]);
  }

  // Set breadcrumbs for order products
  setOrderProductsBreadcrumbs(orderTypeId: number, categoryId: number, categoryName: string, subCategoryId: number, subCategoryName: string): void {
    const orderTypeName = this.getOrderTypeName(orderTypeId);
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: this.getHomeRoute(orderTypeId) },
      { label: orderTypeName, route: `/order/${orderTypeId}/categories` },
      { label: 'التصنيفات', route: `/order/${orderTypeId}/categories` },
      { label: categoryName, route: `/order/${orderTypeId}/categories/${categoryId}/sub-categories` },
      { label: subCategoryName, route: `/order/${orderTypeId}/categories/${categoryId}/sub-categories/${subCategoryId}/products` },
      { label: 'المنتجات', route: null }
    ]);
  }

  // Set breadcrumbs for product detail
  setProductDetailBreadcrumbs(orderTypeId: number, categoryId: number, categoryName: string, subCategoryId: number, subCategoryName: string, productName: string): void {
    const orderTypeName = this.getOrderTypeName(orderTypeId);
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: this.getHomeRoute(orderTypeId) },
      { label: orderTypeName, route: `/order/${orderTypeId}/categories` },
      { label: 'التصنيفات', route: `/order/${orderTypeId}/categories` },
      { label: categoryName, route: `/order/${orderTypeId}/categories/${categoryId}/sub-categories` },
      { label: subCategoryName, route: `/order/${orderTypeId}/categories/${categoryId}/sub-categories/${subCategoryId}/products` },
      { label: productName, route: null }
    ]);
  }

  // Set breadcrumbs for cart
  setCartBreadcrumbs(orderTypeId: number): void {
    const orderTypeName = this.getOrderTypeName(orderTypeId);
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: this.getHomeRoute(orderTypeId) },
      { label: orderTypeName, route: `/order/${orderTypeId}/categories` },
      { label: 'السلة', route: null }
    ]);
  }

  // Set breadcrumbs for order summary
  setOrderSummaryBreadcrumbs(orderTypeId: number): void {
    const orderTypeName = this.getOrderTypeName(orderTypeId);
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: this.getHomeRoute(orderTypeId) },
      { label: orderTypeName, route: `/order/${orderTypeId}/categories` },
      { label: 'ملخص الطلب', route: null }
    ]);
  }

  // Helper method to get order type name
  private getOrderTypeName(orderTypeId: number): string {
    switch (orderTypeId) {
      case 1:
        return 'طلبات الشراء';
      case 2:
        return 'طلبات البيع';
      default:
        return 'الطلبات';
    }
  }

  // Set breadcrumbs for pending orders
  setPendingOrdersBreadcrumbs(orderTypeId?: number): void {
    this.breadcrumbService.setFrom([
      { label: this.translate.instant('BREADCRUMB.HOME'), route: this.getHomeRoute(orderTypeId) },
      { label: this.translate.instant('BREADCRUMB.PENDING_ORDERS'), route: null }
    ]);
  }
}
