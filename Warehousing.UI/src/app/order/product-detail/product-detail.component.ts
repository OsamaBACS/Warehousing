import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductsService } from '../../admin/services/products.service';
import { StoreService } from '../../admin/services/store.service';
import { ProductVariantService } from '../services/product-variant.service';
import { ProductModifierService } from '../services/product-modifier.service';
import { CartService } from '../../shared/services/cart.service';
import { Product } from '../../admin/models/product';
import { StoreSimple } from '../../admin/models/StoreSimple';
import { ProductVariant } from '../models/ProductVariant';
import { ProductModifierGroup } from '../models/ProductModifier';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-product-detail',
  standalone: false,
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  productId!: number;
  orderTypeId!: number;
  serverUrl = environment.resourcesUrl;
  
  // Navigation parameters
  categoryId!: number;
  subCategoryId!: number;
  
  // Stores
  allStores: StoreSimple[] = [];
  selectedStoreId: number | null = null;
  
  // Variants and Modifiers
  productVariants: ProductVariant[] = [];
  productModifiers: ProductModifierGroup[] = [];
  selectedVariantId: number | null = null;
  selectedModifiers: { [modifierId: number]: number[] } = {};
  
  // Form
  productForm!: FormGroup;
  quantity = 1;
  
  // Loading states
  isLoading = true;
  isAddingToCart = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService,
    private storeService: StoreService,
    private productVariantService: ProductVariantService,
    private productModifierService: ProductModifierService,
    private cartService: CartService,
    private fb: FormBuilder
  ) {
    this.productForm = this.fb.group({
      storeId: [null, Validators.required],
      variantId: [null],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.productId = Number(params.get('productId'));
      this.orderTypeId = Number(params.get('orderTypeId'));
      
      if (this.productId) {
        this.loadProduct();
        this.loadStores();
        this.loadProductVariants();
        this.loadProductModifiers();
      }
    });
    
    // Get navigation parameters from parent route
    this.route.parent?.paramMap.subscribe(params => {
      this.categoryId = Number(params.get('categoryId'));
      this.subCategoryId = Number(params.get('subCategoryId'));
    });
  }

  loadProduct(): void {
    this.isLoading = true;
    this.productsService.GetProductById(this.productId).subscribe({
      next: (product) => {
        this.product = product;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading product:', err);
        this.isLoading = false;
      }
    });
  }

  loadStores(): void {
    this.storeService.GetActiveStores().subscribe({
      next: (stores) => {
        this.allStores = stores;
      },
      error: (err) => {
        console.error('Error loading stores:', err);
      }
    });
  }

  loadProductVariants(): void {
    this.productVariantService.getProductVariantsByProductId(this.productId).subscribe({
      next: (variants) => {
        this.productVariants = variants;
        // Set default variant if available
        const defaultVariant = variants.find(v => v.isDefault);
        if (defaultVariant) {
          this.selectedVariantId = defaultVariant.id;
          this.productForm.patchValue({ variantId: defaultVariant.id });
        }
      },
      error: (err) => {
        console.error('Error loading variants:', err);
      }
    });
  }

  loadProductModifiers(): void {
    this.productModifierService.getProductModifiersByProductId(this.productId).subscribe({
      next: (modifiers) => {
        this.productModifiers = modifiers;
        // Initialize selected modifiers
        modifiers.forEach(modifier => {
          this.selectedModifiers[modifier.modifierId] = [];
        });
      },
      error: (err) => {
        console.error('Error loading modifiers:', err);
      }
    });
  }

  onStoreChange(storeId: number | null): void {
    this.selectedStoreId = storeId;
    this.productForm.patchValue({ storeId });
  }

  onVariantChange(variantId: number | null): void {
    this.selectedVariantId = variantId;
    this.productForm.patchValue({ variantId });
  }

  onModifierChange(modifierId: number, optionId: number, isChecked: boolean): void {
    if (!this.selectedModifiers[modifierId]) {
      this.selectedModifiers[modifierId] = [];
    }

    if (isChecked) {
      this.selectedModifiers[modifierId].push(optionId);
    } else {
      const index = this.selectedModifiers[modifierId].indexOf(optionId);
      if (index > -1) {
        this.selectedModifiers[modifierId].splice(index, 1);
      }
    }
  }

  isModifierOptionSelected(modifierId: number, optionId: number): boolean {
    return this.selectedModifiers[modifierId]?.includes(optionId) || false;
  }

  onQuantityChange(quantity: number): void {
    if (quantity < 1) {
      this.quantity = 1;
    } else {
      this.quantity = quantity;
    }
    this.productForm.patchValue({ quantity: this.quantity });
  }

  async addToCart(): Promise<void> {
    if (!this.product || !this.selectedStoreId) {
      alert('يرجى اختيار المستودع أولاً');
      return;
    }

    if (this.cartService.hasActiveOrder(this.orderTypeId)) {
      const message = this.orderTypeId === 1 ? 'يرجى إنهاء طلب الشراء أولاً' : 'يرجى إنهاء طلب البيع أولاً';
      alert(message);
      return;
    }

    // Validate required modifiers
    for (const modifier of this.productModifiers) {
      if (modifier.isRequired && (!this.selectedModifiers[modifier.modifierId] || this.selectedModifiers[modifier.modifierId].length === 0)) {
        alert(`يرجى اختيار ${modifier.modifierName || modifier.modifier?.name}`);
        return;
      }
    }

    this.isAddingToCart = true;

    try {
      // Validate stock
      const isValid = await this.cartService.validateStockForCartItem(this.product.id, this.selectedStoreId, this.quantity);
      if (!isValid) {
        return;
      }

      // Add to cart
      this.cartService.addToCart(this.product, this.quantity, this.selectedStoreId);
      
      // Show success message
      alert('تم إضافة المنتج إلى السلة بنجاح');
      
      // Navigate back to products list
      this.router.navigate(['/order', this.orderTypeId, 'categories', this.product.subCategory?.categoryId, 'sub-categories', this.product.subCategoryId, 'products']);
      
    } catch (error) {
      console.error('Error adding to cart:', error);
      alert('حدث خطأ أثناء إضافة المنتج إلى السلة');
    } finally {
      this.isAddingToCart = false;
    }
  }

  goBack(): void {
    if (this.categoryId && this.subCategoryId) {
      this.router.navigate(['/order', this.orderTypeId, 'categories', this.categoryId, 'sub-categories', this.subCategoryId, 'products']);
    } else {
      // Fallback to categories if we can't determine the exact path
      this.router.navigate(['/order', this.orderTypeId, 'categories']);
    }
  }

  getProductPrice(): number {
    if (!this.product) return 0;
    
    let price = this.product.sellingPrice;
    
    // Add variant price adjustment
    if (this.selectedVariantId) {
      const variant = this.productVariants.find(v => v.id === this.selectedVariantId);
      if (variant?.priceAdjustment) {
        price += variant.priceAdjustment;
      }
    }
    
    // Add modifier price adjustments
    for (const modifierId in this.selectedModifiers) {
      const modifier = this.productModifiers.find(m => m.modifierId === Number(modifierId));
      if (modifier?.modifier?.options) {
        for (const optionId of this.selectedModifiers[modifierId]) {
          const option = modifier.modifier.options.find(o => o.id === optionId);
          if (option?.priceAdjustment) {
            price += option.priceAdjustment;
          }
        }
      }
    }
    
    return price;
  }

  // Get quantity for a specific store
  getStoreQuantity(storeId: number): number {
    if (!this.product?.inventories) return 0;
    
    const inventory = this.product.inventories.find(inv => inv.storeId === storeId);
    return inventory?.quantity || 0;
  }
}
