import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { AuthActions } from './auth.actions';

@Injectable()
export class AuthEffects {
  private readonly actions$ = inject(Actions);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginSubmitted),
      switchMap(({ email, password }) =>
        this.authService.login({ email, password }).pipe(
          map(response =>
            AuthActions.loginSuccess({
              accessToken: response.accessToken,
              expiresAt: response.expiresAt,
              user: {
                id: response.user.id,
                fullName: response.user.fullName,
                email: response.user.email,
                role: response.user.role,
              },
            })
          ),
          catchError((error: HttpErrorResponse) =>
            of(AuthActions.loginFailure({ error: error.error?.title ?? 'Invalid credentials' }))
          )
        )
      )
    )
  );

  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutRequested),
      switchMap(() =>
        this.authService.logout().pipe(
          map(() => AuthActions.logoutCompleted()),
          // Logout clears server-side state regardless of the HTTP outcome — treat any error
          // the same as success so the client session is never left in a half-cleared state.
          catchError(() => of(AuthActions.logoutCompleted()))
        )
      )
    )
  );

  logoutCompleted$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutCompleted),
        tap(() => void this.router.navigateByUrl('/auth/login'))
      ),
    { dispatch: false }
  );
}
