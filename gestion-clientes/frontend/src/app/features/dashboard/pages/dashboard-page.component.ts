import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { ClienteService } from '../../../core/services/cliente.service';
import { DashboardSummary } from '../../../core/models/cliente.model';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTableModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <div>
          <h1 class="page-title">Dashboard</h1>
          <p class="page-subtitle">Resumen ejecutivo y métricas principales de clientes</p>
        </div>
        <a mat-raised-button color="primary" routerLink="/clientes/nuevo">
          <mat-icon>add</mat-icon> Nuevo Cliente
        </a>
      </div>

      @if (cargando()) {
        <div class="spinner-container">
          <mat-spinner diameter="40"></mat-spinner>
        </div>
      } @else if (resumen()) {
        <div class="stats-grid">
          <mat-card class="stat-card">
            <mat-card-content class="stat-content">
              <div class="stat-icon total-icon">
                <mat-icon>groups</mat-icon>
              </div>
              <div class="stat-data">
                <span class="stat-value">{{ resumen()?.totalClientes }}</span>
                <span class="stat-label">Clientes Totales</span>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content class="stat-content">
              <div class="stat-icon active-icon">
                <mat-icon>check_circle</mat-icon>
              </div>
              <div class="stat-data">
                <span class="stat-value">{{ resumen()?.clientesActivos }}</span>
                <span class="stat-label">Clientes Activos</span>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content class="stat-content">
              <div class="stat-icon inactive-icon">
                <mat-icon>block</mat-icon>
              </div>
              <div class="stat-data">
                <span class="stat-value">{{ resumen()?.clientesInactivos }}</span>
                <span class="stat-label">Clientes Inactivos</span>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content class="stat-content">
              <div class="stat-icon recent-icon">
                <mat-icon>person_add</mat-icon>
              </div>
              <div class="stat-data">
                <span class="stat-value">{{ resumen()?.clientesRecientes }}</span>
                <span class="stat-label">Registros Recientes (30d)</span>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <mat-card class="recent-card">
          <div class="card-header-flex">
            <h2>Últimos Clientes Registrados</h2>
            <a mat-button color="primary" routerLink="/clientes">Ver todos los clientes →</a>
          </div>

          @if (resumen()?.ultimosClientes?.length) {
            <div class="table-responsive">
              <table mat-table [dataSource]="resumen()!.ultimosClientes" class="custom-table">
                <ng-container matColumnDef="nombreCompleto">
                  <th mat-header-cell *matHeaderCellDef>Cliente</th>
                  <td mat-cell *matCellDef="let cliente" class="font-medium">
                    {{ cliente.nombres }} {{ cliente.apellidos }}
                  </td>
                </ng-container>

                <ng-container matColumnDef="ruc">
                  <th mat-header-cell *matHeaderCellDef>RUC</th>
                  <td mat-cell *matCellDef="let cliente">{{ cliente.ruc }}</td>
                </ng-container>

                <ng-container matColumnDef="email">
                  <th mat-header-cell *matHeaderCellDef>Email</th>
                  <td mat-cell *matCellDef="let cliente">{{ cliente.email }}</td>
                </ng-container>

                <ng-container matColumnDef="fecha">
                  <th mat-header-cell *matHeaderCellDef>Fecha Registro</th>
                  <td mat-cell *matCellDef="let cliente">
                    {{ cliente.fechaRegistro | date:'dd/MM/yyyy' }}
                  </td>
                </ng-container>

                <ng-container matColumnDef="estado">
                  <th mat-header-cell *matHeaderCellDef>Estado</th>
                  <td mat-cell *matCellDef="let cliente">
                    <span [class]="cliente.activo ? 'badge badge-success' : 'badge badge-danger'">
                      {{ cliente.activo ? 'Activo' : 'Inactivo' }}
                    </span>
                  </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="columnas"></tr>
                <tr mat-row *matRowDef="let row; columns: columnas;"></tr>
              </table>
            </div>
          } @else {
            <div class="empty-state">
              <mat-icon>person_off</mat-icon>
              <p>No existen clientes registrados actualmente.</p>
            </div>
          }
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .dashboard-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .page-title {
      font-size: 1.75rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0;
    }
    .page-subtitle {
      color: #64748b;
      margin: 0.25rem 0 0 0;
      font-size: 0.95rem;
    }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1.25rem;
    }
    .stat-card {
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      background: #ffffff;
      transition: transform 0.2s, box-shadow 0.2s;
    }
    .stat-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.08);
    }
    .stat-content {
      display: flex;
      align-items: center;
      gap: 1.25rem;
      padding: 1.25rem;
    }
    .stat-icon {
      width: 52px;
      height: 52px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .stat-icon mat-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
    }
    .total-icon { background-color: #eff6ff; color: #2563eb; }
    .active-icon { background-color: #f0fdf4; color: #16a34a; }
    .inactive-icon { background-color: #fef2f2; color: #dc2626; }
    .recent-icon { background-color: #faf5ff; color: #9333ea; }

    .stat-data {
      display: flex;
      flex-direction: column;
    }
    .stat-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: #0f172a;
      line-height: 1.2;
    }
    .stat-label {
      font-size: 0.85rem;
      color: #64748b;
      font-weight: 500;
    }
    .recent-card {
      padding: 1.5rem;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      background: #ffffff;
    }
    .card-header-flex {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .card-header-flex h2 {
      font-size: 1.2rem;
      font-weight: 600;
      color: #1e293b;
      margin: 0;
    }
    .spinner-container {
      display: flex;
      justify-content: center;
      padding: 3rem 0;
    }
    .empty-state {
      text-align: center;
      padding: 2.5rem;
      color: #94a3b8;
    }
    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 0.5rem;
    }
    .font-medium { font-weight: 500; }
    .badge {
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
    }
    .badge-success { background: #dcfce7; color: #15803d; }
    .badge-danger { background: #fee2e2; color: #b91c1c; }
    .table-responsive {
      overflow-x: auto;
    }
    .custom-table {
      width: 100%;
    }
  `]
})
export class DashboardPageComponent implements OnInit {
  private clienteService = inject(ClienteService);

  resumen = signal<DashboardSummary | null>(null);
  cargando = signal<boolean>(true);
  columnas: string[] = ['nombreCompleto', 'ruc', 'email', 'fecha', 'estado'];

  ngOnInit(): void {
    this.cargarDashboard();
  }

  cargarDashboard(): void {
    this.cargando.set(true);
    this.clienteService.obtenerDashboard().subscribe({
      next: (data) => {
        this.resumen.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
      }
    });
  }
}
