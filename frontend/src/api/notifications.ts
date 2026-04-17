import { apiGet, apiPatch, apiDelete } from './client';
import type { NotificationDto, PagedResponse } from '../types';

export function getMyNotifications(
  page = 1,
  pageSize = 20,
  from?: string,
  to?: string,
  isRead?: boolean,
): Promise<PagedResponse<NotificationDto>> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (from) params.set('from', from);
  if (to) params.set('to', to);
  if (isRead !== undefined) params.set('isRead', String(isRead));
  return apiGet<PagedResponse<NotificationDto>>(`/notifications/my?${params}`);
}

export function getUnreadCount(): Promise<number> {
  return apiGet<number>('/notifications/unread-count');
}

export function markAsRead(id: string): Promise<void> {
  return apiPatch<void>(`/notifications/${id}/read`);
}

export function markAllAsRead(): Promise<void> {
  return apiPatch<void>('/notifications/read-all');
}

export function deleteNotification(id: string): Promise<void> {
  return apiDelete<void>(`/notifications/${id}`);
}
