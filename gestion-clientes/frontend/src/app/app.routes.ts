import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/pages/dashboard-page.component').then(m => m.DashboardPageComponent)
  },
  {
    path: 'clientes',
    loadComponent: () => import('./features/clientes/pages/cliente-list-page.component').then(m => m.ClienteListPageComponent)
  },
  {
    path: 'clientes/nuevo',
    loadComponent: () => import('./features/clientes/pages/cliente-form-page.component').then(m => m.ClienteFormPageComponent)
  },
  {
    path: 'clientes/editar/:id',
    loadComponent: () => import('./features/clientes/pages/cliente-form-page.component').then(m => m.ClienteFormPageComponent)
  },
  {
    path: 'clientes/:id',
    loadComponent: () => import('./features/clientes/pages/cliente-detail-page.component').then(m => m.ClienteDetailPageComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
