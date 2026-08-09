import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { AuthUser } from './auth.state';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login Submitted': props<{ email: string; password: string }>(),
    'Login Success': props<{ accessToken: string; expiresAt: string; user: AuthUser }>(),
    'Login Failure': props<{ error: string }>(),
    'Token Refreshed': props<{ accessToken: string; expiresAt: string; user: AuthUser }>(),
    'Logout Requested': emptyProps(),
    'Logout Completed': emptyProps(),
    'Session Cleared': emptyProps(),
  },
});
