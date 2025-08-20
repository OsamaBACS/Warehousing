import { ResolveFn } from '@angular/router';
import { Store } from '../models/store';
import { StoreService } from '../services/store.service';
import { inject } from '@angular/core';

export const StoresResolver: ResolveFn<Store[]> = 
(
  route, 
  state,
  storeService: StoreService = inject(StoreService)
) => {
  return storeService.GetStores();
};
