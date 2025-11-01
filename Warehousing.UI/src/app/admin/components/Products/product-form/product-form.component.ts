import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ProductsService } from '../../../services/products.service';
import { CategoriesService } from '../../../services/categories.service';
import { UnitsService } from '../../../services/units.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Unit } from '../../../models/unit';
import { environment } from '../../../../../environments/environment';
import { Product } from '../../../models/product';
import { Store } from '../../../models/store';
import { SubCategory } from '../../../models/SubCategory';
import { ProductVariant } from '../../../models/ProductVariant';
import { ProductModifierGroup } from '../../../models/ProductModifier';
import { ProductVariantsComponent } from '../product-variants/product-variants';
import { ProductModifiersComponent } from '../product-modifiers/product-modifiers';
import { ImageUploader } from '../../../../shared/components/image-uploader/image-uploader';
import { AuthService } from '../../../../core/services/auth.service';
import { Inventory } from '../../../models/Inventory';
import { InventoryService } from '../../../services/inventory.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, ProductVariantsComponent, ProductModifiersComponent, ImageUploader],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnInit {

  generalQuantity: number = 0;
  variantQuantities: { id: number, name: string, quantity: number }[] = [];
  allocation = { variantId: 0, amount: 1 };
  allocationMsg = '';
  allocationSuccess = false;
  isAdmin = false;
  inventoryData: Inventory[] = [];
  // Totals
  variantsTotal: number = 0;
  overallTotal: number = 0;
  // Recall state per variant
  recallAmountByVariant: { [variantId: number]: number } = {};
  // Per-store breakdown for beautiful display
  storeInventoryRows: { storeId: number; storeName: string; generalQuantity: number; variants: { id: number; name: string; quantity: number }[] }[] = [];
  // Store variants list for dropdown (updated when variants change)
  productVariantsList: ProductVariant[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: ProductsService,
    private inventoryService: InventoryService,
    private categoriesService: CategoriesService,
    private unitsService: UnitsService,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
    this.isAdmin = this.authService.isAdmin;
  }

  ngOnInit(): void {
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const productId = +params['productId'];
      if (productId) {
        this.productService.GetProductById(productId).subscribe({
          next: (res) => {
            this.initializingForm(res);
            this.loadInventory(res.id);
          },
          error: (err) => {
            console.log(err.error);
          }
        });
      }
    });
    this.unitsService.GetUnits().subscribe({
      next: (res) => {
        this.units = res;
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

  loadInventory(productId: number) {
    const storeId = this.stores && this.stores.length > 0 ? this.stores[0].id : 1;
    this.inventoryService.GetInventoryByProduct(productId).subscribe((data: any[]) => {
      // Normalize casing from backend (PascalCase) to camelCase expected by UI
      const normalized = (data || []).map(d => ({
        id: d.id ?? d.Id,
        productId: d.productId ?? d.ProductId,
        storeId: d.storeId ?? d.StoreId,
        quantity: d.quantity ?? d.Quantity,
        variantId: d.variantId ?? d.VariantId,
        store: d.store ?? d.Store,
        variant: d.variant ?? d.Variant,
      }));
      this.inventoryData = normalized as any;

      // General quantity = sum across all stores where no variant
      const generalEntries = normalized.filter(i => i.variantId == null || i.variantId === 0);
      this.generalQuantity = generalEntries.reduce((sum, i) => sum + (i.quantity || 0), 0);

      // Build per-store breakdown
      const byStore: { [storeId: number]: { storeId: number; storeName: string; generalQuantity: number; variants: { [vid: number]: number } } } = {};
      normalized.forEach(i => {
        const sId = i.storeId;
        const sName = i.store?.nameAr ?? i.store?.NameAr ?? `Store ${sId}`;
        if (!byStore[sId]) {
          byStore[sId] = { storeId: sId, storeName: sName, generalQuantity: 0, variants: {} };
        }
        if (i.variantId == null || i.variantId === 0) {
          byStore[sId].generalQuantity += i.quantity || 0;
        } else {
          const vid = i.variantId as number;
          byStore[sId].variants[vid] = (byStore[sId].variants[vid] || 0) + (i.quantity || 0);
        }
      });
      // Use stored variants list (updated when variants change) or fallback to form value
      const productVariants: { id: number; name: string }[] = (this.productVariantsList.length > 0 
        ? this.productVariantsList 
        : (this.productForm.value?.variants || [])).map((v: any) => ({ id: v.id, name: v.name }));
      this.storeInventoryRows = Object.values(byStore).map(row => {
        const variantsArr: { id: number; name: string; quantity: number }[] = Object.keys(row.variants).map(k => {
          const vid = +k;
          const pv = productVariants.find(x => x.id === vid);
          return { id: vid, name: pv?.name || ('Variant #' + vid), quantity: row.variants[vid] };
        });
        return { storeId: row.storeId, storeName: row.storeName, generalQuantity: row.generalQuantity, variants: variantsArr };
      }).sort((a, b) => a.storeId - b.storeId);

      // Build variant quantities aggregated across stores
      this.variantQuantities = [];
      // Use stored variants list (updated when variants change) or fallback to form value
      const variants = this.productVariantsList.length > 0 ? this.productVariantsList : (this.productForm.value?.variants || []);
      if (variants.length === 0) {
        const variantGroups = normalized.filter(i => i.variantId != null && i.variantId > 0);
        const sums: { [id: number]: number } = {};
        variantGroups.forEach(i => {
          const vid = i.variantId as number;
          sums[vid] = (sums[vid] || 0) + (i.quantity || 0);
        });
        Object.keys(sums).forEach(k => {
          const vid = +k;
          this.variantQuantities.push({ id: vid, name: 'Variant #' + vid, quantity: sums[vid] });
          this.recallAmountByVariant[vid] = this.recallAmountByVariant[vid] || 1;
        });
      } else {
        variants.forEach((v: any) => {
          const sumQty = normalized
            .filter(i => i.variantId == v.id)
            .reduce((sum, i) => sum + (i.quantity || 0), 0);
          this.variantQuantities.push({ id: v.id || 0, name: v.name, quantity: sumQty });
          this.recallAmountByVariant[v.id] = this.recallAmountByVariant[v.id] || 1;
        })
      }

      // compute totals
      this.variantsTotal = this.variantQuantities.reduce((sum, v) => sum + (v.quantity || 0), 0);
      this.overallTotal = this.generalQuantity + this.variantsTotal;
    });
  }

  allocateToVariant() {
    if (!this.allocation.variantId || this.allocation.amount < 1 || this.allocation.amount > this.generalQuantity) {
      this.allocationMsg = 'الرجاء إدخال كمية صحيحة ومتغير صحيح';
      this.allocationSuccess = false;
      return;
    }
    const productId = this.productForm.get('id')?.value;
    // Try to identify the general inventoryId (for API)
    const generalInventory = this.inventoryData.find(i => !i.variantId || i.variantId == 0);
    if (!generalInventory) {
      this.allocationMsg = 'لا يوجد كمية عامة متاحة للتوزيع';
      this.allocationSuccess = false;
      return;
    }
    const body = {
      productId,
      storeId: generalInventory.storeId,
      generalInventoryId: generalInventory.id,
      generalQuantity: generalInventory.quantity,
      allocations: [
        {
          variantId: this.allocation.variantId,
          quantity: this.allocation.amount,
        },
      ],
    };
    this.productService.splitGeneralQuantityToVariants(body).subscribe({
      next: () => {
        this.allocationMsg = 'تم توزيع الكمية بنجاح';
        this.allocationSuccess = true;
        this.notification.success('تم توزيع الكمية بنجاح');
        this.loadInventory(productId);
        this.allocation.amount = 1;
        this.allocation.variantId = 0;
      },
      error: (err) => {
        this.allocationMsg = err.error?.message || 'حدث خطأ أثناء توزيع الكمية';
        this.allocationSuccess = false;
        this.notification.error(this.allocationMsg);
      }
    });
  }

  recallFromVariant(variantId: number) {
    const amount = this.recallAmountByVariant[variantId] || 0;
    if (amount < 1) {
      this.notification.error('الرجاء إدخال كمية صحيحة');
      return;
    }
    const productId = this.productForm.get('id')?.value;
    const generalInventory = this.inventoryData.find(i => !i.variantId || i.variantId == 0);
    if (!generalInventory) {
      this.notification.error('لا يوجد مخزون عام لاستقبال الكمية');
      return;
    }
    this.productService.RecallStockFromVariant(productId, {
      storeId: generalInventory.storeId,
      variantId: variantId,
      quantity: amount,
    }).subscribe({
      next: () => {
        this.notification.success('تم الاسترجاع للكمية العامة');
        this.loadInventory(productId);
      },
      error: (err) => {
        this.notification.error(err.error || 'فشل استرجاع الكمية');
      }
    })
  }

  productForm!: FormGroup;
  subCategories: SubCategory[] = [];
  units: Unit[] = [];
  imageToShow: any;
  initialImageUrl: string | null = null;
  url = environment.resourcesUrl;
  stores: Store[] = [];

  initializingForm(product: Product | null) {
    this.productForm = this.fb.group({
      id: [product?.id || 0],
      code: [product?.code || ''],
      nameEn: [product?.nameEn || ''],
      nameAr: [product?.nameAr || '', Validators.required],
      description: [product?.description || ''],
      reorderLevel: [product?.reorderLevel || 0],
      costPrice: [product?.costPrice, Validators.required],
      sellingPrice: [product?.sellingPrice, Validators.required],
      image: [null],
      isActive: [product?.isActive ?? true],
      subCategoryId: [product?.subCategoryId || null, Validators.required],
      unitId: [product?.unitId || null, Validators.required],
    });
    if(product) {
      this.initialImageUrl = this.url + product.imagePath;
      this.detectFiles(this.initialImageUrl);
    }
  }

  detectFiles(fileOrEvent: Event | File | string) {
    let file: File | null = null;

    if (fileOrEvent instanceof File) {
      // Case 1: Directly passed a File
      file = fileOrEvent;
    } else if (
      fileOrEvent &&
      fileOrEvent instanceof Event &&
      (fileOrEvent.target as HTMLInputElement)?.files?.length
    ) {
      // Case 2: File input change event
      file = (fileOrEvent.target as HTMLInputElement).files![0];
    }

    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imageToShow = e.target.result;
        this.image.setValue(file); // assuming `this.image` is your form control
      };
      reader.readAsDataURL(file);
    } else {
      this.imageToShow = fileOrEvent;
      this.image.setValue(fileOrEvent);
    }
  }

  save(): void {
    if (this.productForm.valid) {
      const formData = new FormData();
      formData.append('id', this.id.value);
      formData.append('code', this.code.value);
      formData.append('nameAr', this.nameAr.value);
      formData.append('nameEn', this.nameEn.value);
      formData.append('description', this.description.value);
      formData.append('reorderLevel', this.reorderLevel.value);
      formData.append('costPrice', this.costPrice.value);
      formData.append('sellingPrice', this.sellingPrice.value);

      const file = this.image.value;

      if (file instanceof File) {
        formData.append('image', file, file.name);
      }

      formData.append('isActive', this.isActive.value);
      formData.append('subCategoryId', this.subCategoryId.value);
      formData.append('unitId', this.unitId.value);
      this.productService.SaveProduct(formData).subscribe({
          next: (res) => {
            console.log(res);
            if(res) {
              this.notification.success('Successfully saved', 'Product');
              this.router.navigate(['../products'], { relativeTo: this.route })
            }
            else {
              this.notification.error('Error while saving', 'Product')
            }
          },
          error: (err) => {
            console.log(err.error)
            this.notification.error(err.error, 'Product')
          }
        });
    }
  }

  cancel(): void {
    this.router.navigate(['../products'], { relativeTo: this.route })
  }

  //---------------------------
  // Getters Method
  //---------------------------
  get id(): FormControl {
    return this.productForm.get('id') as FormControl;
  }
  get code(): FormControl {
    return this.productForm.get('code') as FormControl;
  }
  get nameAr(): FormControl {
    return this.productForm.get('nameAr') as FormControl;
  }
  get nameEn(): FormControl {
    return this.productForm.get('nameEn') as FormControl;
  }
  get description(): FormControl {
    return this.productForm.get('description') as FormControl;
  }
  get reorderLevel(): FormControl {
    return this.productForm.get('reorderLevel') as FormControl;
  }
  get costPrice(): FormControl {
    return this.productForm.get('costPrice') as FormControl;
  }
  get sellingPrice(): FormControl {
    return this.productForm.get('sellingPrice') as FormControl;
  }
  get image(): FormControl {
    return this.productForm.get('image') as FormControl;
  }
  get isActive(): FormControl {
    return this.productForm.get('isActive') as FormControl;
  }
  get subCategoryId(): FormControl {
    return this.productForm.get('subCategoryId') as FormControl;
  }
  get unitId(): FormControl {
    return this.productForm.get('unitId') as FormControl;
  }

  // Handle variants and modifiers updates
  onVariantsUpdated(variants: ProductVariant[]): void {
    console.log('Variants updated:', variants);
    // Store the updated variants list
    this.productVariantsList = variants || [];
    
    // If product is loaded, reload inventory to refresh the dropdown
    const productId = this.productForm.get('id')?.value;
    if (productId && productId > 0) {
      this.loadInventory(productId);
    }
  }

  onModifiersUpdated(modifiers: ProductModifierGroup[]): void {
    console.log('Modifiers updated:', modifiers);
    // You can add additional logic here if needed
  }
}
