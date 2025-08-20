import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Unit } from '../models/unit';

@Injectable({
  providedIn: 'root'
})
export class UnitsService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Units/';

  GetUnits() {
    return this.http.get<Unit[]>(`${this.url}GetUnits`);
  }

  GetUnitById(unitId: number) {
    return this.http.get<Unit>(`${this.url}GetUnitById?Id=${unitId}`);
  }

  SaveUnit(unit: any) {
    return this.http.post<any>(`${this.url}SaveUnit`, unit);
  }
}
