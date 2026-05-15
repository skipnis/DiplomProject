import { apiGet, apiPut, apiPost, apiDelete, apiPostForm } from './client';
import type { MyProfile, UserProfile, UserSearchResult, GiftProfileDto, FulfilledWishItem, BlacklistItemDto } from '../types';

export function getMyProfile(): Promise<MyProfile> {
  return apiGet<MyProfile>('/users/me');
}

export function updateMyProfile(data: {
  displayName: string;
  username: string;
  bio: string | null;
  birthDate: string | null;
  showFulfilledWishes: boolean;
}): Promise<void> {
  return apiPut<void>('/users/me', data);
}

export function getUserFulfilledWishes(id: string): Promise<FulfilledWishItem[]> {
  return apiGet<FulfilledWishItem[]>(`/users/${id}/fulfilled-wishes`);
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

export function requestAccountDeletion(): Promise<void> {
  return apiPost<void>('/users/me/delete-confirmation');
}

export function deleteMyAccount(code: string): Promise<void> {
  return apiDelete<void>('/users/me', { code });
}

export function getMyGiftProfile(): Promise<GiftProfileDto> {
  return apiGet<GiftProfileDto>('/users/me/gift-profile');
}

export function getUserGiftProfile(id: string): Promise<GiftProfileDto> {
  return apiGet<GiftProfileDto>(`/users/${id}/gift-profile`);
}

export function checkUsernameAvailability(username: string): Promise<void> {
  return apiGet<void>(`/users/check-username?username=${encodeURIComponent(username)}`);
}

export function getMyBlacklist(): Promise<BlacklistItemDto[]> {
  return apiGet<BlacklistItemDto[]>('/users/me/blacklist');
}

export function getUserBlacklist(id: string): Promise<BlacklistItemDto[]> {
  return apiGet<BlacklistItemDto[]>(`/users/${id}/blacklist`);
}

export function addBlacklistItem(title: string): Promise<BlacklistItemDto> {
  return apiPost<BlacklistItemDto>('/users/me/blacklist', { title });
}

export function deleteBlacklistItem(itemId: string): Promise<void> {
  return apiDelete<void>(`/users/me/blacklist/${itemId}`);
}

export function updateBlacklistItem(itemId: string, title: string): Promise<void> {
  return apiPut<void>(`/users/me/blacklist/${itemId}`, { title });
}
