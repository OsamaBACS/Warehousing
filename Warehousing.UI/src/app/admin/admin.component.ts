import { Component, OnInit } from '@angular/core';
import { ThemeService } from '../shared/services/theme.service';

@Component({
  selector: 'app-admin',
  standalone: false,
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminComponent implements OnInit {
  isDarkMode = false;

  constructor(private themeService: ThemeService) {}

  ngOnInit() {
    this.themeService.isDarkMode$.subscribe(mode => {
      this.isDarkMode = mode;
    });
  }

  toggleDarkMode() {}
}