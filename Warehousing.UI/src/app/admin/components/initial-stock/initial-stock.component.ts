import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Product } from '../../models/product';
import { StoreSimple } from '../../models/StoreSimple';
import { InitialStockItem, InitialStockService } from '../../services/initial-stock.service';
import { ProductsService } from '../../services/products.service';
import { StoreService } from '../../services/store.service';

@Component({
  selector: 'app-initial-stock',
  templateUrl: './initial-stock.component.html',
  styleUrls: ['./initial-stock.component.scss'],
  standalone: false
})
export class InitialStockComponent implements OnInit {
  initialStockForm!: FormGroup;
  products: Product[] = [];
  stores: StoreSimple[] = [];
  isLoading = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private initialStockService: InitialStockService,
    private productsService: ProductsService,
    private storeService: StoreService,
    private toastr: ToastrService,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadData();
  }

  initializeForm(): void {
    this.initialStockForm = this.fb.group({
      notes: [''],
      items: this.fb.array([])
    });
  }

  get itemsFormArray(): FormArray {
    return this.initialStockForm.get('items') as FormArray;
  }

  loadData(): void {
    this.isLoading = true;
    
    // Load products and stores in parallel
    Promise.all([
      this.productsService.GetProducts().toPromise(),
      this.storeService.GetActiveStores().toPromise()
    ]).then(([products, stores]) => {
      this.products = products || [];
      this.stores = stores || [];
      this.isLoading = false;
    }).catch(error => {
      this.toastr.error('خطأ في تحميل البيانات', 'خطأ');
      this.isLoading = false;
    });
  }

  addStockItem(): void {
    const itemForm = this.fb.group({
      productId: [null, Validators.required],
      storeId: [null, Validators.required],
      quantity: [0, [Validators.required, Validators.min(0)]]
    });

    this.itemsFormArray.push(itemForm);
  }

  removeStockItem(index: number): void {
    this.itemsFormArray.removeAt(index);
  }

  getProductName(productId: number): string {
    const product = this.products.find(p => p.id === productId);
    return product ? product.nameAr : '';
  }

  getStoreName(storeId: number): string {
    const store = this.stores.find(s => s.id === storeId);
    return store ? store.name : '';
  }

  onSubmit(): void {
    if (this.initialStockForm.valid && this.itemsFormArray.length > 0) {
      this.isSubmitting = true;
      
      const formValue = this.initialStockForm.value;
      const request: InitialStockItem[] = formValue.items.map((item: any) => ({
        productId: item.productId,
        storeId: item.storeId,
        quantity: item.quantity
      }));

      this.initialStockService.setupInitialStock({
        items: request,
        notes: formValue.notes
      }).subscribe({
        next: (response) => {
          if (response.success) {
            this.toastr.success('تم إعداد الرصيد الابتدائي بنجاح', 'نجح');
            this.router.navigate(['/app/admin/inventory-report']);
          } else {
            this.toastr.error('فشل في إعداد الرصيد الابتدائي', 'خطأ');
          }
          this.isSubmitting = false;
        },
        error: (error) => {
          this.toastr.error('خطأ في إعداد الرصيد الابتدائي', 'خطأ');
          this.isSubmitting = false;
        }
      });
    } else {
      this.toastr.warning('يرجى إضافة عنصر واحد على الأقل', 'تحذير');
    }
  }

  clearForm(): void {
    this.itemsFormArray.clear();
    this.initialStockForm.patchValue({ notes: '' });
  }

  getTotalItems(): number {
    return this.itemsFormArray.length;
  }

  getTotalQuantity(): number {
    return this.itemsFormArray.controls.reduce((total, control) => {
      return total + (control.get('quantity')?.value || 0);
    }, 0);
  }
}
