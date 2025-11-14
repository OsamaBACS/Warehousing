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
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'لوحة التحكم', route: null }
    ]);
  }

  // Set breadcrumbs for products management
  setProductsBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المنتجات', route: null }
    ]);
  }

  // Set breadcrumbs for product form
  setProductFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المنتجات', route: '/admin/products' },
      { label: isEdit ? 'تعديل المنتج' : 'إضافة منتج جديد', route: null }
    ]);
  }

  // Set breadcrumbs for categories management
  setCategoriesBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'التصنيفات', route: null }
    ]);
  }

  // Set breadcrumbs for category form
  setCategoryFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'التصنيفات', route: '/admin/categories' },
      { label: isEdit ? 'تعديل التصنيف' : 'إضافة تصنيف جديد', route: null }
    ]);
  }

  // Set breadcrumbs for subcategories management
  setSubCategoriesBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'التصنيفات', route: '/admin/categories' },
      { label: 'التصنيفات الفرعية', route: null }
    ]);
  }

  // Set breadcrumbs for subcategory form
  setSubCategoryFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'التصنيفات', route: '/admin/categories' },
      { label: isEdit ? 'تعديل التصنيف الفرعي' : 'إضافة تصنيف فرعي جديد', route: null }
    ]);
  }

  // Set breadcrumbs for stores management
  setStoresBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المستودعات', route: null }
    ]);
  }

  // Set breadcrumbs for store form
  setStoreFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المستودعات', route: '/admin/stores' },
      { label: isEdit ? 'تعديل المستودع' : 'إضافة مستودع جديد', route: null }
    ]);
  }

  // Set breadcrumbs for users management
  setUsersBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المستخدمين', route: null }
    ]);
  }

  // Set breadcrumbs for user form
  setUserFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المستخدمين', route: '/admin/users' },
      { label: isEdit ? 'تعديل المستخدم' : 'إضافة مستخدم جديد', route: null }
    ]);
  }

  // Set breadcrumbs for roles management
  setRolesBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'الأدوار', route: null }
    ]);
  }

  // Set breadcrumbs for role form
  setRoleFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'الأدوار', route: '/admin/roles' },
      { label: isEdit ? 'تعديل الدور' : 'إضافة دور جديد', route: null }
    ]);
  }

  // Set breadcrumbs for orders management
  setOrdersBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'الطلبات', route: null }
    ]);
  }

  // Set breadcrumbs for order form
  setOrderFormBreadcrumbs(isEdit: boolean = false): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'الطلبات', route: '/admin/order-list' },
      { label: isEdit ? 'تعديل الطلب' : 'إضافة طلب جديد', route: null }
    ]);
  }

  // Set breadcrumbs for inventory management
  setInventoryBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'المخزون', route: null }
    ]);
  }

  // Set breadcrumbs for reports
  setReportsBreadcrumbs(reportType: string = 'التقارير'): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'التقارير', route: '/admin/inventory-report' },
      { label: reportType, route: null }
    ]);
  }

  // Set breadcrumbs for company settings
  setCompanyBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'الشركة', route: null }
    ]);
  }

  // Set breadcrumbs for company form
  setCompanyFormBreadcrumbs(): void {
    this.breadcrumbService.setFrom([
      { label: 'الرئيسية', route: '/admin/dashboard' },
      { label: 'الشركة', route: '/admin/company' },
      { label: 'تعديل بيانات الشركة', route: null }
    ]);
  }
}
