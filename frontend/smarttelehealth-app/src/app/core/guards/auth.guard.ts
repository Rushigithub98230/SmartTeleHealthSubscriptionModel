import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Auth Guard
 * Protects routes that require authentication
 * Redirects to appropriate login page based on URL
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Determine which login page to redirect to
  const targetUrl = state.url;
  if (targetUrl.startsWith('/webadmin')) {
    router.navigate(['/webadmin/login'], { queryParams: { returnUrl: state.url } });
  } else {
    router.navigate(['/web/login'], { queryParams: { returnUrl: state.url } });
  }

  return false;
};


