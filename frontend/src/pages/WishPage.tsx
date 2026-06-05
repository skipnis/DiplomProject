import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { getWish, deleteWish, fulfillWish, unfulfillWish, duplicateWish, copyWish, regenerateWishShareToken, getGiftBadges } from '../api/wishes';
import { getFulfilledBadgeDefinitions } from '../api/catalog';
import { reserveWish, cancelReservation, getMyReservations } from '../api/reservations';
import { getWishlist, getMyWishlists } from '../api/wishlists';
import { getImageUrl, STORAGE_URL } from '../api/client';
import { generateWishShareCard } from '../lib/generateWishShareCard';

import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { PRIORITY_LABELS, CURRENCY_LABELS } from '../types';
import type { WishDto, WishlistDto, WishlistSummaryDto, FulfilledWishBadgeDto, FulfilledBadgeDefinitionDto } from '../types';
import { Copy, Pencil, Trash2, Share2 } from 'lucide-react';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';

const PRIORITY_BADGE: Record<number, string> = {
  0: 'bg-muted text-muted-foreground',
  1: 'bg-green-100 text-green-700',
  2: 'bg-yellow-100 text-yellow-700',
  3: 'bg-red-100 text-red-700',
  4: 'bg-purple-100 text-purple-700',
};

function CopyModal({ wishlists, onSelect, open, onClose }: { wishlists: WishlistSummaryDto[]; onSelect: (id: string) => void; open: boolean; onClose: () => void }) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-sm">
        <DialogTitle>Копировать в вишлист</DialogTitle>
        {wishlists.length === 0
          ? <p className="text-muted-foreground text-sm">Нет других вишлистов</p>
          : <div className="flex flex-col gap-1 max-h-72 overflow-y-auto">
              {wishlists.map((wl) => (
                <Button key={wl.id} variant="ghost" className="justify-start" onClick={() => onSelect(wl.id)}>
                  {wl.emoji} {wl.name}
                </Button>
              ))}
            </div>
        }
        <Button variant="ghost" onClick={onClose}>Отмена</Button>
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

export default function WishPage() {
  const { id: wishlistId, wishId } = useParams<{ id: string; wishId: string }>();
  const navigate = useNavigate();
  const { user: me } = useAuth();
  const errorToast = useToast();

  const [wish, setWish] = useState<WishDto | null>(null);
  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [isMineReserved, setIsMineReserved] = useState(false);
  const [loading, setLoading] = useState(true);
  const [shareLoading, setShareLoading] = useState(false);
  const [shareCardLoading, setShareCardLoading] = useState(false);
  const [shareCardBlobUrl, setShareCardBlobUrl] = useState<string | null>(null);
  const [showCopy, setShowCopy] = useState(false);
  const [copyWishlists, setCopyWishlists] = useState<WishlistSummaryDto[]>([]);
  const [existingGiftBadges, setExistingGiftBadges] = useState<FulfilledWishBadgeDto[] | null>(null);
  const [fulfilledBadgeDefinitions, setFulfilledBadgeDefinitions] = useState<FulfilledBadgeDefinitionDto[]>([]);
  const [showFulfillConfirm, setShowFulfillConfirm] = useState(false);
  const [showUnfulfillConfirm, setShowUnfulfillConfirm] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const myRole = wishlist?.members.find((member) => member.userId === me?.id)?.role ?? null;
  const isOwner = myRole === 2;
  const canEdit = myRole !== null && myRole >= 1;
  const isSystem = !!wishlist?.isSystem;
  const isWishAuthor = wish?.createdByUserId ? wish.createdByUserId === me?.id : isOwner;

  useEffect(() => {
    if (!wishlistId || !wishId) return;
    Promise.all([getWish(wishlistId, wishId), getWishlist(wishlistId), getFulfilledBadgeDefinitions()])
      .then(async ([fetchedWish, fetchedWishlist, definitions]) => {
        setWish(fetchedWish);
        setWishlist(fetchedWishlist);
        setFulfilledBadgeDefinitions(definitions);
        if (me) {
          try { const reservations = await getMyReservations(1, 100); setIsMineReserved(reservations.items.some((reservation) => reservation.wishId === wishId)); }
          catch { /* ignore */ }
        }
        if (fetchedWish.isFulfilled && fetchedWish.fulfilledByReserverId && !fetchedWish.hasGiftBadges) {
          try {
            const badges = await getGiftBadges(wishlistId, wishId);
            setExistingGiftBadges(badges);
          } catch {
            setExistingGiftBadges([]);
          }
        }
      })
      .catch((e) => errorToast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [wishlistId, wishId, me]);

  const handleReserve = async () => {
    if (!me) { errorToast.warning('Войдите в аккаунт, чтобы забронировать желание'); return; }
    if (!wishlistId || !wishId || !wish) return;
    try {
      if (isMineReserved) { await cancelReservation(wishId); setIsMineReserved(false); setWish((prev) => prev ? { ...prev, isReserved: false } : prev); }
      else { await reserveWish(wishId, wishlistId); setIsMineReserved(true); setWish((prev) => prev ? { ...prev, isReserved: true } : prev); }
    } catch (e) { errorToast.error(parseError(e)); }
  };

  const executeFulfill = async () => {
    if (!wishlistId || !wishId || !wish) return;
    try {
      const { hasGifter } = await fulfillWish(wishlistId, wishId);
      const refreshedWish = await getWish(wishlistId, wishId);
      setWish(refreshedWish);
      if (hasGifter) {
        toast('Желание исполнено!', {
          action: {
            label: 'Оценить дарителя',
            onClick: () => navigate(`/wishlists/${wishlistId}/wishes/${wishId}/rate-gifter`),
          },
        });
      } else {
        toast.success('Желание исполнено!');
      }
    } catch (e) { errorToast.error(parseError(e)); }
  };

  const executeUnfulfill = async () => {
    if (!wishlistId || !wishId || !wish) return;
    try {
      await unfulfillWish(wishlistId, wishId);
      setWish((prev) => prev ? { ...prev, isFulfilled: false } : prev);
    } catch (e) { errorToast.error(parseError(e)); }
  };

  const handleFulfillClick = () => {
    if (!wish) return;
    if (wish.isFulfilled) {
      setShowUnfulfillConfirm(true);
    } else {
      setShowFulfillConfirm(true);
    }
  };

  const handleDuplicate = async () => {
    if (!wishlistId || !wishId) return;
    try { const result = await duplicateWish(wishlistId, wishId); errorToast.success('Желание продублировано'); navigate(`/wishlists/${wishlistId}/wishes/${result.wishId}`); }
    catch (e) { errorToast.error(parseError(e)); }
  };

  const handleOpenCopy = async () => {
    if (!me) { errorToast.warning('Войдите в аккаунт, чтобы копировать желание'); return; }
    try { const wishlists = await getMyWishlists(); setCopyWishlists(wishlists.filter((wl) => wl.id !== wishlistId)); setShowCopy(true); }
    catch (e) { errorToast.error(parseError(e)); }
  };

  const handleCopy = async (targetId: string) => {
    if (!wishlistId || !wishId) return;
    setShowCopy(false);
    try { const result = await copyWish(wishlistId, wishId, targetId); errorToast.success('Желание скопировано'); navigate(`/wishlists/${targetId}/wishes/${result.wishId}`); }
    catch (e) { errorToast.error(parseError(e)); }
  };

  const executeDelete = async () => {
    if (!wishlistId || !wishId) return;
    try { await deleteWish(wishlistId, wishId); navigate(`/wishlists/${wishlistId}`); }
    catch (e) { errorToast.error(parseError(e)); }
  };

  const handleRegenerateShareToken = async () => {
    if (!wishlistId || !wishId) return;
    setShareLoading(true);
    try {
      const { token } = await regenerateWishShareToken(wishlistId, wishId);
      setWish((prev) => prev ? { ...prev, shareToken: token } : prev);
      errorToast.success('Ссылка обновлена');
    } catch (e) {
      errorToast.error(parseError(e));
    } finally {
      setShareLoading(false);
    }
  };

  const handleShare = async () => {
    if (!wish?.shareToken || shareCardLoading) return;
    setShareCardLoading(true);
    try {
      const blob = await generateWishShareCard({
        name: wish.name,
        priority: wish.priority,
        imagePath: wish.imagePath,
        shareToken: wish.shareToken,
        ownerDisplayName: me?.displayName ?? '',
        storageUrl: STORAGE_URL,
      });
      const file = new File([blob], 'wish.png', { type: 'image/png' });
      if (navigator.canShare?.({ files: [file] })) {
        await navigator.share({ files: [file] });
      } else {
        const blobUrl = URL.createObjectURL(blob);
        setShareCardBlobUrl(blobUrl);
      }
    } catch (error) {
      if (error instanceof Error && error.name !== 'AbortError') {
        errorToast.error('Не удалось поделиться');
      }
    } finally {
      setShareCardLoading(false);
    }
  };

  const handleShareCardModalClose = () => {
    if (shareCardBlobUrl) URL.revokeObjectURL(shareCardBlobUrl);
    setShareCardBlobUrl(null);
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!wish) return <div className="text-center py-12 text-muted-foreground">Желание не найдено</div>;

  const imageUrl = getImageUrl(wish.imagePath);

  return (
    <div className="max-w-xl mx-auto">
      <div className="flex items-center justify-between mb-7 gap-3">
        <Link to={`/wishlists/${wishlistId}`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>← Назад</Link>
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground" onClick={handleOpenCopy} title="Копировать в вишлист">
            <Copy size={16} />
          </Button>
          {isWishAuthor && (
            <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground" onClick={() => navigate(`/wishlists/${wishlistId}/wishes/${wishId}/edit`)} title="Редактировать">
              <Pencil size={16} />
            </Button>
          )}
          {isWishAuthor && (
            <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground hover:text-destructive" onClick={() => setShowDeleteConfirm(true)} title="Удалить">
              <Trash2 size={16} />
            </Button>
          )}
          {isOwner && !isSystem && wish?.shareToken && (
            <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground" onClick={handleShare} title="Поделиться" disabled={shareCardLoading}>
              <Share2 size={16} />
            </Button>
          )}
          {canEdit && (
            <DropdownMenu>
              <DropdownMenuTrigger className={`${buttonVariants({ variant: 'ghost', size: 'sm' })} h-8 w-8 p-0 text-muted-foreground text-lg`}>⋯</DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={handleDuplicate}>Дублировать</DropdownMenuItem>
                {isOwner && !isSystem && wish?.shareToken && (
                  <DropdownMenuItem onClick={handleRegenerateShareToken} disabled={shareLoading}>
                    {shareLoading ? 'Сбрасываем...' : 'Сбросить ссылку для шаринга'}
                  </DropdownMenuItem>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </div>

      <Card>
        <CardContent className="pt-6">
          {imageUrl && (
            <div className="relative w-full overflow-hidden rounded-lg mb-5">
              <img src={imageUrl} alt="" aria-hidden className="absolute inset-0 w-full h-full object-cover scale-110 blur-xl opacity-60" />
              <img src={imageUrl} alt={wish.name} className="relative w-full max-h-72 object-contain" />
            </div>
          )}

          <h1 className="text-xl font-extrabold tracking-tight mb-1">{wish.name}</h1>
          {wishlist && wishlist.members.length > 1 && wish.createdByDisplayName && (
            <div className="text-xs text-muted-foreground mb-2">Добавил: {wish.createdByDisplayName}</div>
          )}
          {wish.description && (
            <div className="mb-4">
              <div className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-1.5">Описание</div>
              <p className="text-muted-foreground text-sm">{wish.description}</p>
            </div>
          )}

          <div className="flex flex-wrap gap-6 mb-4">
            {wish.price != null && (
              <div>
                <div className="text-xs text-muted-foreground mb-0.5">Цена</div>
                <div className="font-bold text-primary text-lg">{wish.price} {wish.currency != null ? CURRENCY_LABELS[wish.currency] : ''}</div>
              </div>
            )}
            <div>
              <div className="text-xs text-muted-foreground mb-0.5">Приоритет</div>
              <span className={`text-xs px-2 py-0.5 rounded-full font-semibold ${PRIORITY_BADGE[wish.priority]}`}>{PRIORITY_LABELS[wish.priority]}</span>
            </div>
            <div>
              <div className="text-xs text-muted-foreground mb-0.5">Статус</div>
              <div className="font-semibold text-sm">
                {wish.isFulfilled
                  ? wish.fulfilledByDisplayName
                    ? `✅ Исполнено: ${wish.fulfilledByDisplayName}`
                    : '✅ Исполнено собой любимым'
                  : wish.isReserved ? '🔒 Забронировано' : '⏳ Ожидает'}
              </div>
            </div>
          </div>

          {wish.url && (
            <a href={wish.url} target="_blank" rel="noopener noreferrer" className={`${buttonVariants({ variant: 'ghost', size: 'sm' })} mb-4`}>🔗 Перейти к товару</a>
          )}

          {existingGiftBadges && existingGiftBadges.length > 0 && (
            <>
              <Separator className="mb-4" />
              <div>
                <div className="text-xs text-muted-foreground mb-2">Впечатление от подарка</div>
                <div className="flex flex-wrap gap-1.5">
                  {existingGiftBadges.map((badge) => {
                    const def = fulfilledBadgeDefinitions.find((d) => d.id === badge.badgeType);
                    return (
                      <span key={badge.badgeType} className="text-sm px-3 py-1 rounded-full bg-primary/10 text-primary border border-primary/20">
                        {def ? <>{def.emoji}<span className="hidden sm:inline"> {def.label}</span></> : `Бейдж #${badge.badgeType}`}
                      </span>
                    );
                  })}
                </div>
              </div>
            </>
          )}

          <Separator className="mb-4 mt-4" />

          <div className="flex flex-wrap gap-2">
            {isWishAuthor && !(wish.isFulfilled && wish.hasGiftBadges) && (
              <Button variant="secondary" onClick={handleFulfillClick}>
                {wish.isFulfilled ? '↩ Отметить не исполненным' : '✓ Отметить исполненным'}
              </Button>
            )}
            {isWishAuthor && wish.isFulfilled && wish.fulfilledByReserverId && !wish.hasGiftBadges && (
              <Button variant="outline" onClick={() => navigate(`/wishlists/${wishlistId}/wishes/${wishId}/rate-gifter`)}>
                🎁 Оценить дарителя
              </Button>
            )}
            {!isWishAuthor && !wish.isFulfilled && !isSystem && (
              <Button
                variant={isMineReserved ? 'destructive' : wish.isReserved ? 'ghost' : 'default'}
                onClick={handleReserve}
                disabled={wish.isReserved && !isMineReserved}
              >
                {isMineReserved ? 'Отменить бронирование' : wish.isReserved ? 'Уже забронировано' : 'Забронировать'}
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      <CopyModal wishlists={copyWishlists} onSelect={handleCopy} open={showCopy} onClose={() => setShowCopy(false)} />
      <ConfirmModal
        open={showFulfillConfirm}
        onClose={() => setShowFulfillConfirm(false)}
        onConfirm={executeFulfill}
        title="Отметить желание исполненным?"
        confirmLabel="Отметить исполненным"
      />
      <ConfirmModal
        open={showUnfulfillConfirm}
        onClose={() => setShowUnfulfillConfirm(false)}
        onConfirm={executeUnfulfill}
        title="Снять отметку исполнения?"
        description="Желание вернётся в статус ожидания."
        confirmLabel="Снять отметку"
        confirmVariant="secondary"
      />
      <ConfirmModal
        open={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={executeDelete}
        title="Удалить желание?"
        description="Это действие нельзя отменить."
        confirmLabel="Удалить"
        confirmVariant="destructive"
      />
      <Dialog open={!!shareCardBlobUrl} onOpenChange={(open) => { if (!open) handleShareCardModalClose(); }}>
        <DialogContent className="max-w-sm p-4">
          <DialogTitle className="text-base">Поделиться желанием</DialogTitle>
          {shareCardBlobUrl && (
            <>
              <img src={shareCardBlobUrl} alt="Карточка желания" className="w-full rounded-lg" />
              <a
                href={shareCardBlobUrl}
                download={`wish-${wish?.name.slice(0, 30).replace(/\s+/g, '-')}.png`}
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
