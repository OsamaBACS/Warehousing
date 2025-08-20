import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Breadcrumb } from '../../models/Breadcrumb';
import { BreadcrumbService } from '../../services/breadcrumb.service';

@Component({
  selector: 'app-breadcrumb',
  standalone: false,
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent {

  breadcrumbs: Breadcrumb[] = [];

  constructor(
    private breadcrumbService: BreadcrumbService,
    private router: Router
  ) {
    this.breadcrumbService.breadcrumbs$.subscribe(breadcrumbs => {
      this.breadcrumbs = breadcrumbs;
    });
  }

  navigate(route: string | null): void {
    if (route) {
      this.router.navigateByUrl(route);
    }
  }
}
