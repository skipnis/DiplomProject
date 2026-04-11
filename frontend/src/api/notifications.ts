import { apiGet, apiPatch } from './client';
import type { NotificationDto, PagedResponse } from '../types';

export function getMyNotifications(page = 1, pageSize = 20): Promise<PagedResponse<NotificationDto>> {
  return apiGet<PagedResponse<NotificationDto>>(`/notifications/my?page=${page}&pageSize=${pageSize}`);
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
