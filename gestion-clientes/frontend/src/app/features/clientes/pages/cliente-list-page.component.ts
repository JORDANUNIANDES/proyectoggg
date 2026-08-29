import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ClienteService } from '../../../core/services/cliente.service';
import { Cliente } from '../../../core/models/cliente.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-cliente-list-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  template: `
    <div class="clientes-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Clientes</h1>
          <p class="page-subtitle">Gestión centralizada y administración de la nómina de clientes</p>
        </div>
        <a mat-raised-button color="primary" routerLink="/clientes/nuevo">
          <mat-icon>person_add</mat-icon> Nuevo Cliente
        </a>
      </div>

      <mat-card class="filter-card">
        <div class="filter-grid">
          <mat-form-field appearance="outline" class="search-field">
            <mat-label>Buscar cliente...</mat-label>
            <input matInput [(ngModel)]="busquedaTexto" (keyup.enter)="aplicarFiltros()" placeholder="Por nombre, RUC, email, teléfono...">
            <button mat-icon-button matSuffix (click)="aplicarFiltros()">
              <mat-icon>search</mat-icon>
            </button>
          </mat-form-field>

          <mat-form-field appearance="outline" class="select-field">
            <mat-label>Filtrar por Estado</mat-label>
            <mat-select [(ngModel)]="filtroEstado" (selectionChange)="aplicarFiltros()">
              <mat-option value="todos">Todos los estados</mat-option>
              <mat-option value="activos">Solo Activos</mat-option>
              <mat-option value="inactivos">Solo Inactivos</mat-option>
            </mat-select>
          </mat-form-field>

          <button mat-stroked-button (click)="limpiarFiltros()" class="reset-btn">
            <mat-icon>refresh</mat-icon> Limpiar
          </button>
        </div>
      </mat-card>

      <mat-card class="table-card">
        @if (cargando()) {
          <div class="spinner-container">
            <mat-spinner diameter="40"></mat-spinner>
          </div>
        } @else {
          <div class="table-responsive">
            <table mat-table [dataSource]="dataSource" matSort class="custom-table">
              <ng-container matColumnDef="nombreCompleto">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Nombre Completo</th>
                <td mat-cell *matCellDef="let cliente" class="font-medium">
                  {{ cliente.nombres }} {{ cliente.apellidos }}
                </td>
              </ng-container>

              <ng-container matColumnDef="ruc">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>RUC</th>
                <td mat-cell *matCellDef="let cliente">{{ cliente.ruc }}</td>
              </ng-container>

              <ng-container matColumnDef="email">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Email</th>
                <td mat-cell *matCellDef="let cliente">{{ cliente.email }}</td>
              </ng-container>

              <ng-container matColumnDef="telefono">
                <th mat-header-cell *matHeaderCellDef>Teléfono</th>
                <td mat-cell *matCellDef="let cliente">{{ cliente.telefono || '-' }}</td>
              </ng-container>

              <ng-container matColumnDef="estado">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Estado</th>
                <td mat-cell *matCellDef="let cliente">
                  <span [class]="cliente.activo ? 'badge badge-success' : 'badge badge-danger'">
                    {{ cliente.activo ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
              </ng-container>

              <ng-container matColumnDef="acciones">
                <th mat-header-cell *matHeaderCellDef class="text-right">Acciones</th>
                <td mat-cell *matCellDef="let cliente" class="text-right action-buttons">
                  <a mat-icon-button color="primary" [routerLink]="['/clientes', cliente.id]" title="Ver detalles">
                    <mat-icon>visibility</mat-icon>
                  </a>
                  <a mat-icon-button color="accent" [routerLink]="['/clientes/editar', cliente.id]" title="Editar cliente">
                    <mat-icon>edit</mat-icon>
                  </a>
                  @if (cliente.activo) {
                    <button mat-icon-button color="warn" (click)="confirmarDesactivar(cliente)" title="Desactivar cliente">
                      <mat-icon>block</mat-icon>
                    </button>
                  } @else {
                    <button mat-icon-button class="activate-btn" (click)="activarCliente(cliente)" title="Activar cliente">
                      <mat-icon>check_circle</mat-icon>
                    </button>
                  }
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="columnas"></tr>
              <tr mat-row *matRowDef="let row; columns: columnas;"></tr>
            </table>

            @if (dataSource.data.length === 0) {
              <div class="empty-state">
                <mat-icon>search_off</mat-icon>
                <p>No se encontraron clientes que coincidan con la búsqueda.</p>
              </div>
            }
          </div>

          <mat-paginator [pageSizeOptions]="[10, 25, 50]" showFirstLastButtons></mat-paginator>
        }
      </mat-card>
    </div>
  `,
  styles: [`
    .clientes-container {
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
    .filter-card {
      padding: 1.25rem;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      background: #ffffff;
    }
    .filter-grid {
      display: flex;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .search-field {
      flex: 1;
      min-width: 260px;
    }
    .select-field {
      width: 200px;
    }
    .reset-btn {
      height: 54px;
      margin-bottom: 22px;
    }
    .table-card {
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      background: #ffffff;
      overflow: hidden;
    }
    .spinner-container {
      display: flex;
      justify-content: center;
      padding: 4rem 0;
    }
    .table-responsive {
      overflow-x: auto;
    }
    .custom-table {
      width: 100%;
    }
    .font-medium { font-weight: 500; color: #1e293b; }
    .badge {
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
    }
    .badge-success { background: #dcfce7; color: #15803d; }
    .badge-danger { background: #fee2e2; color: #b91c1c; }
    .text-right { text-align: right; }
    .activate-btn { color: #16a34a; }
    .empty-state {
      text-align: center;
      padding: 3rem;
      color: #94a3b8;
    }
    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 0.5rem;
    }
  `]
})
export class ClienteListPageComponent implements OnInit {
  private clienteService = inject(ClienteService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  dataSource = new MatTableDataSource<Cliente>([]);
  cargando = signal<boolean>(true);
  columnas: string[] = ['nombreCompleto', 'ruc', 'email', 'telefono', 'estado', 'acciones'];

  busquedaTexto = '';
  filtroEstado = 'todos';

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.cargarClientes();
  }

  cargarClientes(): void {
    this.cargando.set(true);
    let estadoBool: boolean | undefined = undefined;
    if (this.filtroEstado === 'activos') estadoBool = true;
    if (this.filtroEstado === 'inactivos') estadoBool = false;

    this.clienteService.obtenerClientes(this.busquedaTexto, estadoBool).subscribe({
      next: (clientes) => {
        this.dataSource.data = clientes;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
      }
    });
  }

  aplicarFiltros(): void {
    this.cargarClientes();
  }

  limpiarFiltros(): void {
    this.busquedaTexto = '';
    this.filtroEstado = 'todos';
    this.cargarClientes();
  }

  confirmarDesactivar(cliente: Cliente): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        titulo: 'Desactivar Cliente',
        mensaje: `¿Está seguro de que desea desactivar al cliente ${cliente.nombres} ${cliente.apellidos}?`,
        textoConfirmar: 'Desactivar',
        textoCancelar: 'Cancelar'
      }
    });

    dialogRef.afterClosed().subscribe((confirmado) => {
      if (confirmado) {
        this.clienteService.desactivar(cliente.id).subscribe({
          next: () => {
            this.snackBar.open('✓ Cliente desactivado correctamente', 'Cerrar', { duration: 4000 });
            this.cargarClientes();
          }
        });
      }
    });
  }

  activarCliente(cliente: Cliente): void {
    this.clienteService.activar(cliente.id).subscribe({
      next: () => {
        this.snackBar.open('✓ Cliente activado correctamente', 'Cerrar', { duration: 4000 });
        this.cargarClientes();
      }
    });
  }
}
