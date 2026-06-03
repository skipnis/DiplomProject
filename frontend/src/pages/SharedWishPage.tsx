import { useEffect, useState } from 'react';
import { useParams, Link, useLocation } from 'react-router-dom';
import { getSharedWish } from '../api/share';
import { getImageUrl } from '../api/client';
import { reserveWish, cancelReservation, getMyReservations } from '../api/reservations';
import { PRIORITY_LABELS, CURRENCY_LABELS } from '../types';
import type { SharedWishResponse } from '../types';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

const PRIORITY_BADGE: Record<number, string> = {
  0: 'bg-muted text-muted-foreground',
  1: 'bg-green-100 text-green-700',
  2: 'bg-yellow-100 text-yellow-700',
  3: 'bg-red-100 text-red-700',
  4: 'bg-purple-100 text-purple-700',
};

export default function SharedWishPage() {
  const { token } = useParams<{ token: string }>();
  const { user: me } = useAuth();
  const toast = useToast();
  const location = useLocation();

  const [wish, setWish] = useState<SharedWishResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);
  const [isMineReserved, setIsMineReserved] = useState(false);
  const [reserveLoading, setReserveLoading] = useState(false);

  useEffect(() => {
    if (!token) return;
    getSharedWish(token)
      .then(async (fetchedWish) => {
        setWish(fetchedWish);
        if (me) {
          try {
            const reservations = await getMyReservations(1, 100);
            setIsMineReserved(reservations.items.some((r) => r.wishId === fetchedWish.id));
          } catch {
            // not critical
          }
        }
      })
      .catch(() => setNotFound(true))
      .finally(() => setLoading(false));
  }, [token, me]);

  const handleReserve = async () => {
    if (!wish || reserveLoading) return;
    setReserveLoading(true);
    try {
      if (isMineReserved) {
        await cancelReservation(wish.id);
        setIsMineReserved(false);
        setWish((prev) => prev ? { ...prev, isReserved: false } : prev);
        toast.success('Бронирование отменено');
      } else {
        await reserveWish(wish.id, wish.wishlistId);
        setIsMineReserved(true);
        setWish((prev) => prev ? { ...prev, isReserved: true } : prev);
        toast.success('Желание забронировано');
      }
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setReserveLoading(false);
    }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  if (notFound || !wish) {
    return (
      <div className="max-w-xl mx-auto text-center py-16">
        <div className="text-4xl mb-4">🔗</div>
        <h1 className="text-xl font-bold mb-2">Ссылка недействительна</h1>
        <p className="text-muted-foreground text-sm mb-6">Желание не найдено или ссылка была обновлена.</p>
        <Link to="/" className={buttonVariants({ variant: 'secondary' })}>На главную</Link>
      </div>
    );
  }

  const imageUrl = getImageUrl(wish.imagePath);
  const isOwner = me?.id === wish.ownerId;
  const redirectParam = `?redirect=${encodeURIComponent(location.pathname)}`;

  return (
    <div className="max-w-xl mx-auto">
      <div className="flex items-center justify-between mb-6 gap-3 flex-wrap">
        <div className="text-sm text-muted-foreground">
          Автор:{' '}
          <Link to={`/users/${wish.ownerId}`} className="font-semibold text-foreground hover:text-primary transition-colors">
            {wish.ownerUsername}
          </Link>
        </div>
        <Link to={`/wishlists/${wish.wishlistId}`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>
          Смотреть в вишлисте →
        </Link>
      </div>

      <Card>
        <CardContent className="pt-6">
          {imageUrl && (
            <div className="relative w-full overflow-hidden rounded-lg mb-5">
              <img src={imageUrl} alt="" aria-hidden className="absolute inset-0 w-full h-full object-cover scale-110 blur-xl opacity-60" />
              <img src={imageUrl} alt={wish.name} className="relative w-full max-h-72 object-contain" />
            </div>
          )}

          <h1 className="text-xl font-extrabold tracking-tight mb-2">{wish.name}</h1>
          {wish.description && <p className="text-muted-foreground text-sm mb-4">{wish.description}</p>}

          <div className="flex flex-wrap gap-6 mb-4">
            {wish.price != null && (
              <div>
                <div className="text-xs text-muted-foreground mb-0.5">Цена</div>
                <div className="font-bold text-primary text-lg">
                  {wish.price} {wish.currency != null ? CURRENCY_LABELS[wish.currency] : ''}
                </div>
              </div>
            )}
            <div>
              <div className="text-xs text-muted-foreground mb-0.5">Приоритет</div>
              <span className={`text-xs px-2 py-0.5 rounded-full font-semibold ${PRIORITY_BADGE[wish.priority]}`}>
                {PRIORITY_LABELS[wish.priority]}
              </span>
            </div>

          </div>

          {wish.url && (
            <a
              href={wish.url}
              target="_blank"
              rel="noopener noreferrer"
              className={`${buttonVariants({ variant: 'ghost', size: 'sm' })} mb-2`}
            >
              🔗 Перейти к товару
            </a>
          )}

          {!wish.isFulfilled && !isOwner && (
            <div className="mt-4">
              {me ? (
                <Button
                  variant={isMineReserved ? 'destructive' : wish.isReserved ? 'ghost' : 'default'}
                  onClick={handleReserve}
                  disabled={(wish.isReserved && !isMineReserved) || reserveLoading}
                >
                  {isMineReserved ? 'Отменить бронирование' : wish.isReserved ? 'Уже забронировано' : 'Забронировать'}
                </Button>
              ) : (
                <div className="rounded-xl border bg-card p-5 text-center">
                  <div className="text-2xl mb-2">🎁</div>
                  <p className="font-semibold mb-1">Хочешь подарить это?</p>
                  <p className="text-sm text-muted-foreground mb-4">
                    Зарегистрируйся в Wishapp — помогай друзьям исполнять желания и создавай свои вишлисты.
                  </p>
                  <div className="flex justify-center gap-3">
                    <Link to={`/login${redirectParam}`} className={buttonVariants({ variant: 'default' })}>Присоединиться</Link>
                    <Link to={`/login${redirectParam}`} className={buttonVariants({ variant: 'outline' })}>Войти</Link>
                  </div>
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
