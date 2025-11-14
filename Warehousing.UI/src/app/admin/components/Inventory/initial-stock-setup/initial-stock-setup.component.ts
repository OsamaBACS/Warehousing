import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { InventoryService } from '../../../services/inventory.service';
import { ProductsService } from '../../../services/products.service';
import { StoreService } from '../../../services/store.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Product } from '../../../models/product';
import { Store } from '../../../models/store';

@Component({
  selector: 'app-initial-stock-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule],
  templateUrl: './initial-stock-setup.component.html',
  styleUrl: './initial-stock-setup.component.scss'
})
export class InitialStockSetupComponent implements OnInit {

  initialStockForm!: FormGroup;
  products: Product[] = [];
  stores: Store[] = [];
  loading = false;
  selectedProducts: Product[] = [];
  bulkQuantity: number = 0;
  bulkStoreId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private inventoryService: InventoryService,
    private productService: ProductsService,
    private storeService: StoreService,
    private notification: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadProducts();
    this.loadStores();
  }

  initializeForm(): void {
    this.initialStockForm = this.fb.group({
      productId: [null, Validators.required],
      storeId: [null, Validators.required],
      quantity: [0, [Validators.required, Validators.min(0.01)]]
    });
  }

  loadProducts(): void {
    this.productService.GetProducts().subscribe({
      next: (res) => {
        // Filter products based on user permissions
        this.products = (res || []).filter(product => this.authService.hasProduct(product.id!));
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.notification.error('خطأ في تحميل المنتجات', 'Initial Stock Setup');
      }
    });
  }

  loadStores(): void {
    this.storeService.GetStores().subscribe({
      next: (res) => {
        this.stores = res || [];
      },
      error: (err) => {
        console.error('Error loading stores:', err);
        this.notification.error('خطأ في تحميل المستودعات', 'Initial Stock Setup');
      }
    });
  }

  addInitialStock(): void {
    if (this.initialStockForm.valid) {
      const formData = this.initialStockForm.value;
      
      this.loading = true;
      this.inventoryService.SingleInitialStockSetup(formData).subscribe({
        next: (res) => {
          this.loading = false;
          this.notification.success('تم إنشاء المخزون الابتدائي بنجاح', 'Initial Stock Setup');
          this.initialStockForm.reset();
          this.initialStockForm.patchValue({ quantity: 0 });
        },
        error: (err) => {
          this.loading = false;
          console.error('Error creating initial stock:', err);
          this.notification.error(err.error || 'خطأ في إنشاء المخزون الابتدائي', 'Initial Stock Setup');
        }
      });
    } else {
      this.notification.error('يرجى ملء جميع الحقول المطلوبة', 'Initial Stock Setup');
    }
  }

  // Bulk operations
  selectAllProducts(): void {
    this.selectedProducts = [...this.products];
  }

  clearSelection(): void {
    this.selectedProducts = [];
  }

  toggleProductSelection(product: Product): void {
    const index = this.selectedProducts.findIndex(p => p.id === product.id);
    if (index > -1) {
      this.selectedProducts.splice(index, 1);
    } else {
      this.selectedProducts.push(product);
    }
  }

  isProductSelected(product: Product): boolean {
    return this.selectedProducts.some(p => p.id === product.id);
  }

  bulkCreateInitialStock(): void {
    if (this.selectedProducts.length === 0) {
      this.notification.error('يرجى اختيار منتج واحد على الأقل', 'Bulk Initial Stock');
      return;
    }

    if (!this.bulkStoreId) {
      this.notification.error('يرجى اختيار المستودع', 'Bulk Initial Stock');
      return;
    }

    if (this.bulkQuantity <= 0) {
      this.notification.error('يرجى إدخال كمية صحيحة', 'Bulk Initial Stock');
      return;
    }

    this.loading = true;
    const bulkData = {
      productIds: this.selectedProducts.map(p => p.id),
      storeId: this.bulkStoreId,
      quantity: this.bulkQuantity
    };

    this.inventoryService.BulkInitialStockSetup(bulkData).subscribe({
      next: (res) => {
        this.loading = false;
        this.notification.success(`تم إنشاء المخزون الابتدائي لـ ${this.selectedProducts.length} منتج بنجاح`, 'Bulk Initial Stock');
        this.clearSelection();
        this.bulkQuantity = 0;
        this.bulkStoreId = null;
      },
      error: (err) => {
        this.loading = false;
        console.error('Error creating bulk initial stock:', err);
        this.notification.error(err.error || 'خطأ في إنشاء المخزون الابتدائي المجمع', 'Bulk Initial Stock');
      }
    });
  }

  // Getters for form controls
  get productId() { return this.initialStockForm.get('productId'); }
  get storeId() { return this.initialStockForm.get('storeId'); }
  get quantity() { return this.initialStockForm.get('quantity'); }
}
