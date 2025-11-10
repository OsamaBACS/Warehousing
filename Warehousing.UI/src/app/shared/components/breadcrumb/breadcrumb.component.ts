import { Component, OnInit } from '@angular/core';
import { BreadcrumbService } from '../../services/breadcrumb.service';
import { Breadcrumb } from '../../models/Breadcrumb';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss'],
  standalone: false
})
export class BreadcrumbComponent implements OnInit {
  breadcrumbs: Breadcrumb[] = [];
  isRTL = true;

  constructor(
    private breadcrumbService: BreadcrumbService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.breadcrumbService.breadcrumbs$.subscribe(breadcrumbs => {
      this.breadcrumbs = breadcrumbs;
    });

    // Set initial language direction
    this.isRTL = this.languageService.currentLang === 'ar';
    
    // Listen for document direction changes
    const observer = new MutationObserver(() => {
      this.isRTL = document.documentElement.getAttribute('dir') === 'rtl';
    });
    
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['dir']
    });
  }

}