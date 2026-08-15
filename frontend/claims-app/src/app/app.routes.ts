import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/claims', pathMatch: 'full' },
  {
    path: 'claims',
    loadComponent: () =>
      import('./features/claims/pages/claim-list/claim-list.component').then((m) => m.ClaimListComponent),
  },
  {
    path: 'claims/new',
    loadComponent: () =>
      import('./features/claims/pages/claim-form/claim-form.component').then((m) => m.ClaimFormComponent),
  },
  {
    path: 'claims/:id',
    loadComponent: () =>
      import('./features/claims/pages/claim-detail/claim-detail.component').then((m) => m.ClaimDetailComponent),
  },
  {
    path: 'claims/:id/edit',
    loadComponent: () =>
      import('./features/claims/pages/claim-form/claim-form.component').then((m) => m.ClaimFormComponent),
  },
  { path: '**', redirectTo: '/claims' },
];