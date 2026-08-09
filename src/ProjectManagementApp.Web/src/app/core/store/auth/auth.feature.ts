import { createFeature, createReducer, on } from '@ngrx/store';
import { initialAuthState } from './auth.state';
import { AuthActions } from './auth.actions';

export const authFeature = createFeature({
  name: 'auth',
  reducer: createReducer(
    initialAuthState,
    on(AuthActions.loginSubmitted, state => ({ ...state, isLoading: true, error: null })),
    on(AuthActions.loginSuccess, AuthActions.tokenRefreshed, (state, { accessToken, expiresAt, user }) => ({
      ...state,
      isLoading: false,
      accessToken,
      expiresAt,
      user,
      error: null,
    })),
    on(AuthActions.loginFailure, (state, { error }) => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(AuthActions.sessionCleared, AuthActions.logoutCompleted, () => initialAuthState)
  ),
});
