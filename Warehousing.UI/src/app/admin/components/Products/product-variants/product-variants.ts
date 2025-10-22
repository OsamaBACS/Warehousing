import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ProductVariant, ProductVariantCreateRequest } from '../../../models/ProductVariant';
import { ProductVariantService } from '../../../services/product-variant.service';

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
  variantForm: FormGroup;
  isEditing = false;
  editingVariantId?: number;
  loading = false;
  showAddForm = false;

  constructor(
    private fb: FormBuilder,
    private variantService: ProductVariantService
  ) {
    this.variantForm = this.fb.group({
      name: ['', Validators.required],
      code: [''],
      description: [''],
      priceAdjustment: [0],
      costAdjustment: [0],
      stockQuantity: [0],
      reorderLevel: [0],
      isActive: [true],
      isDefault: [false],
      displayOrder: [0]
    });
  }

  ngOnInit(): void {
    this.loadVariants();
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
      const variantData: ProductVariantCreateRequest = {
        productId: this.productId,
        ...formValue
      };

      if (this.isEditing && this.editingVariantId) {
        this.updateVariant(variantData);
      } else {
        this.createVariant(variantData);
      }
    }
  }

  createVariant(variantData: ProductVariantCreateRequest): void {
    this.variantService.createVariant(variantData).subscribe({
      next: (variant) => {
        this.variants.push(variant);
        this.showAddForm = false; // Hide the form after successful creation
        this.resetForm();
        this.variantsUpdated.emit(this.variants);
      },
      error: (error) => {
        console.error('Error creating variant:', error);
      }
    });
  }

  updateVariant(variantData: ProductVariantCreateRequest): void {
    if (!this.editingVariantId) return;

    this.variantService.updateVariant(this.editingVariantId, { id: this.editingVariantId, ...variantData }).subscribe({
      next: (variant) => {
        const index = this.variants.findIndex(v => v.id === this.editingVariantId);
        if (index !== -1) {
          this.variants[index] = variant;
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
    this.variantForm.patchValue({
      name: variant.name,
      code: variant.code,
      description: variant.description,
      priceAdjustment: variant.priceAdjustment,
      costAdjustment: variant.costAdjustment,
      stockQuantity: variant.stockQuantity,
      reorderLevel: variant.reorderLevel,
      isActive: variant.isActive,
      isDefault: variant.isDefault,
      displayOrder: variant.displayOrder
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
      stockQuantity: 0,
      reorderLevel: 0,
      isActive: true,
      isDefault: false,
      displayOrder: 0
    });
    this.isEditing = false;
    this.editingVariantId = undefined;
    this.showAddForm = true; // Show the form when adding a new variant
  }

  cancelEdit(): void {
    this.showAddForm = false; // Hide the form when canceling
    this.resetForm();
  }
}