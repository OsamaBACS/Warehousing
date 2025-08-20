import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './core/components/login/login';
import { HomeComponent } from './home/home.component';
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

const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
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
    path: 'home', 
    component: HomeComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'cart', component: CartComponent,
    resolve: {
      productsResolver: ProductsResolver,
      suppliersResolver: SuppliersResolver,
      StoresResolver: StoresResolver,
      statusesResolver: StatusesResolver,
      customersRsolver: customersResolver,
      subCategoriesResolver: SubCategoriesResolver,
      unitsResolver: UnitsResolver
    },
    canActivate: [AuthGuard]
  },
  {
    path: 'pending-orders', component: OrderPendingListComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'login',
    component: Login
  },
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
