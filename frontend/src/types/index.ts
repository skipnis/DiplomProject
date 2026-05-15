// Enums match C# backend integer values (no JsonStringEnumConverter)
export type WishlistVisibility = 0 | 1 | 2 | 3; // Public | Friends | SelectedFriends | Private
export type SystemWishlistType = 'None' | 'Hidden' | 'Blacklist';
export type NotificationType = 1 | 2 | 3 | 10 | 11 | 20 | 21 | 22 | 30 | 31;
export type ProposalSourceType = 1 | 2 | 3; // Catalog | Wishlist | Custom
export type ProposalStatus = 0 | 1 | 2;      // Pending | Liked | Disliked
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

export const PRIORITY_LABELS: Record<number, string> = {
  0: '—', 1: 'Неплохо бы', 2: 'Хочу', 3: 'Очень хочу', 4: 'Мечта',
};

export const CURRENCY_LABELS: Record<number, string> = {
  0: 'BYN', 1: 'RUB', 2: 'USD', 3: 'EUR',
};

export const NOTIFICATION_TYPE_LABELS: Record<number, string> = {
  1: 'Ваш подарок забронировали',
  2: 'Бронирование отменено',
  3: 'Подарок передан',
  10: 'Новая заявка в друзья',
  11: 'Заявка в друзья принята',
  20: 'Вас добавили в вишлист',
  21: 'Вас удалили из вишлиста',
  22: 'Ваша роль изменена',
  30: 'Новое предложение подарка',
  31: 'Ответ на предложение',
};

export const ROLE_LABELS: Record<number, string> = {
  0: 'Зритель', 1: 'Редактор', 2: 'Владелец',
};

export interface MyProfile {
  id: string;
  displayName: string;
  username: string | null;
  email: string;
  avatarPath: string | null;
  avatarUrl: string | null;
  bio: string | null;
  birthDate: string | null;
  isGoogleCalendarConnected: boolean;
  isOnboarded: boolean;
  showFulfilledWishes: boolean;
}

export interface FulfilledWishItem {
  id: string;
  wishName: string;
  imagePath: string | null;
  wishlistName: string;
  fulfilledAt: string;
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
  wishCount: number;
  fulfilledWishCount: number;
  createdAt: string;
}

export interface WishSummaryDto {
  id: string;
  name: string;
  price: number | null;
  currency: Currency | null;
  priority: WishPriority;
  imagePath: string | null;
  isFulfilled: boolean;
  isReserved: boolean;
  hasGiftBadges: boolean;
  createdByUserId: string | null;
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
  fulfilledByReserverId: string | null;
  hasGiftBadges: boolean;
  createdByUserId: string | null;
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

export interface WishReservedOnMyWishDto {
  wishId: string;
  wishlistId: string;
  wishName: string;
  wishlistName: string;
  wishImagePath: string | null;
  wishPrice: number | null;
  wishCurrency: Currency | null;
  reservedByUserId: string;
  reservedByDisplayName: string;
  reservedAt: string;
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
  isPublished: boolean;
}

export type AchievementRuleType = 1 | 2; // SpecificBadgeCount | UniqueBadgeTypes

export interface CatalogBadgeDefinitionDto {
  id: number;
  emoji: string;
  slug: string;
  label: string;
  description: string;
  isActive: boolean;
}

export interface FulfilledBadgeDefinitionDto {
  id: number;
  emoji: string;
  slug: string;
  label: string;
  description: string;
  isActive: boolean;
}

export interface AchievementDefinitionAdminDto {
  id: number;
  name: string;
  description: string;
  emoji: string;
  ruleType: AchievementRuleType;
  linkedBadgeTypeId: number | null;
  threshold: number;
  order: number;
  isActive: boolean;
}

export interface CatalogItemBadgeDto {
  badgeType: number;
  emoji: string;
  slug: string;
  label: string;
  voteCount: number;
  myVote: boolean;
}

export interface FulfilledWishBadgeDto {
  badgeType: number;
  createdAt: string;
}

export interface UserAchievementDto {
  definitionId: number;
  name: string;
  description: string;
  emoji: string;
  progress: number;
  threshold: number;
  isEarned: boolean;
  earnedAt: string | null;
}

export interface BadgeCountDto {
  badgeType: number;
  emoji: string;
  label: string;
  count: number;
}

export interface GiftProfileDto {
  giftsGiven: number;
  giftsWithBadges: number;
  hitRate: number;
  level: number;
  levelName: string;
  nextLevelThreshold: number;
  achievements: UserAchievementDto[];
  badgesReceived: BadgeCountDto[];
}



export interface CatalogItemSummaryDto {
  id: string;
  name: string;
  price: number | null;
  currency: string | null;
  imagePath: string | null;
  url: string | null;
  categoryId: string;
  categoryName: string;
  wishCount: number;
  collectionItemDescription: string | null;
  badges: CatalogItemBadgeDto[];
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
  wishCount: number;
  collectionItemDescription: string | null;
  badges: CatalogItemBadgeDto[];
  occasions: OccasionDto[];
}

export interface CreateCatalogItemRequest {
  name: string;
  description: string | null;
  price: number | null;
  currency: Currency | null;
  imagePath: string | null;
  url: string | null;
  categoryId: string;
  occasionIds: string[];
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
  occasion: OccasionDto | null;
  coverImagePath: string | null;
  order: number;
  itemCount: number;
}

export interface CatalogCollectionDto {
  id: string;
  name: string;
  description: string | null;
  occasion: OccasionDto | null;
  coverImagePath: string | null;
  order: number;
  items: CatalogItemSummaryDto[];
}

export interface CatalogCollectionAdminDto {
  id: string;
  name: string;
  description: string | null;
  occasion: OccasionDto | null;
  coverImagePath: string | null;
  order: number;
  isPublished: boolean;
  itemCount: number;
  createdAt: string;
}

export interface CreateCollectionRequest {
  name: string;
  description: string | null;
  occasionId: string | null;
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

export interface OccasionDto {
  id: string;
  key: string;
  label: string;
  order: number;
}

export interface NotificationDto {
  id: string;
  type: NotificationType;
  payload: Record<string, unknown>;
  isRead: boolean;
  createdAt: string;
}

export interface FulfilledWishRecordDto {
  id: string;
  wishId: string;
  gifterId: string | null;
  gifterDisplayName: string | null;
  wishName: string;
  wishDescription: string | null;
  price: number | null;
  currency: Currency | null;
  imagePath: string | null;
  wishlistName: string;
  fulfilledAt: string;
}

export interface IncomingProposalDto {
  id: string;
  sourceType: ProposalSourceType;
  senderAlias: string;
  hintMessage: string | null;
  isViewedByRecipient: boolean;
  status: ProposalStatus;
  recipientComment: string | null;
  createdAt: string;
  reactedAt: string | null;
  catalogItemId: string | null;
  catalogItemName: string | null;
  catalogItemImagePath: string | null;
  catalogItemPrice: number | null;
  wishlistItemId: string | null;
  wishlistItemName: string | null;
  wishlistItemImagePath: string | null;
  customTitle: string | null;
  customDescription: string | null;
  customImagePath: string | null;
}

export interface OutgoingProposalDto {
  id: string;
  sourceType: ProposalSourceType;
  status: ProposalStatus;
  hintMessage: string | null;
  recipientComment: string | null;
  createdAt: string;
  reactedAt: string | null;
  recipientId: string;
  recipientDisplayName: string;
  recipientAvatarUrl: string | null;
  catalogItemId: string | null;
  catalogItemName: string | null;
  catalogItemImagePath: string | null;
  catalogItemPrice: number | null;
  wishlistItemId: string | null;
  wishlistItemName: string | null;
  wishlistItemImagePath: string | null;
  customTitle: string | null;
  customDescription: string | null;
  customImagePath: string | null;
}

export interface ProposalDetailDto {
  id: string;
  sourceType: ProposalSourceType;
  senderAlias: string;
  hintMessage: string | null;
  isViewedByRecipient: boolean;
  status: ProposalStatus;
  recipientComment: string | null;
  createdAt: string;
  reactedAt: string | null;
  isOwnProposal: boolean;
  recipientId: string | null;
  recipientDisplayName: string | null;
  recipientAvatarUrl: string | null;
  catalogItemId: string | null;
  catalogItemName: string | null;
  catalogItemImagePath: string | null;
  catalogItemPrice: number | null;
  catalogItemUrl: string | null;
  wishlistItemId: string | null;
  wishlistItemName: string | null;
  wishlistItemImagePath: string | null;
  wishlistItemDescription: string | null;
  customTitle: string | null;
  customDescription: string | null;
  customImagePath: string | null;
}

export interface CreateProposalRequest {
  recipientId: string;
  sourceType: ProposalSourceType;
  catalogItemId: string | null;
  wishlistItemId: string | null;
  customTitle: string | null;
  customDescription: string | null;
  hintMessage: string | null;
  senderAlias: string | null;
}

export interface ReactToProposalRequest {
  status: 1 | 2;
  comment: string | null;
}

export const PROPOSAL_STATUS_LABELS: Record<ProposalStatus, string> = {
  0: 'Ожидает ответа',
  1: 'Хочу',
  2: 'Не моё',
};

export const PROPOSAL_SOURCE_LABELS: Record<ProposalSourceType, string> = {
  1: 'Каталог',
  2: 'Из вишлиста',
  3: 'Своя идея',
};

export interface BlacklistItemDto {
  id: string;
  title: string;
  createdAt: string;
}
