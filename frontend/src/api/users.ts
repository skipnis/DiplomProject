import { apiGet, apiPut, apiPost, apiDelete } from './client';
import type { MyProfile, UserProfile, UserSearchResult } from '../types';

export function getMyProfile(): Promise<MyProfile> {
  return apiGet<MyProfile>('/users/me');
}

export function updateMyProfile(data: {
  username: string;
  bio: string | null;
  birthDate: string | null;
}): Promise<void> {
  return apiPut<void>('/users/me', data);
}

export function getUserProfile(id: string): Promise<UserProfile> {
  return apiGet<UserProfile>(`/users/${id}`);
}

export function searchUsers(username: string): Promise<{ items: UserSearchResult[] }> {
  return apiGet<{ items: UserSearchResult[] }>(`/users/search?username=${encodeURIComponent(username)}`);
}

export function connectGoogleCalendar(code: string): Promise<void> {
  return apiPost<void>('/users/me/google-calendar', { code });
}

export function disconnectGoogleCalendar(): Promise<void> {
  return apiDelete<void>('/users/me/google-calendar');
}

export function deleteMyAccount(): Promise<void> {
  return apiDelete<void>('/users/me');
}
