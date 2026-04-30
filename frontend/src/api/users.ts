import { apiGet, apiPut, apiPost, apiDelete, apiPostForm } from './client';
import type { MyProfile, UserProfile, UserSearchResult, GiftProfileDto } from '../types';

export function getMyProfile(): Promise<MyProfile> {
  return apiGet<MyProfile>('/users/me');
}

export function updateMyProfile(data: {
  displayName: string;
  username: string;
  bio: string | null;
  birthDate: string | null;
}): Promise<void> {
  return apiPut<void>('/users/me', data);
}

export function getUserProfile(id: string): Promise<UserProfile> {
  return apiGet<UserProfile>(`/users/${id}`);
}

export function searchUsers(displayName: string): Promise<{ items: UserSearchResult[] }> {
  return apiGet<{ items: UserSearchResult[] }>(`/users/search?displayName=${encodeURIComponent(displayName)}`);
}

export function connectGoogleCalendar(code: string): Promise<void> {
  return apiPost<void>('/users/me/google-calendar', { code });
}

export function disconnectGoogleCalendar(): Promise<void> {
  return apiDelete<void>('/users/me/google-calendar');
}

export function uploadAvatar(file: File): Promise<{ avatarUrl: string }> {
  const formData = new FormData();
  formData.append('file', file);
  return apiPostForm<{ avatarUrl: string }>('/users/me/avatar', formData);
}

export function deleteAvatar(): Promise<void> {
  return apiDelete<void>('/users/me/avatar');
}

export function deleteMyAccount(): Promise<void> {
  return apiDelete<void>('/users/me');
}

export function getMyGiftProfile(): Promise<GiftProfileDto> {
  return apiGet<GiftProfileDto>('/users/me/gift-profile');
}

export function getUserGiftProfile(id: string): Promise<GiftProfileDto> {
  return apiGet<GiftProfileDto>(`/users/${id}/gift-profile`);
}
