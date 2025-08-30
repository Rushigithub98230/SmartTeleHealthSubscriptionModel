import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/admin/subscriptions', pathMatch: 'full' },
  { 
    path: 'admin/subscriptions', 
    loadComponent: () => import('./admin/subscription-management/subscription-management').then(m => m.SubscriptionManagementComponent) 
  }
];
