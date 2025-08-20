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
    { label: 'التصنيفات', route: '/admin/category', icon: 'bi bi-box-seam' },
    { label: 'التصنيفات الفرعية', route: '/admin/sub-category', icon: 'bi bi-box-seam' },
    { label: 'وحدات القياس', route: '/admin/unit', icon: 'bi bi-box-seam' },
    { label: 'المستودعات', route: '/admin/store', icon: 'bi bi-box-seam' },
    { label: 'المنتجات', route: '/admin/products', icon: 'bi bi-box-seam', permission: this.permissionsEnum.VIEW_PRODUCTS },
    { label: 'المستخدمين', route: '/admin/users', icon: 'bi bi-people', permission: this.permissionsEnum.VIEW_USERS },
    { label: 'اجهزة المستخدمين', route: '/admin/users-devices', icon: 'bi bi-phone', permission: this.permissionsEnum.VIEW_USERS },
    { label: 'الصلاحيات', route: '/admin/roles', icon: 'bi bi-person-badge', permission: this.permissionsEnum.VIEW_ROLES },
    { label: 'المستهلكين', route: '/admin/customers', icon: 'bi bi-person-fill', permission: this.permissionsEnum.VIEW_CUSTOMERS },
    { label: 'الموردين', route: '/admin/suppliers', icon: 'bi bi-truck', permission: this.permissionsEnum.VIEW_SUPPLIERS },
    { label: 'الطلبات', route: '/admin/order-list', icon: 'bi bi-cart-check', permission: this.permissionsEnum.VIEW_SALE_ORDERS },
    { label: 'الشركة', route: '/admin/company', icon: 'bi bi-building', permission: this.permissionsEnum.VIEW_SETTINGS },
    { label: 'التقارير', route: '/admin/inventory-report', icon: 'bi bi-file-earmark-text', permission: this.permissionsEnum.VIEW_INVENTORY_REPORT }
  ];

}
