import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { OrderRoutingModule } from './order-routing.module';
import { OrderComponent } from './order.component';
import { OrderCategoriesComponent } from './order-categories/order-categories.component';
import { OrderSubCategoriesComponent } from './order-sub-categories/order-sub-categories.component';
import { OrderProductsComponent } from './order-products/order-products.component';
import { MySharedModule } from '../shared/my-shared-module';


@NgModule({
  declarations: [
    OrderComponent,
    OrderCategoriesComponent,
    OrderSubCategoriesComponent,
    OrderProductsComponent
  ],
  imports: [
    CommonModule,
    OrderRoutingModule,
    MySharedModule
  ]
})
export class OrderModule { }
