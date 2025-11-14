import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
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
    });
  }

  loadData(): void {
    this.storeService.GetStores().subscribe({
      next: (stores) => this.stores = stores,
      error: (err) => console.error('Error loading stores:', err)
    });

    this.productService.GetProducts().subscribe({
      next: (products) => this.products = products,
      error: (err) => console.error('Error loading products:', err)
    });
  }

  loadTransfer(id: number): void {
    this.transferService.GetTransferWithItems(id).subscribe({
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
      error: (err) => {
        console.error('Error loading transfer:', err);
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
        this.transferService.UpdateTransfer(this.transferId, transferData).subscribe({
          next: (response) => {
            this.notification.success('Transfer updated successfully', 'Store Transfer');
            this.router.navigate(['../store-transfers'], { relativeTo: this.route });
          },
          error: (err) => {
            console.error('Error updating transfer:', err);
            this.notification.error('Error updating transfer', 'Store Transfer');
          }
        });
      } else {
        this.transferService.CreateTransfer(transferData).subscribe({
          next: (response) => {
            this.notification.success('Transfer created successfully', 'Store Transfer');
            this.router.navigate(['../store-transfers'], { relativeTo: this.route });
          },
          error: (err) => {
            console.error('Error creating transfer:', err);
            this.notification.error('Error creating transfer', 'Store Transfer');
          }
        });
      }
    } else {
      this.notification.warning('Please fill in all required fields and add at least one item', 'Store Transfer');
    }
  }

  cancel(): void {
    this.router.navigate(['../store-transfers'], { relativeTo: this.route });
  }

  getProductName(productId: number): string {
    const product = this.products.find(p => p.id === productId);
    return product ? product.nameAr : '';
  }

  getStoreName(storeId: number): string {
    const store = this.stores.find(s => s.id === storeId);
    return store ? store.nameAr : '';
  }
}
