import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLoginComponent } from './auth/login.component';
import { AdminRegisterComponent } from './auth/register.component';
import { PlanManagementComponent } from './plan-management/plan-management.component';
import { UserSubscriptionsComponent } from './user-subscriptions/user-subscriptions.component';
import { ManualActionsComponent } from './manual-actions/manual-actions.component';
import { AnalyticsDashboardComponent } from './analytics-dashboard/analytics-dashboard.component';
import { AuthGuard } from './auth/auth.guard';

const routes: Routes = [
  { path: 'login', component: AdminLoginComponent },
  { path: 'register', component: AdminRegisterComponent },
  { path: 'plans', component: PlanManagementComponent, canActivate: [AuthGuard] },
  { path: 'subscriptions', component: UserSubscriptionsComponent, canActivate: [AuthGuard] },
  { path: 'manual-actions', component: ManualActionsComponent, canActivate: [AuthGuard] },
  { path: 'analytics', component: AnalyticsDashboardComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: 'analytics', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule {}
