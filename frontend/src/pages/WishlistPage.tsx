import { useEffect, useState, useCallback } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getWishlist, deleteWishlist } from '../api/wishlists';
import { getWishes, deleteWish, fulfillWish, unfulfillWish } from '../api/wishes';
import { reserveWish, cancelReservation, getMyReservations } from '../api/reservations';
import { getUserProfile } from '../api/users';
import { getImageUrl, API_URL } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { VISIBILITY_LABELS, ROLE_LABELS, PRIORITY_LABELS } from '../types';
import type { WishlistDto, WishDto, WishlistMemberRole, UserProfile } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';

function QrModal({ url, open, onClose }: { url: string; open: boolean; onClose: () => void }) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-sm">
        <DialogTitle>QR-код</DialogTitle>
        <img src={url} alt="QR код" className="w-full rounded-lg" />
      </DialogContent>
    </Dialog>
  );
}

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
  const [wishes, setWishes] = useState<WishDto[]>([]);
  const [wishPage, setWishPage] = useState(1);
  const [wishTotalPages, setWishTotalPages] = useState(1);
  const [wishTotalCount, setWishTotalCount] = useState(0);
  const [memberProfiles, setMemberProfiles] = useState<Record<string, UserProfile>>({});
  const [myReservationIds, setMyReservationIds] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [wishesLoading, setWishesLoading] = useState(false);
  const [showQr, setShowQr] = useState(false);
  const PAGE_SIZE = 12;

  const myRole: WishlistMemberRole | null = wishlist?.members.find((m) => m.userId === me?.id)?.role ?? null;
  const isOwner = myRole === 2;
  const canEdit = myRole !== null && myRole >= 1;

  const loadWishes = useCallback(async (page: number) => {
    if (!id) return;
    setWishesLoading(true);
    try {
      const ws = await getWishes(id, page, PAGE_SIZE);
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
      toast.error(parseError(e));
    } finally {
      setLoading(false);
    }
  }, [id, me]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => { loadWishes(wishPage); }, [wishPage]);

  const handleDeleteWishlist = async () => {
    if (!id || !confirm('Удалить вишлист?')) return;
    try { await deleteWishlist(id); navigate('/wishlists'); } catch (e) { toast.error(parseError(e)); }
  };

  const handleDeleteWish = async (wishId: string) => {
    if (!id || !confirm('Удалить желание?')) return;
    try {
      await deleteWish(id, wishId);
      setWishes((prev) => prev.filter((w) => w.id !== wishId));
      setWishTotalCount((n) => n - 1);
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleFulfill = async (wish: WishDto) => {
    if (!id) return;
    try {
      if (wish.isFulfilled) await unfulfillWish(id, wish.id); else await fulfillWish(id, wish.id);
      setWishes((prev) => prev.map((w) => w.id === wish.id ? { ...w, isFulfilled: !w.isFulfilled } : w));
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleReserve = async (wish: WishDto) => {
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
  if (!wishlist) return <div className="text-center py-12 text-muted-foreground">Вишлист не найден</div>;

  return (
    <div>
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex items-start gap-4 flex-wrap">
            <div className="text-4xl leading-none">{wishlist.emoji || '📋'}</div>
            <div className="flex-1 min-w-0">
              <h1 className="text-xl font-extrabold tracking-tight">{wishlist.name}</h1>
              {wishlist.description && <p className="text-sm text-muted-foreground mt-1">{wishlist.description}</p>}
              <div className="flex flex-wrap gap-1 mt-2">
                <Badge variant="secondary">{VISIBILITY_LABELS[wishlist.visibility]}</Badge>
                {wishlist.isSystem && <Badge variant="secondary">⚙️ Системный</Badge>}
                {isOwner && wishlist.isSurpriseModeEnabled && <Badge variant="secondary">🎁 Сюрприз</Badge>}
              </div>
            </div>
            <div className="flex flex-wrap gap-2">
              {wishlist.visibility === 0 && <Button variant="ghost" size="sm" onClick={() => setShowQr(true)}>📷 QR</Button>}
              {canEdit && <Link to={`/wishlists/${id}/edit`} className={buttonVariants({ variant: 'secondary', size: 'sm' })}>Изменить</Link>}
              {isOwner && <Button variant="destructive" size="sm" onClick={handleDeleteWishlist}>Удалить</Button>}
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="flex items-center justify-between mb-5">
        <h2 className="text-lg font-bold">Желания ({wishTotalCount})</h2>
        {canEdit && <Link to={`/wishlists/${id}/wishes/new`} className={buttonVariants({ size: 'sm' })}>+ Добавить</Link>}
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
            const shouldBlur = wish.isReserved && myRole === null;
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
                  {shouldBlur && <div className="absolute inset-0 flex items-center justify-center font-bold text-muted-foreground text-sm bg-white/30">🔒 Зарезервировано</div>}
                  {wish.isFulfilled && <div className="absolute top-2 right-2 bg-green-500 text-white rounded-full px-2 py-0.5 text-xs font-semibold">✓ Исполнено</div>}
                </Link>
                <div className="flex flex-wrap gap-1 mt-2">
                  {me && !isOwner && !wish.isFulfilled && (
                    <Button size="sm" variant={iMineReserved ? 'destructive' : wish.isReserved ? 'ghost' : 'secondary'} onClick={() => handleReserve(wish)} disabled={wish.isReserved && !iMineReserved}>
                      {iMineReserved ? 'Отменить резерв' : wish.isReserved ? 'Зарезервировано' : 'Зарезервировать'}
                    </Button>
                  )}
                  {isOwner && (
                    <>
                      <Button size="sm" variant="ghost" onClick={() => handleFulfill(wish)}>{wish.isFulfilled ? '↩ Не исполнено' : '✓ Исполнено'}</Button>
                      <Link to={`/wishlists/${id}/wishes/${wish.id}/edit`} className={buttonVariants({ size: 'sm', variant: 'ghost' })}>Изменить</Link>
                      <Button size="sm" variant="destructive" onClick={() => handleDeleteWish(wish.id)}>Удалить</Button>
                    </>
                  )}
                  {canEdit && !isOwner && <Link to={`/wishlists/${id}/wishes/${wish.id}/edit`} className={buttonVariants({ size: 'sm', variant: 'ghost' })}>Изменить</Link>}
                </div>
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

      {canEdit && wishlist.members.length > 0 && (
        <>
          <Separator className="my-6" />
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-bold">Участники</h2>
            {isOwner && <Link to={`/wishlists/${id}/edit`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>Управление</Link>}
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
    </div>
  );
}
