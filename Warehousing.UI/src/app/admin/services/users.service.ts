import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { User, UserPagination } from '../models/users';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Users/';

  GetUsersPagination(pageIndex: number, pageSize: number) {
    return this.http.get<UserPagination>(
      this.url +
      `GetUsersPagination?pageIndex=${pageIndex}` +
      `&pageSize=${pageSize}`
    );
  }

  SearchUsersPagination(pageIndex: number, pageSize: number, keyword: string) {
    return this.http.get<UserPagination>(
      this.url +
      `SearchUsersPagination?pageIndex=${pageIndex}` +
      `&pageSize=${pageSize}` +
      `&keyword=${keyword}`
    );
  }

  SaveUser(user: User) {
    return this.http.post<any>(`${this.url}SaveUser`, user);
  }

  GetUsers() {
    return this.http.get<User[]>(`${this.url}GetUsers`);
  }

  GetUserById(id: number) {
    return this.http.get<any>(`${this.url}GetUserById?id=${id}`);
  }

  ChangePasswordForAdmin(id: number) {
    return this.http.get<any>(`${this.url}ChangePasswordForAdmin?id=${id}`);
  }
}
