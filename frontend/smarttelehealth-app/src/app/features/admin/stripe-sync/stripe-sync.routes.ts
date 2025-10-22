import { Routes } from '@angular/router';

/**
 * Admin Stripe Sync Management Routes
 * Phase 5: Stripe Synchronization Dashboard
 * 
 * Base path: /webadmin/stripe-sync
 * Access: Admin only (protected by adminGuard in parent route)
 */
export const STRIPE_SYNC_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./sync-dashboard/sync-dashboard.component')
      .then(m => m.StripeSyncDashboardComponent)
  }
];

