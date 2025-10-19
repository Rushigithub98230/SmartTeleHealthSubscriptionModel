import { Routes } from '@angular/router';

export const MARKETING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'plans',
    loadComponent: () => import('./plans/plan-list/plan-list.component').then(m => m.PlanListComponent)
  },
  {
    path: 'plans/:id',
    loadComponent: () => import('./plans/plan-detail/plan-detail.component').then(m => m.PlanDetailComponent)
  },
  {
    path: 'pricing',
    loadComponent: () => import('./pricing/pricing.component').then(m => m.PricingComponent)
  }
];


