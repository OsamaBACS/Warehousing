import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { TranslateModule } from "@ngx-translate/core";
import { ToastrModule } from "ngx-toastr";
import { MySharedModule } from "../shared/my-shared-module";
import { AdminRoutingModule } from "./admin-routing.module";
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
import { FilterByPipe } from "./pipes/filter-by-pipe";
import { getQuantityChangedSum } from "./pipes/get-quantity-changed-sum-pipe-pipe";
import { ProductsComponent } from "./components/Products/products/products.component";
import { SubCategoryComponent } from './components/SubCategories/sub-category/sub-category.component';
import { SubCategoryFormComponent } from './components/SubCategories/sub-category-form/sub-category-form.component';
import { UserDevicesComponent } from './components/Users/user-devices/user-devices.component';
import { OrderListComponent } from './components/Orders/order-list/order-list.component';
import { OrderItemsListComponent } from './components/Orders/order-items-list/order-items-list.component';



@NgModule({
  declarations: [
    AdminComponent,
    Dashboard,
    UsersComponent,
    RolesComponent,
    CustomersComponent,
    CompanyComponent,
    CompanyFormComponent,
    InventoryReportComponent,
    FilterByPipe,
    CustomersFormComponent,
    SuppliersComponent,
    SuppliersFormComponent,
    MainComponent,
    RolesFormComponent,
    UsersFormComponent,
    TransactionsComponent,
    getQuantityChangedSum,
    CategoryComponent,
    UnitComponent,
    StoreComponent,
    StoreFormComponent,
    CategoryFormComponent,
    UnitFormComponent,
    ProductsComponent,
    ProductFormComponent,
    SubCategoryComponent,
    SubCategoryFormComponent,
    UserDevicesComponent,
    OrderListComponent,
    OrderItemsListComponent
  ],
  imports: [
    CommonModule,
    AdminRoutingModule,
    TranslateModule,
    ToastrModule,
    MySharedModule
  ]
})
export class AdminModule { }
