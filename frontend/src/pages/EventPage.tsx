import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getEvent, deleteEvent, syncToGoogleCalendar, linkWishlist } from '../api/events';
import { getMyWishlists } from '../api/wishlists';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { EventDto, WishlistSummaryDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { daysUntil } from '@/lib/utils';

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
}

export default function EventPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const { user } = useAuth();

  const [event, setEvent] = useState<EventDto | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [linkingWishlist, setLinkingWishlist] = useState(false);
  const [showWishlistPicker, setShowWishlistPicker] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([getEvent(id), getMyWishlists()])
      .then(([ev, wls]) => { setEvent(ev); setWishlists(wls); })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  async function handleSync() {
    if (!id) return;
    setSyncing(true);
    try { await syncToGoogleCalendar(id); setEvent(await getEvent(id)); toast.success('Добавлено в Google Calendar'); }
    catch (e) { toast.error(parseError(e)); }
    finally { setSyncing(false); }
  }

  async function handleDelete() {
    if (!id || !window.confirm('Удалить это событие?')) return;
    setDeleting(true);
    try { await deleteEvent(id); navigate('/events'); }
    catch (e) { toast.error(parseError(e)); setDeleting(false); }
  }

  async function handleLinkWishlist(wishlistId: string | null) {
    if (!id) return;
    setLinkingWishlist(true);
    try {
      await linkWishlist(id, wishlistId);
      setEvent(await getEvent(id));
      setShowWishlistPicker(false);
      toast.success(wishlistId ? 'Вишлист привязан' : 'Вишлист отвязан');
    } catch (e) { toast.error(parseError(e)); }
    finally { setLinkingWishlist(false); }
  }

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!event) return <div className="text-center py-12 text-muted-foreground">Событие не найдено</div>;

  const days = daysUntil(event.date);
  const linkedWishlist = wishlists.find((w) => w.id === event.linkedWishlistId);

  return (
    <div className="max-w-2xl mx-auto">
      <div className="flex items-start justify-between mb-7 gap-4 flex-wrap">
        <div>
          <Link to="/events" className="text-sm text-muted-foreground hover:text-foreground">← События</Link>
          <h1 className="text-2xl font-extrabold tracking-tight mt-1">{event.title}</h1>
        </div>
        <div className="flex gap-2">
          <Link to={`/events/${id}/edit`} className={buttonVariants({ variant: 'secondary', size: 'sm' })}>Редактировать</Link>
          <Button variant="destructive" size="sm" onClick={handleDelete} disabled={deleting}>{deleting ? 'Удаление...' : 'Удалить'}</Button>
        </div>
      </div>

      <Card className="mb-4">
        <CardContent className="pt-6 flex flex-col gap-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Дата</span>
            <span className="text-sm font-medium">{formatDate(event.date)}</span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">До события</span>
            <span className="text-sm font-medium">
              {days === 0 && <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Сегодня!</Badge>}
              {days > 0 && `${days} дн.`}
              {days < 0 && <span className="text-muted-foreground">{Math.abs(days)} дн. назад</span>}
            </span>
          </div>
          {event.description && (
            <>
              <Separator />
              <div className="flex items-start justify-between gap-4">
                <span className="text-sm text-muted-foreground">Описание</span>
                <span className="text-sm text-right">{event.description}</span>
              </div>
            </>
          )}
          <Separator />
          <div className="flex items-center justify-between gap-4">
            <span className="text-sm text-muted-foreground">Google Calendar</span>
            <span>
              {event.isLinkedToGoogleCalendar
                ? <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Синхронизировано</Badge>
                : user?.isGoogleCalendarConnected
                ? <Button variant="secondary" size="sm" onClick={handleSync} disabled={syncing}>{syncing ? 'Добавление...' : 'Добавить в Google Calendar'}</Button>
                : <span className="text-sm text-muted-foreground">Не синхронизировано</span>
              }
            </span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <h2 className="font-semibold mb-3">Привязанный вишлист</h2>
          {linkedWishlist ? (
            <div className="flex items-center justify-between gap-3 mb-3">
              <Link to={`/wishlists/${linkedWishlist.id}`} className="text-sm font-medium hover:underline">
                {linkedWishlist.emoji && <span className="mr-1">{linkedWishlist.emoji}</span>}
                {linkedWishlist.name}
              </Link>
              <Button variant="ghost" size="sm" onClick={() => handleLinkWishlist(null)} disabled={linkingWishlist}>Отвязать</Button>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground mb-3">Вишлист не привязан.</p>
          )}

          {!showWishlistPicker ? (
            <Button variant="secondary" size="sm" onClick={() => setShowWishlistPicker(true)}>
              {linkedWishlist ? 'Изменить вишлист' : 'Привязать вишлист'}
            </Button>
          ) : (
            <div className="flex flex-col gap-1 mt-2">
              {wishlists.map((wl) => (
                <button
                  key={wl.id}
                  className={`flex items-center gap-2 px-3 py-2 rounded-md text-sm text-left hover:bg-muted transition-colors ${wl.id === event.linkedWishlistId ? 'bg-primary/10 text-primary font-medium' : ''}`}
                  onClick={() => handleLinkWishlist(wl.id)}
                  disabled={linkingWishlist}
                >
                  {wl.emoji && <span>{wl.emoji}</span>}
                  <span className="flex-1">{wl.name}</span>
                  {wl.id === event.linkedWishlistId && <Badge variant="secondary" className="text-xs">Текущий</Badge>}
                </button>
              ))}
              <Button variant="ghost" size="sm" className="mt-1 w-fit" onClick={() => setShowWishlistPicker(false)}>Отмена</Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
