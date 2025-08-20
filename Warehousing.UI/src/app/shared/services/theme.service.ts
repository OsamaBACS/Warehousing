import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private darkModeSubject = new BehaviorSubject<boolean>(false);
  isDarkMode$ = this.darkModeSubject.asObservable();

  constructor() {
    const saved = localStorage.getItem('dark-mode');
    this.darkModeSubject.next(saved === 'true');
  }

  toggle() {
    const current = this.darkModeSubject.value;
    const newMode = !current;
    this.darkModeSubject.next(newMode);
    localStorage.setItem('dark-mode', newMode.toString());
  }

  setDarkMode(mode: boolean) {
    this.darkModeSubject.next(mode);
    localStorage.setItem('dark-mode', mode.toString());
  }

  get isDarkMode(): boolean {
    return this.darkModeSubject.value;
  }
}
