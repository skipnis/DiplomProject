import { apiGet } from './client';
import type { SharedWishResponse } from '../types';

export function getSharedWish(token: string): Promise<SharedWishResponse> {
  return apiGet<SharedWishResponse>(`/share/${token}`);
}
