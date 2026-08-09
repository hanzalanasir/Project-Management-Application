import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { Observable, catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { NotificationService } from '../../shared/notification/notification.service';
import { AuthService } from '../services/auth.service';
import { AuthActions } from '../store/auth/auth.actions';
import type { components } from '../api/generated/auth.v1';

type AuthTokensResponse = components['schemas']['AuthTokensResponse'];

// Single-flight refresh: several concurrent 401s share exactly ONE call to /api/auth/refresh
// (Constitution VII.4, quickstart V14.3). Module-scoped so it survives across interceptor
// invocations within the same page session — reset once the shared call finishes either way.
let refreshInProgress$: Observable<AuthTokensResponse> | null = null;

// General HTTP error -> notification funnel (Constitution VII.7), plus the 401 single-flight
// refresh-and-retry behavior (US5, T108).
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);
  const authService = inject(AuthService);
  const store = inject(Store);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      if (error.status === 401 && !req.url.includes('/api/auth/login') && !req.url.includes('/api/auth/refresh')) {
        return handleUnauthorized(req, next, authService, store);
      }

      if (error.status !== 401) {
        const detail = (error.error && typeof error.error === 'object' && 'title' in error.error)
          ? String((error.error as { title: unknown }).title)
          : 'Request failed.';
        notifications.notifyError(detail);
      }

      return throwError(() => error);
    })
  );
};

function handleUnauthorized(req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService, store: Store) {
  if (!refreshInProgress$) {
    refreshInProgress$ = authService.refresh().pipe(
      shareReplay(1),
      finalize(() => {
        refreshInProgress$ = null;
      })
    );
  }

  return refreshInProgress$.pipe(
    switchMap(response => {
      store.dispatch(AuthActions.tokenRefreshed({
        accessToken: response.accessToken,
        expiresAt: response.expiresAt,
        user: {
          id: response.user.id,
          fullName: response.user.fullName,
          email: response.user.email,
          role: response.user.role,
        },
      }));

      const retried = req.clone({ setHeaders: { Authorization: `Bearer ${response.accessToken}` } });
      return next(retried);
    }),
    catchError(refreshError => {
      store.dispatch(AuthActions.logoutRequested());
      return throwError(() => refreshError);
    })
  );
}
