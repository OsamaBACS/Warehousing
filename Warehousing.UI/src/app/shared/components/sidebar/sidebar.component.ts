import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService } from '../../../core/services/language.service';
import { CartService } from '../../services/cart.service';
import { filter, Subscription } from 'rxjs';

export interface SidebarMenuItem {
  label: string;
  route?: string | null;
  icon: string;
  permission?: string | string[] | null;
  badge?: () => number | null;
  orderType?: number;
  action?: () => void;
}

export interface SidebarMenuGroup {
  title?: string;
  items: SidebarMenuItem[];
}

export interface SidebarConfig {
  title: string;
  titleIcon: string;
  headerGradient: string;
  activeGradient: string;
  menuGroups: SidebarMenuGroup[];
  quickActions?: SidebarMenuItem[];
  showCart?: boolean;
  showLanguageToggle?: boolean;
  showLogout?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit, OnDestroy, OnChanges {
  @Input() config!: SidebarConfig;
  @Input() sidebarOpen = false;
  @Output() sidebarOpenChange = new EventEmitter<boolean>();
  
  private _sidebarOpen = false;
  
  currentRoute = '';
  isRTL = true;
  private subscriptions: Subscription[] = [];

  get sidebarOpenValue(): boolean {
    return this._sidebarOpen;
  }

  set sidebarOpenValue(value: boolean) {
    this._sidebarOpen = value;
    this.sidebarOpenChange.emit(value);
  }

  constructor(
    public router: Router,
    public authService: AuthService,
    public languageService: LanguageService,
    public cartService: CartService
  ) {}

  ngOnInit() {
    this._sidebarOpen = this.sidebarOpen;
    
    // Track current route for active menu highlighting
    const routeSub = this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.urlAfterRedirects;
      });
    this.subscriptions.push(routeSub);
    
    // Set initial route
    this.currentRoute = this.router.url;

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

  ngOnChanges(changes: SimpleChanges) {
    if (changes['sidebarOpen'] && changes['sidebarOpen'].currentValue !== undefined) {
      this._sidebarOpen = changes['sidebarOpen'].currentValue;
    }
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  hasAnyPermission(...perms: (string | string[] | undefined)[]): boolean {
    const flattened = perms.flat().filter(p => p !== undefined) as string[];
    if (flattened.length === 0) return true;
    return flattened.some(p => this.authService.hasPermission(p));
  }

  hasMenuItemPermission(item: SidebarMenuItem): boolean {
    if (!item.permission) return true;
    const perms = Array.isArray(item.permission) ? item.permission : [item.permission];
    return this.hasAnyPermission(...perms);
  }

  isActiveRoute(route?: string | null): boolean {
    if (!route) return false;
    const currentPath = this.currentRoute.split('?')[0];
    return currentPath === route || currentPath.startsWith(`${route}/`);
  }

  toggleLanguage() {
    this.languageService.setLanguage(this.languageService.currentLang === 'en' ? 'ar' : 'en');
  }

  getLanguageTooltip(): string {
    if (this.languageService.currentLang === 'ar') {
      return 'تبديل اللغة: الإنجليزية';
    } else {
      return 'Toggle Language: Arabic';
    }
  }

  logout() {
    localStorage.clear();
    this.cartService.clearCart();
    this.router.navigate(['/login']);
  }

  getUserDisplayName(): string {
    if (this.isRTL && this.authService.nameAr) {
      const nameAr = this.authService.nameAr;
      if (nameAr.includes('Ø') || nameAr.includes('Ù')) {
        return this.authService.username;
      }
      return nameAr;
    } else if (this.authService.nameEn) {
      return this.authService.nameEn;
    } else {
      return this.authService.username;
    }
  }

  executeAction(item: SidebarMenuItem) {
    if (item.action) {
      item.action();
    } else if (item.route) {
      this.router.navigate([item.route]);
    }
  }

  getCartCount(): number | null {
    let count: number | null = null;
    this.cartService.cartCount$.subscribe(c => count = c).unsubscribe();
    return count;
  }
}

