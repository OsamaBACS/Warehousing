import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, ValidatorFn, AbstractControl } from '@angular/forms';
import { StoreTransferService } from '../../../services/storeTransfer.service';
import { StoreService } from '../../../services/store.service';
import { ProductsService } from '../../../services/products.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '../../../models/store';
import { Product } from '../../../models/product';
import { StoreTransferDto } from '../../../models/storeTransfer';

@Component({
  selector: 'app-store-transfer-form',
  standalone: false,
  templateUrl: './store-transfer-form.component.html',
  styleUrl: './store-transfer-form.component.scss'
})
export class StoreTransferFormComponent implements OnInit {

  transferForm!: FormGroup;
  stores: Store[] = [];
  products: Product[] = [];
  isEditMode = false;
  transferId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private transferService: StoreTransferService,
    private storeService: StoreService,
    private productService: ProductsService,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadData();

    this.route.queryParams.subscribe(params => {
      const id = +params['id'];
      if (id) {
        this.isEditMode = true;
        this.transferId = id;
        this.loadTransfer(id);
      }
    });
  }

  initializeForm(): void {
    this.transferForm = this.fb.group({
      id: [0],
      transferDate: [new Date().toISOString().split('T')[0], Validators.required],
      notes: [''],
      fromStoreId: [null, Validators.required],
      toStoreId: [null, Validators.required],
      statusId: [1], // Draft status
      items: this.fb.array([])
    }, { validators: this.storesMustDifferValidator });

    this.transferForm.get('fromStoreId')?.valueChanges.subscribe(fromStoreId => {
      const toControl = this.transferForm.get('toStoreId');
      if (toControl) {
        if (toControl.value === fromStoreId) {
          toControl.setValue(null);
        }
        toControl.updateValueAndValidity({ emitEvent: false });
      }
    });
  }

  loadData(): void {
    this.storeService.GetStores().subscribe({
      next: (stores) => this.stores = stores
    });

    this.productService.GetProducts().subscribe({
      next: (products) => this.products = products
    });
  }

  loadTransfer(id: number): void {
    this.transferService.getTransferWithItems(id).subscribe({
      next: (transfer) => {
        this.transferForm.patchValue({
          id: transfer.id,
          transferDate: transfer.transferDate.split('T')[0],
          notes: transfer.notes,
          fromStoreId: transfer.fromStoreId,
          toStoreId: transfer.toStoreId,
          statusId: transfer.statusId
        });

        // Load transfer items
        transfer.items.forEach(item => {
          this.addTransferItem(item.productId, item.quantity, item.unitCost, item.notes);
        });
      },
      error: () => {
        this.notification.error('Error loading transfer', 'Store Transfer');
      }
    });
  }

  get items(): FormArray {
    return this.transferForm.get('items') as FormArray;
  }

  addTransferItem(productId: number = 0, quantity: number = 0, unitCost: number = 0, notes: string = ''): void {
    const itemForm = this.fb.group({
      id: [0],
      productId: [productId, Validators.required],
      quantity: [quantity, [Validators.required, Validators.min(0.01)]],
      unitCost: [unitCost, [Validators.required, Validators.min(0)]],
      notes: [notes]
    });

    this.items.push(itemForm);
  }

  removeTransferItem(index: number): void {
    this.items.removeAt(index);
  }

  onProductChange(index: number, event: Event): void {
    const target = event.target as HTMLSelectElement;
    const productId = +target.value;
    const product = this.products.find(p => p.id === productId);
    if (product) {
      this.items.at(index).patchValue({
        unitCost: product.costPrice
      });
    }
  }

  saveTransfer(): void {
    if (this.transferForm.valid && this.items.length > 0) {
      const transferData: StoreTransferDto = this.transferForm.value;

      if (this.isEditMode && this.transferId) {
        this.transferService.updateTransfer(this.transferId, transferData).subscribe({
          next: () => {
            this.notification.success('Transfer updated successfully', 'Store Transfer');
            this.resetFormState();
            this.router.navigate(['/admin/store-transfers']);
          },
          error: () => {
            this.notification.error('Error updating transfer', 'Store Transfer');
          }
        });
      } else {
        this.transferService.createTransfer(transferData).subscribe({
          next: () => {
            this.notification.success('Transfer created successfully', 'Store Transfer');
            this.resetFormState();
          },
          error: () => {
            this.notification.error('Error creating transfer', 'Store Transfer');
          }
        });
      }
    } else {
      this.notification.warning('Please fill in all required fields and add at least one item', 'Store Transfer');
    }
  }

  cancel(): void {
    this.resetFormState();
  }

  getProductName(productId: number): string {
    const product = this.products.find(p => p.id === productId);
    return product ? product.nameAr : '';
  }

  getStoreName(storeId: number): string {
    const store = this.stores.find(s => s.id === storeId);
    return store ? store.nameAr : '';
  }

  get destinationStores(): Store[] {
    const fromStoreId = this.transferForm?.get('fromStoreId')?.value;
    return this.stores.filter(store => store.id !== fromStoreId);
  }

  private storesMustDifferValidator: ValidatorFn = (group: AbstractControl) => {
    const fromStoreId = group.get('fromStoreId')?.value;
    const toStoreId = group.get('toStoreId')?.value;
    if (fromStoreId && toStoreId && fromStoreId === toStoreId) {
      group.get('toStoreId')?.setErrors({ sameStore: true });
      return { sameStore: true };
    }
    return null;
  };

  private resetFormState(): void {
    this.isEditMode = false;
    this.transferId = null;
    this.items.clear();
    this.transferForm.reset({
      id: 0,
      transferDate: new Date().toISOString().split('T')[0],
      notes: '',
      fromStoreId: null,
      toStoreId: null,
      statusId: 1,
      items: []
    });
    this.transferForm.markAsPristine();
    this.transferForm.markAsUntouched();
  }
}
