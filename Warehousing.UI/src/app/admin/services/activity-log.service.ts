import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface UserActivityLog {
  id: number;
  userId: number;
  username: string; // This will be mapped from UserName in the response
  action: string;
  module: string;
  timestamp: string;
  ipAddress: string;
  details: string;
  createdAt: string;
  createdBy: string;
}

export interface ActivityLogResponse {
  data: UserActivityLog[];
  totalCount: number;
}

export interface ActivityLogFilters {
  userId?: number;
  startDate?: string;
  endDate?: string;
  action?: string;
  module?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ActivityLogService {
  private apiUrl = `${environment.baseUrl}/ActivityLog`;

  constructor(private http: HttpClient) { }

  getUserActivityLogs(filters: ActivityLogFilters = {}): Observable<ActivityLogResponse> {
    let params = new HttpParams();
    
    if (filters.userId) params = params.set('userId', filters.userId.toString());
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.endDate) params = params.set('endDate', filters.endDate);
    if (filters.action) params = params.set('action', filters.action);
    if (filters.module) params = params.set('module', filters.module);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/GetUserActivityLogs`, { params }).pipe(
      map(response => ({
        data: response.data.map((log: any) => ({
          ...log,
          username: log.userName || 'Unknown User'
        })),
        totalCount: response.totalCount
      }))
    );
  }

  getAllActivityLogs(filters: ActivityLogFilters = {}): Observable<ActivityLogResponse> {
    let params = new HttpParams();
    
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.endDate) params = params.set('endDate', filters.endDate);
    if (filters.action) params = params.set('action', filters.action);
    if (filters.module) params = params.set('module', filters.module);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/GetAllActivityLogs`, { params }).pipe(
      map(response => {
        return {
          data: response.data.map((log: any) => ({
            ...log,
            username: log.userName || 'Unknown User'
          })),
          totalCount: response.totalCount
        };
      })
    );
  }

  // Backend currently exposes GetAllActivityLogs/GetUserActivityLogs/ClearOldActivityLogs
  clearOldLogs(daysToKeep: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/ClearOldActivityLogs`, { params: new HttpParams().set('daysToKeep', daysToKeep) });
  }

  deleteActivityLog(id: number): Observable<void> {
    // If delete by id is not implemented server-side, this can be wired later.
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
