import { Injectable } from '@angular/core';
import { BreadcrumbService } from '../../shared/services/breadcrumb.service';
import { Breadcrumb } from '../../shared/models/Breadcrumb';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class AdminBreadcrumbService {

  constructor(
    private breadcrumbService: BreadcrumbService,
    private translate: TranslateService
  ) { }

  // Set breadcrumbs for admin dashboard
  setDashboardBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: this.translate.instant('BREADCRUMB.HOME'), route: '/home' },
      { label: this.translate.instant('BREADCRUMB.DASHBOARD'), route: null }
    ]);
  }

  // Set breadcrumbs for products management
  setProductsBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: this.translate.instant('BREADCRUMB.HOME'), route: '/home' },
      { label: this.translate.instant('BREADCRUMB.ADMIN'), route: '/admin/products' },
      { label: this.translate.instant('BREADCRUMB.PRODUCTS'), route: null }
    ]);
  }

  // Set breadcrumbs for product form
  setProductFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة المنتجات', route: '/admin/products' },
      { label: isEdit ? 'تعديل المنتج' : 'إضافة منتج جديد', route: null }
    ]);
  }

  // Set breadcrumbs for categories management
  setCategoriesBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة التصنيفات', route: '/admin/categories' },
      { label: 'التصنيفات', route: null }
    ]);
  }

  // Set breadcrumbs for category form
  setCategoryFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة التصنيفات', route: '/admin/categories' },
      { label: isEdit ? 'تعديل التصنيف' : 'إضافة تصنيف جديد', route: null }
    ]);
  }

  // Set breadcrumbs for subcategories management
  setSubCategoriesBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة التصنيفات', route: '/admin/categories' },
      { label: 'التصنيفات الفرعية', route: null }
    ]);
  }

  // Set breadcrumbs for subcategory form
  setSubCategoryFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة التصنيفات', route: '/admin/categories' },
      { label: isEdit ? 'تعديل التصنيف الفرعي' : 'إضافة تصنيف فرعي جديد', route: null }
    ]);
  }

  // Set breadcrumbs for stores management
  setStoresBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة المستودعات', route: '/admin/stores' },
      { label: 'المستودعات', route: null }
    ]);
  }

  // Set breadcrumbs for store form
  setStoreFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة المستودعات', route: '/admin/stores' },
      { label: isEdit ? 'تعديل المستودع' : 'إضافة مستودع جديد', route: null }
    ]);
  }

  // Set breadcrumbs for users management
  setUsersBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: this.translate.instant('BREADCRUMB.HOME'), route: '/home' },
      { label: this.translate.instant('BREADCRUMB.ADMIN'), route: '/admin/users' },
      { label: this.translate.instant('BREADCRUMB.USERS'), route: null }
    ]);
  }

  // Set breadcrumbs for user form
  setUserFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة المستخدمين', route: '/admin/users' },
      { label: isEdit ? 'تعديل المستخدم' : 'إضافة مستخدم جديد', route: null }
    ]);
  }

  // Set breadcrumbs for roles management
  setRolesBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة الأدوار', route: '/admin/roles' },
      { label: 'الأدوار', route: null }
    ]);
  }

  // Set breadcrumbs for role form
  setRoleFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة الأدوار', route: '/admin/roles' },
      { label: isEdit ? 'تعديل الدور' : 'إضافة دور جديد', route: null }
    ]);
  }

  // Set breadcrumbs for orders management
  setOrdersBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة الطلبات', route: '/admin/orders' },
      { label: 'الطلبات', route: null }
    ]);
  }

  // Set breadcrumbs for order form
  setOrderFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة الطلبات', route: '/admin/orders' },
      { label: isEdit ? 'تعديل الطلب' : 'إضافة طلب جديد', route: null }
    ]);
  }

  // Set breadcrumbs for inventory management
  setInventoryBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إدارة المخزون', route: '/admin/inventory' },
      { label: 'المخزون', route: null }
    ]);
  }

  // Set breadcrumbs for reports
  setReportsBreadcrumbs(reportType: string = 'التقارير'): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'التقارير', route: '/admin/reports' },
      { label: reportType, route: null }
    ]);
  }

  // Set breadcrumbs for company settings
  setCompanyBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إعدادات الشركة', route: '/admin/company' },
      { label: 'الشركة', route: null }
    ]);
  }

  // Set breadcrumbs for company form
  setCompanyFormBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/home' },
      { label: 'إعدادات الشركة', route: '/admin/company' },
      { label: 'تعديل بيانات الشركة', route: null }
    ]);
  }
}
