import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AssignPermissionsDto } from '../models/AssignPermissionsDto';
import { Permission } from '../models/permission';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Permissions/';

  GetAllPermissions() {
    return this.http.get<Permission[]>(`${this.url}GetAllPermissions`)
  }

  AssignPermissionsToRole(assignPermissionsDto: AssignPermissionsDto) {
    return this.http.post(`${this.url}AssignPermissionsToRole`, assignPermissionsDto)
  }

  GetUserPermissions(userId: number) {
    return this.http.get(`${this.url}GetUserPermissions?userId=${userId}`)
  }
}
