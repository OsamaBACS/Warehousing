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



@NgModule({
  declarations: [
    Spinner,
    ImageUploader,
    ConfirmModalComponent,
    CompanyHeaderComponent,
    CompanyFooterComponent,
    CartComponent,
    BreadcrumbComponent,
    OrderPendingListComponent
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
    TranslateModule
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
    ReactiveFormsModule,
    RouterModule
  ]
})
export class MySharedModule { }
