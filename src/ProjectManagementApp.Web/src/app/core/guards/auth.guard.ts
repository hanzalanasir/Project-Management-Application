import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { authFeature } from '../store/auth/auth.feature';

// Guards are the only navigation-blocking mechanism (Constitution VII.5) — no component redirects.
export const authGuard: CanActivateFn = () => {
  const store = inject(Store);
  const router = inject(Router);

  const accessToken = store.selectSignal(authFeature.selectAccessToken)();
  return accessToken ? true : router.createUrlTree(['/auth/login']);
};
