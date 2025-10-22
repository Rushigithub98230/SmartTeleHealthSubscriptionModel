import { Routes } from '@angular/router';

/**
 * Admin Invoice Management Routes
 * Phase 4: Invoice Management
 * 
 * Base path: /webadmin/invoices
 * Access: Admin only (protected by adminGuard in parent route)
 */
export const INVOICE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./invoice-list/invoice-list.component')
      .then(m => m.AdminInvoiceListComponent)
  }
];

