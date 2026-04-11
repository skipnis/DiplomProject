// Enums match C# backend integer values (no JsonStringEnumConverter)
export type WishlistVisibility = 0 | 1 | 2 | 3; // Public | Friends | SelectedFriends | Private
export type SystemWishlistType = 'None' | 'Hidden' | 'Blacklist';
export type Currency = 0 | 1 | 2 | 3;           // BYN | RUB | USD | EUR
export type WishPriority = 0 | 1 | 2 | 3 | 4;   // None | NiceToHave | Want | ReallyWant | Dream
export type WishlistMemberRole = 0 | 1 | 2;      // Viewer | Editor | Owner
export type FriendshipStatus = 0 | 1 | 2;        // Pending | Accepted | Declined

// Display helpers
export const VISIBILITY_LABELS: Record<number, string> = {
  0: '🌍 Публичный',
  1: '👥 Для друзей',
  2: '👤 Избранные',
  3: '🔒 Приватный',
};

export const VISIBILITY_ICONS: Record<number, string> = {
  0: '🌍', 1: '👥', 2: '👤', 3: '🔒',
};

export const PRIORITY_LABELS: Record<number, string> = {
  0: '—', 1: 'Неплохо бы', 2: 'Хочу', 3: 'Очень хочу', 4: 'Мечта',
};

export const PRIORITY_CLASS: Record<number, string> = {
  0: 'priority-None', 1: 'priority-NiceToHave', 2: 'priority-Want', 3: 'priority-ReallyWant', 4: 'priority-Dream',
};

export const CURRENCY_LABELS: Record<number, string> = {
  0: 'BYN', 1: 'RUB', 2: 'USD', 3: 'EUR',
};

export const ROLE_LABELS: Record<number, string> = {
  0: 'Зритель', 1: 'Редактор', 2: 'Владелец',
};

export interface MyProfile {
  id: string;
  displayName: string;
  username: string | null;
  email: string;
  avatarUrl: string | null;
  bio: string | null;
  birthDate: string | null;
  isGoogleCalendarConnected: boolean;
  isOnboarded: boolean;
}

export interface UserProfile {
  id: string;
  displayName: string;
  username: string | null;
  avatarUrl: string | null;
  bio: string | null;
  receivedCount: number;
  giftedCount: number;
}

export interface UserSearchResult {
  id: string;
  displayName: string;
  username: string | null;
  avatarUrl: string | null;
}

export interface WishlistMemberDto {
  userId: string;
  role: WishlistMemberRole;
  customRoleName: string | null;
  joinedAt: string;
}

export interface WishlistDto {
  id: string;
  name: string;
  description: string | null;
  emoji: string | null;
  visibility: WishlistVisibility;
  isSystem: boolean;
  systemType: SystemWishlistType;
  isSurpriseModeEnabled: boolean;
  createdAt: string;
  fulfilledWishCount: number;
  members: WishlistMemberDto[];
}

export interface WishlistSummaryDto {
  id: string;
  name: string;
  description: string | null;
  emoji: string | null;
  visibility: WishlistVisibility;
  isSystem: boolean;
  systemType: SystemWishlistType;
  wishCount: number;
  fulfilledWishCount: number;
  createdAt: string;
}

export interface WishDto {
  id: string;
  name: string;
  description: string | null;
  price: number | null;
  currency: Currency | null;
  priority: WishPriority;
  url: string | null;
  imagePath: string | null;
  createdAt: string;
  isFulfilled: boolean;
  fulfilledAt: string | null;
  isReserved: boolean;
  shareToken: string | null;
}

export interface SharedWishResponse {
  id: string;
  wishlistId: string;
  name: string;
  description: string | null;
  price: number | null;
  currency: Currency | null;
  priority: WishPriority;
  url: string | null;
  imagePath: string | null;
  isFulfilled: boolean;
  isReserved: boolean;
  wishlistVisibility: WishlistVisibility;
  ownerUsername: string;
}

export interface ParsedWishData {
  name: string | null;
  description: string | null;
  price: number | null;
  currency: string | null;
  externalImageUrl: string | null;
}

export interface FriendInfo {
  userId: string;
  username: string;
  avatarUrl: string | null;
}

export interface FriendshipRequest {
  friendshipId: string;
  userId: string;
  username: string;
  avatarUrl: string | null;
  createdAt: string;
}

export interface MyReservationDto {
  reservationId: string;
  wishId: string;
  wishlistId: string;
  wishName: string;
  wishImagePath: string | null;
  wishPrice: number | null;
  wishCurrency: Currency | null;
  wishlistName: string;
  wishlistOwnerName: string;
  reservedAt: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface WishlistMemberInvite {
  userId: string;
  role: WishlistMemberRole;
}

export interface EventDto {
  id: string;
  title: string;
  description: string | null;
  date: string;
  isLinkedToGoogleCalendar: boolean;
  linkedWishlistId: string | null;
  createdAt: string;
}

export interface CatalogCategoryDto {
  id: string;
  name: string;
  order: number;
}

export interface CatalogItemDto {
  id: string;
  name: string;
  description: string | null;
  price: number | null;
  currency: string | null;
  imagePath: string | null;
  url: string | null;
  categoryId: string;
  categoryName: string;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  averageRating: number | null;
  ratingCount: number;
  myRating: number | null;
}

export interface CreateCatalogItemRequest {
  name: string;
  description: string | null;
  price: number | null;
  currency: Currency | null;
  imagePath: string | null;
  url: string | null;
  categoryId: string;
}

export interface UpdateCatalogItemRequest extends CreateCatalogItemRequest {
  isPublished: boolean;
}

export interface CreateCatalogCategoryRequest {
  name: string;
  order: number;
}

export interface UpdateCatalogCategoryRequest {
  name: string;
  order: number;
}

export interface CatalogCollectionSummaryDto {
  id: string;
  name: string;
  description: string | null;
  occasion: string | null;
  coverImagePath: string | null;
  order: number;
  itemCount: number;
}

export interface CatalogCollectionDto {
  id: string;
  name: string;
  description: string | null;
  occasion: string | null;
  coverImagePath: string | null;
  order: number;
  items: CatalogItemDto[];
}

export interface CatalogCollectionAdminDto {
  id: string;
  name: string;
  description: string | null;
  occasion: string | null;
  coverImagePath: string | null;
  order: number;
  isPublished: boolean;
  itemCount: number;
  createdAt: string;
}

export interface CreateCollectionRequest {
  name: string;
  description: string | null;
  occasion: string | null;
  coverImagePath: string | null;
  order: number;
}

export interface UpdateCollectionRequest extends CreateCollectionRequest {
  isPublished: boolean;
}

export function getWishlistEmoji(w: { emoji: string | null; isSystem: boolean; name: string }): string {
  if (w.emoji) return w.emoji;
  if (w.isSystem) {
    if (w.name === 'Скрытые') return '🙈';
    if (w.name === 'Чёрный список') return '❌';
    return '⚙️';
  }
  return '📋';
}

export const OCCASION_LABELS: Record<string, string> = {
  birthday: '🎂 День рождения',
  new_year: '🎆 Новый год',
  valentine: '💝 День влюблённых',
  wedding: '💍 Свадьба',
  anniversary: '🥂 Юбилей',
  graduation: '🎓 Выпускной',
  baby_shower: '👶 Рождение ребёнка',
  housewarming: '🏠 Новоселье',
  christmas: '🎄 Рождество',
  easter: '🐣 Пасха',
  other: '🎁 Другое',
};
