import { Routes } from '@angular/router';

/**
 * Admin Payment Management Routes
 * Phase 3: Failed Payment Management
 * 
 * Base path: /webadmin/payments
 * Access: Admin only (protected by adminGuard in parent route)
 */
export const PAYMENT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./failed-payments-dashboard/failed-payments-dashboard.component')
      .then(m => m.FailedPaymentsDashboardComponent)
  }
];

