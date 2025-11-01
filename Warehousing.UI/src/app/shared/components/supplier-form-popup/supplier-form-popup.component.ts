import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SupplierService } from '../../../admin/services/supplier.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-supplier-form-popup',
  templateUrl: './supplier-form-popup.component.html',
  styleUrls: ['./supplier-form-popup.component.scss'],
  standalone: false
})
export class SupplierFormPopupComponent implements OnInit {
  supplierForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private toastr: ToastrService,
    public dialogRef: MatDialogRef<SupplierFormPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.supplierForm = this.fb.group({
      id: [0],
      name: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9+\-\s()]+$/)]],
      email: ['', [Validators.email]],
      address: ['', [Validators.required, Validators.minLength(5)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    if (this.data && this.data.supplier) {
      this.supplierForm.patchValue(this.data.supplier);
    }
  }

  onSubmit(): void {
    if (this.supplierForm.valid) {
      this.isLoading = true;
      
      // Use form values directly as they match the Supplier entity
      const supplierData = this.supplierForm.value;
      
      this.supplierService.SaveSupplier(supplierData).subscribe({
        next: (response: any) => {
          this.toastr.success('تم إنشاء المورد بنجاح', 'نجح');
          this.dialogRef.close(response);
        },
        error: (error: any) => {
          this.toastr.error('حدث خطأ أثناء إنشاء المورد', 'خطأ');
          console.error('Error creating supplier:', error);
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
    Object.keys(this.supplierForm.controls).forEach(key => {
      const control = this.supplierForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.supplierForm.get(fieldName);
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
