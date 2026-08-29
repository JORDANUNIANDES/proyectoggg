import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmDialogData {
  titulo: string;
  mensaje: string;
  textoConfirmar?: string;
  textoCancelar?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title class="dialog-title">{{ data.titulo }}</h2>
    <mat-dialog-content class="dialog-content">
      <p>{{ data.mensaje }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end" class="dialog-actions">
      <button mat-button (click)="cancelar()">{{ data.textoCancelar || 'Cancelar' }}</button>
      <button mat-raised-button color="warn" (click)="confirmar()">{{ data.textoConfirmar || 'Aceptar' }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-title {
      font-weight: 600;
      color: #1e293b;
    }
    .dialog-content {
      color: #475569;
      font-size: 0.95rem;
    }
    .dialog-actions {
      padding-top: 1rem;
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  cancelar(): void {
    this.dialogRef.close(false);
  }

  confirmar(): void {
    this.dialogRef.close(true);
  }
}
