import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { isDevMode } from '@angular/core';
import { catchError, throwError } from 'rxjs';

/**
 * Manejo común de errores HTTP.
 * En desarrollo registra los errores 5xx en consola. No expone mensajes
 * técnicos al usuario: cada componente decide qué mensaje mostrar.
 */
export const httpErrorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (isDevMode() && error.status >= 500) {
        console.error('Error de API no controlado', error);
      }
      return throwError(() => error);
    }),
  );