import { Component, OnInit } from '@angular/core';
import { CategoriesService } from '../../../services/categories.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { Observable } from 'rxjs';
import { Category } from '../../../models/category';

@Component({
  selector: 'app-category',
  standalone: false,
  templateUrl: './category.component.html',
  styleUrl: './category.component.scss'
})
export class CategoryComponent implements OnInit {

  constructor(
    private categoriesService: CategoriesService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService
  ){}

  ngOnInit(): void {
    this.loadcategorys();
    this.serverUrl = this.categoriesService.url.substring(0, this.categoriesService.url.indexOf('api'));
  }

  categorys$!: Observable<Category[]>;
  serverUrl = '';

  loadcategorys() {
    this.categorys$ = this.categoriesService.GetCategories();
  }

  onEdit(categoryId: number | null): void {
    if (categoryId) {
      this.router.navigate(['../category-form'], { relativeTo: this.route, queryParams: { categoryId } });
    } else {
      this.router.navigate(['../category-form'], { relativeTo: this.route });
    }
  }

}
