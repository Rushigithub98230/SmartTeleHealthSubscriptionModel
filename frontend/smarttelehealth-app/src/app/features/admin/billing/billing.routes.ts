import { Routes } from '@angular/router';

export const ADMIN_BILLING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./billing-list/billing-list.component').then(m => m.AdminBillingListComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./billing-detail/billing-detail.component').then(m => m.AdminBillingDetailComponent)
  }
];

