import { ResolveFn } from '@angular/router';
import { StatusService } from '../services/status.service';
import { inject } from '@angular/core';
import { Status } from '../models/status';

export const StatusesResolver: ResolveFn<Status[]> = 
(
  route, 
  state,
  statusService: StatusService = inject(StatusService)
) => {
  return statusService.GetStatusList();
};
