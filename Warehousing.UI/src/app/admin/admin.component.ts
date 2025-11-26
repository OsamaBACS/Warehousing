import { Component, OnInit } from '@angular/core';
import { ThemeService } from '../shared/services/theme.service';
import { AuthService } from '../core/services/auth.service';
import { Router, NavigationEnd } from '@angular/router';
import { LanguageService } from '../core/services/language.service';
import { filter } from 'rxjs/operators';
import { SidebarConfigService } from '../shared/services/sidebar-config.service';
import { SidebarConfig } from '../shared/components/sidebar/sidebar.component';

@Component({
  selector: 'app-admin',
  standalone: false,
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminComponent implements OnInit {
  isDarkMode = false;
  sidebarOpen = false;
  sidebarConfig!: SidebarConfig;
  isRTL = true;

  constructor(
    private themeService: ThemeService,
    public authService: AuthService,
    public router: Router,
    public languageService: LanguageService,
    private sidebarConfigService: SidebarConfigService
  ) {}

  ngOnInit() {
    this.themeService.isDarkMode$.subscribe(mode => {
      this.isDarkMode = mode;
    });

    // Track current route for active menu highlighting
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        // Route tracking is handled by sidebar component
      });
    
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

    // Load sidebar configuration (unified)
    this.sidebarConfig = this.sidebarConfigService.getUnifiedConfig();
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar() {
    this.sidebarOpen = false;
  }
}
