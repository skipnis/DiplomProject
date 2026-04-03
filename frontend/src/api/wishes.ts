import { apiGet, apiPost, apiPut, apiDelete, apiPatch, apiPostForm } from './client';
import type { WishDto, WishPriority, Currency, ParsedWishData, PagedResponse } from '../types';

export function getWishes(wishlistId: string, page = 1, pageSize = 20): Promise<PagedResponse<WishDto>> {
  return apiGet<PagedResponse<WishDto>>(`/wishlists/${wishlistId}/wishes?page=${page}&pageSize=${pageSize}`);
}

export function getWish(wishlistId: string, wishId: string): Promise<WishDto> {
  return apiGet<WishDto>(`/wishlists/${wishlistId}/wishes/${wishId}`);
}

export function addWish(
  wishlistId: string,
  data: {
    name: string;
    description: string | null;
    price: number | null;
    currency: Currency | null;
    url: string | null;
    priority: WishPriority;
  },
): Promise<{ wishId: string }> {
  return apiPost<{ wishId: string }>(`/wishlists/${wishlistId}/wishes`, data);
}

export function updateWish(
  wishlistId: string,
  wishId: string,
  data: {
    name: string;
    description: string | null;
    price: number | null;
    currency: Currency | null;
    priority: WishPriority;
    url: string | null;
  },
): Promise<void> {
  return apiPut<void>(`/wishlists/${wishlistId}/wishes/${wishId}`, data);
}

export function deleteWish(wishlistId: string, wishId: string): Promise<void> {
  return apiDelete<void>(`/wishlists/${wishlistId}/wishes/${wishId}`);
}

export function fulfillWish(wishlistId: string, wishId: string): Promise<void> {
  return apiPatch<void>(`/wishlists/${wishlistId}/wishes/${wishId}/fulfill`);
}

export function unfulfillWish(wishlistId: string, wishId: string): Promise<void> {
  return apiPatch<void>(`/wishlists/${wishlistId}/wishes/${wishId}/unfulfill`);
}

export function parseWishUrl(url: string): Promise<ParsedWishData> {
  return apiPost<ParsedWishData>('/wishlists/wishes/parse-url', { url });
}

export function uploadWishImage(
  wishlistId: string,
  wishId: string,
  fileOrUrl: File | string,
): Promise<{ imagePath: string }> {
  const fd = new FormData();
  if (typeof fileOrUrl === 'string') {
    fd.append('externalImageUrl', fileOrUrl);
  } else {
    fd.append('file', fileOrUrl);
  }
  return apiPostForm<{ imagePath: string }>(
    `/wishlists/${wishlistId}/wishes/${wishId}/image`,
    fd,
  );
}

export function deleteWishImage(wishlistId: string, wishId: string): Promise<void> {
  return apiDelete<void>(`/wishlists/${wishlistId}/wishes/${wishId}/image`);
}

export function duplicateWish(wishlistId: string, wishId: string): Promise<{ wishId: string }> {
  return apiPost<{ wishId: string }>(`/wishlists/${wishlistId}/wishes/${wishId}/duplicate`, {});
}

export function copyWish(wishlistId: string, wishId: string, targetWishlistId: string): Promise<{ wishId: string }> {
  return apiPost<{ wishId: string }>(`/wishlists/${wishlistId}/wishes/${wishId}/copy`, { targetWishlistId });
}
