import { Routes } from '@angular/router';

export const LOGS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./admin-logs.component').then(m => m.AdminLogsComponent)
  }
];

