import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMyNotifications, markAsRead, markAllAsRead, deleteNotification } from '../api/notifications';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { NotificationDto, PagedResponse } from '../types';
import { NOTIFICATION_TYPE_LABELS, ROLE_LABELS } from '../types';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

type ReadFilter = 'all' | 'unread' | 'read';

function formatDate(iso: string) {
  const d = new Date(iso);
  return d.toLocaleString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
}

function NotificationItem({
  n,
  onRead,
  onDelete,
}: {
  n: NotificationDto;
  onRead: (id: string) => void;
  onDelete: (id: string) => void;
}) {
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
      case 1: return `${p.reservedByDisplayName ?? 'Кто-то'} забронировал «${p.wishName ?? '...'}» в вишлисте «${p.wishlistName ?? '...'}»`;
      case 2: return `${p.cancelledByDisplayName ?? 'Кто-то'} отменил бронирование «${p.wishName ?? '...'}» в «${p.wishlistName ?? '...'}»`;
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
  const cardContent = (
    <Card className={`transition-colors hover:bg-muted/50 ${n.isRead ? 'opacity-60' : ''}`}>
      <CardContent className="flex items-start gap-3 py-3 px-4">
        <div
          className={`w-2 h-2 rounded-full mt-1.5 shrink-0 ${n.isRead ? 'bg-muted-foreground/30' : 'bg-primary'}`}
        />
        <div className="flex-1 min-w-0 cursor-pointer" onClick={handleClick}>
          <p className="text-sm font-medium">{NOTIFICATION_TYPE_LABELS[n.type] ?? 'Уведомление'}</p>
          <p className="text-sm text-muted-foreground mt-0.5">{getDescription()}</p>
        </div>
        <span className="text-xs text-muted-foreground shrink-0 mt-0.5">{formatDate(n.createdAt)}</span>
        <Button
          variant="ghost"
          size="sm"
          className="shrink-0 h-7 w-7 p-0 text-muted-foreground hover:text-destructive"
          onClick={(e) => { e.preventDefault(); onDelete(n.id); }}
        >
          ✕
        </Button>
      </CardContent>
    </Card>
  );

  if (link) {
    return <Link to={link}>{cardContent}</Link>;
  }
  return cardContent;
}

const READ_FILTER_LABELS: Record<ReadFilter, string> = {
  all: 'Все',
  unread: 'Непрочитанные',
  read: 'Прочитанные',
};

export default function NotificationsPage() {
  const toast = useToast();
  const [data, setData] = useState<PagedResponse<NotificationDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [readFilter, setReadFilter] = useState<ReadFilter>('all');

  const isReadParam = readFilter === 'unread' ? false : readFilter === 'read' ? true : undefined;

  const load = (p: number, f: string, t: string, readParam: boolean | undefined) => {
    setLoading(true);
    getMyNotifications(p, 20, f || undefined, t || undefined, readParam)
      .then(setData)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(page, from, to, isReadParam); }, [page]);

  useEffect(() => {
    const handler = (e: Event) => {
      const n = (e as CustomEvent<NotificationDto>).detail;
      setData((prev) => {
        if (!prev) return prev;
        if (prev.items.some((item) => item.id === n.id)) return prev;
        return { ...prev, items: [n, ...prev.items] };
      });
    };
    window.addEventListener('new-notification', handler);
    return () => window.removeEventListener('new-notification', handler);
  }, []);

  const applyFilters = () => {
    setPage(1);
    load(1, from, to, isReadParam);
  };

  const resetFilters = () => {
    setFrom('');
    setTo('');
    setReadFilter('all');
    setPage(1);
    load(1, '', '', undefined);
  };

  const hasActiveFilters = from || to || readFilter !== 'all';

  const handleRead = async (id: string) => {
    try {
      await markAsRead(id);
      setData((prev) =>
        prev ? { ...prev, items: prev.items.map((n) => (n.id === id ? { ...n, isRead: true } : n)) } : prev,
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

  const handleDelete = async (id: string) => {
    try {
      await deleteNotification(id);
      setData((prev) =>
        prev ? { ...prev, items: prev.items.filter((n) => n.id !== id) } : prev,
      );
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const unreadCount = data?.items.filter((n) => !n.isRead).length ?? 0;

  return (
    <div>
      <div className="flex items-center justify-between mb-7 gap-4 flex-wrap">
        <h1 className="text-2xl font-bold">Уведомления</h1>
        <div className="flex gap-2">
          {unreadCount > 0 && (
            <Button variant="outline" size="sm" onClick={handleReadAll}>
              Прочитать все
            </Button>
          )}
          <Button
            variant="outline"
            size="sm"
            onClick={() => setFiltersOpen((o) => !o)}
            className={hasActiveFilters ? 'border-primary text-primary' : ''}
          >
            Фильтры {hasActiveFilters ? '●' : filtersOpen ? '▲' : '▼'}
          </Button>
        </div>
      </div>

      {filtersOpen && (
        <div className="mb-5 p-4 rounded-lg border bg-muted/30 flex flex-col gap-4">
          <div className="flex flex-wrap gap-3 items-end">
            <div className="flex flex-col gap-1">
              <Label htmlFor="from" className="text-xs text-muted-foreground">С</Label>
              <Input
                id="from"
                type="date"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
                className="w-40 h-8 text-sm"
              />
            </div>
            <div className="flex flex-col gap-1">
              <Label htmlFor="to" className="text-xs text-muted-foreground">По</Label>
              <Input
                id="to"
                type="date"
                value={to}
                onChange={(e) => setTo(e.target.value)}
                className="w-40 h-8 text-sm"
              />
            </div>
          </div>

          <div className="flex flex-col gap-1">
            <span className="text-xs text-muted-foreground">Статус</span>
            <div className="flex gap-2">
              {(['all', 'unread', 'read'] as ReadFilter[]).map((filter) => (
                <Button
                  key={filter}
                  variant={readFilter === filter ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setReadFilter(filter)}
                >
                  {READ_FILTER_LABELS[filter]}
                </Button>
              ))}
            </div>
          </div>

          <div className="flex gap-2">
            <Button size="sm" onClick={applyFilters}>Применить</Button>
            {hasActiveFilters && (
              <Button variant="ghost" size="sm" onClick={resetFilters}>Сбросить</Button>
            )}
          </div>
        </div>
      )}

      {loading && <p className="text-muted-foreground text-sm">Загрузка...</p>}

      {!loading && data?.items.length === 0 && (
        <p className="text-muted-foreground text-sm">Нет уведомлений</p>
      )}

      <div className="flex flex-col gap-2">
        {data?.items.map((n) => (
          <NotificationItem key={n.id} n={n} onRead={handleRead} onDelete={handleDelete} />
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
