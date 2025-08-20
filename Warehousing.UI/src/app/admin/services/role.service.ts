import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Role, RoleDto } from '../models/role';
import { RoleCreateUpdateDto } from '../models/RoleCreateUpdateDto';
import { AssignRoleDto } from '../models/AssignRoleDto';

@Injectable({
  providedIn: 'root'
})
export class RoleService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Roles/';

  GetRoles() {
    return this.http.get<Role[]>(`${this.url}GetRoles`);
  }

  GetRoleById(id: number) {
    return this.http.get<RoleDto>(`${this.url}GetRoleById?id=${id}`);
  }

  saveRole(role: RoleCreateUpdateDto) {
    return this.http.post(`${this.url}SaveRole`, role);
  }

  deleteRole(id: number) {
    return this.http.delete(`${this.url}?id=${id}`)
  }

  AssignRoleToUser(role: AssignRoleDto) {
    return this.http.post(`${this.url}AssignRoleToUser`, role)
  }
}
