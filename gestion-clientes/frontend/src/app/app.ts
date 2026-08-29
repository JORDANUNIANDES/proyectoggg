import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule
  ],
  template: `
    <div class="app-layout">
      <mat-toolbar class="main-toolbar">
        <div class="brand-title">
          <mat-icon class="brand-icon">business</mat-icon>
          <span>Gestión de Clientes</span>
        </div>

        <nav class="desktop-nav">
          <a mat-button routerLink="/dashboard" routerLinkActive="active-nav">
            <mat-icon>dashboard</mat-icon> Dashboard
          </a>
          <a mat-button routerLink="/clientes" routerLinkActive="active-nav" [routerLinkActiveOptions]="{exact: false}">
            <mat-icon>people</mat-icon> Clientes
          </a>
        </nav>
      </mat-toolbar>

      <main class="main-content">
        <div class="content-wrapper">
          <router-outlet></router-outlet>
        </div>
      </main>

      <footer class="main-footer">
        <p>&copy; 2026 Sistema de Gestión de Clientes. Desarrollado con .NET 10 y Angular 22.</p>
      </footer>
    </div>
  `,
  styles: [`
    .app-layout {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: #f8fafc;
      color: #0f172a;
      font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    }
    .main-toolbar {
      background: #ffffff !important;
      color: #0f172a !important;
      border-bottom: 1px solid #e2e8f0;
      padding: 0 2rem;
      display: flex;
      justify-content: space-between;
      box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.03);
    }
    .brand-title {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-weight: 700;
      font-size: 1.25rem;
      color: #0f172a;
    }
    .brand-icon {
      color: #2563eb;
    }
    .desktop-nav {
      display: flex;
      gap: 0.5rem;
    }
    .desktop-nav a {
      font-weight: 500;
      color: #64748b;
      display: flex;
      align-items: center;
      gap: 0.4rem;
      padding: 0.5rem 1rem;
      border-radius: 8px;
    }
    .desktop-nav a.active-nav {
      color: #2563eb;
      background-color: #eff6ff;
    }
    .main-content {
      flex: 1;
      padding: 2rem;
    }
    .content-wrapper {
      max-width: 1200px;
      margin: 0 auto;
    }
    .main-footer {
      background: #ffffff;
      border-top: 1px solid #e2e8f0;
      padding: 1.25rem 2rem;
      text-align: center;
      color: #94a3b8;
      font-size: 0.875rem;
    }
    @media (max-width: 640px) {
      .main-toolbar {
        padding: 0 1rem;
      }
      .main-content {
        padding: 1rem;
      }
    }
  `]
})
export class AppComponent {}
