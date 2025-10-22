import { Component, OnInit } from '@angular/core';
import { PermissionsEnum } from '../../constants/enums/permissions.enum';
import { ThemeService } from '../../../shared/services/theme.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-main',
  standalone: false,
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent implements OnInit {
  isDarkMode = false;
  permissionsEnum = PermissionsEnum;

  constructor(
    private themeService: ThemeService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.themeService.isDarkMode$.subscribe(mode => {
      this.isDarkMode = mode;
    });
  }

  toggleDarkMode() { }

  hasAnyPermission(...perms: string[]): boolean {
    return perms.some(p => this.authService.hasPermission(p));
  }

  dashboardCards = [
    { label: 'لوحة التحكم', route: '/admin/dashboard', icon: 'bi bi-speedometer2' },
    { label: 'التصنيفات', route: '/admin/category', icon: 'bi bi-box-seam', permission: this.permissionsEnum.VIEW_CATEGORIES },
    { label: 'التصنيفات الفرعية', route: '/admin/sub-category', icon: 'bi bi-diagram-3', permission: this.permissionsEnum.VIEW_SUBCATEGORIES },
    { label: 'وحدات القياس', route: '/admin/unit', icon: 'bi bi-rulers', permission: this.permissionsEnum.VIEW_UNITS },
    { label: 'المستودعات', route: '/admin/store', icon: 'bi bi-building', permission: this.permissionsEnum.VIEW_STORES },
    { label: 'المنتجات', route: '/admin/products', icon: 'bi bi-box-seam', permission: this.permissionsEnum.VIEW_PRODUCTS },
    { label: 'نقلات المستودعات', route: '/admin/store-transfers', icon: 'bi bi-arrow-left-right', permission: this.permissionsEnum.VIEW_STORE_TRANSFERS },
        { label: 'إدارة المخزون', route: '/admin/inventory-management', icon: 'bi bi-clipboard-data', permission: this.permissionsEnum.VIEW_INVENTORY_MANAGEMENT },
        { label: 'الرصيد الابتدائي', route: '/admin/initial-stock', icon: 'bi bi-box-seam', permission: this.permissionsEnum.MANAGE_INVENTORY },
    { label: 'المستخدمين', route: '/admin/users', icon: 'bi bi-people', permission: this.permissionsEnum.VIEW_USERS },
    { label: 'اجهزة المستخدمين', route: '/admin/users-devices', icon: 'bi bi-phone', permission: this.permissionsEnum.VIEW_USERS },
    { label: 'الصلاحيات', route: '/admin/roles', icon: 'bi bi-person-badge', permission: this.permissionsEnum.VIEW_ROLES },
    { label: 'المستهلكين', route: '/admin/customers', icon: 'bi bi-person-fill', permission: this.permissionsEnum.VIEW_CUSTOMERS },
    { label: 'الموردين', route: '/admin/suppliers', icon: 'bi bi-truck', permission: this.permissionsEnum.VIEW_SUPPLIERS },
    { label: 'الطلبات', route: '/admin/order-list', icon: 'bi bi-cart-check', permission: this.permissionsEnum.VIEW_SALE_ORDERS },
    { label: 'الشركة', route: '/admin/company', icon: 'bi bi-building', permission: this.permissionsEnum.VIEW_SETTINGS }
  ];

  reportCards = [
    { label: 'تقرير الجرد', route: '/admin/inventory-report', icon: 'bi bi-file-earmark-text', permission: this.permissionsEnum.VIEW_INVENTORY_REPORT },
    { label: 'تقارير المخزون الشاملة', route: '/admin/inventory-report?tab=stock-movement', icon: 'bi bi-graph-up', permission: this.permissionsEnum.VIEW_INVENTORY_REPORT, description: 'حركة المخزون، التقييم، المخزون المنخفض، الاتجاهات' },
    { label: 'معاملات المنتج', route: '/admin/transaction', icon: 'bi bi-arrow-left-right', permission: this.permissionsEnum.VIEW_INVENTORY_REPORT, description: 'تفاصيل معاملات منتج محدد' }
  ];

}
