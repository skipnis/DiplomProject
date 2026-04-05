import { apiPost } from './client';

export function googleSignIn(idToken: string): Promise<void> {
  return apiPost<void>('/auth/google', { idToken });
}

export function sendOtp(email: string): Promise<void> {
  return apiPost<void>('/auth/email/send-otp', { email });
}

export function verifyOtp(email: string, code: string): Promise<void> {
  return apiPost<void>('/auth/email/verify-otp', { email, code });
}

export function logout(): Promise<void> {
  return apiPost<void>('/auth/logout');
}
