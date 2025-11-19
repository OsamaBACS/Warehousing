import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SubCategoryService } from '../../../services/sub-category.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SubCategory } from '../../../models/SubCategory';
import { Category } from '../../../models/category';
import { ImageUrlService } from '../../../../shared/services/image-url.service';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-sub-category-form',
  standalone: false,
  templateUrl: './sub-category-form.component.html',
  styleUrl: './sub-category-form.component.scss'
})
export class SubCategoryFormComponent implements OnInit {

  subCategoryForm!: FormGroup;
  categories: Category[] = [];
  serverUrl = environment.resourcesUrl || '';

  constructor(
    private fb: FormBuilder,
    private subCategoryService: SubCategoryService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    public imageUrlService: ImageUrlService
  ) {
    this.categories = this.route.snapshot.data['categoriesResolver'];
  }

  ngOnInit(): void {
    this.initializingForm(null);

    this.route.queryParams.subscribe(params => {
      const categoryId = +params['categoryId'];
      if (categoryId) {
        this.subCategoryService.GetSubCategoryById(categoryId).subscribe({
          next: (res) => {
            this.initializingForm(res);
          },
          error: (err) => {
          }
        });
      }
    });
  }

  initializingForm(subCategory: SubCategory | null) {
    this.subCategoryForm = this.fb.group({
      id: [subCategory ? subCategory.id : 0],
      nameEn: [subCategory ? subCategory.nameEn : '', Validators.required],
      nameAr: [subCategory ? subCategory.nameAr : '', Validators.required],
      categoryId: [subCategory ? subCategory.categoryId : null, Validators.required],
      description: [subCategory ? subCategory.description : ''],
      imagePath: [subCategory ? subCategory.imagePath : ''],
      image: [null],
      isActive: [subCategory ? subCategory.isActive : true]
    });
  }

  onSubmit() {
    if (this.subCategoryForm.valid) {
      const formData = new FormData();
      formData.append('id', this.subCategoryForm.get('id')?.value);
      formData.append('nameEn', this.subCategoryForm.get('nameEn')?.value);
      formData.append('nameAr', this.subCategoryForm.get('nameAr')?.value);
      formData.append('categoryId', this.subCategoryForm.get('categoryId')?.value);
      formData.append('description', this.subCategoryForm.get('description')?.value);
      formData.append('isActive', this.subCategoryForm.get('isActive')?.value);

      const imageFile = this.subCategoryForm.get('image')?.value;
      if (imageFile instanceof File) {
        formData.append('image', imageFile, imageFile.name);
      }

      this.subCategoryService.SaveSubCategory(formData).subscribe({
        next: (res) => {
          if(res) {
            this.toastr.success('تم اضافة تصنيف بنجاح', 'subCategory');
            this.router.navigate(['../sub-category'], { relativeTo: this.route });
          }
          else {
            this.toastr.error(res, 'subCategory')
          }
        },
        error: (err) => {
          this.toastr.error(err.error, 'subCategory')
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['../sub-category'], { relativeTo: this.route });
  }
}