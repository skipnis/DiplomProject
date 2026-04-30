import { apiGet, apiPost, apiPut, apiDelete } from './client';
import type {
  WishlistDto,
  WishlistSummaryDto,
  WishlistVisibility,
  WishlistMemberInvite,
  WishlistMemberDto,
  WishlistMemberRole,
  PagedResponse,
  FulfilledWishRecordDto,
} from '../types';

export async function getMyWishlists(): Promise<WishlistSummaryDto[]> {
  const res = await apiGet<PagedResponse<WishlistSummaryDto>>('/wishlists/');
  return res.items;
}

export async function getUserWishlists(userId: string): Promise<WishlistSummaryDto[]> {
  const res = await apiGet<PagedResponse<WishlistSummaryDto>>(`/wishlists/users/${userId}`);
  return res.items;
}

export function getWishlist(id: string): Promise<WishlistDto> {
  return apiGet<WishlistDto>(`/wishlists/${id}`);
}

export function createWishlist(data: {
  name: string;
  description: string | null;
  emoji: string | null;
  visibility: WishlistVisibility;
  isSurpriseModeEnabled?: boolean;
  members?: WishlistMemberInvite[];
}): Promise<{ id: string; name: string }> {
  return apiPost<{ id: string; name: string }>('/wishlists/', data);
}

export function updateWishlist(
  id: string,
  data: {
    name: string;
    description: string | null;
    emoji: string | null;
    visibility: WishlistVisibility;
  },
): Promise<void> {
  return apiPut<void>(`/wishlists/${id}`, data);
}

export function deleteWishlist(id: string): Promise<void> {
  return apiDelete<void>(`/wishlists/${id}`);
}

export function getWishlistMembers(id: string): Promise<WishlistMemberDto[]> {
  return apiGet<WishlistMemberDto[]>(`/wishlists/${id}/members`);
}

export function addWishlistMembers(
  id: string,
  members: WishlistMemberInvite[],
): Promise<void> {
  return apiPost<void>(`/wishlists/${id}/members`, { members });
}

export function removeWishlistMember(id: string, userId: string): Promise<void> {
  return apiDelete<void>(`/wishlists/${id}/members/${userId}`);
}

export function updateMemberRole(
  id: string,
  userId: string,
  role: WishlistMemberRole,
  customRoleName: string | null,
): Promise<void> {
  return apiPut<void>(`/wishlists/${id}/members/${userId}/role`, { role, customRoleName });
}

export function getMyFulfilledWishes(): Promise<FulfilledWishRecordDto[]> {
  return apiGet<FulfilledWishRecordDto[]>('/wishlists/fulfilled');
}
