import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '../../../models/store';
import { StoreService } from '../../../services/store.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-store-form',
  standalone: false,
  templateUrl: './store-form.component.html',
  styleUrl: './store-form.component.scss'
})
export class StoreFormComponent implements OnInit {

  storeForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private storeService: StoreService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {
  }

  ngOnInit(): void {
    this.initializingForm(null);

    this.route.queryParams.subscribe(params => {
      const storeId = +params['storeId'];
      if (storeId) {
        this.storeService.GetStoreById(storeId).subscribe({
          next: (res) => {
            this.initializingForm(res);
          },
          error: (err) => {
            console.error(err.error);
          }
        });
      }
    });
  }

  initializingForm(store: Store | null) {
    this.storeForm = this.fb.group({
      id: [store ? store.id : 0],
      nameEn: [store ? store.nameAr : '', Validators.required],
      nameAr: [store ? store.nameEn : '', Validators.required],
      description: [store ? store.description : ''],
      isActive: [store ? store.isActive : true],
    });
  }

  onSubmit() {
    if (this.storeForm.valid) {
      const storeData: Store = this.storeForm.value;
      this.storeService.SaveStore(this.storeForm.value).subscribe({
        next: (res) => {
          if(res) {
            this.toastr.success('تم اضافة مستودع بنجاح', 'STORES');
            this.router.navigate(['../store'], { relativeTo: this.route });
          }
          else {
            this.toastr.error(res, 'STORES')
          }
        },
        error: (err) => {
          this.toastr.error(err.error, 'STORES')
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['../store'], { relativeTo: this.route });
  }
}
