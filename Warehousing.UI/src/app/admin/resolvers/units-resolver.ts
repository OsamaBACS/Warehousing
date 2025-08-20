import { ResolveFn } from '@angular/router';
import { UnitsService } from '../services/units.service';
import { inject } from '@angular/core';
import { Unit } from '../models/unit';

export const UnitsResolver: ResolveFn<Unit[]> = 
(
  route, 
  state,
  unitsservice: UnitsService = inject(UnitsService)
) => {
  return unitsservice.GetUnits();
};
