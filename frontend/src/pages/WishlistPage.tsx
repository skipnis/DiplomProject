import { useEffect, useState, useCallback } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getWishlist, deleteWishlist } from '../api/wishlists';
import { getWishes, deleteWish, fulfillWish, unfulfillWish, getWish, addGiftBadges } from '../api/wishes';
import { getFulfilledBadgeDefinitions } from '../api/catalog';
import { Select, SelectContent, SelectItem, SelectTrigger } from '@/components/ui/select';
import { reserveWish, cancelReservation, getMyReservations } from '../api/reservations';
import { getUserProfile } from '../api/users';
import { getImageUrl, API_URL } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError, ApiError } from '../utils/errors';
import { VISIBILITY_LABELS, ROLE_LABELS, PRIORITY_LABELS, getWishlistEmoji } from '../types';
import type { WishlistDto, WishSummaryDto, WishlistMemberRole, UserProfile, FulfilledBadgeDefinitionDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';

function QrModal({ url, open, onClose }: { url: string; open: boolean; onClose: () => void }) {
  const [blobUrl, setBlobUrl] = useState<string | null>(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    if (!open) return;
    setBlobUrl(null);
    setFailed(false);
    fetch(url, { credentials: 'include' })
      .then((res) => { if (!res.ok) throw new Error(); return res.blob(); })
      .then((blob) => setBlobUrl(URL.createObjectURL(blob)))
      .catch(() => setFailed(true));
    return () => { setBlobUrl((prev) => { if (prev) URL.revokeObjectURL(prev); return null; }); };
  }, [open, url]);

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-sm">
        <DialogTitle>QR-код</DialogTitle>
        {failed
          ? <p className="text-center text-sm text-muted-foreground py-4">Не удалось загрузить QR-код</p>
          : blobUrl
            ? <img src={blobUrl} alt="QR код" className="w-full rounded-lg" />
            : <div className="h-48 flex items-center justify-center text-muted-foreground text-sm">Загрузка...</div>}
      </DialogContent>
    </Dialog>
  );
}

function GiftBadgesModal({ open, onClose, onSubmit, definitions }: {
  open: boolean;
  onClose: () => void;
  onSubmit: (badges: number[]) => Promise<void>;
  definitions: FulfilledBadgeDefinitionDto[];
}) {
  const [selected, setSelected] = useState<number[]>([]);
  const [submitting, setSubmitting] = useState(false);

  const toggleBadge = (badgeType: number) => {
    setSelected((prev) => {
      if (prev.includes(badgeType)) return prev.filter((b) => b !== badgeType);
      if (prev.length >= 3) return prev;
      return [...prev, badgeType];
    });
  };

  const handleSubmit = async () => {
    if (selected.length === 0) return;
    setSubmitting(true);
    try { await onSubmit(selected); }
    finally { setSubmitting(false); }
  };

  useEffect(() => {
    if (!open) setSelected([]);
  }, [open]);

  const activeDefinitions = definitions.filter((def) => def.isActive);

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-sm">
        <DialogTitle>Оцените подарок</DialogTitle>
        <p className="text-sm text-muted-foreground">Отметьте до 3 характеристик, которые лучше всего описывают впечатление от этого подарка.</p>
        <div className="flex flex-wrap gap-2 mt-1">
          {activeDefinitions.map((def) => {
            const isSelected = selected.includes(def.id);
            const isDisabled = !isSelected && selected.length >= 3;
            return (
              <button
                key={def.id}
                onClick={() => !isDisabled && toggleBadge(def.id)}
                className={[
                  'px-3 py-1.5 rounded-full text-sm border transition-all',
                  isSelected
                    ? 'border-primary bg-primary/10 text-primary font-semibold'
                    : isDisabled
                      ? 'border-border bg-background text-muted-foreground opacity-40 cursor-not-allowed'
                      : 'border-border bg-background text-foreground cursor-pointer hover:border-primary/60',
                ].join(' ')}
              >
                {def.emoji} {def.label}
              </button>
            );
          })}
        </div>
        {selected.length >= 3 && (
          <p className="text-xs text-muted-foreground">Максимум 3 бейджа</p>
        )}
        <div className="flex gap-2 justify-end mt-2">
          <Button variant="ghost" onClick={onClose}>Пропустить</Button>
          <Button disabled={selected.length === 0 || submitting} onClick={handleSubmit}>
            {submitting ? 'Отправка...' : 'Отправить'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

function ConfirmModal({ open, onClose, onConfirm, title, description, confirmLabel, confirmVariant }: {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description?: string;
  confirmLabel: string;
  confirmVariant?: 'default' | 'destructive' | 'secondary';
}) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-sm">
        <DialogTitle>{title}</DialogTitle>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
        <div className="flex gap-2 justify-end mt-2">
          <Button variant="ghost" onClick={onClose}>Отмена</Button>
          <Button variant={confirmVariant ?? 'default'} onClick={() => { onConfirm(); onClose(); }}>
            {confirmLabel}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

type WishSortOption = { label: string; sortBy: string; direction: string };

const WISH_SORT_OPTIONS: WishSortOption[] = [
  { label: 'По дате ↓', sortBy: 'CreatedAt', direction: 'Desc' },
  { label: 'По дате ↑', sortBy: 'CreatedAt', direction: 'Asc' },
  { label: 'По алфавиту ↑', sortBy: 'Name', direction: 'Asc' },
  { label: 'По алфавиту ↓', sortBy: 'Name', direction: 'Desc' },
  { label: 'По приоритету ↓', sortBy: 'Priority', direction: 'Desc' },
  { label: 'По приоритету ↑', sortBy: 'Priority', direction: 'Asc' },
  { label: 'Невыполненные первыми', sortBy: 'Status', direction: 'Asc' },
];

const PRIORITY_BADGE: Record<number, string> = {
  0: 'bg-muted text-muted-foreground',
  1: 'bg-green-100 text-green-700',
  2: 'bg-yellow-100 text-yellow-700',
  3: 'bg-red-100 text-red-700',
  4: 'bg-purple-100 text-purple-700',
};

export default function WishlistPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user: me } = useAuth();
  const toast = useToast();

  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [wishes, setWishes] = useState<WishSummaryDto[]>([]);
  const [wishPage, setWishPage] = useState(1);
  const [wishTotalPages, setWishTotalPages] = useState(1);
  const [wishTotalCount, setWishTotalCount] = useState(0);
  const [memberProfiles, setMemberProfiles] = useState<Record<string, UserProfile>>({});
  const [myReservationIds, setMyReservationIds] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [wishesLoading, setWishesLoading] = useState(false);
  const [showQr, setShowQr] = useState(false);
  const [accessDenied, setAccessDenied] = useState(false);
  const [notFound, setNotFound] = useState(false);
  const [wishSortKey, setWishSortKey] = useState('CreatedAt_Desc');
  const [fulfilledBadgeDefinitions, setFulfilledBadgeDefinitions] = useState<FulfilledBadgeDefinitionDto[]>([]);
  const [showGiftBadgesModal, setShowGiftBadgesModal] = useState(false);
  const [pendingGiftBadgeWishId, setPendingGiftBadgeWishId] = useState<string | null>(null);
  const [pendingFulfillWish, setPendingFulfillWish] = useState<WishSummaryDto | null>(null);
  const [pendingReserveWish, setPendingReserveWish] = useState<WishSummaryDto | null>(null);
  const [pendingCancelReserveWish, setPendingCancelReserveWish] = useState<WishSummaryDto | null>(null);
  const [pendingDeleteWish, setPendingDeleteWish] = useState<WishSummaryDto | null>(null);
  const [showDeleteWishlistConfirm, setShowDeleteWishlistConfirm] = useState(false);
  const PAGE_SIZE = 12;

  const currentWishSort = WISH_SORT_OPTIONS.find((o) => `${o.sortBy}_${o.direction}` === wishSortKey) ?? WISH_SORT_OPTIONS[0];

  const myRole: WishlistMemberRole | null = wishlist?.members.find((m) => m.userId === me?.id)?.role ?? null;
  const isOwner = myRole === 2;
  const canEdit = myRole !== null && myRole >= 1;
  const isSystem = !!wishlist?.isSystem;

  const ownerMember = wishlist?.members.find((m) => m.role === 2);
  const ownerProfile = ownerMember ? memberProfiles[ownerMember.userId] : undefined;

  const loadWishes = useCallback(async (page: number, sortBy: string, direction: string) => {
    if (!id) return;
    setWishesLoading(true);
    try {
      const ws = await getWishes(id, page, PAGE_SIZE, sortBy, direction);
      setWishes(ws.items);
      setWishTotalPages(Math.ceil(ws.totalCount / ws.pageSize));
      setWishTotalCount(ws.totalCount);
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setWishesLoading(false);
    }
  }, [id]);

  const load = useCallback(async () => {
    if (!id) return;
    try {
      const [wl, definitions] = await Promise.all([getWishlist(id), getFulfilledBadgeDefinitions()]);
      setWishlist(wl);
      setFulfilledBadgeDefinitions(definitions);
      const profiles: Record<string, UserProfile> = {};
      await Promise.all(wl.members.map(async (m) => {
        try { profiles[m.userId] = await getUserProfile(m.userId); } catch { /* ignore */ }
      }));
      setMemberProfiles(profiles);
      if (me) {
        try {
          const reservations = await getMyReservations(1, 100);
          setMyReservationIds(new Set(reservations.items.map((r) => r.wishId)));
        } catch { /* ignore */ }
      }
    } catch (e) {
      if (e instanceof ApiError && (e.status === 403 || e.status === 401)) {
        setAccessDenied(true);
      } else if (e instanceof ApiError && e.status === 404) {
        setNotFound(true);
      } else {
        toast.error(parseError(e));
      }
    } finally {
      setLoading(false);
    }
  }, [id, me]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => { loadWishes(wishPage, currentWishSort.sortBy, currentWishSort.direction); }, [wishPage, wishSortKey]);

  const executeDeleteWishlist = async () => {
    if (!id) return;
    try { await deleteWishlist(id); navigate('/wishlists'); } catch (e) { toast.error(parseError(e)); }
  };

  const handleShare = async () => {
    const url = `${window.location.origin}/wishlists/${id}`;
    if (navigator.share) {
      try {
        await navigator.share({ title: wishlist?.name, url });
      } catch {
        // user cancelled — do nothing
      }
      return;
    }
    try {
      await navigator.clipboard.writeText(url);
      toast.success('Ссылка скопирована');
    } catch {
      toast.error('Не удалось скопировать ссылку');
    }
  };

  const executeDeleteWish = async (wish: WishSummaryDto) => {
    if (!id) return;
    try {
      await deleteWish(id, wish.id);
      setWishes((prev) => prev.filter((w) => w.id !== wish.id));
      setWishTotalCount((n) => n - 1);
    } catch (e) { toast.error(parseError(e)); }
  };

  const executeFulfill = async (wish: WishSummaryDto) => {
    if (!id) return;
    try {
      await fulfillWish(id, wish.id);
      setWishes((prev) => prev.map((w) => w.id === wish.id ? { ...w, isFulfilled: true, isReserved: false } : w));
      setMyReservationIds((prev) => { const s = new Set(prev); s.delete(wish.id); return s; });
      const fullWish = await getWish(id, wish.id);
      if (fullWish.fulfilledByReserverId && !fullWish.hasGiftBadges) {
        setPendingGiftBadgeWishId(wish.id);
        setShowGiftBadgesModal(true);
      }
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleFulfillClick = (wish: WishSummaryDto) => {
    if (wish.isFulfilled) {
      handleUnfulfill(wish);
    } else {
      setPendingFulfillWish(wish);
    }
  };

  const handleUnfulfill = async (wish: WishSummaryDto) => {
    if (!id) return;
    try {
      await unfulfillWish(id, wish.id);
      setWishes((prev) => prev.map((w) => w.id === wish.id ? { ...w, isFulfilled: false } : w));
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleGiftBadgesSubmit = async (badgeTypes: number[]) => {
    if (!id || !pendingGiftBadgeWishId) return;
    try {
      await addGiftBadges(id, pendingGiftBadgeWishId, badgeTypes);
      setWishes((prev) => prev.map((w) => w.id === pendingGiftBadgeWishId ? { ...w, hasGiftBadges: true } : w));
      toast.success('Спасибо за оценку!');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setShowGiftBadgesModal(false);
      setPendingGiftBadgeWishId(null);
    }
  };

  const handleReserve = async (wish: WishSummaryDto) => {
    if (!id) return;
    try {
      if (myReservationIds.has(wish.id)) {
        await cancelReservation(wish.id);
        setMyReservationIds((prev) => { const s = new Set(prev); s.delete(wish.id); return s; });
        setWishes((prev) => prev.map((w) => w.id === wish.id ? { ...w, isReserved: false } : w));
      } else {
        await reserveWish(wish.id, id);
        setMyReservationIds((prev) => new Set([...prev, wish.id]));
        setWishes((prev) => prev.map((w) => w.id === wish.id ? { ...w, isReserved: true } : w));
      }
    } catch (e) { toast.error(parseError(e)); }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  if (accessDenied) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
        <div className="text-6xl mb-5">🔒</div>
        <h1 className="text-2xl font-extrabold tracking-tight mb-2">Доступ ограничен</h1>
        <p className="text-muted-foreground max-w-sm mb-1">
          Вишлист недоступен. Возможно, вы не в списке друзей владельца или доступ ограничен только для выбранных участников.
        </p>
        <p className="text-muted-foreground text-sm mb-6">
          Попробуйте добавить владельца в друзья или попросите его открыть доступ.
        </p>
        <div className="flex gap-3">
          <Button variant="secondary" onClick={() => navigate(-1)}>← Назад</Button>
          <Link to="/wishlists" className={buttonVariants()}>Мои вишлисты</Link>
        </div>
      </div>
    );
  }

  if (notFound || !wishlist) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
        <div className="text-6xl mb-5">🎁</div>
        <h1 className="text-2xl font-extrabold tracking-tight mb-2">Вишлист не найден</h1>
        <p className="text-muted-foreground mb-6">Такого вишлиста не существует или он был удалён.</p>
        <Link to="/wishlists" className={buttonVariants()}>Мои вишлисты</Link>
      </div>
    );
  }

  return (
    <div>
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex items-start gap-4 flex-wrap">
            <div className="text-4xl leading-none">{getWishlistEmoji(wishlist)}</div>
            <div className="flex-1 min-w-0">
              <h1 className="text-xl font-extrabold tracking-tight">{wishlist.name}</h1>
              {wishlist.description && <p className="text-sm text-muted-foreground mt-1">{wishlist.description}</p>}
              <div className="flex flex-wrap gap-1 mt-2">
                <Badge variant="secondary">{VISIBILITY_LABELS[wishlist.visibility]}</Badge>
                {isOwner && wishlist.isSystem && <Badge variant="secondary">⚙️ Системный</Badge>}
                {isOwner && wishlist.isSurpriseModeEnabled && <Badge variant="secondary">🎁 Сюрприз</Badge>}
              </div>
              {ownerProfile && !isOwner && ownerMember && (
                <Link
                  to={`/users/${ownerMember.userId}`}
                  className="flex items-center gap-2 mt-2 text-sm text-muted-foreground hover:text-foreground transition-colors w-fit"
                >
                  <Avatar className="h-5 w-5">
                    <AvatarImage src={getImageUrl(ownerProfile.avatarUrl) ?? undefined} />
                    <AvatarFallback className="text-[10px]">{ownerProfile.displayName[0]}</AvatarFallback>
                  </Avatar>
                  <span>{ownerProfile.displayName}</span>
                </Link>
              )}
            </div>
            {(canEdit || isOwner || (wishlist.visibility === 0 && !isSystem) || ((wishlist.visibility === 0 || isOwner) && !isSystem)) && (
              <DropdownMenu>
                <DropdownMenuTrigger className={`${buttonVariants({ variant: 'ghost', size: 'sm' })} h-8 w-8 p-0 text-muted-foreground text-lg`}>⋯</DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  {wishlist.visibility === 0 && !isSystem && (
                    <DropdownMenuItem onClick={() => setShowQr(true)}>📷 QR-код</DropdownMenuItem>
                  )}
                  {(wishlist.visibility === 0 || isOwner) && !isSystem && (
                    <DropdownMenuItem onClick={handleShare}>Поделиться</DropdownMenuItem>
                  )}
                  {canEdit && (
                    <DropdownMenuItem onClick={() => navigate(`/wishlists/${id}/edit`)}>Изменить</DropdownMenuItem>
                  )}
                  {isOwner && (
                    <>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem className="text-destructive focus:text-destructive" onClick={() => setShowDeleteWishlistConfirm(true)}>Удалить</DropdownMenuItem>
                    </>
                  )}
                </DropdownMenuContent>
              </DropdownMenu>
            )}
          </div>
        </CardContent>
      </Card>

      <div className="flex items-center justify-between mb-5 gap-3 flex-wrap">
        <h2 className="text-lg font-bold">
          Желания ({wishTotalCount})
          {wishlist.fulfilledWishCount > 0 && (
            <span className="ml-2 text-sm font-normal text-green-600">✓ {wishlist.fulfilledWishCount} исполнено</span>
          )}
        </h2>
        <div className="flex items-center gap-2">
          <Select value={wishSortKey} onValueChange={(value) => { if (value) { setWishSortKey(value); setWishPage(1); } }}>
            <SelectTrigger className="w-44 h-8 text-xs">
              <span>{currentWishSort.label}</span>
            </SelectTrigger>
            <SelectContent>
              {WISH_SORT_OPTIONS.map((o) => (
                <SelectItem key={`${o.sortBy}_${o.direction}`} value={`${o.sortBy}_${o.direction}`} className="text-xs">
                  {o.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {canEdit && <Link to={`/wishlists/${id}/wishes/new`} className={buttonVariants({ size: 'sm' })}>+ Добавить</Link>}
        </div>
      </div>

      {wishesLoading ? (
        <div className="flex items-center justify-center min-h-[100px] text-muted-foreground">Загрузка...</div>
      ) : wishes.length === 0 ? (
        <div className="text-center py-12">
          <div className="text-4xl mb-3">🎁</div>
          <p className="font-semibold mb-3">Нет желаний</p>
          {canEdit && <Link to={`/wishlists/${id}/wishes/new`} className={buttonVariants()}>Добавить желание</Link>}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {wishes.map((wish) => {
            const iMineReserved = myReservationIds.has(wish.id);
            const shouldBlur = wish.isReserved && !iMineReserved && !wish.isFulfilled && myRole === null;
            return (
              <div key={wish.id} className="relative">
                <Link to={`/wishlists/${id}/wishes/${wish.id}`} className={`block rounded-xl border bg-card overflow-hidden hover:shadow-md hover:-translate-y-0.5 transition-all ${wish.isFulfilled ? 'opacity-60' : ''}`}>
                  <div className={shouldBlur ? 'blur-sm pointer-events-none' : ''}>
                    {wish.imagePath
                      ? <img src={getImageUrl(wish.imagePath)!} alt={wish.name} className="w-full h-40 object-contain bg-muted" />
                      : <div className="w-full h-40 bg-muted flex items-center justify-center text-4xl">🎁</div>
                    }
                    <div className="p-3 flex flex-col gap-1">
                      <div className="font-semibold text-sm leading-snug">{wish.name}</div>
                      {wish.price != null && (
                        <div className="font-bold text-primary text-sm">{wish.price} {wish.currency != null ? ['BYN','RUB','USD','EUR'][wish.currency] : ''}</div>
                      )}
                      {wish.priority > 0 && (
                        <span className={`text-xs px-2 py-0.5 rounded-full font-semibold w-fit ${PRIORITY_BADGE[wish.priority]}`}>{PRIORITY_LABELS[wish.priority]}</span>
                      )}
                    </div>
                  </div>
                  {shouldBlur && <div className="absolute inset-0 flex items-center justify-center font-bold text-muted-foreground text-sm bg-background/60">🔒 Забронировано</div>}
                  {wish.isFulfilled && <div className="absolute top-2 left-2 bg-green-500 text-white rounded-full px-2 py-0.5 text-xs font-semibold">Исполнено</div>}
                </Link>
                {(isOwner || (canEdit && !isOwner && wish.createdByUserId === me?.id)) && (
                  <div className="absolute top-2 right-2 z-10">
                    <DropdownMenu>
                      <DropdownMenuTrigger className="flex items-center justify-center h-7 w-7 rounded-full bg-white/85 dark:bg-black/65 text-foreground/70 hover:bg-white dark:hover:bg-black/85 transition-colors shadow-sm text-base font-bold">⋯</DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => navigate(`/wishlists/${id}/wishes/${wish.id}/edit`)}>Изменить</DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem className="text-destructive focus:text-destructive" onClick={() => setPendingDeleteWish(wish)}>Удалить</DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>
                )}
                {isOwner && !isSystem && (
                  <div className="flex items-center gap-1 mt-2">
                    <Button size="sm" variant={wish.isFulfilled ? 'secondary' : 'default'} onClick={() => handleFulfillClick(wish)}>
                      {wish.isFulfilled ? '↩ Не исполнено' : '✓ Исполнить'}
                    </Button>
                  </div>
                )}
                {me && !isOwner && !wish.isFulfilled && !isSystem && (
                  <div className="flex items-center gap-1 mt-2">
                    <Button size="sm" variant={iMineReserved ? 'destructive' : wish.isReserved ? 'ghost' : 'secondary'} onClick={() => { if (iMineReserved) { setPendingCancelReserveWish(wish); } else if (!wish.isReserved) { setPendingReserveWish(wish); } }} disabled={wish.isReserved && !iMineReserved}>
                      {iMineReserved ? 'Отменить бронь' : wish.isReserved ? 'Забронировано' : 'Забронировать'}
                    </Button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {wishTotalPages > 1 && (
        <div className="flex items-center justify-center gap-3 mt-6">
          <Button variant="ghost" size="sm" disabled={wishPage === 1} onClick={() => setWishPage((p) => p - 1)}>← Назад</Button>
          <span className="text-sm text-muted-foreground">{wishPage} / {wishTotalPages}</span>
          <Button variant="ghost" size="sm" disabled={wishPage === wishTotalPages} onClick={() => setWishPage((p) => p + 1)}>Вперёд →</Button>
        </div>
      )}

      {canEdit && !isSystem && wishlist.members.length > 0 && (
        <>
          <Separator className="my-6" />
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-bold">Участники</h2>
            {isOwner && <Link to={`/wishlists/${id}/members`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>Управление</Link>}
          </div>
          <Card>
            <CardContent className="pt-4 flex flex-col gap-2">
              {wishlist.members.map((m) => {
                const p = memberProfiles[m.userId];
                return (
                  <div key={m.userId} className="flex items-center gap-3 py-1">
                    <Avatar className="h-8 w-8">
                      <AvatarImage src={getImageUrl(p?.avatarUrl) ?? undefined} />
                      <AvatarFallback className="text-xs">{(p?.username ?? m.userId)[0].toUpperCase()}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1">
                      <Link to={`/users/${m.userId}`} className="text-sm font-semibold hover:underline">{p?.username ?? m.userId.slice(0, 8) + '…'}</Link>
                    </div>
                    <Badge variant="secondary">{m.customRoleName || ROLE_LABELS[m.role]}</Badge>
                  </div>
                );
              })}
            </CardContent>
          </Card>
        </>
      )}

      <QrModal url={`${API_URL}/wishlists/${id}/qr`} open={showQr} onClose={() => setShowQr(false)} />

      <GiftBadgesModal
        open={showGiftBadgesModal}
        onClose={() => { setShowGiftBadgesModal(false); setPendingGiftBadgeWishId(null); }}
        onSubmit={handleGiftBadgesSubmit}
        definitions={fulfilledBadgeDefinitions}
      />

      <ConfirmModal
        open={!!pendingFulfillWish}
        onClose={() => setPendingFulfillWish(null)}
        onConfirm={() => { if (pendingFulfillWish) executeFulfill(pendingFulfillWish); }}
        title="Отметить желание исполненным?"
        description="Если у желания есть бронирование, вам будет предложено оценить подарок."
        confirmLabel="Отметить исполненным"
      />

      <ConfirmModal
        open={!!pendingReserveWish}
        onClose={() => setPendingReserveWish(null)}
        onConfirm={() => { if (pendingReserveWish) handleReserve(pendingReserveWish); }}
        title="Забронировать желание?"
        description="Вы берёте на себя обязательство подарить этот подарок. Другие пользователи увидят, что желание уже забронировано."
        confirmLabel="Забронировать"
      />

      <ConfirmModal
        open={!!pendingCancelReserveWish}
        onClose={() => setPendingCancelReserveWish(null)}
        onConfirm={() => { if (pendingCancelReserveWish) handleReserve(pendingCancelReserveWish); }}
        title="Отменить бронирование?"
        description="Желание снова станет доступно для бронирования другими пользователями."
        confirmLabel="Отменить бронь"
        confirmVariant="destructive"
      />

      <ConfirmModal
        open={!!pendingDeleteWish}
        onClose={() => setPendingDeleteWish(null)}
        onConfirm={() => { if (pendingDeleteWish) executeDeleteWish(pendingDeleteWish); }}
        title="Удалить желание?"
        description="Это действие нельзя отменить."
        confirmLabel="Удалить"
        confirmVariant="destructive"
      />

      <ConfirmModal
        open={showDeleteWishlistConfirm}
        onClose={() => setShowDeleteWishlistConfirm(false)}
        onConfirm={executeDeleteWishlist}
        title="Удалить вишлист?"
        description="Все желания в этом вишлисте также будут удалены. Это действие нельзя отменить."
        confirmLabel="Удалить"
        confirmVariant="destructive"
      />
    </div>
  );
}
