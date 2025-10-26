import { Injectable } from '@angular/core';
import { BehaviorSubject, filter } from 'rxjs';
import { Breadcrumb } from '../models/Breadcrumb';
import { NavigationEnd, Router, ActivatedRoute } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {

  private breadcrumbsSubject = new BehaviorSubject<Breadcrumb[]>([]);
  public breadcrumbs$ = this.breadcrumbsSubject.asObservable();

  // Route to label mapping
  private routeLabels: { [key: string]: string } = {
    'main': 'الرئيسية',
    'dashboard': 'لوحة التحكم',
    'products': 'المنتجات',
    'product-form': 'نموذج المنتج',
    'modifier-management': 'إدارة المكونات',
    'users': 'المستخدمين',
    'users-form': 'نموذج المستخدم',
    'users-devices': 'أجهزة المستخدمين',
    'roles': 'الصلاحيات',
    'roles-form': 'نموذج الصلاحية',
    'categories': 'التصنيفات',
    'category-form': 'نموذج التصنيف',
    'sub-categories': 'التصنيفات الفرعية',
    'sub-category-form': 'نموذج التصنيف الفرعي',
    'units': 'وحدات القياس',
    'unit-form': 'نموذج وحدة القياس',
    'stores': 'المستودعات',
    'store-form': 'نموذج المستودع',
    'store-transfers': 'تحويلات المستودعات',
    'suppliers': 'الموردين',
    'suppliers-form': 'نموذج المورد',
    'customers': 'العملاء',
    'customers-form': 'نموذج العميل',
    'orders': 'الطلبات',
    'order-list': 'قائمة الطلبات',
    'order-items-list': 'تفاصيل الطلب',
    'inventory-management': 'إدارة المخزون',
    'initial-stock': 'المخزون الابتدائي',
    'inventory-report': 'تقرير المخزون',
    'transactions': 'المعاملات',
    'company': 'الشركة',
    'company-form': 'نموذج الشركة',
    'reports': 'التقارير',
    'analysis': 'التحليل'
  };

  constructor(private router: Router) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.generateBreadcrumbs(event.url);
      });
  }

  private generateBreadcrumbs(url: string): void {
    const breadcrumbs: Breadcrumb[] = [];
    const urlSegments = url.split('/').filter(segment => segment !== '');
    
    // Always start with home
    breadcrumbs.push({ label: 'الرئيسية', route: '/admin/main' });
    
    // Generate breadcrumbs from URL segments, skipping 'admin' segment
    let currentPath = '/admin';
    for (let i = 1; i < urlSegments.length; i++) {
      const segment = urlSegments[i];
      
      // Skip the 'admin' segment itself
      if (segment === 'admin') {
        continue;
      }
      
      currentPath += `/${segment}`;
      
      // Skip query parameters
      if (segment.includes('?')) {
        const cleanSegment = segment.split('?')[0];
        const label = this.routeLabels[cleanSegment] || this.formatLabel(cleanSegment);
        breadcrumbs.push({ label, route: null });
      } else {
        const label = this.routeLabels[segment] || this.formatLabel(segment);
        const isLast = i === urlSegments.length - 1;
        breadcrumbs.push({ 
          label, 
          route: isLast ? null : currentPath 
        });
      }
    }
    
    this.breadcrumbsSubject.next(breadcrumbs);
  }

  private formatLabel(segment: string): string {
    // Convert kebab-case to readable text
    return segment
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  // Set breadcrumbs manually (for custom cases)
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
