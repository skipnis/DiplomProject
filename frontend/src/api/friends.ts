import { apiGet, apiPost, apiPut, apiDelete } from './client';
import type { FriendInfo, FriendshipRequest, PagedResponse } from '../types';

export function getFriends(page = 1, pageSize = 20): Promise<PagedResponse<FriendInfo>> {
  return apiGet<PagedResponse<FriendInfo>>(`/friendships/?page=${page}&pageSize=${pageSize}`);
}

export function getFriendshipRequests(status = 'Pending', page = 1, pageSize = 20): Promise<PagedResponse<FriendshipRequest>> {
  return apiGet<PagedResponse<FriendshipRequest>>(`/friendships/requests?status=${status}&page=${page}&pageSize=${pageSize}`);
}

export function sendFriendRequest(userId: string): Promise<void> {
  return apiPost<void>(`/friendships/${userId}`);
}

export function acceptFriendRequest(userId: string): Promise<void> {
  return apiPut<void>(`/friendships/${userId}/accept`);
}

export function declineFriendRequest(userId: string): Promise<void> {
  return apiPut<void>(`/friendships/${userId}/decline`);
}

export function removeFriend(userId: string): Promise<void> {
  return apiDelete<void>(`/friendships/${userId}`);
}
