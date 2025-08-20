import { ResolveFn } from '@angular/router';
import { CompaniesService } from '../services/companies.service';
import { inject } from '@angular/core';
import { Company } from '../models/Company';

export const CompaniesResolver: ResolveFn<Company> = 
(
  route, 
  state,
  companiesService: CompaniesService = inject(CompaniesService)
) => {
  return companiesService.GetCompanies();
};
