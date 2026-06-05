import { useEffect, useState, useCallback } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getWishlist, deleteWishlist } from '../api/wishlists';
import { getWishes, deleteWish, fulfillWish, unfulfillWish } from '../api/wishes';
import { getEventByWishlist } from '../api/events';
import { generateWishlistShareCard } from '../lib/generateWishlistShareCard';
import { toast } from 'sonner';
import { Select, SelectContent, SelectItem, SelectTrigger } from '@/components/ui/select';
import { reserveWish, cancelReservation, getMyReservations } from '../api/reservations';
import { getUserProfile } from '../api/users';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { parseError, ApiError } from '../utils/errors';
import { VISIBILITY_LABELS, ROLE_LABELS, PRIORITY_LABELS, getWishlistEmoji } from '../types';
import type { WishlistDto, WishSummaryDto, WishlistMemberRole, UserProfile } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Pencil, Trash2, Share2 } from 'lucide-react';

const PLACEHOLDER_GRADIENTS = [
  'from-violet-500 to-purple-700',
  'from-blue-500 to-cyan-600',
  'from-emerald-500 to-teal-700',
  'from-rose-500 to-pink-700',
  'from-amber-500 to-orange-600',
  'from-indigo-500 to-blue-700',
  'from-fuchsia-500 to-pink-600',
  'from-sky-500 to-indigo-600',
];

function wishNameHash(name: string): number {
  let hash = 0;
  for (let index = 0; index < name.length; index++) {
    hash = (hash * 31 + name.charCodeAt(index)) >>> 0;
  }
  return hash;
}

function WishImagePlaceholder({ name }: { name: string }) {
  const gradient = PLACEHOLDER_GRADIENTS[wishNameHash(name) % PLACEHOLDER_GRADIENTS.length];
  const firstChar = [...name.trim()][0]?.toUpperCase() ?? '?';
  return (
    <div className={`w-full h-40 bg-gradient-to-br ${gradient} flex items-center justify-center`}>
      <span className="text-5xl font-bold text-white/90 select-none">{firstChar}</span>
    </div>
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
  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [wishes, setWishes] = useState<WishSummaryDto[]>([]);
  const [wishPage, setWishPage] = useState(1);
  const [wishTotalPages, setWishTotalPages] = useState(1);
  const [wishTotalCount, setWishTotalCount] = useState(0);
  const [memberProfiles, setMemberProfiles] = useState<Record<string, UserProfile>>({});
  const [myReservationIds, setMyReservationIds] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [wishesLoading, setWishesLoading] = useState(false);
  const [accessDenied, setAccessDenied] = useState(false);
  const [notFound, setNotFound] = useState(false);
  const [wishSortKey, setWishSortKey] = useState('CreatedAt_Desc');
  const [pendingFulfillWish, setPendingFulfillWish] = useState<WishSummaryDto | null>(null);
  const [pendingReserveWish, setPendingReserveWish] = useState<WishSummaryDto | null>(null);
  const [pendingCancelReserveWish, setPendingCancelReserveWish] = useState<WishSummaryDto | null>(null);
  const [pendingDeleteWish, setPendingDeleteWish] = useState<WishSummaryDto | null>(null);
  const [showDeleteWishlistConfirm, setShowDeleteWishlistConfirm] = useState(false);
  const [shareCardLoading, setShareCardLoading] = useState(false);
  const [shareCardBlobUrl, setShareCardBlobUrl] = useState<string | null>(null);
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
      const wl = await getWishlist(id);
      setWishlist(wl);
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
    if (!wishlist || !id || shareCardLoading) return;
    setShareCardLoading(true);
    try {
      let linkedEvent: { title: string; date: string } | undefined;
      if (isOwner) {
        try {
          const found = await getEventByWishlist(id);
          linkedEvent = { title: found.title, date: found.date };
        } catch {
          // not critical — 404 means no linked event
        }
      }

      const blob = await generateWishlistShareCard({
        name: wishlist.name,
        emoji: getWishlistEmoji(wishlist),
        ownerDisplayName: isOwner ? (me?.displayName ?? '') : (ownerProfile?.displayName ?? ''),
        wishlistId: id,
        wishCount: wishTotalCount,
        eventTitle: linkedEvent?.title,
        eventDate: linkedEvent?.date,
      });

      const file = new File([blob], 'wishlist.png', { type: 'image/png' });
      if (navigator.canShare?.({ files: [file] })) {
        await navigator.share({ files: [file] });
      } else {
        const blobUrl = URL.createObjectURL(blob);
        setShareCardBlobUrl(blobUrl);
      }
    } catch (error) {
      if (error instanceof Error && error.name !== 'AbortError') {
        toast.error('Не удалось поделиться');
      }
    } finally {
      setShareCardLoading(false);
    }
  };

  const handleShareCardModalClose = () => {
    if (shareCardBlobUrl) URL.revokeObjectURL(shareCardBlobUrl);
    setShareCardBlobUrl(null);
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
      const { hasGifter } = await fulfillWish(id, wish.id);
      setWishes((prev) => prev.map((w) => w.id === wish.id ? { ...w, isFulfilled: true, isReserved: false } : w));
      setMyReservationIds((prev) => { const updated = new Set(prev); updated.delete(wish.id); return updated; });
      if (hasGifter) {
        toast('Желание исполнено!', {
          action: { label: 'Оценить дарителя', onClick: () => navigate(`/wishlists/${id}/wishes/${wish.id}/rate-gifter`) },
        });
      } else {
        toast.success('Желание исполнено!');
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
            <div className="flex items-center gap-1">
              {(wishlist.visibility === 0 || isOwner) && wishlist.visibility !== 3 && !isSystem && (
                <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground" onClick={handleShare} title="Поделиться" disabled={shareCardLoading}>
                  <Share2 size={16} />
                </Button>
              )}
              {canEdit && (
                <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground" onClick={() => navigate(`/wishlists/${id}/edit`)} title="Изменить">
                  <Pencil size={16} />
                </Button>
              )}
              {isOwner && !isSystem && (
                <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground hover:text-destructive" onClick={() => setShowDeleteWishlistConfirm(true)} title="Удалить">
                  <Trash2 size={16} />
                </Button>
              )}
            </div>
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
            const isWishAuthor = wish.createdByUserId ? wish.createdByUserId === me?.id : isOwner;
            return (
              <div key={wish.id} className="relative">
                <Link to={`/wishlists/${id}/wishes/${wish.id}`} className={`block rounded-xl border bg-card overflow-hidden hover:shadow-md hover:-translate-y-0.5 transition-all ${wish.isFulfilled ? 'opacity-60' : ''}`}>
                  <div className={shouldBlur ? 'blur-sm pointer-events-none' : ''}>
                    {wish.imagePath
                      ? (
                        <div className="relative w-full h-40 overflow-hidden">
                          <img src={getImageUrl(wish.imagePath)!} alt="" aria-hidden className="absolute inset-0 w-full h-full object-cover scale-110 blur-xl opacity-60" />
                          <img src={getImageUrl(wish.imagePath)!} alt={wish.name} className="relative w-full h-full object-contain" />
                        </div>
                      )
                      : <WishImagePlaceholder name={wish.name} />
                    }
                    <div className="p-3 flex flex-col gap-1">
                      <div className="font-semibold text-sm leading-snug">{wish.name}</div>
                      {wish.price != null && (
                        <div className="font-bold text-primary text-sm">{wish.price} {wish.currency != null ? ['BYN','RUB','USD','EUR'][wish.currency] : ''}</div>
                      )}
                    </div>
                  </div>
                  {shouldBlur && <div className="absolute inset-0 flex items-center justify-center font-bold text-muted-foreground text-sm bg-background/60">🔒 Забронировано</div>}
                  {wish.isFulfilled && (
                    <div className="absolute top-2 left-2 bg-green-500 text-white rounded-full px-2 py-0.5 text-xs font-semibold">
                      {wish.fulfilledByDisplayName ? `Исполнено: ${wish.fulfilledByDisplayName}` : 'Исполнено собой любимым'}
                    </div>
                  )}
                  {wish.priority > 0 && <span className={`absolute top-2 right-2 text-xs px-2 py-0.5 rounded-full font-semibold ${PRIORITY_BADGE[wish.priority]}`}>{PRIORITY_LABELS[wish.priority]}</span>}
                </Link>
                {isWishAuthor && !isSystem && !(wish.isFulfilled && wish.hasGiftBadges) && (
                  <div className="flex items-center gap-1 mt-2">
                    <Button size="sm" variant={wish.isFulfilled ? 'secondary' : 'default'} onClick={() => handleFulfillClick(wish)}>
                      {wish.isFulfilled ? '↩ Не исполнено' : '✓ Исполнить'}
                    </Button>
                  </div>
                )}
                {!isWishAuthor && !wish.isFulfilled && !isSystem && (
                  <div className="flex items-center gap-1 mt-2">
                    <Button size="sm" variant={iMineReserved ? 'destructive' : wish.isReserved ? 'ghost' : 'secondary'} onClick={() => { if (!me) { toast.warning('Войдите в аккаунт, чтобы забронировать желание'); return; } if (iMineReserved) { setPendingCancelReserveWish(wish); } else if (!wish.isReserved) { setPendingReserveWish(wish); } }} disabled={wish.isReserved && !iMineReserved}>
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


      <ConfirmModal
        open={!!pendingFulfillWish}
        onClose={() => setPendingFulfillWish(null)}
        onConfirm={() => { if (pendingFulfillWish) executeFulfill(pendingFulfillWish); }}
        title="Отметить желание исполненным?"
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

      <Dialog open={!!shareCardBlobUrl} onOpenChange={(open) => { if (!open) handleShareCardModalClose(); }}>
        <DialogContent className="max-w-sm p-4">
          <DialogTitle className="text-base">Поделиться вишлистом</DialogTitle>
          {shareCardBlobUrl && (
            <>
              <img src={shareCardBlobUrl} alt="Карточка вишлиста" className="w-full rounded-lg" />
              <a
                href={shareCardBlobUrl}
                download={`wishlist-${wishlist?.name.slice(0, 30).replace(/\s+/g, '-')}.png`}
                className={buttonVariants({ variant: 'secondary' })}
              >
                Сохранить изображение
              </a>
            </>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
