import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let mensajeError = 'Ha ocurrido un error inesperado.';

      if (error.error && error.error.message) {
        mensajeError = error.error.message;
      } else if (error.status === 0) {
        mensajeError = 'No se pudo conectar con el servidor backend.';
      } else if (error.status === 404) {
        mensajeError = 'Recurso no encontrado.';
      } else if (error.status === 409) {
        mensajeError = error.error?.message || 'Conflicto: El registro ya existe.';
      }

      snackBar.open(mensajeError, 'Cerrar', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['error-snackbar']
      });

      return throwError(() => error);
    })
  );
};
