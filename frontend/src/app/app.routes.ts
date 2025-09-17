import { Routes } from '@angular/router';
import { AuthGuard } from './admin/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/admin/login', pathMatch: 'full' },
  
  // Admin Authentication Routes (Public)
  { 
    path: 'admin/login', 
    loadComponent: () => import('./admin/auth/login.component').then(m => m.AdminLoginComponent) 
  },
  { 
    path: 'admin/register', 
    loadComponent: () => import('./admin/auth/register.component').then(m => m.AdminRegisterComponent) 
  },
  
  // Admin Portal Routes (Protected)
  {
    path: 'admin',
    loadComponent: () => import('./admin/admin-layout.component').then(m => m.AdminLayoutComponent),
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'subscriptions', pathMatch: 'full' },
      { 
        path: 'subscriptions', 
        loadComponent: () => import('./admin/subscription-management/subscription-management').then(m => m.SubscriptionManagementComponent),
        data: { view: 'subscriptions' }
      },
      { 
        path: 'plans', 
        loadComponent: () => import('./admin/subscription-management/subscription-management').then(m => m.SubscriptionManagementComponent),
        data: { view: 'plans' }
      },
      { 
        path: 'categories', 
        loadComponent: () => import('./admin/category-management/category-management').then(m => m.CategoryManagementComponent)
      },
      { 
        path: 'analytics', 
        loadComponent: () => import('./admin/dashboard/dashboard').then(m => m.DashboardComponent)
      }
    ]
  },
  
  // Catch all route
  { path: '**', redirectTo: '/admin/login' }
];
