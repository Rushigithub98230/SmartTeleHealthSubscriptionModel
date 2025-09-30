import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './admin/auth/auth.guard';

const routes: Routes = [
  // Home/Marketing Page (Public)
  { 
    path: '', 
    loadComponent: () => import('./home/plan-category-list/plan-category-list.component').then(m => m.PlanCategoryListComponent) 
  },
  { 
    path: 'home', 
    loadComponent: () => import('./home/plan-category-list/plan-category-list.component').then(m => m.PlanCategoryListComponent) 
  },
  
  // Subscription Success/Cancel Pages (Public)
  { 
    path: 'subscription/success', 
    loadComponent: () => import('./subscription-success/subscription-success.component').then(m => m.SubscriptionSuccessComponent) 
  },
  { 
    path: 'subscription/cancel', 
    loadComponent: () => import('./subscription-cancel/subscription-cancel.component').then(m => m.SubscriptionCancelComponent) 
  },
  
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
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { 
        path: 'dashboard', 
        loadComponent: () => import('./admin/dashboard/dashboard.component').then(m => m.DashboardComponent) 
      },
      { 
        path: 'subscriptions', 
        loadComponent: () => import('./admin/subscription-management/subscription-management').then(m => m.SubscriptionManagementComponent),
        data: { view: 'subscriptions' }
      },
      { 
        path: 'categories', 
        loadComponent: () => import('./admin/category-management/category-management.component').then(m => m.CategoryManagementComponent) 
      },
      { 
        path: 'plans', 
        loadComponent: () => import('./admin/subscription-management/subscription-management').then(m => m.SubscriptionManagementComponent),
        data: { view: 'plans' }
      },
      { 
        path: 'analytics', 
        loadComponent: () => import('./admin/dashboard/dashboard.component').then(m => m.DashboardComponent) 
      },
      { 
        path: 'stripe-testing', 
        loadComponent: () => import('./admin/stripe-testing/stripe-testing-enhanced.component').then(m => m.StripeTestingEnhancedComponent) 
      }
    ]
  },
  
  // Catch all route - redirect to home
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
