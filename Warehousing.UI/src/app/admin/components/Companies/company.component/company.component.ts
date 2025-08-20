import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { CompaniesService } from '../../../services/companies.service';
import { LanguageService } from '../../../../core/services/language.service';
import { CompanyPagination } from '../../../models/Company';

@Component({
  selector: 'app-company.component',
  standalone: false,
  templateUrl: './company.component.html',
  styleUrl: './company.component.scss'
})
export class CompanyComponent implements OnInit {

  constructor(
    private fb: FormBuilder,
    private companiesService: CompaniesService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService
  ) {}

  companies$!: Observable<CompanyPagination>;
  companyForm!: FormGroup;
  pageIndex = 1;
  pageSize = 8;
  totalPages = 1;
  totalPagesArray: number[] = [];
  serverUrl = '';

  ngOnInit(): void {
    this.loadCompanies();

    this.companyForm = this.fb.group({
      id: [null],
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      addressAr: ['', Validators.required],
      addressEn: ['', Validators.required],
      termsAr: ['', Validators.required],
      termsEn: ['', Validators.required],
      footerNoteAr: ['', Validators.required],
      footerNoteEn: ['', Validators.required],
      taxNumber: ['', Validators.required],
      currencyCode: ['', Validators.required],
      logoUrl: ['']
    });

    this.serverUrl = this.companiesService.url.substring(0, this.companiesService.url.indexOf('api'));
  }

  loadCompanies(): void {
    this.companies$ = this.companiesService.GetCompaniesPagination(this.pageIndex, this.pageSize).pipe(
      tap(res => {
        this.totalPages = Math.ceil(res.totals / this.pageSize);
        this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
      })
    );
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex = page;
    this.loadCompanies();
  }

  getPageRange(): number[] {
    const delta = 2; // how many pages to show before/after current
    const range: number[] = [];

    const left = Math.max(2, this.pageIndex - delta);
    const right = Math.min(this.totalPages - 1, this.pageIndex + delta);

    for (let i = left; i <= right; i++) {
      range.push(i);
    }

    return range;
  }

  openForm(companyId: number | null) {
    if (companyId) {
      this.router.navigate(['../company-form'], { relativeTo: this.route, queryParams: { companyId } });
    } else {
      this.router.navigate(['../company-form'], { relativeTo: this.route });
    }
  }
}
