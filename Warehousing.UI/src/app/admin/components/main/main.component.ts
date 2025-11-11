import { Component, OnInit } from '@angular/core';
import { PermissionsEnum } from '../../constants/enums/permissions.enum';
import { ThemeService } from '../../../shared/services/theme.service';
import { AuthService } from '../../../core/services/auth.service';
import { AdminBreadcrumbService } from '../../services/admin-breadcrumb.service';

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
    private authService: AuthService,
    private adminBreadcrumbService: AdminBreadcrumbService
  ) { }

  ngOnInit() {
    this.themeService.isDarkMode$.subscribe(mode => {
      this.isDarkMode = mode;
    });
    
    // Set breadcrumbs for admin main page
    this.adminBreadcrumbService.setDashboardBreadcrumbs();
  }

  toggleDarkMode() { }

  hasAnyPermission(...perms: string[]): boolean {
    return perms.some(p => this.authService.hasPermission(p));
  }

  dashboardCards = [
    { label: 'BREADCRUMB.DASHBOARD', route: '/admin/dashboard', icon: 'bi bi-speedometer2' },
    { label: 'BREADCRUMB.CATEGORIES', route: '/admin/category', icon: 'bi bi-box-seam', permission: this.permissionsEnum.VIEW_CATEGORIES },
    { label: 'BREADCRUMB.SUBCATEGORIES', route: '/admin/sub-category', icon: 'bi bi-diagram-3', permission: this.permissionsEnum.VIEW_SUBCATEGORIES },
    { label: 'BREADCRUMB.UNITS', route: '/admin/unit', icon: 'bi bi-rulers', permission: this.permissionsEnum.VIEW_UNITS },
    { label: 'BREADCRUMB.STORES', route: '/admin/store', icon: 'bi bi-building', permission: this.permissionsEnum.VIEW_STORES },
    { label: 'BREADCRUMB.PRODUCTS', route: '/admin/products', icon: 'bi bi-box-seam', permission: this.permissionsEnum.VIEW_PRODUCTS },
    { label: 'BREADCRUMB.MODIFIER_MANAGEMENT', route: '/admin/modifier-management', icon: 'bi bi-gear', permission: this.permissionsEnum.VIEW_PRODUCTS },
    { label: 'BREADCRUMB.STORE_TRANSFERS', route: '/admin/store-transfers', icon: 'bi bi-arrow-left-right', permission: this.permissionsEnum.VIEW_STORE_TRANSFERS },
    { label: 'BREADCRUMB.INVENTORY', route: '/admin/inventory-management', icon: 'bi bi-clipboard-data', permission: this.permissionsEnum.VIEW_INVENTORY_MANAGEMENT },
    { label: 'BREADCRUMB.INITIAL_STOCK_SETUP', route: '/admin/initial-stock-setup', icon: 'bi bi-boxes', permission: this.permissionsEnum.MANAGE_INVENTORY },
    { label: 'BREADCRUMB.INITIAL_STOCK', route: '/admin/initial-stock', icon: 'bi bi-box-seam', permission: this.permissionsEnum.MANAGE_INVENTORY },
    { label: 'BREADCRUMB.USERS', route: '/admin/users', icon: 'bi bi-people', permission: this.permissionsEnum.VIEW_USERS },
    { label: 'BREADCRUMB.USER_DEVICES', route: '/admin/users-devices', icon: 'bi bi-phone', permission: this.permissionsEnum.VIEW_USERS },
    { label: 'BREADCRUMB.ROLES', route: '/admin/roles', icon: 'bi bi-person-badge', permission: this.permissionsEnum.VIEW_ROLES },
    { label: 'BREADCRUMB.CUSTOMERS', route: '/admin/customers', icon: 'bi bi-person-fill', permission: this.permissionsEnum.VIEW_CUSTOMERS },
    { label: 'BREADCRUMB.SUPPLIERS', route: '/admin/suppliers', icon: 'bi bi-truck', permission: this.permissionsEnum.VIEW_SUPPLIERS },
    { label: 'BREADCRUMB.PURCHASE', route: '/order/1/categories', icon: 'bi bi-cart-plus', permission: this.permissionsEnum.VIEW_PURCHASE_ORDERS },
    { label: 'BREADCRUMB.ORDERS', route: '/admin/order-list', icon: 'bi bi-cart-check', permission: this.permissionsEnum.VIEW_SALE_ORDERS },
    { label: 'BREADCRUMB.COMPANIES', route: '/admin/company', icon: 'bi bi-building', permission: this.permissionsEnum.VIEW_SETTINGS },
    { label: 'BREADCRUMB.ACTIVITY_LOGS', route: '/admin/activity-logs', icon: 'bi bi-clock-history', permission: this.permissionsEnum.VIEW_ADMIN },
    { label: 'BREADCRUMB.WORKING_HOURS', route: '/admin/working-hours', icon: 'bi bi-calendar-check', permission: this.permissionsEnum.VIEW_ADMIN },
    { label: 'إعدادات الطابعة', route: '/admin/printer-configurations', icon: 'bi bi-printer', permission: this.permissionsEnum.VIEW_PRINTER_CONFIGURATIONS }
  ];

  reportCards = [
    { label: 'REPORTS.INVENTORY_REPORT', route: '/admin/inventory-report', icon: 'bi bi-file-earmark-text', permission: this.permissionsEnum.VIEW_INVENTORY_REPORT }
  ];

}
