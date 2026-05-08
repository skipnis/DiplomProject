import { apiPost } from './client';

export function googleSignIn(idToken: string, rememberMe: boolean): Promise<void> {
  return apiPost<void>('/auth/google', { idToken, rememberMe });
}

export function sendOtp(email: string): Promise<void> {
  return apiPost<void>('/auth/email/send-otp', { email });
}

export function verifyOtp(email: string, code: string, rememberMe: boolean): Promise<void> {
  return apiPost<void>('/auth/email/verify-otp', { email, code, rememberMe });
}

export function logout(): Promise<void> {
  return apiPost<void>('/auth/logout');
}
