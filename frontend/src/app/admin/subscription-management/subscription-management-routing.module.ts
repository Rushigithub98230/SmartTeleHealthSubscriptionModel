import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EnhancedSubscriptionManagementComponent } from './enhanced-subscription-management.component';

const routes: Routes = [
  {
    path: '',
    component: EnhancedSubscriptionManagementComponent,
    data: {
      title: 'Subscription Management',
      breadcrumb: 'Subscription Management'
    }
  },
  {
    path: 'subscriptions',
    component: EnhancedSubscriptionManagementComponent,
    data: {
      title: 'Subscriptions',
      breadcrumb: 'Subscriptions',
      viewMode: 'subscriptions'
    }
  },
  {
    path: 'plans',
    component: EnhancedSubscriptionManagementComponent,
    data: {
      title: 'Subscription Plans',
      breadcrumb: 'Plans',
      viewMode: 'plans'
    }
  },
  {
    path: 'analytics',
    component: EnhancedSubscriptionManagementComponent,
    data: {
      title: 'Subscription Analytics',
      breadcrumb: 'Analytics',
      viewMode: 'analytics'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SubscriptionManagementRoutingModule { }
