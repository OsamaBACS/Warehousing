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
      nameEn: [category ? category.nameAr : '', Validators.required],
      nameAr: [category ? category.nameEn : '', Validators.required],
      description: [category ? category.description : ''],
      isActive: [category ? category.isActive : true]
    });
  }

  onSubmit() {
    if (this.categoryForm.valid) {
      this.categoriesService.SaveCategory(this.categoryForm.value).subscribe({
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
