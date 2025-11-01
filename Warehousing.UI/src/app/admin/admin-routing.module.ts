import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { AuthGuard } from "../core/guards/auth-guard.guard";
import { PermissionGuard } from "../core/guards/permission-guard.guard";
import { AdminComponent } from "./admin.component";
import { CategoryFormComponent } from "./components/Categories/category-form/category-form.component";
import { CategoryComponent } from "./components/Categories/category/category.component";
import { CompanyFormComponent } from "./components/Companies/company-form.component/company-form.component";
import { CompanyComponent } from "./components/Companies/company.component/company.component";
import { CustomersFormComponent } from "./components/Customers/customers-form.component/customers-form.component";
import { CustomersComponent } from "./components/Customers/customers.component/customers.component";
import { Dashboard } from "./components/dashboard/dashboard";
import { MainComponent } from "./components/main/main.component";
import { ProductFormComponent } from "./components/Products/product-form/product-form.component";
import { InventoryReportComponent } from "./components/reports/inventory-report.component/inventory-report.component";
import { TransactionsComponent } from "./components/reports/transactions.component/transactions.component";
import { RolesFormComponent } from "./components/Roles/roles-form/roles-form.component";
import { RolesComponent } from "./components/Roles/roles/roles.component";
import { StoreFormComponent } from "./components/Stores/store-form/store-form.component";
import { StoreComponent } from "./components/Stores/store/store.component";
import { SuppliersFormComponent } from "./components/Suppliers/suppliers-form.component/suppliers-form.component";
import { SuppliersComponent } from "./components/Suppliers/suppliers.component/suppliers.component";
import { UnitFormComponent } from "./components/Units/unit-form/unit-form.component";
import { UnitComponent } from "./components/Units/unit/unit.component";
import { UsersFormComponent } from "./components/Users/users-form/users-form.component";
import { UsersComponent } from "./components/Users/users/users.component";
import { PermissionsEnum } from "./constants/enums/permissions.enum";
import { CategoriesResolver } from "./resolvers/categories-resolver";
import { CompaniesResolver } from "./resolvers/companies-resolver";
import { customersResolver } from "./resolvers/customers-resolver";
import { PermissionsResolver } from "./resolvers/permissions-resolver";
import { ProductsResolver } from "./resolvers/products-resolver";
import { StatusesResolver } from "./resolvers/statuses-resolver";
import { StoresResolver } from "./resolvers/stores-resolver-resolver";
import { SuppliersResolver } from "./resolvers/suppliers-resolver";
import { UnitsResolver } from "./resolvers/units-resolver";
import { ProductsComponent } from "./components/Products/products/products.component";
import { SubCategoryComponent } from "./components/SubCategories/sub-category/sub-category.component";
import { SubCategoryFormComponent } from "./components/SubCategories/sub-category-form/sub-category-form.component";
import { SubCategoriesResolver } from "./resolvers/sub-categories-resolver";
import { UsersListResolver } from "./resolvers/users-list-resolver";
import { OrderListComponent } from "./components/Orders/order-list/order-list.component";
import { OrderItemsListComponent } from "./components/Orders/order-items-list/order-items-list.component";
import { StoreTransferFormComponent } from "./components/StoreTransfers/store-transfer-form/store-transfer-form.component";
import { InventoryManagementComponent } from "./components/Inventory/inventory-management/inventory-management.component";
import { InitialStockSetupComponent } from "./components/Inventory/initial-stock-setup/initial-stock-setup.component";
import { InitialStockComponent } from "./components/initial-stock/initial-stock.component";
import { ModifierManagementComponent } from "./components/Products/modifier-management/modifier-management.component";
import { ActivityLogsComponent } from "./components/ActivityLogs/activity-logs.component";
import { WorkingHoursComponent } from "./components/WorkingHours/working-hours.component";


const routes: Routes = [
  {
    path: '',
    component: AdminComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'main', pathMatch: 'full' },
      { 
        path: 'main', 
        component: MainComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_ADMIN] }
      },
      { 
        path: 'dashboard', 
        component: Dashboard,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_ADMIN] }
      },
      {
        path: 'products',
        component: ProductsComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_PRODUCTS, PermissionsEnum.VIEW_PRODUCTS] },
        resolve: {
          StoresResolver: StoresResolver
        }
      },
      {
        path: 'product-form',
        component: ProductFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.EDIT_PRODUCT] },
        resolve: {
          StoresResolver: StoresResolver,
          subCategoriesResolver: SubCategoriesResolver,
        }
      },
      {
        path: 'modifier-management',
        component: ModifierManagementComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_PRODUCTS, PermissionsEnum.EDIT_PRODUCT] }
      },
      { 
        path: 'users', component: UsersComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_USERS, PermissionsEnum.ADD_USER] }
      },
      {
        path: 'users-form',
        component: UsersFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.EDIT_USER] },
        resolve: {
          StoresResolver: StoresResolver
        }
      },
      {
        path: 'roles',
        component: RolesComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_ROLES, PermissionsEnum.ADD_ROLE, PermissionsEnum.EDIT_ROLE] },
      },
      {
        path: 'roles-form',
        component: RolesFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_ROLES, PermissionsEnum.ADD_ROLE] },
        resolve: {
          categoriesResolver: CategoriesResolver,
          productsResolver: ProductsResolver,
          permissionsResolver: PermissionsResolver,
          subCategoriesResolver: SubCategoriesResolver
        },
      },
      // { path: 'permissions', component: Permissions },
      { 
        path: 'customers', component: CustomersComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_CUSTOMERS, PermissionsEnum.ADD_CUSTOMER] }
      },
      {
        path: 'customers-form',
        component: CustomersFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.EDIT_CUSTOMER] }
      },
      { 
        path: 'suppliers', component: SuppliersComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_SUPPLIERS, PermissionsEnum.ADD_SUPPLIER] }
      },
      {
        path: 'suppliers-form',
        component: SuppliersFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.EDIT_SUPPLIER] }
      },
      { 
        path: 'order-list', 
        component: OrderListComponent,
        resolve: {
          productsResolver: ProductsResolver,
          statusesResolver: StatusesResolver,
          suppliersResolver: SuppliersResolver,
        },
        canActivate: [PermissionGuard],
        data: { 
          permission: [
            PermissionsEnum.VIEW_PURCHASE_ORDERS, 
            PermissionsEnum.ADD_PURCHASE_ORDER, 
            PermissionsEnum.EDIT_PURCHASE_ORDER, 
            PermissionsEnum.PRINT_PURCHASE_ORDER,
            PermissionsEnum.VIEW_SALE_ORDERS, 
            PermissionsEnum.ADD_SALE_ORDER, 
            PermissionsEnum.EDIT_SALE_ORDER, 
            PermissionsEnum.PRINT_SALE_ORDER,
          ] 
        }
      },
      { 
        path: 'order-items-list', 
        component: OrderItemsListComponent,
        resolve: {
          subCategoriesResolver: SubCategoriesResolver,
          productsResolver: ProductsResolver,
          statusesResolver: StatusesResolver,
          suppliersResolver: SuppliersResolver,
          customersRsolver: customersResolver,
          unitsResolver: UnitsResolver
        },
        canActivate: [PermissionGuard],
        data: { 
          permission: [
            PermissionsEnum.ADD_PURCHASE_ORDER, 
            PermissionsEnum.EDIT_PURCHASE_ORDER, 
            PermissionsEnum.PRINT_PURCHASE_ORDER,
            PermissionsEnum.ADD_SALE_ORDER, 
            PermissionsEnum.EDIT_SALE_ORDER, 
            PermissionsEnum.PRINT_SALE_ORDER,
          ] 
        }
      },
      { 
        path: 'company', component: CompanyComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_SETTINGS, PermissionsEnum.EDIT_SETTINGS] }
      },
      {
        path: 'company-form',
        component: CompanyFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_SETTINGS, PermissionsEnum.EDIT_SETTINGS] }
      },
      {
        path: 'inventory-report',
        component: InventoryReportComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_INVENTORY_REPORT, PermissionsEnum.PRINT_INVENTORY_REPORT] },
        resolve: {
          StoresResolver: StoresResolver
        }
      },
      { 
        path: 'transaction/:id', 
        component: TransactionsComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_INVENTORY_REPORT] }
      },
      { 
        path: 'store', 
        component: StoreComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.ADD_STORE, PermissionsEnum.EDIT_STORE, PermissionsEnum.VIEW_STORES] }
      },
      { 
        path: 'store-form', 
        component: StoreFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.ADD_STORE, PermissionsEnum.EDIT_STORE, PermissionsEnum.VIEW_STORES] }
      },
      { 
        path: 'category', 
        component: CategoryComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_CATEGORIES, PermissionsEnum.ADD_CATEGORY, PermissionsEnum.EDIT_CATEGORY] }
      },
      { 
        path: 
        'category-form', 
        component: CategoryFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_CATEGORIES, PermissionsEnum.ADD_CATEGORY, PermissionsEnum.EDIT_CATEGORY] }
      },
      { 
        path: 'sub-category', 
        component: SubCategoryComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_SUBCATEGORIES, PermissionsEnum.ADD_SUBCATEGORY, PermissionsEnum.EDIT_SUBCATEGORY] }
      },
      { 
        path: 
        'sub-category-form', 
        component: SubCategoryFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_SUBCATEGORIES, PermissionsEnum.ADD_SUBCATEGORY, PermissionsEnum.EDIT_SUBCATEGORY] },
        resolve: {
          categoriesResolver: CategoriesResolver
        },
      },
      { 
        path: 'unit', 
        component: UnitComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_UNITS, PermissionsEnum.ADD_UNIT, PermissionsEnum.EDIT_UNIT] }
      },
      { 
        path: 'unit-form', 
        component: UnitFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_UNITS, PermissionsEnum.ADD_UNIT, PermissionsEnum.EDIT_UNIT] }
      },
      {
        path: 'store-transfers',
        component: StoreTransferFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_STORE_TRANSFERS, PermissionsEnum.ADD_STORE_TRANSFER] },
        resolve: {
          StoresResolver: StoresResolver,
          productsResolver: ProductsResolver
        }
      },
      {
        path: 'store-transfers-form',
        component: StoreTransferFormComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.ADD_STORE_TRANSFER, PermissionsEnum.EDIT_STORE_TRANSFER] },
        resolve: {
          StoresResolver: StoresResolver,
          productsResolver: ProductsResolver
        }
      },
      {
        path: 'inventory-management',
        component: InventoryManagementComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_INVENTORY_MANAGEMENT, PermissionsEnum.MANAGE_INVENTORY] },
        resolve: {
          StoresResolver: StoresResolver,
          productsResolver: ProductsResolver
        }
      },
      {
        path: 'initial-stock-setup',
        component: InitialStockSetupComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_INVENTORY_MANAGEMENT, PermissionsEnum.MANAGE_INVENTORY] }
      },
      {
        path: 'initial-stock',
        component: InitialStockComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.MANAGE_INVENTORY] }
      },
      {
        path: 'activity-logs',
        component: ActivityLogsComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_ACTIVITY_LOGS] }
      },
      {
        path: 'working-hours',
        component: WorkingHoursComponent,
        canActivate: [PermissionGuard],
        data: { permission: [PermissionsEnum.VIEW_WORKING_HOURS] }
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
