import { Component, Input, OnInit } from '@angular/core';
import { Company } from '../../../admin/models/Company';
import { CompaniesService } from '../../../admin/services/companies.service';
import { LanguageService } from '../../../core/services/language.service';
import { Observable } from 'rxjs';
import { StoreService } from '../../../admin/services/store.service';
import { Store } from '../../../admin/models/store';
import { User } from '../../../admin/models/users';
import { UsersService } from '../../../admin/services/users.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-company-header',
  standalone: false,
  templateUrl: './company-header.component.html',
  styleUrl: './company-header.component.scss'
})
export class CompanyHeaderComponent implements OnInit {
  companies$!: Observable<Company>;
  user$!: Observable<User>;

  constructor(
    private companiesService: CompaniesService,
    public lang: LanguageService,
    private usersService: UsersService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.user$ = this.usersService.GetUserById(+this.authService.userId);
    //this.companies$ = this.companiesService.GetCompanies();
  }
}