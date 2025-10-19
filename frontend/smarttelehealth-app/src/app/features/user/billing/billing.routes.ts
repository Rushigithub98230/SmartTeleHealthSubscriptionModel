import { Routes } from '@angular/router';

export const BILLING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./billing-history/billing-history.component').then(m => m.BillingHistoryComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./billing-detail/billing-detail.component').then(m => m.BillingDetailComponent)
  }
];


