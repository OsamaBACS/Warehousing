import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { OrderRoutingModule } from './order-routing.module';
import { OrderComponent } from './order.component';
import { OrderCategoriesComponent } from './order-categories/order-categories.component';
import { OrderSubCategoriesComponent } from './order-sub-categories/order-sub-categories.component';
import { OrderProductsComponent } from './order-products/order-products.component';
import { ProductDetailComponent } from './product-detail/product-detail.component';
import { MySharedModule } from '../shared/my-shared-module';


@NgModule({
  declarations: [
    OrderComponent,
    OrderCategoriesComponent,
    OrderSubCategoriesComponent,
    OrderProductsComponent,
    ProductDetailComponent
  ],
  imports: [
    CommonModule,
    OrderRoutingModule,
    MySharedModule,
    TranslateModule
  ]
})
export class OrderModule { }
