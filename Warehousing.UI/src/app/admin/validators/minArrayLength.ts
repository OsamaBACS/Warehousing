import { AbstractControl, ValidatorFn } from '@angular/forms';

// Validates that a FormArray has at least a certain number of items
export function minArrayLength(min: number): ValidatorFn {
  return (control: AbstractControl): { [key: string]: any } | null => {
    const array = control as unknown as { length: number };
    return array.length >= min ? null : { minArrayLength: { requiredLength: min, currentLength: array.length } };
  };
}