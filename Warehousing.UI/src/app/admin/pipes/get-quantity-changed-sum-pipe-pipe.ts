import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'getQuantityChangedSum',
  standalone: false
})
export class getQuantityChangedSum implements PipeTransform {

  transform(transactions: any[]): number {
    return transactions.reduce((acc, curr) => acc + curr.quantityChanged, 0);
  }

}
