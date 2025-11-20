import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomersService } from '../../../services/customers.service';

@Component({
  selector: 'app-customers-form.component',
  standalone: false,
  templateUrl: './customers-form.component.html',
  styleUrl: './customers-form.component.scss'
})
export class CustomersFormComponent implements OnInit {

  customerForm!: FormGroup;
  isEditMode = false;
  customerId: number = 0;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private customerService: CustomersService,
    private toastr: ToastrService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.customerForm = this.fb.group({
      id: [0],
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.email]],
      address: ['', Validators.required]
    });

    this.route.queryParams.subscribe(params => {
      if (params['customerId']) {
        this.customerId = +params['customerId'];
        this.isEditMode = true;
        this.loadCustomer(this.customerId);
      }
    });
  }

  loadCustomer(id: number) {
    this.isLoading = true;
    this.customerService.GetCustomerById(id).subscribe({
      next: (customer) => {
        this.customerForm.patchValue(customer);
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Failed to load customer');
        this.isLoading = false;
      }
    });
  }

  onSubmit() {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.customerService.SaveCustomer(this.customerForm.value).subscribe({
      next: () => {
        this.toastr.success('Customer saved successfully');
        this.isLoading = false;
        this.router.navigate(['../customers'], { relativeTo: this.route });
      },
      error: (err) => {
        this.toastr.error(err.error || 'Save failed');
        this.isLoading = false;
      }
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.customerForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) {
        return 'This field is required';
      }
      if (field.errors['email']) {
        return 'Please enter a valid email address';
      }
    }
    return '';
  }

  cancel(): void {
    this.router.navigate(['../customers'], { relativeTo: this.route })
  }
}
