import { Injectable } from '@angular/core';
import { SidebarConfig, SidebarMenuGroup, SidebarMenuItem } from '../components/sidebar/sidebar.component';
import { PermissionsEnum } from '../../admin/constants/enums/permissions.enum';
import { AuthService } from '../../core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class SidebarConfigService {

  constructor(private authService: AuthService) {}

  getAdminConfig(): SidebarConfig {
    return {
      title: 'ADMIN.TITLE',
      titleIcon: 'bi bi-shield-check',
      headerGradient: 'linear-gradient(135deg, #2563eb 0%, #4f46e5 50%, #7c3aed 100%)',
      activeGradient: 'linear-gradient(135deg, #3b82f6 0%, #6366f1 100%)',
      menuGroups: [
        {
          title: 'ADMIN.MENU_GROUPS.MAIN',
          items: [
            { 
              label: 'BREADCRUMB.DASHBOARD', 
              route: '/admin/dashboard', 
              icon: 'bi bi-speedometer2', 
              permission: PermissionsEnum.VIEW_ADMIN 
            },
            { 
              label: 'BREADCRUMB.ORDERS', 
              route: '/admin/order-list', 
              icon: 'bi bi-cart-check', 
              permission: PermissionsEnum.VIEW_SALE_ORDERS 
            },
            { 
              label: 'BREADCRUMB.PURCHASE', 
              route: '/order/1/categories', 
              icon: 'bi bi-cart-plus', 
              permission: PermissionsEnum.VIEW_PURCHASE_ORDERS 
            }
          ]
        },
        {
          title: 'ADMIN.MENU_GROUPS.PRODUCTS',
          items: [
            { 
              label: 'BREADCRUMB.PRODUCTS', 
              route: '/admin/products', 
              icon: 'bi bi-box-seam', 
              permission: PermissionsEnum.VIEW_PRODUCTS 
            },
            { 
              label: 'BREADCRUMB.MODIFIER_MANAGEMENT', 
              route: '/admin/modifier-management', 
              icon: 'bi bi-gear', 
              permission: PermissionsEnum.VIEW_PRODUCTS 
            },
            { 
              label: 'BREADCRUMB.CATEGORIES', 
              route: '/admin/category', 
              icon: 'bi bi-folder', 
              permission: PermissionsEnum.VIEW_CATEGORIES 
            },
            { 
              label: 'BREADCRUMB.SUBCATEGORIES', 
              route: '/admin/sub-category', 
              icon: 'bi bi-folder2-open', 
              permission: PermissionsEnum.VIEW_SUBCATEGORIES 
            },
            { 
              label: 'BREADCRUMB.UNITS', 
              route: '/admin/unit', 
              icon: 'bi bi-rulers', 
              permission: PermissionsEnum.VIEW_UNITS 
            }
          ]
        },
        {
          title: 'ADMIN.MENU_GROUPS.INVENTORY',
          items: [
            { 
              label: 'BREADCRUMB.INVENTORY', 
              route: '/admin/inventory-management', 
              icon: 'bi bi-clipboard-data', 
              permission: PermissionsEnum.VIEW_INVENTORY_MANAGEMENT 
            },
            { 
              label: 'BREADCRUMB.INITIAL_STOCK_SETUP', 
              route: '/admin/initial-stock-setup', 
              icon: 'bi bi-boxes', 
              permission: PermissionsEnum.MANAGE_INVENTORY 
            },
            { 
              label: 'BREADCRUMB.STORE_TRANSFERS', 
              route: '/admin/store-transfers', 
              icon: 'bi bi-arrow-left-right', 
              permission: PermissionsEnum.VIEW_STORE_TRANSFERS 
            },
            { 
              label: 'BREADCRUMB.STORES', 
              route: '/admin/store', 
              icon: 'bi bi-building', 
              permission: PermissionsEnum.VIEW_STORES 
            }
          ]
        },
        {
          title: 'ADMIN.MENU_GROUPS.PEOPLE',
          items: [
            { 
              label: 'BREADCRUMB.USERS', 
              route: '/admin/users', 
              icon: 'bi bi-people', 
              permission: PermissionsEnum.VIEW_USERS 
            },
            { 
              label: 'BREADCRUMB.ROLES', 
              route: '/admin/roles', 
              icon: 'bi bi-person-badge', 
              permission: PermissionsEnum.VIEW_ROLES 
            },
            { 
              label: 'BREADCRUMB.CUSTOMERS', 
              route: '/admin/customers', 
              icon: 'bi bi-person-fill', 
              permission: PermissionsEnum.VIEW_CUSTOMERS 
            },
            { 
              label: 'BREADCRUMB.SUPPLIERS', 
              route: '/admin/suppliers', 
              icon: 'bi bi-truck', 
              permission: PermissionsEnum.VIEW_SUPPLIERS 
            }
          ]
        },
        {
          title: 'ADMIN.MENU_GROUPS.SYSTEM',
          items: [
            { 
              label: 'BREADCRUMB.COMPANIES', 
              route: '/admin/company', 
              icon: 'bi bi-building', 
              permission: PermissionsEnum.VIEW_SETTINGS 
            },
            { 
              label: 'BREADCRUMB.ACTIVITY_LOGS', 
              route: '/admin/activity-logs', 
              icon: 'bi bi-clock-history', 
              permission: PermissionsEnum.VIEW_ADMIN 
            },
            { 
              label: 'BREADCRUMB.WORKING_HOURS', 
              route: '/admin/working-hours', 
              icon: 'bi bi-calendar-check', 
              permission: PermissionsEnum.VIEW_ADMIN 
            }
          ]
        },
        {
          title: 'REPORTS.TITLE',
          items: [
            { 
              label: 'REPORTS.INVENTORY_REPORT', 
              route: '/admin/inventory-report', 
              icon: 'bi bi-file-earmark-text', 
              permission: PermissionsEnum.VIEW_INVENTORY_REPORT 
            }
          ]
        }
      ],
      quickActions: [
        { 
          label: 'BREADCRUMB.ORDERS', 
          route: '/order/2/categories', 
          icon: 'bi bi-cart', 
        }
      ],
      showCart: false,
      showLanguageToggle: true,
      showLogout: true
    };
  }

  getOrderConfig(): SidebarConfig {
    const quickActions: SidebarMenuItem[] = [];
    
    if (this.authService.isAdmin) {
      quickActions.push(
        { 
          label: 'BREADCRUMB.DASHBOARD', 
          route: '/admin/dashboard', 
          icon: 'bi bi-speedometer2'
        },
        { 
          label: 'BREADCRUMB.ORDERS', 
          route: '/admin/order-list', 
          icon: 'bi bi-list-check',
          permission: PermissionsEnum.VIEW_SALE_ORDERS
        }
      );
    }

    return {
      title: 'ORDER.TITLE',
      titleIcon: 'bi bi-cart-check',
      headerGradient: 'linear-gradient(135deg, #059669 0%, #0d9488 50%, #0891b2 100%)',
      activeGradient: 'linear-gradient(135deg, #10b981 0%, #14b8a6 100%)',
      menuGroups: [
        {
          items: [
            { 
              label: 'COMMON.SALE_ORDERS', 
              route: '/order/2/categories', 
              icon: 'bi bi-cart-check', 
              permission: PermissionsEnum.VIEW_SALE_ORDERS
            },
            { 
              label: 'BREADCRUMB.PURCHASE', 
              route: '/order/1/categories', 
              icon: 'bi bi-cart-plus', 
              permission: PermissionsEnum.VIEW_PURCHASE_ORDERS
            },
            { 
              label: 'BREADCRUMB.PENDING_ORDERS', 
              route: '/pending-orders', 
              icon: 'bi bi-list-ul', 
              permission: undefined // Available to all authenticated users
            },
            { 
              label: 'ORDER.CART', 
              route: '/cart', 
              icon: 'bi bi-cart-dash',
              permission: undefined
            }
          ]
        }
      ],
      quickActions: quickActions,
      showCart: true,
      showLanguageToggle: true,
      showLogout: true
    };
  }
}

