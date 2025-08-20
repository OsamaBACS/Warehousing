import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Customer } from '../models/customer';

@Injectable({
  providedIn: 'root'
})
export class CustomersService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Customers/';

  GetCustomers() {
    return this.http.get<Customer[]>(`${this.url}GetCustomers`);
  }

  GetCustomerById(id: number) {
    return this.http.get<Customer>(`${this.url}GetCustomerById?id=${id}`);
  }

  SaveCustomer(customer: Customer){
    return this.http.post(`${this.url}SaveCustomer`, customer);
  }
}
