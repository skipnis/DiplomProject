import { apiGet, apiPost, apiDelete } from './client';
import type { MyReservationDto, PagedResponse, WishReservedOnMyWishDto } from '../types';

export function getMyReservations(
  page = 1,
  pageSize = 20,
): Promise<PagedResponse<MyReservationDto>> {
  return apiGet<PagedResponse<MyReservationDto>>(
    `/reservations/my?page=${page}&pageSize=${pageSize}`,
  );
}

export function reserveWish(wishId: string, wishlistId: string): Promise<void> {
  return apiPost<void>(`/reservations/${wishId}`, { wishlistId });
}

export function cancelReservation(wishId: string): Promise<void> {
  return apiDelete<void>(`/reservations/${wishId}`);
}

export function getReservationsOnMyWishes(): Promise<WishReservedOnMyWishDto[]> {
  return apiGet<WishReservedOnMyWishDto[]>('/reservations/my-wishes');
}
