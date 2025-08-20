import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SupplierService } from '../../../services/supplier.service';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-suppliers-form.component',
  standalone: false,
  templateUrl: './suppliers-form.component.html',
  styleUrl: './suppliers-form.component.scss'
})
export class SuppliersFormComponent implements OnInit {

  supplierForm!: FormGroup;
  isEditMode = false;
  supplierId: number = 0;

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private toastr: ToastrService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.supplierForm = this.fb.group({
      id: [0],
      name: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      address: ['']
    });

    this.route.queryParams.subscribe(params => {
      if (params['supplierId']) {
        this.supplierId = +params['supplierId'];
        this.isEditMode = true;
        this.loadSupplier(this.supplierId);
      }
    });
  }

  loadSupplier(id: number) {
    this.supplierService.GetSupplierById(id).subscribe({
      next: (supplier) => {
        this.supplierForm.patchValue(supplier);
      },
      error: () => {
        this.toastr.error('Failed to load supplier');
      }
    });
  }

  onSubmit() {
    if (this.supplierForm.invalid) return;

    this.supplierService.SaveSupplier(this.supplierForm.value).subscribe({
      next: () => {
        this.toastr.success('Supplier saved successfully');
      },
      error: (err) => {
        this.toastr.error(err.error || 'Save failed');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['../suppliers'], { relativeTo: this.route })
  }
}
