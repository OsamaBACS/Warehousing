import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SubCategoryService } from '../../../services/sub-category.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SubCategory } from '../../../models/SubCategory';
import { Category } from '../../../models/category';

@Component({
  selector: 'app-sub-category-form',
  standalone: false,
  templateUrl: './sub-category-form.component.html',
  styleUrl: './sub-category-form.component.scss'
})
export class SubCategoryFormComponent implements OnInit {

  subCategoryForm!: FormGroup;
  categories: Category[] = [];

  constructor(
    private fb: FormBuilder,
    private subCategoryService: SubCategoryService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
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
            console.error(err.error);
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
      isActive: [subCategory ? subCategory.isActive : true]
    });
  }

  onSubmit() {
    if (this.subCategoryForm.valid) {
      this.subCategoryService.SaveSubCategory(this.subCategoryForm.value).subscribe({
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