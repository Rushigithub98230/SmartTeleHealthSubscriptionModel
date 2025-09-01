import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayoutComponent } from './admin/admin-layout.component';

const routes: Routes = [
  { path: 'admin', component: AdminLayoutComponent, loadChildren: () => import('./admin/admin-routing.module').then(m => m.AdminRoutingModule) },
  { path: '', redirectTo: '/admin', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
