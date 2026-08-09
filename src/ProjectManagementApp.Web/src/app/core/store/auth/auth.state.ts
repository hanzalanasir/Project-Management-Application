export interface AuthUser {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'ProjectManager' | 'TeamMember';
}

export interface AuthState {
  // Access token lives in memory only — NEVER localStorage/sessionStorage (Constitution VII, quickstart).
  accessToken: string | null;
  expiresAt: string | null;
  user: AuthUser | null;
  isLoading: boolean;
  error: string | null;
}

export const initialAuthState: AuthState = {
  accessToken: null,
  expiresAt: null,
  user: null,
  isLoading: false,
  error: null,
};
