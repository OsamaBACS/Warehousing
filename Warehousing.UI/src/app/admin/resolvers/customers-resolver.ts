import { ResolveFn } from '@angular/router';
import { CustomersService } from '../services/customers.service';
import { inject } from '@angular/core';
import { Customer } from '../models/customer';

export const customersResolver: ResolveFn<Customer[]> = 
(
  route, 
  state,
  customersService: CustomersService = inject(CustomersService)
) => {
  return customersService.GetCustomers();
};
