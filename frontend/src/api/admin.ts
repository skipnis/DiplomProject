import { API_URL } from './client';
import { ApiError } from '../utils/errors';
import type {
  AchievementDefinitionAdminDto,
  AchievementRuleType,
  CatalogBadgeDefinitionDto,
  CatalogCategoryDto,
  CatalogCollectionAdminDto,
  CatalogItemDto,
  CreateCatalogCategoryRequest,
  CreateCatalogItemRequest,
  CreateCollectionRequest,
  FulfilledBadgeDefinitionDto,
  OccasionDto,
  PagedResponse,
  ParsedWishData,
  UpdateCatalogCategoryRequest,
  UpdateCatalogItemRequest,
  UpdateCollectionRequest,
} from '../types';

export const ADMIN_TOKEN_KEY = 'admin_token';

export function getAdminToken(): string | null {
  return localStorage.getItem(ADMIN_TOKEN_KEY);
}

async function adminRequest<T>(
  method: string,
  path: string,
  body?: unknown,
): Promise<T> {
  const token = getAdminToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (res.status === 401) {
    localStorage.removeItem(ADMIN_TOKEN_KEY);
    window.location.href = '/admin/login';
    throw new Error('Unauthorized');
  }

  if (!res.ok) {
    const text = await res.text();
    throw new ApiError(res.status, text || `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;

  const ct = res.headers.get('content-type');
  if (ct?.includes('application/json')) {
    return res.json() as Promise<T>;
  }

  return undefined as T;
}

export function adminLogin(username: string, password: string): Promise<{ token: string }> {
  return adminRequest<{ token: string }>('POST', '/admin/auth/login', { username, password });
}

export function adminParseUrl(url: string): Promise<ParsedWishData> {
  return adminRequest<ParsedWishData>('POST', '/wishlists/wishes/parse-url', { url });
}

export function adminGetCategories(): Promise<CatalogCategoryDto[]> {
  return adminRequest<CatalogCategoryDto[]>('GET', '/admin/catalog/categories');
}

export function adminCreateCategory(data: CreateCatalogCategoryRequest): Promise<string> {
  return adminRequest<string>('POST', '/admin/catalog/categories', data);
}

export function adminUpdateCategory(id: string, data: UpdateCatalogCategoryRequest): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/categories/${id}`, data);
}

export function adminDeleteCategory(id: string): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/categories/${id}`);
}

export function adminGetAllItems(params: {
  categoryId?: string;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
}): Promise<PagedResponse<CatalogItemDto>> {
  const query = new URLSearchParams();
  if (params.categoryId) query.set('categoryId', params.categoryId);
  if (params.search) query.set('search', params.search);
  if (params.minPrice !== undefined) query.set('minPrice', String(params.minPrice));
  if (params.maxPrice !== undefined) query.set('maxPrice', String(params.maxPrice));
  if (params.page) query.set('page', String(params.page));
  if (params.pageSize) query.set('pageSize', String(params.pageSize));
  return adminRequest<PagedResponse<CatalogItemDto>>('GET', `/admin/catalog/items?${query.toString()}`);
}

export function adminCreateItem(data: CreateCatalogItemRequest): Promise<string> {
  return adminRequest<string>('POST', '/admin/catalog/items', data);
}

export function adminUpdateItem(id: string, data: UpdateCatalogItemRequest): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/items/${id}`, data);
}

export function adminDeleteItem(id: string): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/items/${id}`);
}

export function adminSetItemPublished(id: string, isPublished: boolean): Promise<void> {
  return adminRequest<void>('PATCH', `/admin/catalog/items/${id}/published`, { isPublished });
}

export function adminSetCategoryPublished(id: string, isPublished: boolean): Promise<void> {
  return adminRequest<void>('PATCH', `/admin/catalog/categories/${id}/published`, { isPublished });
}

export function adminSetCollectionPublished(id: string, isPublished: boolean): Promise<void> {
  return adminRequest<void>('PATCH', `/admin/catalog/collections/${id}/published`, { isPublished });
}

export function adminGetOccasions(): Promise<OccasionDto[]> {
  return adminRequest<OccasionDto[]>('GET', '/admin/catalog/occasions');
}

export function adminCreateOccasion(data: { key: string; label: string; order: number }): Promise<string> {
  return adminRequest<string>('POST', '/admin/catalog/occasions', data);
}

export function adminUpdateOccasion(id: string, data: { key: string; label: string; order: number }): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/occasions/${id}`, data);
}

export function adminDeleteOccasion(id: string): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/occasions/${id}`);
}

export function adminGetAllCollections(): Promise<CatalogCollectionAdminDto[]> {
  return adminRequest<CatalogCollectionAdminDto[]>('GET', '/admin/catalog/collections');
}

export function adminCreateCollection(data: CreateCollectionRequest): Promise<string> {
  return adminRequest<string>('POST', '/admin/catalog/collections', data);
}

export function adminUpdateCollection(id: string, data: UpdateCollectionRequest): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/collections/${id}`, data);
}

export function adminDeleteCollection(id: string): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/collections/${id}`);
}

export function adminAddItemToCollection(collectionId: string, itemId: string, description?: string): Promise<void> {
  return adminRequest<void>('POST', `/admin/catalog/collections/${collectionId}/items/${itemId}`, { description: description || null });
}

export function adminUpdateCollectionItemDescription(collectionId: string, itemId: string, description: string | null): Promise<void> {
  return adminRequest<void>('PATCH', `/admin/catalog/collections/${collectionId}/items/${itemId}/description`, { description });
}

export function adminRemoveItemFromCollection(collectionId: string, itemId: string): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/collections/${collectionId}/items/${itemId}`);
}

export function adminGetCollectionItems(collectionId: string): Promise<CatalogItemDto[]> {
  return adminRequest<CatalogItemDto[]>('GET', `/admin/catalog/collections/${collectionId}/items`);
}

async function adminRequestForm<T>(path: string, data: FormData): Promise<T> {
  const token = getAdminToken();
  const headers: Record<string, string> = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_URL}${path}`, {
    method: 'POST',
    headers,
    body: data,
  });

  if (res.status === 401) {
    localStorage.removeItem(ADMIN_TOKEN_KEY);
    window.location.href = '/admin/login';
    throw new Error('Unauthorized');
  }

  if (!res.ok) {
    const text = await res.text();
    throw new ApiError(res.status, text || `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;

  const ct = res.headers.get('content-type');
  if (ct?.includes('application/json')) {
    return res.json() as Promise<T>;
  }

  return undefined as T;
}

export function adminUploadItemImage(itemId: string, fileOrUrl: File | string): Promise<{ imagePath: string }> {
  const fd = new FormData();
  if (typeof fileOrUrl === 'string') {
    fd.append('externalImageUrl', fileOrUrl);
  } else {
    fd.append('file', fileOrUrl);
  }
  return adminRequestForm<{ imagePath: string }>(`/admin/catalog/items/${itemId}/image`, fd);
}

export function adminUploadCollectionImage(collectionId: string, file: File): Promise<{ coverImagePath: string }> {
  const fd = new FormData();
  fd.append('file', file);
  return adminRequestForm<{ coverImagePath: string }>(`/admin/catalog/collections/${collectionId}/image`, fd);
}

export interface BatchImportItemResult {
  url: string;
  status: 'Success' | 'Partial' | 'Failed';
  itemId: string | null;
  missingFields: string[];
  errorMessage: string | null;
}

export function adminBatchImportItems(data: {
  urls: string[];
  categoryId: string;
}): Promise<BatchImportItemResult[]> {
  return adminRequest<BatchImportItemResult[]>('POST', '/admin/catalog/items/batch-import', data);
}

export function adminGetCatalogBadgeDefinitions(): Promise<CatalogBadgeDefinitionDto[]> {
  return adminRequest<CatalogBadgeDefinitionDto[]>('GET', '/admin/catalog/badge-definitions/catalog');
}

export function adminCreateCatalogBadgeDefinition(data: { label: string; isActive: boolean }): Promise<number> {
  return adminRequest<number>('POST', '/admin/catalog/badge-definitions/catalog', data);
}

export function adminUpdateCatalogBadgeDefinition(id: number, data: { label: string; isActive: boolean }): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/badge-definitions/catalog/${id}`, data);
}

export function adminDeleteCatalogBadgeDefinition(id: number): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/badge-definitions/catalog/${id}`);
}

export function adminGetFulfilledBadgeDefinitions(): Promise<FulfilledBadgeDefinitionDto[]> {
  return adminRequest<FulfilledBadgeDefinitionDto[]>('GET', '/admin/catalog/badge-definitions/fulfilled');
}

export function adminCreateFulfilledBadgeDefinition(data: { label: string; isActive: boolean }): Promise<number> {
  return adminRequest<number>('POST', '/admin/catalog/badge-definitions/fulfilled', data);
}

export function adminUpdateFulfilledBadgeDefinition(id: number, data: { label: string; isActive: boolean }): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/badge-definitions/fulfilled/${id}`, data);
}

export function adminDeleteFulfilledBadgeDefinition(id: number): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/badge-definitions/fulfilled/${id}`);
}

export function adminGetAchievementDefinitions(): Promise<AchievementDefinitionAdminDto[]> {
  return adminRequest<AchievementDefinitionAdminDto[]>('GET', '/admin/catalog/achievements');
}

export function adminCreateAchievementDefinition(data: {
  name: string; description: string; emoji: string;
  ruleType: AchievementRuleType; linkedBadgeTypeId: number | null;
  threshold: number; order: number; isActive: boolean;
}): Promise<number> {
  return adminRequest<number>('POST', '/admin/catalog/achievements', data);
}

export function adminUpdateAchievementDefinition(id: number, data: {
  name: string; description: string; emoji: string;
  ruleType: AchievementRuleType; linkedBadgeTypeId: number | null;
  threshold: number; order: number; isActive: boolean;
}): Promise<void> {
  return adminRequest<void>('PUT', `/admin/catalog/achievements/${id}`, data);
}

export function adminDeleteAchievementDefinition(id: number): Promise<void> {
  return adminRequest<void>('DELETE', `/admin/catalog/achievements/${id}`);
}

export interface AdminStatsResponse {
  users: {
    total: number;
    newLast7Days: number;
    newLast30Days: number;
  };
  content: {
    totalWishlists: number;
    totalWishes: number;
    averageWishesPerWishlist: number;
    wishesWithImage: number;
    wishesWithoutImage: number;
  };
  activity: {
    activeReservations: number;
    fulfilledWishes: number;
    fulfilledWithGifter: number;
    topGifters: { userId: string; displayName: string; fulfilledCount: number }[];
  };
  catalog: {
    topItems: { id: string; name: string; wishCount: number }[];
  };
}

export function adminGetStats(): Promise<AdminStatsResponse> {
  return adminRequest<AdminStatsResponse>('GET', '/admin/stats');
}
