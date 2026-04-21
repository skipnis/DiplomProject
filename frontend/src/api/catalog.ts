import { apiGet, apiPost, apiDelete } from './client';
import type { CatalogCategoryDto, CatalogCollectionDto, CatalogCollectionSummaryDto, CatalogItemDto, OccasionDto, PagedResponse } from '../types';

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
  return apiGet<PagedResponse<CatalogItemDto>>(`/catalog/items?${query.toString()}`);
}

export function getCatalogItem(id: string): Promise<CatalogItemDto> {
  return apiGet<CatalogItemDto>(`/catalog/items/${id}`);
}

export function addWishFromCatalog(wishlistId: string, catalogItemId: string): Promise<string> {
  return apiPost<string>(`/wishlists/${wishlistId}/wishes/from-catalog`, { catalogItemId });
}

export function rateCatalogItem(id: string, value: number): Promise<void> {
  return apiPost<void>(`/catalog/items/${id}/rate`, { value });
}

export function unrateCatalogItem(id: string): Promise<void> {
  return apiDelete<void>(`/catalog/items/${id}/rate`);
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
