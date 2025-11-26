import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './core/components/login/login';
import { CartComponent } from './shared/components/cart/cart.component';
import { ProductsResolver } from './admin/resolvers/products-resolver';
import { SuppliersResolver } from './admin/resolvers/suppliers-resolver';
import { StoresResolver } from './admin/resolvers/stores-resolver-resolver';
import { StatusesResolver } from './admin/resolvers/statuses-resolver';
import { customersResolver } from './admin/resolvers/customers-resolver';
import { AuthGuard } from './core/guards/auth-guard.guard';
import { OrderPendingListComponent } from './shared/components/order-pending-list/order-pending-list.component';
import { SubCategoriesResolver } from './admin/resolvers/sub-categories-resolver';
import { UnitsResolver } from './admin/resolvers/units-resolver';
import { MainLayoutComponent } from './shared/components/main-layout/main-layout.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'app/order/2/categories',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'app',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'order/2/categories',
        pathMatch: 'full',
      },
      {
        path: 'admin',
        loadChildren: () => import('./admin/admin.module').then((m) => m.AdminModule),
      },
      {
        path: 'order',
        loadChildren: () => import('./order/order.module').then((m) => m.OrderModule)
      },
      {
        path: 'cart',
        component: CartComponent,
        resolve: {
          productsResolver: ProductsResolver,
          suppliersResolver: SuppliersResolver,
          StoresResolver: StoresResolver,
          statusesResolver: StatusesResolver,
          customersRsolver: customersResolver,
          subCategoriesResolver: SubCategoriesResolver,
          unitsResolver: UnitsResolver
        }
      },
      {
        path: 'pending-orders',
        component: OrderPendingListComponent
      }
    ]
  },
  { 
    path: '**', 
    redirectTo: 'login' 
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
