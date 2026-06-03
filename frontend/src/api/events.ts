import { apiGet, apiPost, apiPut, apiDelete } from './client';
import type { EventDto, PagedResponse } from '../types';

export function createEvent(data: {
  title: string;
  description: string | null;
  date: string;
}): Promise<{ id: string }> {
  return apiPost('/events', data);
}

export function getMyEvents(page = 1, pageSize = 20): Promise<PagedResponse<EventDto>> {
  return apiGet(`/events/my?page=${page}&pageSize=${pageSize}`);
}

export function getEvent(id: string): Promise<EventDto> {
  return apiGet(`/events/${id}`);
}

export function updateEvent(
  id: string,
  data: { title: string; description: string | null; date: string },
): Promise<void> {
  return apiPut(`/events/${id}`, data);
}

export function deleteEvent(id: string): Promise<void> {
  return apiDelete(`/events/${id}`);
}

export function linkWishlist(id: string, wishlistId: string | null): Promise<void> {
  return apiPut(`/events/${id}/wishlist`, { wishlistId });
}

export function getEventByWishlist(wishlistId: string): Promise<EventDto> {
  return apiGet(`/events/wishlist/${wishlistId}`);
}

