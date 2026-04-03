import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getWish, deleteWish, fulfillWish, unfulfillWish, duplicateWish, copyWish } from '../api/wishes';
import { reserveWish, cancelReservation, getMyReservations } from '../api/reservations';
import { getWishlist, getMyWishlists } from '../api/wishlists';
import { getImageUrl, API_URL } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { PRIORITY_LABELS, CURRENCY_LABELS } from '../types';
import type { WishDto, WishlistDto, WishlistSummaryDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';

const PRIORITY_BADGE: Record<number, string> = {
  0: 'bg-muted text-muted-foreground',
  1: 'bg-green-100 text-green-700',
  2: 'bg-yellow-100 text-yellow-700',
  3: 'bg-red-100 text-red-700',
};

function QrModal({ url, open, onClose }: { url: string; open: boolean; onClose: () => void }) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-sm">
        <DialogTitle>QR-код желания</DialogTitle>
        <img src={url} alt="QR код" className="w-full rounded-lg" />
      </DialogContent>
    </Dialog>
  );
}

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

export default function WishPage() {
  const { id: wishlistId, wishId } = useParams<{ id: string; wishId: string }>();
  const navigate = useNavigate();
  const { user: me } = useAuth();
  const toast = useToast();

  const [wish, setWish] = useState<WishDto | null>(null);
  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [isMineReserved, setIsMineReserved] = useState(false);
  const [loading, setLoading] = useState(true);
  const [showQr, setShowQr] = useState(false);
  const [showCopy, setShowCopy] = useState(false);
  const [copyWishlists, setCopyWishlists] = useState<WishlistSummaryDto[]>([]);

  const myRole = wishlist?.members.find((m) => m.userId === me?.id)?.role ?? null;
  const isOwner = myRole === 2;
  const canEdit = myRole !== null && myRole >= 1;

  useEffect(() => {
    if (!wishlistId || !wishId) return;
    Promise.all([getWish(wishlistId, wishId), getWishlist(wishlistId)])
      .then(async ([w, wl]) => {
        setWish(w);
        setWishlist(wl);
        if (me) {
          try { const res = await getMyReservations(1, 100); setIsMineReserved(res.items.some((r) => r.wishId === wishId)); }
          catch { /* ignore */ }
        }
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [wishlistId, wishId, me]);

  const handleReserve = async () => {
    if (!wishlistId || !wishId || !wish) return;
    try {
      if (isMineReserved) { await cancelReservation(wishId); setIsMineReserved(false); setWish((p) => p ? { ...p, isReserved: false } : p); }
      else { await reserveWish(wishId, wishlistId); setIsMineReserved(true); setWish((p) => p ? { ...p, isReserved: true } : p); }
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleFulfill = async () => {
    if (!wishlistId || !wishId || !wish) return;
    try {
      if (wish.isFulfilled) { await unfulfillWish(wishlistId, wishId); setWish((p) => p ? { ...p, isFulfilled: false } : p); }
      else { await fulfillWish(wishlistId, wishId); setWish((p) => p ? { ...p, isFulfilled: true } : p); }
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleDuplicate = async () => {
    if (!wishlistId || !wishId) return;
    try { const r = await duplicateWish(wishlistId, wishId); toast.success('Желание продублировано'); navigate(`/wishlists/${wishlistId}/wishes/${r.wishId}`); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleOpenCopy = async () => {
    try { const wls = await getMyWishlists(); setCopyWishlists(wls.filter((wl) => wl.id !== wishlistId)); setShowCopy(true); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleCopy = async (targetId: string) => {
    if (!wishlistId || !wishId) return;
    setShowCopy(false);
    try { const r = await copyWish(wishlistId, wishId, targetId); toast.success('Желание скопировано'); navigate(`/wishlists/${targetId}/wishes/${r.wishId}`); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleDelete = async () => {
    if (!wishlistId || !wishId || !confirm('Удалить желание?')) return;
    try { await deleteWish(wishlistId, wishId); navigate(`/wishlists/${wishlistId}`); }
    catch (e) { toast.error(parseError(e)); }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!wish) return <div className="text-center py-12 text-muted-foreground">Желание не найдено</div>;

  const imageUrl = getImageUrl(wish.imagePath);

  return (
    <div className="max-w-xl mx-auto">
      <div className="flex items-center justify-between mb-7 gap-3 flex-wrap">
        <Link to={`/wishlists/${wishlistId}`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>← Назад</Link>
        <div className="flex flex-wrap gap-2">
          {wishlist?.visibility === 0 && <Button variant="ghost" size="sm" onClick={() => setShowQr(true)}>📷 QR</Button>}
          {canEdit && <Button variant="ghost" size="sm" onClick={handleDuplicate}>Дублировать</Button>}
          {me && <Button variant="ghost" size="sm" onClick={handleOpenCopy}>Копировать</Button>}
          {canEdit && <Link to={`/wishlists/${wishlistId}/wishes/${wishId}/edit`} className={buttonVariants({ variant: 'secondary', size: 'sm' })}>Изменить</Link>}
          {isOwner && <Button variant="destructive" size="sm" onClick={handleDelete}>Удалить</Button>}
        </div>
      </div>

      <Card>
        <CardContent className="pt-6">
          {imageUrl && <img src={imageUrl} alt={wish.name} className="w-full max-h-72 object-contain rounded-lg mb-5 bg-muted" />}

          <h1 className="text-xl font-extrabold tracking-tight mb-2">{wish.name}</h1>
          {wish.description && <p className="text-muted-foreground text-sm mb-4">{wish.description}</p>}

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
                {wish.isFulfilled ? '✅ Исполнено' : wish.isReserved ? '🔒 Зарезервировано' : '⏳ Ожидает'}
              </div>
            </div>
          </div>

          {wish.url && (
            <a href={wish.url} target="_blank" rel="noopener noreferrer" className={`${buttonVariants({ variant: 'ghost', size: 'sm' })} mb-4`}>🔗 Перейти к товару</a>
          )}

          <Separator className="mb-4" />

          <div className="flex flex-wrap gap-2">
            {isOwner && (
              <Button variant="secondary" onClick={handleFulfill}>
                {wish.isFulfilled ? '↩ Отметить не исполненным' : '✓ Отметить исполненным'}
              </Button>
            )}
            {me && !isOwner && !wish.isFulfilled && (
              <Button
                variant={isMineReserved ? 'destructive' : wish.isReserved ? 'ghost' : 'default'}
                onClick={handleReserve}
                disabled={wish.isReserved && !isMineReserved}
              >
                {isMineReserved ? 'Отменить резервацию' : wish.isReserved ? 'Уже зарезервировано' : 'Зарезервировать'}
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      <CopyModal wishlists={copyWishlists} onSelect={handleCopy} open={showCopy} onClose={() => setShowCopy(false)} />
      {wishId && <QrModal url={`${API_URL}/wishlists/${wishlistId}/wishes/${wishId}/qr`} open={showQr} onClose={() => setShowQr(false)} />}
    </div>
  );
}
