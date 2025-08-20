import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Supplier } from '../models/supplier';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Supplier/';

  GetSuppliers() {
    return this.http.get<Supplier[]>(`${this.url}GetSuppliers`)
  }

  GetSupplierById(id: number) {
    return this.http.get<Supplier>(`${this.url}GetSupplierById?id=${id}`);
  }

  SaveSupplier(Supplier: Supplier) {
    return this.http.post(`${this.url}SaveSupplier`, Supplier);
  }
}
