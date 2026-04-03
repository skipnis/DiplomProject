import { apiPost } from './client';

export function googleSignIn(idToken: string): Promise<void> {
  return apiPost<void>('/auth/google', { idToken });
}

export function logout(): Promise<void> {
  return apiPost<void>('/auth/logout');
}
