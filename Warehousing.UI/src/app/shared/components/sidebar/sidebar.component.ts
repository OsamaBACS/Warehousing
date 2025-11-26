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
  expandedGroups: Map<string, boolean> = new Map();

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
        this.autoExpandActiveGroup();
      });
    this.subscriptions.push(routeSub);
    
    // Set initial route
    this.currentRoute = this.router.url;
    
    // Initialize expanded groups - expand first group by default, and groups with active routes
    this.initializeExpandedGroups();

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

  getGroupKey(group: SidebarMenuGroup, index: number): string {
    return group.title || `group-${index}`;
  }

  isGroupExpanded(group: SidebarMenuGroup, index: number): boolean {
    // Groups without titles are always expanded (not collapsible)
    if (!group.title) return true;
    
    const key = this.getGroupKey(group, index);
    return this.expandedGroups.get(key) ?? true; // Default to expanded
  }

  toggleGroup(group: SidebarMenuGroup, index: number): void {
    // Only toggle groups with titles
    if (!group.title) return;
    
    const key = this.getGroupKey(group, index);
    const currentState = this.expandedGroups.get(key) ?? true;
    this.expandedGroups.set(key, !currentState);
  }

  initializeExpandedGroups(): void {
    if (!this.config?.menuGroups) return;
    
    // Expand groups that contain the active route
    this.config.menuGroups.forEach((group, index) => {
      const key = this.getGroupKey(group, index);
      const hasActiveRoute = group.items.some(item => 
        item.route && this.isActiveRoute(item.route)
      );
      // Default to expanded if it has active route or is first group
      this.expandedGroups.set(key, hasActiveRoute || index === 0);
    });
  }

  autoExpandActiveGroup(): void {
    if (!this.config?.menuGroups) return;
    
    // Find and expand the group containing the active route
    this.config.menuGroups.forEach((group, index) => {
      const key = this.getGroupKey(group, index);
      const hasActiveRoute = group.items.some(item => 
        item.route && this.isActiveRoute(item.route)
      );
      if (hasActiveRoute) {
        this.expandedGroups.set(key, true);
      }
    });
  }

  hasGroupItems(group: SidebarMenuGroup): boolean {
    return group.items.some(item => this.hasMenuItemPermission(item));
  }
}

