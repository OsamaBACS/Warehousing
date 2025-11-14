import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ProductVariant, ProductVariantCreateRequest } from '../../../models/ProductVariant';
import { ProductVariantService } from '../../../services/product-variant.service';
import { Store } from '../../../models/store';
import { StoreService } from '../../../services/store.service';
import { VariantStockService } from '../../../services/variant-stock.service';
import { ProductsService } from '../../../services/products.service';

@Component({
  selector: 'app-product-variants',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-variants.html',
  styleUrls: ['./product-variants.scss']
})
export class ProductVariantsComponent implements OnInit {
  @Input() productId!: number;
  @Output() variantsUpdated = new EventEmitter<ProductVariant[]>();

  variants: ProductVariant[] = [];
  stores: Store[] = [];
  variantForm: FormGroup;
  isEditing = false;
  editingVariantId?: number;
  loading = false;
  showAddForm = false;

  constructor(
    private fb: FormBuilder,
    private variantService: ProductVariantService,
    private storeService: StoreService,
    private variantStockService: VariantStockService,
    private productService: ProductsService
  ) {
    this.variantForm = this.fb.group({
      name: ['', Validators.required],
      code: [''],
      description: [''],
      priceAdjustment: [0],
      costAdjustment: [0],
      // Stock quantity is managed through Inventory table per variant per store
      reorderLevel: [0],
      stockQuantity: [0], // Add stock quantity field
      isActive: [true],
      isDefault: [false],
      displayOrder: [0],
      storeId: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadVariants();
    this.loadStores();
    this.loadVariantStockData();
  }

  loadStores(): void {
    this.storeService.GetStores().subscribe({
      next: (stores) => {
        this.stores = stores;
      },
      error: (error) => {
        console.error('Error loading stores:', error);
      }
    });
  }

  loadVariants(): void {
    this.loading = true;
    this.variantService.getVariantsByProduct(this.productId).subscribe({
      next: (variants) => {
        this.variants = variants;
        this.variantsUpdated.emit(variants);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading variants:', error);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.variantForm.valid) {
      const formValue = this.variantForm.value;
      const stockQuantity = formValue.stockQuantity || 0;
      const storeId = formValue.storeId;
      
      // Remove stockQuantity from variant data as it's not part of the variant entity
      const { stockQuantity: _, ...variantData } = formValue;
      
      const variantRequest: ProductVariantCreateRequest = {
        productId: this.productId,
        ...variantData
      };

      if (this.isEditing && this.editingVariantId) {
        this.updateVariant(variantRequest, stockQuantity, storeId);
      } else {
        this.createVariant(variantRequest, stockQuantity, storeId);
      }
    }
  }

  createVariant(variantData: ProductVariantCreateRequest, stockQuantity: number, storeId: number): void {
    this.variantService.createVariant(variantData).subscribe({
      next: (variant) => {
        this.variants.push(variant);
        
        // Set stock quantity for the new variant
        if (stockQuantity > 0 && variant.id) {
          this.setVariantStock(variant.id, storeId, stockQuantity);
        }
        
        this.showAddForm = false; // Hide the form after successful creation
        this.resetForm();
        this.variantsUpdated.emit(this.variants);
      },
      error: (error) => {
        console.error('Error creating variant:', error);
      }
    });
  }

  updateVariant(variantData: ProductVariantCreateRequest, stockQuantity: number, storeId: number): void {
    if (!this.editingVariantId) return;

    this.variantService.updateVariant(this.editingVariantId, { id: this.editingVariantId, ...variantData }).subscribe({
      next: (variant) => {
        const index = this.variants.findIndex(v => v.id === this.editingVariantId);
        if (index !== -1) {
          this.variants[index] = variant;
        }
        
        // Update stock quantity for the variant
        if (stockQuantity >= 0 && this.editingVariantId) {
          this.setVariantStock(this.editingVariantId, storeId, stockQuantity);
        }
        
        this.showAddForm = false; // Hide the form after successful update
        this.resetForm();
        this.variantsUpdated.emit(this.variants);
      },
      error: (error) => {
        console.error('Error updating variant:', error);
      }
    });
  }

  editVariant(variant: ProductVariant): void {
    this.isEditing = true;
    this.showAddForm = false; // Hide add form when editing
    this.editingVariantId = variant.id;
    
    // Get current stock quantity for this variant
    const currentStock = this.getTotalVariantStock(variant.id!);
    
    // Find the store with the highest stock for this variant
    let selectedStoreId = this.stores[0]?.id || 1; // Default to first store
    let maxStock = 0;
    
    this.stores.forEach(store => {
      const stock = this.getVariantStockForStore(variant.id!, store.id);
      if (stock > maxStock) {
        maxStock = stock;
        selectedStoreId = store.id;
      }
    });
    
    this.variantForm.patchValue({
      name: variant.name,
      code: variant.code,
      description: variant.description,
      priceAdjustment: variant.priceAdjustment,
      costAdjustment: variant.costAdjustment,
      // stockQuantity is managed through Inventory table per variant per store
      reorderLevel: variant.reorderLevel,
      stockQuantity: currentStock, // Set current stock quantity
      isActive: variant.isActive,
      isDefault: variant.isDefault,
      displayOrder: variant.displayOrder,
      storeId: selectedStoreId // Set the store with highest stock
    });
  }

  deleteVariant(variant: ProductVariant): void {
    if (!variant.id) return;

    if (confirm('Are you sure you want to delete this variant?')) {
      this.variantService.deleteVariant(variant.id).subscribe({
        next: () => {
          this.variants = this.variants.filter(v => v.id !== variant.id);
          this.variantsUpdated.emit(this.variants);
        },
        error: (error) => {
          console.error('Error deleting variant:', error);
        }
      });
    }
  }

  resetForm(): void {
    this.variantForm.reset({
      name: '',
      code: '',
      description: '',
      priceAdjustment: 0,
      costAdjustment: 0,
      // Stock quantity is managed through Inventory table per variant per store
      reorderLevel: 0,
      stockQuantity: 0, // Add stock quantity field
      isActive: true,
      isDefault: false,
      displayOrder: 0,
      storeId: ''
    });
    this.isEditing = false;
    this.editingVariantId = undefined;
    this.showAddForm = true; // Show the form when adding a new variant
  }

  cancelEdit(): void {
    this.showAddForm = false; // Hide the form when canceling
    this.resetForm();
  }

  getVariantStockForStore(variantId: number, storeId: number): number {
    // This will be populated when we load variant stock data
    return this.variantStockData[`${this.productId}-${variantId}-${storeId}`] || 0;
  }

  getTotalVariantStock(variantId: number): number {
    // Calculate total stock across all stores for this variant
    let total = 0;
    this.stores.forEach(store => {
      total += this.getVariantStockForStore(variantId, store.id);
    });
    return total;
  }

  getUnitName(): string {
    // For now, return a default unit name. This should be loaded from the product data
    return 'وحدة';
  }

  variantStockData: { [key: string]: number } = {};

  loadVariantStockData(): void {
    if (this.productId && this.stores.length > 0) {
      this.stores.forEach(store => {
        this.variantStockService.getProductVariantsStock(this.productId, store.id).subscribe({
          next: (variantStockData: any[]) => {
            variantStockData.forEach((variantStock: any) => {
              const key = `${this.productId}-${variantStock.variantId}-${store.id}`;
              this.variantStockData[key] = variantStock.availableQuantity;
            });
          },
          error: (error: any) => {
            console.error('Error loading variant stock:', error);
          }
        });
      });
    }
  }

  setVariantStock(variantId: number, storeId: number, quantity: number): void {
    // Update the variant stock using the new set-variant-stock API
    this.productService.SetVariantStock(this.productId, {
      variantId: variantId,
      storeId: storeId,
      quantity: quantity
    }).subscribe({
      next: (response: any) => {
        console.log('Stock updated successfully:', response);
        // Refresh the variant stock data
        this.loadVariantStockData();
      },
      error: (error: any) => {
        console.error('Error updating variant stock:', error);
      }
    });
  }
}