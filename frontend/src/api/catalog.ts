import { apiGet, apiPost, apiDelete } from './client';
import type {
  CatalogBadgeDefinitionDto,
  CatalogCategoryDto,
  CatalogCollectionDto,
  CatalogCollectionSummaryDto,
  CatalogItemDto,
  FulfilledBadgeDefinitionDto,
  OccasionDto,
  PagedResponse,
} from '../types';

export function getCatalogCategories(): Promise<CatalogCategoryDto[]> {
  return apiGet<CatalogCategoryDto[]>('/catalog/categories');
}

export function getCatalogPriceRange(): Promise<{ max: number }> {
  return apiGet<{ max: number }>('/catalog/price-range');
}

export function getCatalogItems(params: {
  categoryId?: string;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  occasionIds?: string[];
  page?: number;
  pageSize?: number;
}): Promise<PagedResponse<CatalogItemDto>> {
  const query = new URLSearchParams();
  if (params.categoryId) query.set('categoryId', params.categoryId);
  if (params.search) query.set('search', params.search);
  if (params.minPrice !== undefined) query.set('minPrice', String(params.minPrice));
  if (params.maxPrice !== undefined) query.set('maxPrice', String(params.maxPrice));
  if (params.occasionIds?.length) params.occasionIds.forEach((id) => query.append('occasionIds', id));
  if (params.page) query.set('page', String(params.page));
  if (params.pageSize) query.set('pageSize', String(params.pageSize));
  return apiGet<PagedResponse<CatalogItemDto>>(`/catalog/items?${query.toString()}`);
}

export function getCatalogItem(id: string): Promise<CatalogItemDto> {
  return apiGet<CatalogItemDto>(`/catalog/items/${id}`);
}

export function addWishFromCatalog(wishlistId: string, catalogItemId: string): Promise<string> {
  return apiPost<string>(`/wishlists/${wishlistId}/wishes/from-catalog`, { catalogItemId });
}

export function getCatalogOccasions(): Promise<OccasionDto[]> {
  return apiGet<OccasionDto[]>('/catalog/occasions');
}

export function getCatalogCollections(): Promise<CatalogCollectionSummaryDto[]> {
  return apiGet<CatalogCollectionSummaryDto[]>('/catalog/collections');
}

export function getCatalogCollection(id: string): Promise<CatalogCollectionDto> {
  return apiGet<CatalogCollectionDto>(`/catalog/collections/${id}`);
}

export function voteCatalogItemBadge(id: string, badgeType: number): Promise<void> {
  return apiPost<void>(`/catalog/items/${id}/badges/${badgeType}`, {});
}

export function unvoteCatalogItemBadge(id: string, badgeType: number): Promise<void> {
  return apiDelete<void>(`/catalog/items/${id}/badges/${badgeType}`);
}

export function getCatalogBadgeDefinitions(): Promise<CatalogBadgeDefinitionDto[]> {
  return apiGet<CatalogBadgeDefinitionDto[]>('/catalog/badge-definitions');
}

export function getFulfilledBadgeDefinitions(): Promise<FulfilledBadgeDefinitionDto[]> {
  return apiGet<FulfilledBadgeDefinitionDto[]>('/catalog/fulfilled-badge-definitions');
}
