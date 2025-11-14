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
  private manualBreadcrumbsSet = false;

  // Route to label mapping
  private routeLabels: { [key: string]: string } = {
    'main': 'الرئيسية',
    'dashboard': 'لوحة التحكم',
    'products': 'المنتجات',
    'product-form': 'نموذج المنتج',
    'product-detail': 'تفاصيل المنتج',
    'product-details': 'تفاصيل المنتج',
    'modifier-management': 'إدارة المكونات',
    'users': 'المستخدمين',
    'users-form': 'نموذج المستخدم',
    'users-devices': 'أجهزة المستخدمين',
    'roles': 'الأدوار',
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
    'printer-configurations': 'إعدادات الطابعة',
    'printer-configuration-form': 'نموذج إعدادات الطابعة',
    'company-form': 'نموذج الشركة',
    'reports': 'التقارير',
    'analysis': 'التحليل'
  };

  constructor(private router: Router) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        // Clear breadcrumbs on navigation and reset flag
        // Components will set breadcrumbs in ngOnInit if needed
        this.manualBreadcrumbsSet = false;
        this.breadcrumbsSubject.next([]);
        
        // Auto-generate after a short delay to allow components to set manual breadcrumbs
        setTimeout(() => {
          if (!this.manualBreadcrumbsSet && this.breadcrumbsSubject.value.length === 0) {
            this.generateBreadcrumbs(event.url);
          }
        }, 150);
      });
  }

  private generateBreadcrumbs(url: string): void {
    // Don't auto-generate if breadcrumbs were manually set
    if (this.manualBreadcrumbsSet || this.breadcrumbsSubject.value.length > 0) {
      return;
    }

    const breadcrumbs: Breadcrumb[] = [];
    const urlSegments = url.split('/').filter(segment => segment !== '' && segment !== '?');
    
    // Always start with home
    breadcrumbs.push({ label: 'الرئيسية', route: '/admin/dashboard' });
    
    // Generate breadcrumbs from URL segments
    let currentPath = '';
    let foundAdmin = false;
    
    for (let i = 0; i < urlSegments.length; i++) {
      const segment = urlSegments[i];
      const cleanSegment = segment.split('?')[0]; // Remove query params
      
      // Mark when we find 'admin' segment
      if (segment === 'admin') {
        foundAdmin = true;
        currentPath = '/admin';
        continue;
      }
      
      // Only process segments after 'admin'
      if (!foundAdmin) {
        continue;
      }
      
      // Skip numeric segments (IDs) - they shouldn't appear in breadcrumbs
      if (/^\d+$/.test(cleanSegment)) {
        // Keep the path updated but don't add breadcrumb for IDs
        currentPath += `/${cleanSegment}`;
        continue;
      }
      
      currentPath += `/${cleanSegment}`;
      
      // Get label from mapping or format it
      const label = this.routeLabels[cleanSegment] || this.formatLabel(cleanSegment);
      const isLast = i === urlSegments.length - 1;
      
      breadcrumbs.push({ 
        label, 
        route: isLast ? null : currentPath 
      });
    }
    
    // Only update if we have breadcrumbs to show
    if (breadcrumbs.length > 1) {
      this.breadcrumbsSubject.next(breadcrumbs);
    }
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
    this.manualBreadcrumbsSet = true;
    this.breadcrumbsSubject.next(breadcrumbs);
  }

  // Add breadcrumb dynamically
  addBreadcrumb(label: string, route: string | null): void {
    this.manualBreadcrumbsSet = true;
    const current = this.breadcrumbsSubject.value;
    const newBreadcrumbs = [...current, { label, route }];
    this.breadcrumbsSubject.next(newBreadcrumbs);
  }

  // Reset breadcrumbs
  reset(): void {
    this.manualBreadcrumbsSet = false;
    this.breadcrumbsSubject.next([]);
  }

  // Set breadcrumbs from array
  setFrom(routeArray: { label: string; route: string | null }[]) {
    this.manualBreadcrumbsSet = true;
    this.breadcrumbsSubject.next(routeArray);
  }
}
