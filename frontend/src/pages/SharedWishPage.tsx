import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getSharedWish } from '../api/share';
import { getImageUrl } from '../api/client';
import { PRIORITY_LABELS, CURRENCY_LABELS } from '../types';
import type { SharedWishResponse } from '../types';
import { buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';

const PRIORITY_BADGE: Record<number, string> = {
  0: 'bg-muted text-muted-foreground',
  1: 'bg-green-100 text-green-700',
  2: 'bg-yellow-100 text-yellow-700',
  3: 'bg-red-100 text-red-700',
  4: 'bg-purple-100 text-purple-700',
};

export default function SharedWishPage() {
  const { token } = useParams<{ token: string }>();

  const [wish, setWish] = useState<SharedWishResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    if (!token) return;
    getSharedWish(token)
      .then((w) => setWish(w))
      .catch(() => setNotFound(true))
      .finally(() => setLoading(false));
  }, [token]);

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

  return (
    <div className="max-w-xl mx-auto">
      <div className="flex items-center justify-between mb-6 gap-3 flex-wrap">
        <div className="text-sm text-muted-foreground">
          Желание <span className="font-semibold text-foreground">{wish.ownerUsername}</span>
        </div>
        <Link to={`/wishlists/${wish.wishlistId}`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>
          Смотреть в вишлисте →
        </Link>
      </div>

      <Card>
        <CardContent className="pt-6">
          {imageUrl && (
            <img src={imageUrl} alt={wish.name} className="w-full max-h-72 object-contain rounded-lg mb-5 bg-muted" />
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
            <div>
              <div className="text-xs text-muted-foreground mb-0.5">Статус</div>
              <div className="font-semibold text-sm">
                {wish.isFulfilled ? '✅ Исполнено' : wish.isReserved ? '🔒 Забронировано' : '⏳ Ожидает'}
              </div>
            </div>
          </div>

          {wish.url && (
            <a
              href={wish.url}
              target="_blank"
              rel="noopener noreferrer"
              className={`${buttonVariants({ variant: 'ghost', size: 'sm' })} mb-4`}
            >
              🔗 Перейти к товару
            </a>
          )}

          <Separator className="mb-4" />

          <Link to={`/wishlists/${wish.wishlistId}`} className={buttonVariants({ variant: 'secondary' })}>
            Открыть вишлист
          </Link>
        </CardContent>
      </Card>
    </div>
  );
}
