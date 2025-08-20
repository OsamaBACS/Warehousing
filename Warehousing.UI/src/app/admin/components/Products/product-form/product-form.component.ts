import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ProductsService } from '../../../services/products.service';
import { CategoriesService } from '../../../services/categories.service';
import { UnitsService } from '../../../services/units.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Category } from '../../../models/category';
import { Unit } from '../../../models/unit';
import { environment } from '../../../../../environments/environment';
import { Product } from '../../../models/product';
import { Store } from '../../../models/store';
import { SubCategory } from '../../../models/SubCategory';

@Component({
  selector: 'app-product-form',
  standalone: false,
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnInit {

  constructor(
    private fb: FormBuilder,
    private productService: ProductsService,
    private categoriesService: CategoriesService,
    private unitsService: UnitsService,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
    this.subCategories = this.route.snapshot.data['subCategoriesResolver'];
  }

  ngOnInit(): void {
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const productId = +params['productId'];
      if (productId) {
        this.productService.GetProductById(productId)
          .subscribe({
            next: (res) => {
              this.initializingForm(res)
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
      openingBalance: [product?.openingBalance, Validators.required],
      reorderLevel: [product?.reorderLevel, Validators.required],
      quantityInStock: [product?.quantityInStock, Validators.required],
      costPrice: [product?.costPrice, Validators.required],
      sellingPrice: [product?.sellingPrice, Validators.required],
      image: [null],
      isActive: [product?.isActive ?? true],
      subCategoryId: [product?.subCategoryId || null, Validators.required],
      unitId: [product?.unitId || null, Validators.required],
      storeId: [product?.storeId || null, Validators.required],
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
      formData.append('openingBalance', this.openingBalance.value);
      formData.append('reorderLevel', this.reorderLevel.value);
      formData.append('quantityInStock', this.quantityInStock.value);
      formData.append('costPrice', this.costPrice.value);
      formData.append('sellingPrice', this.sellingPrice.value);
      formData.append('storeId', this.storeId.value);

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
  get openingBalance(): FormControl {
    return this.productForm.get('openingBalance') as FormControl;
  }
  get reorderLevel(): FormControl {
    return this.productForm.get('reorderLevel') as FormControl;
  }
  get quantityInStock(): FormControl {
    return this.productForm.get('quantityInStock') as FormControl;
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
  get storeId(): FormControl {
    return this.productForm.get('storeId') as FormControl;
  }
}
