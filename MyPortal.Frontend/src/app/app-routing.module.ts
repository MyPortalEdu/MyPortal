import {NgModule} from '@angular/core';
import {PreloadAllModules, RouterModule, Routes} from '@angular/router';
import {AuthGuard} from './core/guards/auth.guard';
import {PortalRedirectComponent} from './core/components/portal-redirect/portal-redirect.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'portal'
  },
  {
    path: 'portal',
    canActivate: [AuthGuard],
    component: PortalRedirectComponent
  },
  {
    path: 'staff',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/staff/staff.module').then(m => m.StaffModule)
  },
  {
    path: 'student',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/student/student.module').then(m => m.StudentModule)
  },
  {
    path: 'parent',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/parent/parent.module').then(m => m.ParentModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {preloadingStrategy: PreloadAllModules})],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
