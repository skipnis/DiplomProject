import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMyNotifications, markAsRead, markAllAsRead } from '../api/notifications';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { NotificationDto, PagedResponse } from '../types';
import { NOTIFICATION_TYPE_LABELS, ROLE_LABELS } from '../types';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

function formatDate(iso: string) {
  const d = new Date(iso);
  return d.toLocaleString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
}

function NotificationItem({ n, onRead }: { n: NotificationDto; onRead: (id: string) => void }) {
  const p = n.payload as Record<string, unknown>;

  const handleClick = () => {
    if (!n.isRead) onRead(n.id);
  };

  const getLink = (): string | null => {
    if (p.wishlistId) return `/wishlists/${p.wishlistId}`;
    if (p.requesterId) return `/users/${p.requesterId}`;
    if (p.acceptedByUserId) return `/users/${p.acceptedByUserId}`;
    return null;
  };

  const getDescription = (): string => {
    switch (n.type) {
      case 1: return `${p.reservedByDisplayName ?? 'Кто-то'} зарезервировал «${p.wishName ?? '...'}» в вишлисте «${p.wishlistName ?? '...'}»`;
      case 2: return `${p.cancelledByDisplayName ?? 'Кто-то'} отменил резервацию «${p.wishName ?? '...'}» в «${p.wishlistName ?? '...'}»`;
      case 3: return `«${p.wishName ?? '...'}» отмечено как подаренное в «${p.wishlistName ?? '...'}»`;
      case 10: return `${p.requesterDisplayName ?? 'Пользователь'} отправил вам заявку в друзья`;
      case 11: return `${p.acceptedByDisplayName ?? 'Пользователь'} принял вашу заявку в друзья`;
      case 20: return `${p.addedByDisplayName ?? 'Кто-то'} добавил вас в вишлист «${p.wishlistName ?? '...'}» с ролью ${ROLE_LABELS[p.role as number] ?? ''}`;
      case 21: return `${p.removedByDisplayName ?? 'Кто-то'} удалил вас из вишлиста «${p.wishlistName ?? '...'}»`;
      case 22: return `Ваша роль в вишлисте «${p.wishlistName ?? '...'}» изменена на ${p.customRoleName ? `«${p.customRoleName}»` : (ROLE_LABELS[p.newRole as number] ?? '')}`;
      default: return 'Уведомление';
    }
  };

  const link = getLink();
  const content = (
    <Card
      className={`cursor-pointer transition-colors hover:bg-muted/50 ${n.isRead ? 'opacity-60' : ''}`}
      onClick={handleClick}
    >
      <CardContent className="flex items-start gap-3 py-3 px-4">
        <div className={`w-2 h-2 rounded-full mt-1.5 shrink-0 ${n.isRead ? 'bg-muted-foreground/30' : 'bg-primary'}`} />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium">{NOTIFICATION_TYPE_LABELS[n.type] ?? 'Уведомление'}</p>
          <p className="text-sm text-muted-foreground mt-0.5">{getDescription()}</p>
        </div>
        <span className="text-xs text-muted-foreground shrink-0">{formatDate(n.createdAt)}</span>
      </CardContent>
    </Card>
  );

  if (link) {
    return <Link to={link}>{content}</Link>;
  }
  return content;
}

export default function NotificationsPage() {
  const toast = useToast();
  const [data, setData] = useState<PagedResponse<NotificationDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const load = (p: number) => {
    setLoading(true);
    getMyNotifications(p, 20)
      .then(setData)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(page); }, [page]);

  const handleRead = async (id: string) => {
    try {
      await markAsRead(id);
      setData((prev) =>
        prev
          ? { ...prev, items: prev.items.map((n) => (n.id === id ? { ...n, isRead: true } : n)) }
          : prev,
      );
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const handleReadAll = async () => {
    try {
      await markAllAsRead();
      setData((prev) =>
        prev ? { ...prev, items: prev.items.map((n) => ({ ...n, isRead: true })) } : prev,
      );
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const unreadCount = data?.items.filter((n) => !n.isRead).length ?? 0;

  return (
    <div className="max-w-2xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Уведомления</h1>
        {unreadCount > 0 && (
          <Button variant="outline" size="sm" onClick={handleReadAll}>
            Прочитать все
          </Button>
        )}
      </div>

      {loading && <p className="text-muted-foreground text-sm">Загрузка...</p>}

      {!loading && data?.items.length === 0 && (
        <p className="text-muted-foreground text-sm">Нет уведомлений</p>
      )}

      <div className="flex flex-col gap-2">
        {data?.items.map((n) => (
          <NotificationItem key={n.id} n={n} onRead={handleRead} />
        ))}
      </div>

      {data && (data.hasPreviousPage || data.hasNextPage) && (
        <div className="flex gap-2 mt-6 justify-center">
          <Button variant="outline" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>
            Назад
          </Button>
          <span className="text-sm text-muted-foreground self-center">стр. {page}</span>
          <Button variant="outline" size="sm" disabled={!data.hasNextPage} onClick={() => setPage((p) => p + 1)}>
            Вперёд
          </Button>
        </div>
      )}
    </div>
  );
}
