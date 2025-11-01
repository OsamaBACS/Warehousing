import { HttpEvent, HttpInterceptorFn, HttpRequest, HttpHandler, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { inject } from '@angular/core';
import { Router } from '@angular/router';

export const JwtInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<any>> => {
  const router = inject(Router);
  const token = localStorage.getItem('jwt_token');

  // Helper: check if token valid (not expired, has claim)
  function isTokenValid(token: string | null): boolean {
    if (!token) return false;

    try {
      const tokenParts = token.split('.');
      if (tokenParts.length !== 3) return false;
      
      const payloadBase64 = tokenParts[1];
      
      // Decode base64 URL-safe encoded string
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
      const decodedPayload = new TextDecoder('utf-8').decode(bytes);
      
      const payload = JSON.parse(decodedPayload);
      const isExpired = payload.exp * 1000 < Date.now();

      return !!payload.UserId && !isExpired;
    } catch (e) {
      return false;
    }
  }

  // Logout helper
  function logout() {
    localStorage.removeItem('jwt_token');
    router.navigate(['/login']);
  }

  // If token exists but invalid, logout immediately
  if (token && !isTokenValid(token)) {
    logout();
    // Return empty observable or throw error to block request
    return throwError(() => new Error('Token expired or invalid'));
  }

  // Clone request with Authorization header if token exists
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        logout();
      }
      return throwError(() => error);
    })
  );
};
