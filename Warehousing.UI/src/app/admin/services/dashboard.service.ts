import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private url = `${environment.baseUrl}/Dashboard/`;

  constructor(private http: HttpClient) { }

  // Get dashboard overview with key metrics
  getDashboardOverview(): Observable<any> {
    return this.http.get<any>(`${this.url}overview`);
  }

  // Get recent inventory transactions
  getRecentTransactions(count: number = 10): Observable<any[]> {
    return this.http.get<any[]>(`${this.url}recent-transactions?count=${count}`);
  }

  // Get top products by quantity
  getTopProducts(count: number = 10): Observable<any[]> {
    return this.http.get<any[]>(`${this.url}top-products?count=${count}`);
  }

  // Get store performance metrics
  getStorePerformance(): Observable<any[]> {
    return this.http.get<any[]>(`${this.url}store-performance`);
  }

  // Get monthly transaction trends
  getMonthlyTransactions(months: number = 6): Observable<any[]> {
    return this.http.get<any[]>(`${this.url}monthly-transactions?months=${months}`);
  }

  // Get system alerts (low stock, out of stock, etc.)
  getAlerts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.url}alerts`);
  }
}
