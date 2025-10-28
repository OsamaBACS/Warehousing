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
  private subCategoryIds: string[] = [];
  public userId: string = '';
  public username: string = '';
  public isAdmin: boolean = false;
  private usernameSubject = new BehaviorSubject<string>(this.getUsernameFromStorage());
  public username$ = this.usernameSubject.asObservable();

  login(credentials: { username: string, password: string, fingerprint: string }) {
    return this.http.post<LoginResult>(`${this.url}login`, credentials).pipe(
      tap(res => {
        if (res.token && res.status == null) {
          localStorage.setItem('jwt_token', res.token);
          
          try {
            const payload = JSON.parse(atob(res.token.split('.')[1]));

            // Clear existing data
            localStorage.removeItem('permissions');
            localStorage.removeItem('category');
            localStorage.removeItem('product');
            localStorage.removeItem('subCategory');
            localStorage.removeItem('userId');

            // Handle both array and comma-separated string formats
            const permissions = Array.isArray(payload.Permission) ? payload.Permission : (payload.Permission || '');
            const categories = Array.isArray(payload.Category) ? payload.Category : (payload.Category || '');
            const products = Array.isArray(payload.Product) ? payload.Product : (payload.Product || '');
            const subCategories = Array.isArray(payload.SubCategory) ? payload.SubCategory : (payload.SubCategory || '');
            const isAdmin = payload.IsAdmin === 'true';

            this.setUserPermissions(permissions, categories, products, subCategories, payload.UserId, payload.UserName, isAdmin);
          } catch (error) {
            console.error('Error parsing JWT token:', error);
            // Clear everything on parsing error
            this.logout();
          }
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('permissions');
    localStorage.removeItem('category');
    localStorage.removeItem('product');
    localStorage.removeItem('subCategory');
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('isAdmin');
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
    this.subCategoryIds = [];
    this.userId = '';
    this.username = '';
    this.isAdmin = false;
    this.usernameSubject.next('');
  }


  isAuthenticated(): boolean {
    return !!localStorage.getItem('jwt_token');
  }

  setUserPermissions(perms: string | string[], catIds: string | string[], prodIds: string | string[], subCatIds: string | string[], userId: string, username: string, isAdmin: boolean = false) {
    // Handle comma-separated strings from JWT token
    this.permissions = Array.isArray(perms) ? perms : (typeof perms === 'string' ? perms.split(',').filter((p: string) => p.trim()) : []);
    this.categoryIds = Array.isArray(catIds) ? catIds : (typeof catIds === 'string' ? catIds.split(',').filter((c: string) => c.trim()) : []);
    this.productIds = Array.isArray(prodIds) ? prodIds : (typeof prodIds === 'string' ? prodIds.split(',').filter((p: string) => p.trim()) : []);
    this.subCategoryIds = Array.isArray(subCatIds) ? subCatIds : (typeof subCatIds === 'string' ? subCatIds.split(',').filter((s: string) => s.trim()) : []);
    this.isAdmin = isAdmin;
    
    localStorage.setItem('permissions', JSON.stringify(this.permissions));
    localStorage.setItem('category', JSON.stringify(this.categoryIds));
    localStorage.setItem('product', JSON.stringify(this.productIds));
    localStorage.setItem('subCategory', JSON.stringify(this.subCategoryIds));
    localStorage.setItem('userId', JSON.stringify(userId));
    localStorage.setItem('username', JSON.stringify(username));
    localStorage.setItem('isAdmin', JSON.stringify(isAdmin));
    this.username = username;
    this.usernameSubject.next(username);
  }

  hasPermission(permission: string): boolean {
    // ✅ OPTIMIZED: Admin users have all permissions
    if (this.isAdmin) {
      return true;
    }
    return this.permissions.includes(permission);
  }

  hasCategory(catId: number): boolean {
    // ✅ OPTIMIZED: Admin users have access to all categories
    if (this.isAdmin) {
      return true;
    }
    return this.categoryIds.includes(catId.toString());
  }

  hasProduct(prodId: number): boolean {
    // ✅ OPTIMIZED: Admin users have access to all products
    if (this.isAdmin) {
      return true;
    }
    return this.productIds.includes(prodId.toString());
  }

  hasSubCategory(subCatId: number): boolean {
    // ✅ OPTIMIZED: Admin users have access to all subcategories
    if (this.isAdmin) {
      return true;
    }
    return this.subCategoryIds.includes(subCatId.toString());
  }

  loadPermissionsFromStorage() {
    try {
      const stored = localStorage.getItem('permissions');
      const cat = localStorage.getItem('category');
      const prod = localStorage.getItem('product');
      const subCat = localStorage.getItem('subCategory');
      const userId = localStorage.getItem('userId');
      const username = localStorage.getItem('username');
      const isAdmin = localStorage.getItem('isAdmin');
      
      // Safely parse permissions with size limit
      if (stored && stored !== 'undefined' && stored.length < 100000) { // 100KB limit
        try {
          this.permissions = JSON.parse(stored);
          if (!Array.isArray(this.permissions)) {
            this.permissions = [];
          }
        } catch (e) {
          console.error('Failed to parse permissions from localStorage', e);
          this.permissions = [];
        }
      } else {
        this.permissions = [];
      }

      // Safely parse category IDs with size limit
      if (cat && cat !== 'undefined' && cat.length < 100000) {
        try {
          this.categoryIds = JSON.parse(cat);
          if (!Array.isArray(this.categoryIds)) {
            this.categoryIds = [];
          }
        } catch (e) {
          console.error('Failed to parse categoryIds from localStorage', e);
          this.categoryIds = [];
        }
      } else {
        this.categoryIds = [];
      }

      // Safely parse product IDs with size limit
      if (prod && prod !== 'undefined' && prod.length < 100000) {
        try {
          this.productIds = JSON.parse(prod);
          if (!Array.isArray(this.productIds)) {
            this.productIds = [];
          }
        } catch (e) {
          console.error('Failed to parse productIds from localStorage', e);
          this.productIds = [];
        }
      } else {
        this.productIds = [];
      }

      // Safely parse subcategory IDs with size limit
      if (subCat && subCat !== 'undefined' && subCat.length < 100000) {
        try {
          this.subCategoryIds = JSON.parse(subCat);
          if (!Array.isArray(this.subCategoryIds)) {
            this.subCategoryIds = [];
          }
        } catch (e) {
          console.error('Failed to parse subCategoryIds from localStorage', e);
          this.subCategoryIds = [];
        }
      } else {
        this.subCategoryIds = [];
      }

      // Parse user ID
      if (userId && userId !== 'undefined') {
        try {
          this.userId = JSON.parse(userId);
        } catch (e) {
          console.error('Failed to parse userId from localStorage', e);
          this.userId = '';
        }
      }

      // Parse username
      if (username && username !== 'undefined') {
        try {
          this.username = JSON.parse(username);
        } catch (e) {
          console.error('Failed to parse username from localStorage', e);
          this.username = '';
        }
      }

      // Parse isAdmin flag
      if (isAdmin && isAdmin !== 'undefined') {
        try {
          this.isAdmin = JSON.parse(isAdmin);
        } catch (e) {
          console.error('Failed to parse isAdmin from localStorage', e);
          this.isAdmin = false;
        }
      }
    } catch (error) {
      console.error('Critical error in loadPermissionsFromStorage:', error);
      // Clear all data on critical error
      this.clearUser();
      this.clearLocalStorage();
    }
  }

  private clearLocalStorage() {
    localStorage.removeItem('permissions');
    localStorage.removeItem('category');
    localStorage.removeItem('product');
    localStorage.removeItem('subCategory');
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('isAdmin');
    localStorage.removeItem('jwt_token');
  }

}
