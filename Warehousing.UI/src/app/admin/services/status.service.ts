import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Status } from '../models/status';

@Injectable({
  providedIn: 'root'
})
export class StatusService {

  constructor(private http: HttpClient) { }
  url = environment.baseUrl + '/Status/';

  GetStatusList() {
    return this.http.get<Status[]>(`${this.url}GetStatusList`)
  }
}
