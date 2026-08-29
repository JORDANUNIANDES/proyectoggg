import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ClienteService } from '../../../core/services/cliente.service';
import { Cliente } from '../../../core/models/cliente.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-cliente-detail-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  template: `
    <div class="detail-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Detalles del Cliente</h1>
          <p class="page-subtitle">Consulta de la información completa del expediente de cliente</p>
        </div>
        <a mat-stroked-button routerLink="/clientes">
          <mat-icon>arrow_back</mat-icon> Volver
        </a>
      </div>

      @if (cargando()) {
        <div class="spinner-container">
          <mat-spinner diameter="40"></mat-spinner>
        </div>
      } @else if (cliente()) {
        <mat-card class="detail-card">
          <div class="card-top-header">
            <div class="user-info-main">
              <div class="avatar-box">
                {{ cliente()?.nombres?.charAt(0) }}{{ cliente()?.apellidos?.charAt(0) }}
              </div>
              <div>
                <h2>{{ cliente()?.nombres }} {{ cliente()?.apellidos }}</h2>
                <span [class]="cliente()?.activo ? 'badge badge-success' : 'badge badge-danger'">
                  {{ cliente()?.activo ? 'Cliente Activo' : 'Cliente Inactivo' }}
                </span>
              </div>
            </div>

            <div class="top-actions">
              <a mat-raised-button color="accent" [routerLink]="['/clientes/editar', cliente()?.id]">
                <mat-icon>edit</mat-icon> Editar
              </a>
              @if (cliente()?.activo) {
                <button mat-raised-button color="warn" (click)="confirmarDesactivar()">
                  <mat-icon>block</mat-icon> Desactivar
                </button>
              } @else {
                <button mat-raised-button class="activate-btn" (click)="activarCliente()">
                  <mat-icon>check_circle</mat-icon> Activar
                </button>
              }
            </div>
          </div>

          <div class="detail-grid">
            <div class="info-group">
              <span class="info-label"><mat-icon>badge</mat-icon> RUC</span>
              <span class="info-value">{{ cliente()?.ruc }}</span>
            </div>

            <div class="info-group">
              <span class="info-label"><mat-icon>email</mat-icon> Correo Electrónico</span>
              <span class="info-value">{{ cliente()?.email }}</span>
            </div>

            <div class="info-group">
              <span class="info-label"><mat-icon>phone</mat-icon> Teléfono</span>
              <span class="info-value">{{ cliente()?.telefono || 'No especificado' }}</span>
            </div>

            <div class="info-group">
              <span class="info-label"><mat-icon>calendar_today</mat-icon> Fecha de Registro</span>
              <span class="info-value">{{ cliente()?.fechaRegistro | date:'dd/MM/yyyy HH:mm' }}</span>
            </div>

            <div class="info-group full-width">
              <span class="info-label"><mat-icon>location_on</mat-icon> Dirección</span>
              <span class="info-value">{{ cliente()?.direccion }}</span>
            </div>
          </div>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .detail-container {
      max-width: 850px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .page-header {
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
    .detail-card {
      padding: 2rem;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      background: #ffffff;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }
    .card-top-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid #f1f5f9;
    }
    .user-info-main {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .avatar-box {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: #3b82f6;
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 1.25rem;
      text-transform: uppercase;
    }
    .user-info-main h2 {
      font-size: 1.4rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0 0 0.25rem 0;
    }
    .top-actions {
      display: flex;
      gap: 0.75rem;
    }
    .activate-btn {
      background-color: #16a34a !important;
      color: #ffffff !important;
    }
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 1.5rem;
    }
    @media (max-width: 640px) {
      .detail-grid {
        grid-template-columns: 1fr;
      }
    }
    .info-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .info-group.full-width {
      grid-column: span 2;
    }
    @media (max-width: 640px) {
      .info-group.full-width {
        grid-column: span 1;
      }
    }
    .info-label {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.85rem;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.025em;
    }
    .info-label mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: #94a3b8;
    }
    .info-value {
      font-size: 1.05rem;
      color: #1e293b;
      font-weight: 500;
    }
    .badge {
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
      display: inline-block;
    }
    .badge-success { background: #dcfce7; color: #15803d; }
    .badge-danger { background: #fee2e2; color: #b91c1c; }
    .spinner-container {
      display: flex;
      justify-content: center;
      padding: 3rem 0;
    }
  `]
})
export class ClienteDetailPageComponent implements OnInit {
  private clienteService = inject(ClienteService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  cliente = signal<Cliente | null>(null);
  cargando = signal<boolean>(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.cargarCliente(+id);
    } else {
      this.router.navigate(['/clientes']);
    }
  }

  cargarCliente(id: number): void {
    this.cargando.set(true);
    this.clienteService.obtenerPorId(id).subscribe({
      next: (data) => {
        this.cliente.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.router.navigate(['/clientes']);
      }
    });
  }

  confirmarDesactivar(): void {
    if (!this.cliente()) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        titulo: 'Desactivar Cliente',
        mensaje: `¿Está seguro de que desea desactivar al cliente ${this.cliente()?.nombres} ${this.cliente()?.apellidos}?`,
        textoConfirmar: 'Desactivar',
        textoCancelar: 'Cancelar'
      }
    });

    dialogRef.afterClosed().subscribe((confirmado) => {
      if (confirmado && this.cliente()) {
        this.clienteService.desactivar(this.cliente()!.id).subscribe({
          next: () => {
            this.snackBar.open('✓ Cliente desactivado correctamente', 'Cerrar', { duration: 4000 });
            this.cargarCliente(this.cliente()!.id);
          }
        });
      }
    });
  }

  activarCliente(): void {
    if (!this.cliente()) return;

    this.clienteService.activar(this.cliente()!.id).subscribe({
      next: () => {
        this.snackBar.open('✓ Cliente activado correctamente', 'Cerrar', { duration: 4000 });
        this.cargarCliente(this.cliente()!.id);
      }
    });
  }
}
