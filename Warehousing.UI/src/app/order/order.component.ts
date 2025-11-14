import { Component, OnInit } from '@angular/core';
import { ThemeService } from '../shared/services/theme.service';
import { AuthService } from '../core/services/auth.service';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { LanguageService } from '../core/services/language.service';
import { CartService } from '../shared/services/cart.service';
import { filter } from 'rxjs/operators';
import { SidebarConfigService } from '../shared/services/sidebar-config.service';
import { SidebarConfig } from '../shared/components/sidebar/sidebar.component';

@Component({
  selector: 'app-order',
  standalone: false,
  templateUrl: './order.component.html',
  styleUrl: './order.component.scss'
})
export class OrderComponent implements OnInit {
  isDarkMode = false;
  sidebarOpen = false;
  sidebarConfig!: SidebarConfig;
  isRTL = true;
  orderTypeId: number = 2; // Default to sale orders

  constructor(
    private themeService: ThemeService,
    public authService: AuthService,
    public router: Router,
    public languageService: LanguageService,
    public cartService: CartService,
    private route: ActivatedRoute,
    private sidebarConfigService: SidebarConfigService
  ) {}

  ngOnInit() {
    this.themeService.isDarkMode$.subscribe(mode => {
      this.isDarkMode = mode;
    });

    // Get orderTypeId from route
    this.route.params.subscribe(params => {
      this.orderTypeId = +params['orderTypeId'] || 2;
    });

    // Track current route for active menu highlighting
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        // Route tracking is handled by sidebar component
        // Extract orderTypeId from route
        const match = event.urlAfterRedirects.match(/\/order\/(\d+)/);
        if (match) {
          this.orderTypeId = +match[1];
        }
      });
    
    // Set initial route
    // Set initial language direction
    this.isRTL = this.languageService.currentLang === 'ar';
    
    // On desktop, sidebar should always be open
    if (window.innerWidth >= 1024) {
      this.sidebarOpen = true;
    }
    
    // Listen for document direction changes
    const observer = new MutationObserver(() => {
      this.isRTL = document.documentElement.getAttribute('dir') === 'rtl';
    });
    
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['dir']
    });

    // Load sidebar configuration
    this.sidebarConfig = this.sidebarConfigService.getOrderConfig();
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar() {
    this.sidebarOpen = false;
  }
}
