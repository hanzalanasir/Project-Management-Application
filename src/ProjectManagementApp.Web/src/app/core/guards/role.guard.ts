import { inject } from '@angular/core';
import { CanMatchFn, Route, UrlSegment } from '@angular/router';
import { Store } from '@ngrx/store';
import { authFeature } from '../store/auth/auth.feature';

// CanMatch (not CanActivate): a lazy chunk whose role fails is not even downloaded.
export function roleGuard(allowedRoles: string[]): CanMatchFn {
  return (_route: Route, _segments: UrlSegment[]) => {
    const store = inject(Store);
    const role = store.selectSignal(authFeature.selectUser)()?.role;
    return !!role && allowedRoles.includes(role);
  };
}
