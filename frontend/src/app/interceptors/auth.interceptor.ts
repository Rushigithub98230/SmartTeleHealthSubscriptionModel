import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  
  // Get the auth token from localStorage
  const token = localStorage.getItem('adminToken');
  
  console.log('AuthInterceptor - Request URL:', req.url);
  console.log('AuthInterceptor - Has token:', !!token);
  if (token) {
    console.log('AuthInterceptor - Token (first 20 chars):', token.substring(0, 20) + '...');
  }
  
  // Clone the request and add the authorization header if token exists
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('AuthInterceptor - Added Authorization header to request');
  } else {
    console.log('AuthInterceptor - No token found, request sent without Authorization header');
  }

  // Handle the request and catch any errors
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.log('AuthInterceptor - Request failed with status:', error.status);
      console.log('AuthInterceptor - Error details:', error);
      
      // If we get a 401 Unauthorized, redirect to login
      if (error.status === 401) {
        console.log('AuthInterceptor - Unauthorized request, redirecting to login');
        localStorage.removeItem('adminToken');
        localStorage.removeItem('adminUser');
        router.navigate(['/admin/login']);
      }
      
      return throwError(() => error);
    })
  );
};
