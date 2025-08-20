import { ResolveFn } from '@angular/router';
import { SupplierService } from '../services/supplier.service';
import { inject } from '@angular/core';
import { Supplier } from '../models/supplier';

export const SuppliersResolver: ResolveFn<Supplier[]> = 
(
  route, 
  state,
  supplierService: SupplierService = inject(SupplierService)
) => {
  return supplierService.GetSuppliers();
};
