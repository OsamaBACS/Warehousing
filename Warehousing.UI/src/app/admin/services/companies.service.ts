import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Company, CompanyPagination } from '../models/Company';

@Injectable({
  providedIn: 'root'
})
export class CompaniesService {

  url = environment.baseUrl + '/Company/';

  constructor(private http: HttpClient) { }

  GetCompaniesPagination(pageIndex: number, pageSize: number) {
    return this.http.get<CompanyPagination>(
      `${this.url}GetCompaniesPagination?pageIndex=${pageIndex}&pageSize=${pageSize}`
    );
  }

  SearchCompaniesPagination(pageIndex: number, pageSize: number, keyword: string) {
    return this.http.get<CompanyPagination>(
      `${this.url}SearchCompaniesPagination?pageIndex=${pageIndex}&pageSize=${pageSize}&keyword=${keyword}`
    );
  }

  SaveCompany(company: any) {
    return this.http.post<any>(`${this.url}SaveCompany`, company);
  }

  GetCompanyById(companyId: number) {
    return this.http.get<Company>(`${this.url}GetCompanyById?Id=${companyId}`);
  }

  GetCompanies() {
    return this.http.get<Company>(`${this.url}GetCompanies`);
  }
}
