import { Routes } from '@angular/router';

export const PRIVILEGE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./privilege-usage/privilege-usage.component').then(m => m.PrivilegeUsageComponent)
  },
  {
    path: 'history',
    loadComponent: () => import('./usage-history/usage-history.component').then(m => m.UsageHistoryComponent)
  }
];


