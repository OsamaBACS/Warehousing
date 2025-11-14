import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Spinner } from './components/spinner/spinner';
import { MatDialogModule } from '@angular/material/dialog';
import { ImageUploader } from './components/image-uploader/image-uploader';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatListOption } from '@angular/material/list';
import { MatOption } from '@angular/material/autocomplete';
import { ConfirmModalComponent } from './components/confirm-modal.component/confirm-modal.component';
import { CompanyHeaderComponent } from './components/company-header.component/company-header.component';
import { CompanyFooterComponent } from './components/company-footer.component/company-footer.component';
import { TranslateModule } from '@ngx-translate/core';
import { CartComponent } from './components/cart/cart.component';
import { BreadcrumbComponent } from './components/breadcrumb/breadcrumb.component';
import { OrderPendingListComponent } from './components/order-pending-list/order-pending-list.component';
import { CustomerFormPopupComponent } from './components/customer-form-popup/customer-form-popup.component';
import { SupplierFormPopupComponent } from './components/supplier-form-popup/supplier-form-popup.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';



@NgModule({
  declarations: [
    Spinner,
    ConfirmModalComponent,
    CompanyHeaderComponent,
    CompanyFooterComponent,
    CartComponent,
    BreadcrumbComponent,
    OrderPendingListComponent,
    CustomerFormPopupComponent,
    SupplierFormPopupComponent,
    SidebarComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    MatDialogModule,
    ReactiveFormsModule,
    FormsModule,
    MatFormField,
    MatLabel,
    MatListOption,
    MatOption,
    TranslateModule,
    ImageUploader
  ],
  exports: [
    Spinner,
    ImageUploader,
    MatDialogModule,
    FormsModule,
    MatFormField,
    MatLabel,
    MatListOption,
    MatOption,
    ConfirmModalComponent,
    CompanyHeaderComponent,
    CompanyFooterComponent,
    BreadcrumbComponent,
    SidebarComponent,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule
  ]
})
export class MySharedModule { }
