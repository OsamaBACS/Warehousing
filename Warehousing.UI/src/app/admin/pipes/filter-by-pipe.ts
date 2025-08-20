import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filterBy',
  standalone: false
})
export class FilterByPipe implements PipeTransform {

  transform(items: any[], searchText: string, keys: string[]): any[] {
    if (!items) return [];
    if (!searchText) return items;

    searchText = searchText.toLowerCase();

    return items.filter(item => {
      return keys.some(key => {
        const value = item[key];
        return value && value.toString().toLowerCase().includes(searchText);
      });
    });
  }

}
