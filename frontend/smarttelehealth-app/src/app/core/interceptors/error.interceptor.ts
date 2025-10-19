import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Error Interceptor
 * Global error handling for HTTP requests
 * Handles 401 (Unauthorized) by logging out and redirecting
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized - token expired or invalid
      if (error.status === 401) {
        authService.logout();
        
        // Determine which login to redirect to
        const currentUrl = router.url;
        if (currentUrl.startsWith('/webadmin')) {
          router.navigate(['/webadmin/login']);
        } else {
          router.navigate(['/web/login']);
        }
      }

      // Handle 403 Forbidden
      if (error.status === 403) {
        console.error('Access forbidden:', error.message);
        // Optionally show toast notification
      }

      // Pass error through
      return throwError(() => error);
    })
  );
};

