import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMyReservations, cancelReservation } from '../api/reservations';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { MyReservationDto, PagedResponse } from '../types';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

export default function ReservationsPage() {
  const toast = useToast();
  const [data, setData] = useState<PagedResponse<MyReservationDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const load = (p: number) => {
    setLoading(true);
    getMyReservations(p, 20)
      .then(setData)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(page); }, [page]);

  const handleCancel = async (wishId: string) => {
    if (!confirm('Отменить резервацию?')) return;
    try {
      await cancelReservation(wishId);
      setData((prev) => prev ? { ...prev, items: prev.items.filter((r) => r.wishId !== wishId), totalCount: prev.totalCount - 1 } : prev);
    } catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-7 gap-4">
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">Мои резервации</h1>
          {data && <p className="text-sm text-muted-foreground mt-0.5">{data.totalCount} резерваций</p>}
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
      ) : !data || data.items.length === 0 ? (
        <div className="text-center py-16">
          <div className="text-5xl mb-4">🎁</div>
          <p className="font-semibold mb-1">Нет резерваций</p>
          <p className="text-sm text-muted-foreground">Зайди в вишлист друга и зарезервируй желание</p>
        </div>
      ) : (
        <>
          <div className="flex flex-col gap-3">
            {data.items.map((r) => {
              const imgUrl = getImageUrl(r.wishImagePath);
              return (
                <Card key={r.reservationId}>
                  <CardContent className="pt-4 flex items-center gap-4">
                    <div className="w-14 h-14 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-2xl">
                      {imgUrl
                        ? <img src={imgUrl} alt={r.wishName} className="w-full h-full object-cover" />
                        : '🎁'
                      }
                    </div>
                    <div className="flex-1 min-w-0">
                      <Link to={`/wishlists/${r.wishlistId}/wishes/${r.wishId}`} className="font-semibold text-sm hover:underline line-clamp-1">{r.wishName}</Link>
                      <div className="text-xs text-muted-foreground mt-0.5">
                        {r.wishPrice != null && <span>{r.wishPrice} {r.wishCurrency} · </span>}
                        <Link to={`/wishlists/${r.wishlistId}`} className="text-primary hover:underline">{r.wishlistName}</Link>
                        {' · '}{r.wishlistOwnerName}
                      </div>
                      <div className="text-xs text-muted-foreground mt-0.5">
                        {new Date(r.reservedAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}
                      </div>
                    </div>
                    <Button variant="destructive" size="sm" onClick={() => handleCancel(r.wishId)}>Отменить</Button>
                  </CardContent>
                </Card>
              );
            })}
          </div>

          {(data.hasPreviousPage || data.hasNextPage) && (
            <div className="flex items-center justify-center gap-3 mt-6">
              <Button variant="ghost" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>← Назад</Button>
              <span className="text-sm text-muted-foreground">{page} / {Math.ceil(data.totalCount / 20)}</span>
              <Button variant="ghost" size="sm" disabled={!data.hasNextPage} onClick={() => setPage((p) => p + 1)}>Вперёд →</Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
