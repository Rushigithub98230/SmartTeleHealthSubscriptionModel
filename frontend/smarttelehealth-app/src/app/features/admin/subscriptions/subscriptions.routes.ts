import { Routes } from '@angular/router';

export const ADMIN_SUBSCRIPTION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./subscription-list/subscription-list.component').then(m => m.AdminSubscriptionListComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./subscription-detail/subscription-detail.component').then(m => m.AdminSubscriptionDetailComponent)
  }
];


