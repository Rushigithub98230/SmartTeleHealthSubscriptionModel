import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Admin Guard
 * Protects routes that require admin role
 * Must be used with authGuard
 */
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/webadmin/login']);
    return false;
  }

  if (authService.isAdmin()) {
    return true;
  }

  // User is authenticated but not admin - redirect to user portal
  router.navigate(['/web/dashboard']);
  return false;
};


