import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {

  private _currentLang = 'ar';
  constructor(
    private translate: TranslateService
  ) {
    // Initialize with Arabic as default
    this.translate.setDefaultLang('ar');
    this.translate.use('ar');
    document.documentElement.setAttribute('lang', 'ar');
    document.documentElement.setAttribute('dir', 'rtl');
  }

  get currentLang(): string {
    return this._currentLang;
  }

  setLanguage(lang: string) {
    this._currentLang = lang;
    document.documentElement.setAttribute('lang', lang);
    document.documentElement.setAttribute('dir', lang === 'ar' ? 'rtl' : 'ltr');
    this.translate.use(lang);
  }
}
