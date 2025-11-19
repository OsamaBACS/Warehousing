import { Component, OnInit } from '@angular/core';
import { SubCategoryService } from '../../../services/sub-category.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Observable, map } from 'rxjs';
import { SubCategory } from '../../../models/SubCategory';
import { ImageUrlService } from '../../../../shared/services/image-url.service';

@Component({
  selector: 'app-sub-category',
  standalone: false,
  templateUrl: './sub-category.component.html',
  styleUrl: './sub-category.component.scss'
})
export class SubCategoryComponent implements OnInit {

  constructor(
    private subCategoryService: SubCategoryService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    public authService: AuthService,
    public imageUrlService: ImageUrlService
  ){}

  ngOnInit(): void {
    this.loadcategorys();
    this.serverUrl = this.subCategoryService.url.substring(0, this.subCategoryService.url.indexOf('api'));
  }

  subCategorys$!: Observable<SubCategory[]>;
  serverUrl = '';

  loadcategorys() {
    this.subCategorys$ = this.subCategoryService.GetSubCategories().pipe(
      map(subCategories => {
        // Filter sub-categories based on user permissions
        return subCategories.filter(subCategory => this.authService.hasSubCategory(subCategory.id!));
      })
    );
  }

  onEdit(categoryId: number | null): void {
    if (categoryId) {
      this.router.navigate(['../sub-category-form'], { relativeTo: this.route, queryParams: { categoryId } });
    } else {
      this.router.navigate(['../sub-category-form'], { relativeTo: this.route });
    }
  }

}
