import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Store } from '../models/store';

@Injectable({
  providedIn: 'root'
})
export class StoreService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Stores/';

  GetStores() {
    return this.http.get<Store[]>(`${this.url}GetStores`);
  }

  GetStoreById(storeId: number) {
    return this.http.get<Store>(`${this.url}GetStoreById?Id=${storeId}`);
  }

  SaveStore(Store: any) {
    return this.http.post<any>(`${this.url}SaveStore`, Store);
  }
}
