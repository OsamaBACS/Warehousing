import { Injectable } from '@angular/core';
import { SidebarConfig, SidebarMenuGroup, SidebarMenuItem } from '../components/sidebar/sidebar.component';
import { PermissionsEnum } from '../../admin/constants/enums/permissions.enum';
import { AuthService } from '../../core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class SidebarConfigService {

  constructor(private authService: AuthService) {}

  getUnifiedConfig(): SidebarConfig {
    const menuGroups: SidebarMenuGroup[] = [
      // Order Management - Always visible to authenticated users
      {
        title: 'ORDER.TITLE',
        items: [
          { 
            label: 'BREADCRUMB.SALE_ORDERS', 
            route: '/app/order/2/categories', 
            icon: 'bi bi-cart-check', 
            permission: PermissionsEnum.VIEW_SALE_ORDERS
          },
          { 
            label: 'BREADCRUMB.PURCHASE', 
            route: '/app/order/1/categories', 
            icon: 'bi bi-cart-plus', 
            permission: PermissionsEnum.VIEW_PURCHASE_ORDERS
          },
          { 
            label: 'BREADCRUMB.PENDING_ORDERS', 
            route: '/app/pending-orders', 
            icon: 'bi bi-list-ul', 
            permission: undefined // Available to all authenticated users
          },
          { 
            label: 'ORDER.CART', 
            route: '/app/cart', 
            icon: 'bi bi-cart-dash',
            permission: undefined // Available to all authenticated users
          }
        ]
      }
    ];

    // Admin sections - only if user has admin permissions
    if (this.authService.hasPermission(PermissionsEnum.VIEW_ADMIN)) {
      menuGroups.push(
        {
          title: 'ADMIN.MENU_GROUPS.MAIN',
          items: [
            { 
              label: 'BREADCRUMB.DASHBOARD', 
              route: '/app/admin/dashboard', 
              icon: 'bi bi-speedometer2', 
              permission: PermissionsEnum.VIEW_ADMIN 
            },
            { 
              label: 'BREADCRUMB.ORDERS', 
              route: '/app/admin/order-list', 
              icon: 'bi bi-list-check', 
              permission: PermissionsEnum.VIEW_SALE_ORDERS 
            }
          ]
        },
        {
          title: 'ADMIN.MENU_GROUPS.PRODUCTS',
          items: [
            { 
              label: 'BREADCRUMB.PRODUCTS', 
              route: '/app/admin/products', 
              icon: 'bi bi-box-seam', 
              permission: PermissionsEnum.VIEW_PRODUCTS 
            },
            { 
              label: 'BREADCRUMB.MODIFIER_MANAGEMENT', 
              route: '/app/admin/modifier-management', 
              icon: 'bi bi-gear', 
              permission: PermissionsEnum.VIEW_PRODUCTS 
            },
            { 
              label: 'BREADCRUMB.CATEGORIES', 
              route: '/app/admin/category', 
              icon: 'bi bi-folder', 
              permission: PermissionsEnum.VIEW_CATEGORIES 
            },
            { 
              label: 'BREADCRUMB.SUBCATEGORIES', 
              route: '/app/admin/sub-category', 
              icon: 'bi bi-folder2-open', 
              permission: PermissionsEnum.VIEW_SUBCATEGORIES 
            },
            { 
              label: 'BREADCRUMB.UNITS', 
              route: '/app/admin/unit', 
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
              route: '/app/admin/inventory-management', 
              icon: 'bi bi-clipboard-data', 
              permission: PermissionsEnum.VIEW_INVENTORY_MANAGEMENT 
            },
            { 
              label: 'BREADCRUMB.INITIAL_STOCK_SETUP', 
              route: '/app/admin/initial-stock-setup', 
              icon: 'bi bi-boxes', 
              permission: PermissionsEnum.MANAGE_INVENTORY 
            },
            { 
              label: 'BREADCRUMB.STORE_TRANSFERS', 
              route: '/app/admin/store-transfers', 
              icon: 'bi bi-arrow-left-right', 
              permission: PermissionsEnum.VIEW_STORE_TRANSFERS 
            },
            { 
              label: 'BREADCRUMB.STORES', 
              route: '/app/admin/store', 
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
              route: '/app/admin/users', 
              icon: 'bi bi-people', 
              permission: PermissionsEnum.VIEW_USERS 
            },
            { 
              label: 'BREADCRUMB.ROLES', 
              route: '/app/admin/roles', 
              icon: 'bi bi-person-badge', 
              permission: PermissionsEnum.VIEW_ROLES 
            },
            { 
              label: 'BREADCRUMB.CUSTOMERS', 
              route: '/app/admin/customers', 
              icon: 'bi bi-person-fill', 
              permission: PermissionsEnum.VIEW_CUSTOMERS 
            },
            { 
              label: 'BREADCRUMB.SUPPLIERS', 
              route: '/app/admin/suppliers', 
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
              route: '/app/admin/company', 
              icon: 'bi bi-building', 
              permission: PermissionsEnum.VIEW_SETTINGS 
            },
            { 
              label: 'BREADCRUMB.ACTIVITY_LOGS', 
              route: '/app/admin/activity-logs', 
              icon: 'bi bi-clock-history', 
              permission: PermissionsEnum.VIEW_ADMIN 
            },
            { 
              label: 'BREADCRUMB.WORKING_HOURS', 
              route: '/app/admin/working-hours', 
              icon: 'bi bi-calendar-check', 
              permission: PermissionsEnum.VIEW_ADMIN 
            },
            { 
              label: 'إعدادات الطابعة', 
              route: '/app/admin/printer-configurations', 
              icon: 'bi bi-printer', 
              permission: PermissionsEnum.VIEW_PRINTER_CONFIGURATIONS 
            }
          ]
        },
        {
          title: 'REPORTS.TITLE',
          items: [
            { 
              label: 'REPORTS.INVENTORY_REPORT', 
              route: '/app/admin/inventory-report', 
              icon: 'bi bi-file-earmark-text', 
              permission: PermissionsEnum.VIEW_INVENTORY_REPORT 
            },
            { 
              label: 'REPORTS.SALES_PURCHASE_REPORT', 
              route: '/app/admin/order-report', 
              icon: 'bi bi-graph-up', 
              permission: PermissionsEnum.VIEW_INVENTORY_REPORT 
            }
          ]
        }
      );
    }

    return {
      title: 'HOME.TITLE',
      titleIcon: '', // Empty - no icon in header
      headerGradient: 'linear-gradient(135deg, #2563eb 0%, #4f46e5 50%, #7c3aed 100%)',
      activeGradient: 'linear-gradient(135deg, #3b82f6 0%, #6366f1 100%)',
      menuGroups: menuGroups,
      quickActions: [],
      showCart: true,
      showLanguageToggle: true,
      showLogout: true
    };
  }

  // Keep old methods for backward compatibility (but redirect to unified)
  getAdminConfig(): SidebarConfig {
    return this.getUnifiedConfig();
  }

  getOrderConfig(): SidebarConfig {
    return this.getUnifiedConfig();
  }
}

