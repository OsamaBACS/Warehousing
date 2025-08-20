import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderComponent } from './order.component';
import { AuthGuard } from '../core/guards/auth-guard.guard';
import { OrderCategoriesComponent } from './order-categories/order-categories.component';
import { OrderSubCategoriesComponent } from './order-sub-categories/order-sub-categories.component';
import { OrderProductsComponent } from './order-products/order-products.component';
import { CategoriesResolver } from '../admin/resolvers/categories-resolver';
import { StoresResolver } from '../admin/resolvers/stores-resolver-resolver';
import { SubCategoriesResolver } from '../admin/resolvers/sub-categories-resolver';

const routes: Routes = [
  {
    path: ':orderTypeId',
    component: OrderComponent,
    canActivate: [AuthGuard],
    children: [
      { path: 'categories', component: OrderCategoriesComponent },
      {
        path: 'categories/:categoryId/sub-categories',
        component: OrderSubCategoriesComponent,
        resolve: {
          categoriesResolver: CategoriesResolver,
        },
      },
      {
        path: 'categories/:categoryId/sub-categories/:subCategoryId/products',
        component: OrderProductsComponent,
        resolve: {
          subCategoriesResolver: SubCategoriesResolver,
        },
      },
      { path: '', redirectTo: 'categories', pathMatch: 'full' },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrderRoutingModule { }
