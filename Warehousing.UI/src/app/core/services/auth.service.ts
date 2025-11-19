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
  public nameEn: string = '';
  public nameAr: string = '';
  public isAdmin: boolean = false;
  private usernameSubject = new BehaviorSubject<string>(this.getUsernameFromStorage());
  public username$ = this.usernameSubject.asObservable();

  login(credentials: { username: string, password: string }) {
    return this.http.post<LoginResult>(`${this.url}login`, credentials).pipe(
      tap(res => {
        if (res.success && res.token) {
          localStorage.setItem('jwt_token', res.token);
          
          try {
            // Debug: Log the token structure
            
            const tokenParts = res.token.split('.');
            
            if (tokenParts.length !== 3) {
              throw new Error('Invalid JWT token format - expected 3 parts, got ' + tokenParts.length);
            }
            
            // Validate base64 encoding
            const payloadBase64 = tokenParts[1];
            
            // Check if it's valid base64
            let decodedPayload;
            try {
              // Decode base64 URL-safe encoded string
              // Handle padding if needed
              let base64 = payloadBase64.replace(/-/g, '+').replace(/_/g, '/');
              const padding = base64.length % 4;
              if (padding) {
                base64 += '='.repeat(4 - padding);
              }
              
              // Decode base64 to binary string
              const binaryString = atob(base64);
              
              // Convert binary string to UTF-8 string properly
              const bytes = new Uint8Array(binaryString.length);
              for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
              }
              decodedPayload = new TextDecoder('utf-8').decode(bytes);
              
            } catch (base64Error) {
              // Fallback: try direct atob for ASCII-only tokens
              try {
                decodedPayload = atob(payloadBase64);
              } catch (fallbackError) {
                throw new Error('Invalid base64 encoding in JWT payload: ' + (base64Error as Error).message);
              }
            }
            
            const payload = JSON.parse(decodedPayload);

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
            const nameEn = payload.NameEn || payload.UserName || '';
            const nameAr = payload.NameAr || payload.UserName || '';

            this.setUserPermissions(permissions, categories, products, subCategories, payload.UserId, payload.UserName, isAdmin, nameEn, nameAr);
          } catch (error) {
            
            // Clear everything on parsing error
            this.logout();
            
            // Don't show alert immediately, let the user see the console logs first
            
            // Optional: Show a more informative error after a delay
            setTimeout(() => {
              alert('Login failed: JWT token parsing error. Please check console for details and try again.');
            }, 1000);
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
    localStorage.removeItem('nameEn');
    localStorage.removeItem('nameAr');
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
    this.nameEn = '';
    this.nameAr = '';
    this.isAdmin = false;
    this.usernameSubject.next('');
  }


  isAuthenticated(): boolean {
    return !!localStorage.getItem('jwt_token');
  }

  setUserPermissions(perms: string | string[], catIds: string | string[], prodIds: string | string[], subCatIds: string | string[], userId: string, username: string, isAdmin: boolean = false, nameEn: string = '', nameAr: string = '') {
    // Handle comma-separated strings from JWT token
    this.permissions = Array.isArray(perms) ? perms : (typeof perms === 'string' ? perms.split(',').filter((p: string) => p.trim()) : []);
    this.categoryIds = Array.isArray(catIds) ? catIds : (typeof catIds === 'string' ? catIds.split(',').filter((c: string) => c.trim()) : []);
    this.productIds = Array.isArray(prodIds) ? prodIds : (typeof prodIds === 'string' ? prodIds.split(',').filter((p: string) => p.trim()) : []);
    this.subCategoryIds = Array.isArray(subCatIds) ? subCatIds : (typeof subCatIds === 'string' ? subCatIds.split(',').filter((s: string) => s.trim()) : []);
    this.isAdmin = isAdmin;
    this.nameEn = nameEn;
    this.nameAr = nameAr;
    
    localStorage.setItem('permissions', JSON.stringify(this.permissions));
    localStorage.setItem('category', JSON.stringify(this.categoryIds));
    localStorage.setItem('product', JSON.stringify(this.productIds));
    localStorage.setItem('subCategory', JSON.stringify(this.subCategoryIds));
    localStorage.setItem('userId', JSON.stringify(userId));
    localStorage.setItem('username', JSON.stringify(username));
    localStorage.setItem('nameEn', JSON.stringify(nameEn));
    localStorage.setItem('nameAr', JSON.stringify(nameAr));
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

  getProductIds(): string[] {
    return [...this.productIds]; // Return a copy to prevent external modification
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
      const nameEn = localStorage.getItem('nameEn');
      const nameAr = localStorage.getItem('nameAr');
      const isAdmin = localStorage.getItem('isAdmin');
      
      // Safely parse permissions with size limit
      if (stored && stored !== 'undefined' && stored.length < 100000) { // 100KB limit
        try {
          this.permissions = JSON.parse(stored);
          if (!Array.isArray(this.permissions)) {
            this.permissions = [];
          }
        } catch (e) {
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
          this.userId = '';
        }
      }

      // Parse username
      if (username && username !== 'undefined') {
        try {
          this.username = JSON.parse(username);
        } catch (e) {
          this.username = '';
        }
      }

      // Parse nameEn
      if (nameEn && nameEn !== 'undefined') {
        try {
          this.nameEn = JSON.parse(nameEn);
        } catch (e) {
          this.nameEn = '';
        }
      }

      // Parse nameAr
      if (nameAr && nameAr !== 'undefined') {
        try {
          this.nameAr = JSON.parse(nameAr);
        } catch (e) {
          this.nameAr = '';
        }
      }

      // Parse isAdmin flag
      if (isAdmin && isAdmin !== 'undefined') {
        try {
          this.isAdmin = JSON.parse(isAdmin);
        } catch (e) {
          this.isAdmin = false;
        }
      }
    } catch (error) {
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
    localStorage.removeItem('nameEn');
    localStorage.removeItem('nameAr');
    localStorage.removeItem('isAdmin');
    localStorage.removeItem('jwt_token');
  }

}
