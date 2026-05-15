import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMyReservations, cancelReservation, getReservationsOnMyWishes } from '../api/reservations';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { CURRENCY_LABELS } from '../types';
import type { MyReservationDto, PagedResponse, WishReservedOnMyWishDto } from '../types';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
}

export default function ReservationsPage() {
  const toast = useToast();

  const [myData, setMyData] = useState<PagedResponse<MyReservationDto> | null>(null);
  const [myPage, setMyPage] = useState(1);
  const [myLoading, setMyLoading] = useState(true);

  const [myWishesData, setMyWishesData] = useState<WishReservedOnMyWishDto[] | null>(null);
  const [myWishesLoading, setMyWishesLoading] = useState(false);
  const [myWishesLoaded, setMyWishesLoaded] = useState(false);

  const loadMine = (page: number) => {
    setMyLoading(true);
    getMyReservations(page, 20)
      .then(setMyData)
      .catch((error) => toast.error(parseError(error)))
      .finally(() => setMyLoading(false));
  };

  const loadMyWishes = () => {
    setMyWishesLoading(true);
    getReservationsOnMyWishes()
      .then((items) => { setMyWishesData(items); setMyWishesLoaded(true); })
      .catch((error) => toast.error(parseError(error)))
      .finally(() => setMyWishesLoading(false));
  };

  useEffect(() => { loadMine(myPage); }, [myPage]);

  const handleTabChange = (value: string) => {
    if (value === 'myWishes' && !myWishesLoaded) {
      loadMyWishes();
    }
  };

  useEffect(() => {
    if (!myWishesLoaded) loadMyWishes();
  }, []);

  const handleCancel = async (wishId: string) => {
    if (!confirm('Отменить бронирование?')) return;
    try {
      await cancelReservation(wishId);
      setMyData((prev) => prev
        ? { ...prev, items: prev.items.filter((r) => r.wishId !== wishId), totalCount: prev.totalCount - 1 }
        : prev);
    } catch (error) { toast.error(parseError(error)); }
  };

  return (
    <div>
      <h1 className="text-2xl font-extrabold tracking-tight mb-5">Бронирования</h1>

      <Tabs defaultValue="myWishes" onValueChange={handleTabChange}>
        <TabsList className="mb-5 w-full">
          <TabsTrigger value="myWishes" className="flex-1">Мои желания</TabsTrigger>
          <TabsTrigger value="mine" className="flex-1">Желания друзей</TabsTrigger>
        </TabsList>

        <TabsContent value="mine">
        <>
          {myData && <p className="text-sm text-muted-foreground mb-4">{myData.totalCount} бронирований</p>}
          {myLoading ? (
            <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
          ) : !myData || myData.items.length === 0 ? (
            <div className="text-center py-16">
              <div className="text-5xl mb-4">🎁</div>
              <p className="font-semibold mb-1">Нет бронирований</p>
              <p className="text-sm text-muted-foreground">Зайди в вишлист друга и забронируй желание</p>
            </div>
          ) : (
            <>
              <div className="flex flex-col gap-3">
                {myData.items.map((reservation) => {
                  const imgUrl = getImageUrl(reservation.wishImagePath);
                  return (
                    <Card key={reservation.reservationId}>
                      <CardContent className="pt-4 flex items-center gap-4">
                        <div className="w-14 h-14 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-2xl">
                          {imgUrl
                            ? <img src={imgUrl} alt={reservation.wishName} className="w-full h-full object-cover" />
                            : '🎁'
                          }
                        </div>
                        <div className="flex-1 min-w-0">
                          <Link to={`/wishlists/${reservation.wishlistId}/wishes/${reservation.wishId}`} className="font-semibold text-sm hover:underline line-clamp-1">{reservation.wishName}</Link>
                          <div className="text-xs text-muted-foreground mt-0.5">
                            {reservation.wishPrice != null && <span>{reservation.wishPrice} {reservation.wishCurrency != null ? CURRENCY_LABELS[reservation.wishCurrency] : ''} · </span>}
                            <Link to={`/wishlists/${reservation.wishlistId}`} className="text-primary hover:underline">{reservation.wishlistName}</Link>
                            {' · '}{reservation.wishlistOwnerName}
                          </div>
                          <div className="text-xs text-muted-foreground mt-0.5">{formatDate(reservation.reservedAt)}</div>
                        </div>
                        <Button variant="destructive" size="sm" onClick={() => handleCancel(reservation.wishId)}>Отменить</Button>
                      </CardContent>
                    </Card>
                  );
                })}
              </div>

              {(myData.hasPreviousPage || myData.hasNextPage) && (
                <div className="flex items-center justify-center gap-3 mt-6">
                  <Button variant="ghost" size="sm" disabled={!myData.hasPreviousPage} onClick={() => setMyPage((p) => p - 1)}>← Назад</Button>
                  <span className="text-sm text-muted-foreground">{myPage} / {Math.ceil(myData.totalCount / 20)}</span>
                  <Button variant="ghost" size="sm" disabled={!myData.hasNextPage} onClick={() => setMyPage((p) => p + 1)}>Вперёд →</Button>
                </div>
              )}
            </>
          )}
        </>
        </TabsContent>

        <TabsContent value="myWishes">
        <>
          {myWishesData && <p className="text-sm text-muted-foreground mb-4">{myWishesData.length} бронирований</p>}
          {myWishesLoading ? (
            <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
          ) : !myWishesData || myWishesData.length === 0 ? (
            <div className="text-center py-16">
              <div className="text-5xl mb-4">🎁</div>
              <p className="font-semibold mb-1">Никто ещё не бронировал твои желания</p>
              <p className="text-sm text-muted-foreground">Желания из вишлистов с режимом сюрприза здесь не отображаются</p>
            </div>
          ) : (
            <div className="flex flex-col gap-3">
              {myWishesData.map((item) => {
                const imgUrl = getImageUrl(item.wishImagePath);
                return (
                  <Card key={`${item.wishId}-${item.reservedByUserId}`}>
                    <CardContent className="pt-4 flex items-center gap-4">
                      <div className="w-14 h-14 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-2xl">
                        {imgUrl
                          ? <img src={imgUrl} alt={item.wishName} className="w-full h-full object-cover" />
                          : '🎁'
                        }
                      </div>
                      <div className="flex-1 min-w-0">
                        <Link to={`/wishlists/${item.wishlistId}/wishes/${item.wishId}`} className="font-semibold text-sm hover:underline line-clamp-1">{item.wishName}</Link>
                        <div className="text-xs text-muted-foreground mt-0.5">
                          {item.wishPrice != null && <span>{item.wishPrice} {item.wishCurrency != null ? CURRENCY_LABELS[item.wishCurrency] : ''} · </span>}
                          <Link to={`/wishlists/${item.wishlistId}`} className="text-primary hover:underline">{item.wishlistName}</Link>
                        </div>
                        <div className="text-xs text-muted-foreground mt-0.5">
                          Бронирует: <Link to={`/users/${item.reservedByUserId}`} className="text-foreground hover:underline font-medium">{item.reservedByDisplayName}</Link>
                          {' · '}{formatDate(item.reservedAt)}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </>
        </TabsContent>
      </Tabs>
    </div>
  );
}
