import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getWish, deleteWish, fulfillWish, unfulfillWish, duplicateWish, copyWish, regenerateWishShareToken, addGiftBadges, getGiftBadges } from '../api/wishes';
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
    try {
      await onSubmit(selected);
    } finally {
      setSubmitting(false);
    }
  };

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

export default function WishPage() {
  const { id: wishlistId, wishId } = useParams<{ id: string; wishId: string }>();
  const navigate = useNavigate();
  const { user: me } = useAuth();
  const toast = useToast();

  const [wish, setWish] = useState<WishDto | null>(null);
  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [isMineReserved, setIsMineReserved] = useState(false);
  const [loading, setLoading] = useState(true);
  const [shareLoading, setShareLoading] = useState(false);
  const [shareCardLoading, setShareCardLoading] = useState(false);
  const [shareCardBlobUrl, setShareCardBlobUrl] = useState<string | null>(null);
  const [showCopy, setShowCopy] = useState(false);
  const [copyWishlists, setCopyWishlists] = useState<WishlistSummaryDto[]>([]);
  const [showGiftBadgesModal, setShowGiftBadgesModal] = useState(false);
  const [existingGiftBadges, setExistingGiftBadges] = useState<FulfilledWishBadgeDto[] | null>(null);
  const [fulfilledBadgeDefinitions, setFulfilledBadgeDefinitions] = useState<FulfilledBadgeDefinitionDto[]>([]);
  const [showFulfillConfirm, setShowFulfillConfirm] = useState(false);
  const [showUnfulfillConfirm, setShowUnfulfillConfirm] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const myRole = wishlist?.members.find((member) => member.userId === me?.id)?.role ?? null;
  const isOwner = myRole === 2;
  const canEdit = myRole !== null && myRole >= 1;
  const isSystem = !!wishlist?.isSystem;

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
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [wishlistId, wishId, me]);

  const handleReserve = async () => {
    if (!me) { toast.warning('Войдите в аккаунт, чтобы забронировать желание'); return; }
    if (!wishlistId || !wishId || !wish) return;
    try {
      if (isMineReserved) { await cancelReservation(wishId); setIsMineReserved(false); setWish((prev) => prev ? { ...prev, isReserved: false } : prev); }
      else { await reserveWish(wishId, wishlistId); setIsMineReserved(true); setWish((prev) => prev ? { ...prev, isReserved: true } : prev); }
    } catch (e) { toast.error(parseError(e)); }
  };

  const executeFulfill = async () => {
    if (!wishlistId || !wishId || !wish) return;
    try {
      await fulfillWish(wishlistId, wishId);
      const refreshedWish = await getWish(wishlistId, wishId);
      setWish(refreshedWish);
      if (refreshedWish.fulfilledByReserverId && !refreshedWish.hasGiftBadges) {
        setShowGiftBadgesModal(true);
      }
    } catch (e) { toast.error(parseError(e)); }
  };

  const executeUnfulfill = async () => {
    if (!wishlistId || !wishId || !wish) return;
    try {
      await unfulfillWish(wishlistId, wishId);
      setWish((prev) => prev ? { ...prev, isFulfilled: false } : prev);
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleFulfillClick = () => {
    if (!wish) return;
    if (wish.isFulfilled) {
      setShowUnfulfillConfirm(true);
    } else {
      setShowFulfillConfirm(true);
    }
  };

  const handleGiftBadgesSubmit = async (badgeTypes: number[]) => {
    if (!wishlistId || !wishId) return;
    try {
      await addGiftBadges(wishlistId, wishId, badgeTypes);
      setWish((prev) => prev ? { ...prev, hasGiftBadges: true } : prev);
      toast.success('Спасибо за оценку!');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setShowGiftBadgesModal(false);
    }
  };

  const handleDuplicate = async () => {
    if (!wishlistId || !wishId) return;
    try { const result = await duplicateWish(wishlistId, wishId); toast.success('Желание продублировано'); navigate(`/wishlists/${wishlistId}/wishes/${result.wishId}`); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleOpenCopy = async () => {
    if (!me) { toast.warning('Войдите в аккаунт, чтобы копировать желание'); return; }
    try { const wishlists = await getMyWishlists(); setCopyWishlists(wishlists.filter((wl) => wl.id !== wishlistId)); setShowCopy(true); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleCopy = async (targetId: string) => {
    if (!wishlistId || !wishId) return;
    setShowCopy(false);
    try { const result = await copyWish(wishlistId, wishId, targetId); toast.success('Желание скопировано'); navigate(`/wishlists/${targetId}/wishes/${result.wishId}`); }
    catch (e) { toast.error(parseError(e)); }
  };

  const executeDelete = async () => {
    if (!wishlistId || !wishId) return;
    try { await deleteWish(wishlistId, wishId); navigate(`/wishlists/${wishlistId}`); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleRegenerateShareToken = async () => {
    if (!wishlistId || !wishId) return;
    setShareLoading(true);
    try {
      const { token } = await regenerateWishShareToken(wishlistId, wishId);
      setWish((prev) => prev ? { ...prev, shareToken: token } : prev);
      toast.success('Ссылка обновлена');
    } catch (e) {
      toast.error(parseError(e));
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
        wishlistName: wishlist?.name ?? '',
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
          {(isOwner || (canEdit && wish.createdByUserId === me?.id)) && (
            <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-muted-foreground" onClick={() => navigate(`/wishlists/${wishlistId}/wishes/${wishId}/edit`)} title="Редактировать">
              <Pencil size={16} />
            </Button>
          )}
          {(isOwner || (canEdit && wish.createdByUserId === me?.id)) && (
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
                {wish.isFulfilled ? '✅ Исполнено' : wish.isReserved ? '🔒 Забронировано' : '⏳ Ожидает'}
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
            {isOwner && (
              <Button variant="secondary" onClick={handleFulfillClick}>
                {wish.isFulfilled ? '↩ Отметить не исполненным' : '✓ Отметить исполненным'}
              </Button>
            )}
            {!isOwner && !wish.isFulfilled && !isSystem && (
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
      {wishlistId && wishId && (
        <GiftBadgesModal
          open={showGiftBadgesModal}
          onClose={() => setShowGiftBadgesModal(false)}
          onSubmit={handleGiftBadgesSubmit}
          definitions={fulfilledBadgeDefinitions}
        />
      )}
      <ConfirmModal
        open={showFulfillConfirm}
        onClose={() => setShowFulfillConfirm(false)}
        onConfirm={executeFulfill}
        title="Отметить желание исполненным?"
        description="Это действие изменит статус желания. Если у желания есть бронирование, вам будет предложено оценить подарок."
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
