import { Routes } from '@angular/router';

export const SUBSCRIPTION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./subscription-list/subscription-list.component').then(m => m.SubscriptionListComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./subscription-detail/subscription-detail.component').then(m => m.SubscriptionDetailComponent)
  },
  {
    path: 'purchase/:planId',
    loadComponent: () => import('./purchase-plan/purchase-plan.component').then(m => m.PurchasePlanComponent)
  }
];


