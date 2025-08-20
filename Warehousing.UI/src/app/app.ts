import { Component, OnInit, Renderer2 } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './shared/services/theme.service';
import { LanguageService } from './core/services/language.service';
import { CartService } from './shared/services/cart.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected title = 'Warehousing.UI';
  isDarkMode = false;
  cartCount = 3;
  username = '';

  constructor(
    private router: Router,
    private renderer: Renderer2,
    public authService: AuthService,
    public theme: ThemeService,
    private languageService: LanguageService,
    public cartService: CartService
  ) { }

  ngOnInit() {
    // if(location.protocol !== 'https:' && location.hostname !== 'localhost') {
    //   location.href = 'https:' + window.location.href.substring(window.location.protocol.length);
    // }
    this.theme.isDarkMode$.subscribe(mode => {
      this.isDarkMode = mode;
      if (mode) {
        this.renderer.addClass(document.body, 'dark-mode');
      } else {
        this.renderer.removeClass(document.body, 'dark-mode');
      }
    });
    this.authService.username$.subscribe(un => {
      this.username = un;
    });
  }

  get currentLang() {
    return this.languageService.currentLang;
  }

  setLanguage(lang: string) {
    this.languageService.setLanguage(lang);
    // Optionally reload translations or layout changes
  }

  toggleLanguage() {
    this.languageService.setLanguage(this.currentLang === 'en' ? 'ar' : 'en');
  }

  toggleDarkMode(): void {
    this.theme.toggle();
  }

  fetchSavedOrders() {
    
  }

  logout() {
    localStorage.clear();
    this.cartService.clearCart();
    this.router.navigate(['/login']);
  }
}
