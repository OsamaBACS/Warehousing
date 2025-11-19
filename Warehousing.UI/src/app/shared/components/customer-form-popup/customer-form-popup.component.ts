import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CustomersService } from '../../../admin/services/customers.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-customer-form-popup',
  templateUrl: './customer-form-popup.component.html',
  styleUrls: ['./customer-form-popup.component.scss'],
  standalone: false
})
export class CustomerFormPopupComponent implements OnInit {
  customerForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private customerService: CustomersService,
    private toastr: ToastrService,
    public dialogRef: MatDialogRef<CustomerFormPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.customerForm = this.fb.group({
      nameAr: ['', [Validators.required, Validators.minLength(2)]],
      nameEn: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9+\-\s()]+$/)]],
      email: ['', [Validators.email]],
      address: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  ngOnInit(): void {
    if (this.data && this.data.customer) {
      this.customerForm.patchValue(this.data.customer);
    }
  }

  onSubmit(): void {
    if (this.customerForm.valid) {
      this.isLoading = true;
      
      const customerData = this.customerForm.value;
      
      this.customerService.SaveCustomer(customerData).subscribe({
        next: (response: any) => {
          this.toastr.success('تم إنشاء العميل بنجاح', 'نجح');
          this.dialogRef.close(response);
        },
        error: (error: any) => {
          this.toastr.error('حدث خطأ أثناء إنشاء العميل', 'خطأ');
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.customerForm.controls).forEach(key => {
      const control = this.customerForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.customerForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return 'هذا الحقل مطلوب';
      }
      if (control.errors['minlength']) {
        return `يجب أن يكون ${control.errors['minlength'].requiredLength} أحرف على الأقل`;
      }
      if (control.errors['email']) {
        return 'يرجى إدخال بريد إلكتروني صحيح';
      }
      if (control.errors['pattern']) {
        return 'يرجى إدخال رقم هاتف صحيح';
      }
    }
    return '';
  }
}
