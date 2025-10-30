import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

/**
 * Main Application Routes
 * 
 * Structure:
 * - / → Marketing Portal (Public)
 * - /web → User Portal (Authenticated)
 * - /webadmin → Admin Portal (Admin Only)
 */
export const routes: Routes = [
  // Marketing Portal (Public)
  {
    path: '',
    loadChildren: () => import('./features/marketing/marketing.routes').then(m => m.MARKETING_ROUTES)
  },

  // User Portal
  {
    path: 'web',
    children: [
      // Public auth routes
      {
        path: 'login',
        loadComponent: () => import('./features/user/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/user/auth/register/register.component').then(m => m.RegisterComponent)
      },
      // Protected routes
      {
        path: 'dashboard',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'subscriptions',
        canActivate: [authGuard],
        loadChildren: () => import('./features/user/subscriptions/subscriptions.routes').then(m => m.SUBSCRIPTION_ROUTES)
      },
      {
        path: 'privileges',
        canActivate: [authGuard],
        loadChildren: () => import('./features/user/privileges/privileges.routes').then(m => m.PRIVILEGE_ROUTES)
      },
      {
        path: 'billing',
        canActivate: [authGuard],
        loadChildren: () => import('./features/user/billing/billing.routes').then(m => m.BILLING_ROUTES)
      },
      {
        path: 'invoices',
        canActivate: [authGuard],
        children: [
          {
            path: '',
            loadComponent: () => import('./features/user/invoices/invoice-list/invoice-list.component').then(m => m.InvoiceListComponent)
          },
          {
            path: ':invoiceNumber',
            loadComponent: () => import('./features/user/invoices/invoice-detail/invoice-detail.component').then(m => m.InvoiceDetailComponent)
          }
        ]
      },
      {
        path: 'payment-methods',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user/payment-methods/payment-methods.component').then(m => m.PaymentMethodsComponent)
      },
      {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user/profile/profile.component').then(m => m.ProfileComponent)
      }
    ]
  },

  // Admin Portal
  {
    path: 'webadmin',
    children: [
      // Public admin login
      {
        path: 'login',
        loadComponent: () => import('./features/admin/auth/admin-login/admin-login.component').then(m => m.AdminLoginComponent)
      },
      // Protected admin routes
      {
        path: 'dashboard',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/admin/dashboard/dashboard.component').then(m => m.AdminDashboardComponent)
      },
      {
        path: 'plans',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/plans/plans.routes').then(m => m.PLAN_ROUTES)
      },
      {
        path: 'categories',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/categories/categories.routes').then(m => m.CATEGORY_ROUTES)
      },
      {
        path: 'subscriptions',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/subscriptions/subscriptions.routes').then(m => m.ADMIN_SUBSCRIPTION_ROUTES)
      },
      {
        path: 'users',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/users/users.routes').then(m => m.USER_ROUTES)
      },
      {
        path: 'billing',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/billing/billing.routes').then(m => m.ADMIN_BILLING_ROUTES)
      },
      {
        path: 'payments',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/payments/payments.routes').then(m => m.PAYMENT_ROUTES)
      },
      {
        path: 'invoices',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/invoices/invoices.routes').then(m => m.INVOICE_ROUTES)
      },
      {
        path: 'stripe-sync',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/stripe-sync/stripe-sync.routes').then(m => m.STRIPE_SYNC_ROUTES)
      },
      {
        path: 'analytics',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/analytics/analytics.routes').then(m => m.ANALYTICS_ROUTES)
      },
      {
        path: 'reports',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/admin/reports/reports.component').then(m => m.ReportsComponent)
      },
      {
        path: 'system-settings',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/admin/system-settings/system-settings.component').then(m => m.SystemSettingsComponent)
      },
      {
        path: 'settings',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/admin/settings/settings.component').then(m => m.AdminSettingsComponent)
      },
      {
        path: 'logs',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/admin/logs/logs.routes').then(m => m.LOGS_ROUTES)
      }
    ]
  },

  // Redirect unknown routes to home
  { path: '**', redirectTo: '' }
];
