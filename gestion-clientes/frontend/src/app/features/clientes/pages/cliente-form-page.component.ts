import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ClienteService } from '../../../core/services/cliente.service';
import { rucEcuadorValidator } from '../../../shared/validators/ruc-ecuador.validator';

@Component({
  selector: 'app-cliente-form-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="form-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">{{ esEdicion() ? 'Editar Cliente' : 'Nuevo Cliente' }}</h1>
          <p class="page-subtitle">
            {{ esEdicion() ? 'Actualice la información del cliente registrado' : 'Complete el formulario para registrar un nuevo cliente' }}
          </p>
        </div>
        <a mat-stroked-button routerLink="/clientes">
          <mat-icon>arrow_back</mat-icon> Volver
        </a>
      </div>

      <mat-card class="form-card">
        @if (cargando()) {
          <div class="spinner-container">
            <mat-spinner diameter="40"></mat-spinner>
          </div>
        } @else {
          <form [formGroup]="clienteForm" (ngSubmit)="guardarCliente()" class="cliente-form">
            <div class="form-grid">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Nombres</mat-label>
                <input matInput formControlName="nombres" placeholder="Ej. Carlos Alberto">
                @if (clienteForm.get('nombres')?.hasError('required') && clienteForm.get('nombres')?.touched) {
                  <mat-error>El nombre es obligatorio.</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Apellidos</mat-label>
                <input matInput formControlName="apellidos" placeholder="Ej. Mendoza Vera">
                @if (clienteForm.get('apellidos')?.hasError('required') && clienteForm.get('apellidos')?.touched) {
                  <mat-error>El apellido es obligatorio.</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>RUC (Ecuador)</mat-label>
                <input matInput formControlName="ruc" maxlength="13" placeholder="Ej. 1790016919001">
                <mat-hint>13 dígitos numéricos</mat-hint>
                @if (clienteForm.get('ruc')?.touched) {
                  @if (clienteForm.get('ruc')?.hasError('required')) {
                    <mat-error>El RUC es obligatorio.</mat-error>
                  } @else if (clienteForm.get('ruc')?.hasError('rucInvalido')) {
                    <mat-error>{{ clienteForm.get('ruc')?.errors?.['rucInvalido'] }}</mat-error>
                  }
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Correo Electrónico</mat-label>
                <input matInput formControlName="email" type="email" placeholder="Ej. cliente@empresa.com">
                @if (clienteForm.get('email')?.touched) {
                  @if (clienteForm.get('email')?.hasError('required')) {
                    <mat-error>El correo electrónico es obligatorio.</mat-error>
                  } @else if (clienteForm.get('email')?.hasError('email')) {
                    <mat-error>Ingrese un correo electrónico válido.</mat-error>
                  }
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Teléfono / Celular</mat-label>
                <input matInput formControlName="telefono" placeholder="Ej. 0991234567">
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width grid-col-span-2">
                <mat-label>Dirección</mat-label>
                <textarea matInput formControlName="direccion" rows="3" placeholder="Ej. Av. Amazonas N34-12 y Colón, Quito"></textarea>
                @if (clienteForm.get('direccion')?.hasError('required') && clienteForm.get('direccion')?.touched) {
                  <mat-error>La dirección es obligatoria.</mat-error>
                }
              </mat-form-field>
            </div>

            <div class="form-actions">
              <a mat-button routerLink="/clientes" [disabled]="guardando()">Cancelar</a>
              <button mat-raised-button color="primary" type="submit" [disabled]="clienteForm.invalid || guardando()">
                @if (guardando()) {
                  <ng-container>Guardando...</ng-container>
                } @else {
                  <ng-container>{{ esEdicion() ? 'Editar cliente' : 'Guardar cliente' }}</ng-container>
                }
              </button>
            </div>
          </form>
        }
      </mat-card>
    </div>
  `,
  styles: [`
    .form-container {
      max-width: 800px;
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
    .form-card {
      padding: 2rem;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      background: #ffffff;
    }
    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 1.25rem;
    }
    @media (max-width: 640px) {
      .form-grid {
        grid-template-columns: 1fr;
      }
      .grid-col-span-2 {
        grid-column: span 1 !important;
      }
    }
    .grid-col-span-2 {
      grid-column: span 2;
    }
    .full-width {
      width: 100%;
    }
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
      margin-top: 1.5rem;
      padding-top: 1rem;
      border-top: 1px solid #f1f5f9;
    }
    .spinner-container {
      display: flex;
      justify-content: center;
      padding: 3rem 0;
    }
  `]
})
export class ClienteFormPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private clienteService = inject(ClienteService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);

  clienteForm!: FormGroup;
  clienteId = signal<number | null>(null);
  esEdicion = signal<boolean>(false);
  cargando = signal<boolean>(false);
  guardando = signal<boolean>(false);

  ngOnInit(): void {
    this.initForm();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.clienteId.set(+idParam);
      this.esEdicion.set(true);
      this.cargarClienteParaEdicion(+idParam);
    }
  }

  initForm(): void {
    this.clienteForm = this.fb.group({
      nombres: ['', [Validators.required, Validators.maxLength(100)]],
      apellidos: ['', [Validators.required, Validators.maxLength(100)]],
      ruc: ['', [Validators.required, rucEcuadorValidator()]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
      telefono: ['', [Validators.maxLength(20)]],
      direccion: ['', [Validators.required, Validators.maxLength(250)]]
    });
  }

  cargarClienteParaEdicion(id: number): void {
    this.cargando.set(true);
    this.clienteService.obtenerPorId(id).subscribe({
      next: (cliente) => {
        this.clienteForm.patchValue({
          nombres: cliente.nombres,
          apellidos: cliente.apellidos,
          ruc: cliente.ruc,
          email: cliente.email,
          telefono: cliente.telefono,
          direccion: cliente.direccion
        });
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.router.navigate(['/clientes']);
      }
    });
  }

  guardarCliente(): void {
    if (this.clienteForm.invalid) {
      this.clienteForm.markAllAsTouched();
      return;
    }

    this.guardando.set(true);
    const formValue = this.clienteForm.value;

    if (this.esEdicion() && this.clienteId()) {
      this.clienteService.actualizar(this.clienteId()!, formValue).subscribe({
        next: () => {
          this.snackBar.open('✓ Cliente actualizado correctamente', 'Cerrar', { duration: 4000 });
          this.guardando.set(false);
          this.router.navigate(['/clientes']);
        },
        error: () => {
          this.guardando.set(false);
        }
      });
    } else {
      this.clienteService.crear(formValue).subscribe({
        next: () => {
          this.snackBar.open('✓ Cliente registrado correctamente', 'Cerrar', { duration: 4000 });
          this.guardando.set(false);
          this.router.navigate(['/clientes']);
        },
        error: () => {
          this.guardando.set(false);
        }
      });
    }
  }
}
