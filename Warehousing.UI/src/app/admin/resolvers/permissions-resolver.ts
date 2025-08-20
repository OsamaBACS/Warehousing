import { ResolveFn } from '@angular/router';
import { Permission } from '../models/permission';
import { PermissionService } from '../services/permission.service';
import { inject } from '@angular/core';

export const PermissionsResolver: ResolveFn<Permission[]> = 
(
  route, 
  state,
  permissionService: PermissionService = inject(PermissionService)
) => {
  return permissionService.GetAllPermissions();
};
