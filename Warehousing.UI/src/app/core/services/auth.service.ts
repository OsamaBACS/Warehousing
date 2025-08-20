import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BehaviorSubject, tap } from 'rxjs';
import { LoginResult } from '../models/loginResult';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) {
    this.loadPermissionsFromStorage();
  }
  url = environment.baseUrl + '/Auth/';
  private permissions: string[] = [];
  private categoryIds: string[] = [];
  private productIds: string[] = [];
  public userId: string = '';
  public username: string = '';
  private usernameSubject = new BehaviorSubject<string>(this.getUsernameFromStorage());
  public username$ = this.usernameSubject.asObservable();

  login(credentials: { username: string, password: string, fingerprint: string }) {
    return this.http.post<LoginResult>(`${this.url}login`, credentials).pipe(
      tap(res => {
        if (res.token && res.status == null) {
          localStorage.setItem('jwt_token', res.token);
          const payload = JSON.parse(atob(res.token.split('.')[1]));

          localStorage.removeItem('permissions');
          localStorage.removeItem('category');
          localStorage.removeItem('product');
          localStorage.removeItem('userId');


          this.setUserPermissions(payload.Permission, payload.Category, payload.Product, payload.UserId, payload.UserName)
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('permissions');
    localStorage.removeItem('category');
    localStorage.removeItem('product');
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    this.clearUser();
  }

  private getUsernameFromStorage(): string {
    const stored = localStorage.getItem('username');
    return stored ? JSON.parse(stored) : '';
  }

  private clearUser() {
    this.permissions = [];
    this.categoryIds = [];
    this.productIds = [];
    this.userId = '';
    this.username = '';
    this.usernameSubject.next('');
  }


  isAuthenticated(): boolean {
    return !!localStorage.getItem('jwt_token');
  }

  setUserPermissions(perms: string[], catIds: string[], prodIds: string[], userId: string, username: string) {
    this.permissions = perms;
    this.categoryIds = catIds;
    this.productIds = prodIds;
    localStorage.setItem('permissions', JSON.stringify(perms));
    localStorage.setItem('category', JSON.stringify(catIds));
    localStorage.setItem('product', JSON.stringify(prodIds));
    localStorage.setItem('userId', JSON.stringify(userId));
    localStorage.setItem('username', JSON.stringify(username));
    this.username = username;
    this.usernameSubject.next(username);
  }

  hasPermission(permission: string): boolean {
    return this.permissions.includes(permission);
  }

  hasCategory(catId: number): boolean {
    return this.categoryIds.includes(catId.toString());
  }

  hasProduct(prodId: number): boolean {
    return this.productIds.includes(prodId.toString());
  }

  loadPermissionsFromStorage() {
    const stored = localStorage.getItem('permissions');
    const cat = localStorage.getItem('category');
    const prod = localStorage.getItem('product');
    const userId = localStorage.getItem('userId');
    const username = localStorage.getItem('username');
    if (stored) {
      try {
        this.permissions = JSON.parse(stored);
      } catch (e) {
        console.error('Failed to parse permissions from localStorage', e);
        this.permissions = [];
      }
    } else {
      this.permissions = [];
    }

    if (cat) {
      try {
        this.categoryIds = JSON.parse(cat);
      } catch (e) {
        console.error('Failed to parse categoryIds from localStorage', e);
        this.categoryIds = [];
      }
    }
    else {
      this.categoryIds = [];
    }

    if (prod) {
      try {
        this.productIds = JSON.parse(prod);
      } catch (e) {
        console.error('Failed to parse productIds from localStorage', e);
        this.productIds = [];
      }
    }
    else {
      this.productIds = [];
    }

    if (userId && userId != 'undefined') {
      try {
        this.userId = JSON.parse(userId);
      } catch (e) {
        console.error('Failed to parse userId from localStorage', e);
        this.userId = '';
      }
    }

    if (username && username != 'undefined') {
      try {
        this.username = JSON.parse(username);
      } catch (e) {
        console.error('Failed to parse username from localStorage', e);
        this.username = '';
      }
    }
  }

}
