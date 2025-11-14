import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoriesService } from '../../../services/categories.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Category } from '../../../models/category';

@Component({
  selector: 'app-category-form',
  standalone: false,
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.scss'
})
export class CategoryFormComponent implements OnInit {

  categoryForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private categoriesService: CategoriesService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {
  }

  ngOnInit(): void {
    this.initializingForm(null);

    this.route.queryParams.subscribe(params => {
      const categoryId = +params['categoryId'];
      if (categoryId) {
        this.categoriesService.GetCategoryById(categoryId).subscribe({
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

  initializingForm(category: Category | null) {
    this.categoryForm = this.fb.group({
      id: [category ? category.id : 0],
      nameEn: [category ? category.nameEn : ''],
      nameAr: [category ? category.nameAr : '', Validators.required],
      description: [category ? category.description : ''],
      imagePath: [category ? category.imagePath : ''],
      image: [null],
      isActive: [category ? category.isActive : true]
    });
  }

  onSubmit() {
    if (this.categoryForm.valid) {
      const formData = new FormData();
      formData.append('id', this.categoryForm.get('id')?.value);
      formData.append('nameEn', this.categoryForm.get('nameEn')?.value);
      formData.append('nameAr', this.categoryForm.get('nameAr')?.value);
      formData.append('description', this.categoryForm.get('description')?.value);
      formData.append('isActive', this.categoryForm.get('isActive')?.value);

      const imageFile = this.categoryForm.get('image')?.value;
      if (imageFile instanceof File) {
        formData.append('image', imageFile, imageFile.name);
      }

      this.categoriesService.SaveCategory(formData).subscribe({
        next: (res) => {
          if(res) {
            this.toastr.success('تم اضافة تصنيف بنجاح', 'category');
            this.router.navigate(['../category'], { relativeTo: this.route });
          }
          else {
            this.toastr.error(res, 'category')
          }
        },
        error: (err) => {
          this.toastr.error(err.error, 'category')
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['../category'], { relativeTo: this.route });
  }
}
