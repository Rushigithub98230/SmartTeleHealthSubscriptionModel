import { Routes } from '@angular/router';

export const PLAN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./plan-list/plan-list.component').then(m => m.PlanListAdminComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./plan-create/plan-create.component').then(m => m.PlanCreateComponent)
  },
  {
    path: ':id/versions',
    loadComponent: () => import('./plan-version-history/plan-version-history.component').then(m => m.PlanVersionHistoryComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./plan-edit/plan-edit.component').then(m => m.PlanEditComponent)
  }
];


