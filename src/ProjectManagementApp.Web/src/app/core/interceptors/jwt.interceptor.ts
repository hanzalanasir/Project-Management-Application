import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { authFeature } from '../store/auth/auth.feature';

// Attaches Authorization: Bearer to outgoing requests; the access token lives in NgRx memory
// only (never localStorage). Sends withCredentials for /api/auth/* so the refresh cookie travels
// with requests to those endpoints (Constitution VII.4).
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(Store);
  const accessToken = store.selectSignal(authFeature.selectAccessToken)();

  let request = req;

  if (accessToken) {
    request = request.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } });
  }

  if (request.url.startsWith('/api/auth/')) {
    request = request.clone({ withCredentials: true });
  }

  return next(request);
};
