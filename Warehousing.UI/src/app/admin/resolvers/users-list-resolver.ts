import { ResolveFn } from '@angular/router';
import { User } from '../models/users';
import { UsersService } from '../services/users.service';
import { inject } from '@angular/core';

export const UsersListResolver: ResolveFn<User[]> = 
(
  route, 
  state,
  usersService: UsersService = inject(UsersService)
) => {
  return usersService.GetUsers();
};
