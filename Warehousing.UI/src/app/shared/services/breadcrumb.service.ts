import { Injectable } from '@angular/core';
import { BehaviorSubject, filter } from 'rxjs';
import { Breadcrumb } from '../models/Breadcrumb';
import { NavigationEnd, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {

  private breadcrumbsSubject = new BehaviorSubject<Breadcrumb[]>([]);
  public breadcrumbs$ = this.breadcrumbsSubject.asObservable();

  constructor(private router: Router) {
    // Optional: Reset breadcrumbs on navigation
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.reset();
      });
  }

  // Set breadcrumbs manually
  setBreadcrumbs(breadcrumbs: Breadcrumb[]): void {
    this.breadcrumbsSubject.next(breadcrumbs);
  }

  // Add breadcrumb dynamically
  addBreadcrumb(label: string, route: string | null): void {
    const current = this.breadcrumbsSubject.value;
    const newBreadcrumbs = [...current, { label, route }];
    this.breadcrumbsSubject.next(newBreadcrumbs);
  }

  // Reset breadcrumbs
  reset(): void {
    this.breadcrumbsSubject.next([]);
  }

  // Set breadcrumbs from array
  setFrom(routeArray: { label: string; route: string | null }[]) {
    this.breadcrumbsSubject.next(routeArray);
  }
}
