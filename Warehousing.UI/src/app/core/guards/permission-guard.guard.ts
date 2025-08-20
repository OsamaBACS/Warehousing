import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';

export const PermissionGuard: CanActivateFn = 
(
  route, 
  state,
  authService: AuthService = inject(AuthService)
) => {
  const requiredPerms = route.data['permission'] as string[];
  // if (!requiredPerms || requiredPerms.length === 0) {
  //   return true; // Allow access if no permissions defined
  // }
  if(requiredPerms == undefined)
    return false;

  return requiredPerms.some(perm => authService.hasPermission(perm));
};
