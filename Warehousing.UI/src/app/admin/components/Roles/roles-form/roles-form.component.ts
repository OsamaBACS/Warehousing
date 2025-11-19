import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Permission } from '../../../models/permission';
import { Category } from '../../../models/category';
import { Product } from '../../../models/product';
import { SubCategory } from '../../../models/SubCategory';
import { RoleService } from '../../../services/role.service';
import { PermissionService } from '../../../services/permission.service';
import { PrinterConfigurationService, PrinterConfigurationDto } from '../../../services/printer-configuration.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';
import { RoleDtoForAdd } from '../../../models/role';

@Component({
  selector: 'app-roles-form',
  standalone: false,
  templateUrl: './roles-form.component.html',
  styleUrl: './roles-form.component.scss'
})
export class RolesFormComponent implements OnInit {

  roleForm!: FormGroup;
  permissions!: Permission[];
  categories!: Category[];
  products!: Product[];
  subCategories!: SubCategory[];
  groupedPermissions!: { [key: string]: Permission[] };
  printerConfigurations: PrinterConfigurationDto[] = [];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private permissionService: PermissionService,
    private printerConfigService: PrinterConfigurationService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService
  ) {
    this.categories = this.route.snapshot.data['categoriesResolver'];
    this.products = this.route.snapshot.data['productsResolver'];
    this.permissions = this.route.snapshot.data['permissionsResolver'];
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
  }

  ngOnInit(): void {
    // Debug: Check if permissions are loaded
    if (this.permissions) {
      const printerPerms = this.permissions.filter(p => p.code?.includes('PRINTER'));
    }
    this.groupPermissions();
    this.loadPrinterConfigurations();
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const roleId = +params['roleId'];
      if (roleId) {
        this.roleService.GetRoleById(roleId)
          .subscribe({
            next: (res) => {
              const roleDtoForAdd: RoleDtoForAdd = {
                id: res.id,
                code: res.code,
                nameAr: res.nameAr,
                nameEn: res.nameEn,
                rolePermissionIds: res?.permissions.map(p => p.permissionId),
                categoryIds: res?.categoryIds || [],
                productIds: res?.productIds || [],
                subCategoryIds: res?.subCategoryIds || [],
                printerConfigurationId: res?.printerConfigurationId
              };
              this.initializingForm(roleDtoForAdd)
            },
            error: (err) => {
            }
          });
      }
    });
  }

  loadPrinterConfigurations(): void {
    this.printerConfigService.GetPrinterConfigurations().subscribe({
      next: (configs) => {
        this.printerConfigurations = configs;
      },
      error: (err) => {
      }
    });
  }

  groupPermissions(): void {
    this.groupedPermissions = {};
    
    if (!this.permissions || this.permissions.length === 0) {
      return;
    }
    
    this.permissions.forEach(permission => {
      if (!permission.code) {
        return;
      }
      // Debug: Log printer-related permissions
      if (permission.code.includes('PRINTER') || permission.code.includes('printer')) {
      }
      let module = this.getModuleFromCode(permission.code);
      if (!this.groupedPermissions[module]) {
        this.groupedPermissions[module] = [];
      }
      this.groupedPermissions[module].push(permission);
    });
    
    // Debug: Check if printer config module was created
    if (this.groupedPermissions['admin/printer-configurations']) {
    } else {
    }
  }

  getModuleFromCode(code: string): string {
    // Group permissions by admin pages/modules based on actual admin routes
    
    // Dashboard & Main (admin/main)
    if (code === 'VIEW_ADMIN') return 'admin/main';
    
    // Products Management (admin/products)
    if (code === 'VIEW_PRODUCTS' || code === 'ADD_PRODUCT' || code === 'EDIT_PRODUCT' || 
        code === 'DELETE_PRODUCT' || code === 'PRINT_PRODUCTS') return 'admin/products';
    
    // Customers Management (admin/customers)
    if (code === 'VIEW_CUSTOMERS' || code === 'ADD_CUSTOMER' || code === 'EDIT_CUSTOMER' || 
        code === 'DELETE_CUSTOMER' || code === 'PRINT_CUSTOMERS') return 'admin/customers';
    
    // Suppliers Management (admin/suppliers)
    if (code === 'VIEW_SUPPLIERS' || code === 'ADD_SUPPLIER' || code === 'EDIT_SUPPLIER' || 
        code === 'DELETE_SUPPLIER' || code === 'PRINT_SUPPLIERS') return 'admin/suppliers';
    
    // Orders Management (admin/orders) - includes both purchase and sale orders
    if (code === 'VIEW_PURCHASE_ORDERS' || code === 'ADD_PURCHASE_ORDER' || code === 'EDIT_PURCHASE_ORDER' || 
        code === 'DELETE_PURCHASE_ORDER' || code === 'COMPLETE_PURCHASE_ORDER' || code === 'PRINT_PURCHASE_ORDER' ||
        code === 'APPROVE_PURCHASE_ORDER' || code === 'CANCEL_PURCHASE_ORDER' ||
        code === 'VIEW_SALE_ORDERS' || code === 'ADD_SALE_ORDER' || code === 'EDIT_SALE_ORDER' || 
        code === 'DELETE_SALE_ORDER' || code === 'COMPLETE_SALE_ORDER' || code === 'PRINT_SALE_ORDER' ||
        code === 'APPROVE_SALE_ORDER' || code === 'CANCEL_SALE_ORDER') return 'admin/orders';
    
    // Inventory Management (admin/inventory)
    if (code === 'VIEW_INVENTORY_MANAGEMENT' || code === 'MANAGE_INVENTORY' || code === 'ADJUST_INVENTORY' || 
        code === 'VIEW_LOW_STOCK' || code === 'VIEW_INVENTORY_REPORT' || code === 'PRINT_INVENTORY_REPORT') return 'admin/inventory';
    
    // Stores Management (admin/stores) - includes store transfers
    if (code === 'VIEW_STORES' || code === 'ADD_STORE' || code === 'EDIT_STORE' || code === 'DELETE_STORE' || 
        code === 'PRINT_STORES' || code === 'VIEW_STORE_TRANSFERS' || code === 'ADD_STORE_TRANSFER' || 
        code === 'EDIT_STORE_TRANSFER' || code === 'DELETE_STORE_TRANSFER' || code === 'APPROVE_STORE_TRANSFER' || 
        code === 'PRINT_STORE_TRANSFERS') return 'admin/stores';
    
    // Users Management (admin/users)
    if (code === 'VIEW_USERS' || code === 'ADD_USER' || code === 'EDIT_USER' || code === 'DELETE_USER' || 
        code === 'RESET_USER_PASSWORD' || code === 'ASSIGN_ROLES_TO_USER') return 'admin/users';
    
    // Roles Management (admin/roles)
    if (code === 'VIEW_ROLES' || code === 'ADD_ROLE' || code === 'EDIT_ROLE' || code === 'DELETE_ROLE' || 
        code === 'ASSIGN_PERMISSIONS_TO_ROLE') return 'admin/roles';
    
    // Categories Management (admin/categories)
    if (code === 'VIEW_CATEGORIES' || code === 'ADD_CATEGORY' || code === 'EDIT_CATEGORY' || 
        code === 'DELETE_CATEGORY' || code === 'PRINT_CATEGORIES') return 'admin/categories';
    
    // SubCategories Management (admin/subcategories)
    if (code === 'VIEW_SUBCATEGORIES' || code === 'ADD_SUBCATEGORY' || code === 'EDIT_SUBCATEGORY' || 
        code === 'DELETE_SUBCATEGORY' || code === 'PRINT_SUBCATEGORIES') return 'admin/subcategories';
    
    // Units Management (admin/units)
    if (code === 'VIEW_UNITS' || code === 'ADD_UNIT' || code === 'EDIT_UNIT' || 
        code === 'DELETE_UNIT' || code === 'PRINT_UNITS') return 'admin/units';
    
    // Company Settings (admin/company)
    if (code === 'VIEW_SETTINGS' || code === 'EDIT_SETTINGS') return 'admin/company';
    
    // Activity Logs (admin/activity-logs)
    if (code === 'VIEW_ACTIVITY_LOGS' || code === 'EXPORT_ACTIVITY_LOGS') return 'admin/activity-logs';
    
    // Working Hours (admin/working-hours)
    if (code === 'VIEW_WORKING_HOURS' || code === 'EDIT_WORKING_HOURS' || code === 'MANAGE_WORKING_HOURS_EXCEPTIONS') return 'admin/working-hours';
    
    // Printer Configuration Management (admin/printer-configurations)
    if (code === 'VIEW_PRINTER_CONFIGURATIONS' || 
        code === 'ADD_PRINTER_CONFIGURATION' || 
        code === 'EDIT_PRINTER_CONFIGURATION' || 
        code === 'DELETE_PRINTER_CONFIGURATION' ||
        code?.toUpperCase() === 'VIEW_PRINTER_CONFIGURATIONS' ||
        code?.toUpperCase() === 'ADD_PRINTER_CONFIGURATION' ||
        code?.toUpperCase() === 'EDIT_PRINTER_CONFIGURATION' ||
        code?.toUpperCase() === 'DELETE_PRINTER_CONFIGURATION') {
      return 'admin/printer-configurations';
    }
    
    // Additional permissions that don't fit specific pages
    if (code === 'EDIT_APPROVED_INVOICE') return 'admin/orders'; // Invoice editing is part of orders
    
    return 'admin/general';
  }

  getModuleDisplayName(module: string): string {
    const moduleNames: { [key: string]: string } = {
      'admin/main': 'لوحة التحكم الرئيسية',
      'admin/products': 'إدارة المنتجات',
      'admin/customers': 'إدارة العملاء',
      'admin/suppliers': 'إدارة الموردين',
      'admin/orders': 'إدارة الطلبات',
      'admin/inventory': 'إدارة المخزون',
      'admin/stores': 'إدارة المخازن',
      'admin/users': 'إدارة المستخدمين',
      'admin/roles': 'إدارة الأدوار',
      'admin/categories': 'إدارة التصنيفات',
      'admin/subcategories': 'إدارة التصنيفات الفرعية',
      'admin/units': 'إدارة الوحدات',
      'admin/company': 'إعدادات الشركة',
      'admin/activity-logs': 'سجل الأنشطة',
      'admin/working-hours': 'ساعات العمل',
      'admin/printer-configurations': 'إعدادات الطابعة',
      'admin/general': 'عام'
    };
    
    return moduleNames[module] || module;
  }

  getObjectKeys(obj: any): string[] {
    return Object.keys(obj);
  }

  initializingForm(role: RoleDtoForAdd | null) {
    this.roleForm = this.fb.group({
      id: [role?.id || 0],
      code: [role?.code || '', Validators.required],
      nameEn: [role?.nameEn || ''],
      nameAr: [role?.nameAr || '', Validators.required],
      rolePermissionIds: [role?.rolePermissionIds || []],
      categoryIds: [role?.categoryIds || []],
      productIds: [role?.productIds || []],
      subCategoryIds: [role?.subCategoryIds || []],
      printerConfigurationId: [role?.printerConfigurationId || null]
    });
  }

  onPermissionChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentPerms = this.roleForm.get('rolePermissionIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('rolePermissionIds')?.setValue([...currentPerms, id]);
    } else {
      this.roleForm.get('rolePermissionIds')?.setValue(currentPerms.filter((c: number) => c !== id));
    }
  }

  onCategoryChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentCats = this.roleForm.get('categoryIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('categoryIds')?.setValue([...currentCats, id]);
    } else {
      this.roleForm.get('categoryIds')?.setValue(currentCats.filter((c: number) => c !== id));
    }
  }

  onProductChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentProducts = this.roleForm.get('productIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('productIds')?.setValue([...currentProducts, id]);
    } else {
      this.roleForm.get('productIds')?.setValue(currentProducts.filter((p: number) => p !== id));
    }
  }

  onSubCategoryChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentSubCats = this.roleForm.get('subCategoryIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('subCategoryIds')?.setValue([...currentSubCats, id]);
    } else {
      this.roleForm.get('subCategoryIds')?.setValue(currentSubCats.filter((sc: number) => sc !== id));
    }
  }

  // Select All methods
  selectAllPermissions(): void {
    const allPermissionIds = this.permissions.map(p => p.id);
    this.roleForm.get('rolePermissionIds')?.setValue(allPermissionIds);
  }

  deselectAllPermissions(): void {
    this.roleForm.get('rolePermissionIds')?.setValue([]);
  }

  selectAllPermissionsInModule(module: string) {
    const currentPerms = this.roleForm.get('rolePermissionIds')?.value || [];
    const modulePerms = this.groupedPermissions[module]?.map(p => p.id) || [];
    const newPerms = [...new Set([...currentPerms, ...modulePerms])];
    this.roleForm.get('rolePermissionIds')?.setValue(newPerms);
  }

  deselectAllPermissionsInModule(module: string) {
    const currentPerms = this.roleForm.get('rolePermissionIds')?.value || [];
    const modulePerms = this.groupedPermissions[module]?.map(p => p.id) || [];
    const newPerms = currentPerms.filter((id: number) => !modulePerms.includes(id));
    this.roleForm.get('rolePermissionIds')?.setValue(newPerms);
  }

  isModuleFullySelected(module: string): boolean {
    const currentPerms = this.roleForm.get('rolePermissionIds')?.value || [];
    const modulePerms = this.groupedPermissions[module]?.map(p => p.id) || [];
    return modulePerms.every((id: number) => currentPerms.includes(id));
  }

  isModulePartiallySelected(module: string): boolean {
    const currentPerms = this.roleForm.get('rolePermissionIds')?.value || [];
    const modulePerms = this.groupedPermissions[module]?.map(p => p.id) || [];
    const selectedInModule = modulePerms.filter((id: number) => currentPerms.includes(id));
    return selectedInModule.length > 0 && selectedInModule.length < modulePerms.length;
  }

  selectAllCategories(): void {
    const allCategoryIds = this.categories.map(c => c.id);
    this.roleForm.get('categoryIds')?.setValue(allCategoryIds);
  }

  deselectAllCategories(): void {
    this.roleForm.get('categoryIds')?.setValue([]);
  }

  selectAllSubCategories(): void {
    const allSubCategoryIds = this.subCategories.map(sc => sc.id);
    this.roleForm.get('subCategoryIds')?.setValue(allSubCategoryIds);
  }

  deselectAllSubCategories(): void {
    this.roleForm.get('subCategoryIds')?.setValue([]);
  }

  selectAllProducts(): void {
    const allProductIds = this.products.map(p => p.id);
    this.roleForm.get('productIds')?.setValue(allProductIds);
  }

  deselectAllProducts(): void {
    this.roleForm.get('productIds')?.setValue([]);
  }

  save(): void {
    if (this.roleForm.valid) {
      // Map permission IDs to permission codes
      const permissionCodes = this.permissions
        .filter(p => this.roleForm.get('rolePermissionIds')?.value.includes(p.id))
        .map(p => p.code);

      const roleData = {
        id: this.roleForm.get('id')?.value,
        code: this.roleForm.get('code')?.value,
        nameEn: this.roleForm.get('nameEn')?.value,
        nameAr: this.roleForm.get('nameAr')?.value,
        permissionCodes: permissionCodes,
        categoryIds: this.roleForm.get('categoryIds')?.value || [],
        productIds: this.roleForm.get('productIds')?.value || [],
        subCategoryIds: this.roleForm.get('subCategoryIds')?.value || [],
        printerConfigurationId: this.roleForm.get('printerConfigurationId')?.value || null
      };

      this.roleService.saveRole(roleData).subscribe({
        next: (res) => {
          if (res) {
            this.toastr.success('Successfully saved', 'Roles');
            this.router.navigate(['../roles'], { relativeTo: this.route })
          }
          else {
            this.toastr.error('Error while saving', 'Role')
          }
        },
        error: (err) => {
          this.toastr.error(err.error, 'Role')
        }
      });
    }
    else {
      this.toastr.error('Please fill all required fields!', 'Roles');
    }
  }

  cancel(): void {
    this.router.navigate(['../roles'], { relativeTo: this.route })
  }
}
