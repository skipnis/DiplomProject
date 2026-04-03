import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMyEvents } from '../api/events';
import EventCalendar from '../components/EventCalendar';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { EventDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';

function daysUntil(dateStr: string): number {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const d = new Date(dateStr);
  d.setHours(0, 0, 0, 0);
  return Math.ceil((d.getTime() - today.getTime()) / 86400000);
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
}

function pluralDays(n: number): string {
  const abs = Math.abs(n);
  if (abs % 10 === 1 && abs % 100 !== 11) return 'день';
  if ([2, 3, 4].includes(abs % 10) && ![12, 13, 14].includes(abs % 100)) return 'дня';
  return 'дней';
}

function CountdownBadge({ days }: { days: number }) {
  if (days === 0) return <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Сегодня!</Badge>;
  if (days > 0 && days <= 7) return <Badge className="bg-orange-100 text-orange-700 hover:bg-orange-100">через {days} {pluralDays(days)}</Badge>;
  if (days > 0) return <Badge variant="secondary">через {days} {pluralDays(days)}</Badge>;
  return <Badge variant="outline" className="text-muted-foreground">{Math.abs(days)} {pluralDays(days)} назад</Badge>;
}

function EventCard({ event, highlighted }: { event: EventDto; highlighted: boolean }) {
  const ref = useRef<HTMLAnchorElement>(null);
  useEffect(() => { if (highlighted && ref.current) ref.current.scrollIntoView({ behavior: 'smooth', block: 'nearest' }); }, [highlighted]);

  return (
    <Link
      ref={ref}
      to={`/events/${event.id}`}
      className={`block rounded-xl border bg-card p-4 hover:shadow-md hover:-translate-y-0.5 transition-all ${highlighted ? 'ring-2 ring-primary' : ''}`}
    >
      <div className="flex items-start justify-between gap-2 mb-1">
        <span className="font-bold text-sm leading-snug">{event.title}</span>
        <CountdownBadge days={daysUntil(event.date)} />
      </div>
      <span className="text-xs text-muted-foreground">{formatDate(event.date)}</span>
      {event.description && <p className="text-xs text-muted-foreground mt-1 line-clamp-2">{event.description}</p>}
      <div className="flex gap-1 mt-2 flex-wrap">
        {event.isLinkedToGoogleCalendar && <Badge variant="secondary" className="text-xs">Google Calendar</Badge>}
        {event.linkedWishlistId && <Badge variant="secondary" className="text-xs">Вишлист привязан</Badge>}
      </div>
    </Link>
  );
}

const PAGE_SIZE = 20;

export default function EventsPage() {
  const [events, setEvents] = useState<EventDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const listRef = useRef<HTMLDivElement>(null);
  const toast = useToast();

  const loadEvents = async (p: number) => {
    setLoading(true);
    try { const res = await getMyEvents(p, PAGE_SIZE); setEvents(res.items); setTotalPages(Math.ceil(res.totalCount / res.pageSize)); }
    catch (e) { toast.error(parseError(e)); }
    finally { setLoading(false); }
  };

  useEffect(() => { loadEvents(page); }, [page]);

  function handleDateSelect(date: string | null) {
    setSelectedDate(date);
    if (date && listRef.current) setTimeout(() => listRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' }), 50);
  }

  if (loading && events.length === 0) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  const filteredEvents = selectedDate ? events.filter((e) => e.date === selectedDate) : events;
  const upcoming = filteredEvents.filter((e) => daysUntil(e.date) >= 0);
  const past = filteredEvents.filter((e) => daysUntil(e.date) < 0);

  return (
    <div>
      <div className="flex items-center justify-between mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">События</h1>
        <Link to="/events/new" className={buttonVariants()}>+ Добавить</Link>
      </div>

      <div className="mb-6">
        <EventCalendar events={events} selectedDate={selectedDate} onDateSelect={handleDateSelect} />
      </div>

      <div ref={listRef}>
        {selectedDate && (
          <div className="flex items-center gap-3 mb-4">
            <span className="text-sm text-muted-foreground">
              {filteredEvents.length === 0 ? `Нет событий ${formatDate(selectedDate)}` : `События ${formatDate(selectedDate)}`}
            </span>
            <Button variant="ghost" size="sm" onClick={() => setSelectedDate(null)}>Показать все</Button>
          </div>
        )}

        {events.length === 0 && (
          <div className="text-center py-12">
            <div className="text-5xl mb-4">📅</div>
            <p className="font-semibold mb-2">Нет событий</p>
            <p className="text-sm text-muted-foreground mb-4">Добавьте день рождения или другое событие, чтобы не забыть.</p>
            <Link to="/events/new" className={buttonVariants({ size: 'sm' })}>Создать первое событие</Link>
          </div>
        )}

        {upcoming.length > 0 && (
          <section className="mb-6">
            {!selectedDate && <h2 className="text-base font-semibold mb-3">Предстоящие</h2>}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {upcoming.map((e) => <EventCard key={e.id} event={e} highlighted={e.date === selectedDate} />)}
            </div>
          </section>
        )}

        {past.length > 0 && (
          <section>
            {!selectedDate && <h2 className="text-base font-semibold text-muted-foreground mb-3">Прошедшие</h2>}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {past.map((e) => <EventCard key={e.id} event={e} highlighted={e.date === selectedDate} />)}
            </div>
          </section>
        )}

        {totalPages > 1 && !selectedDate && (
          <div className="flex items-center justify-center gap-3 mt-6">
            <Button variant="ghost" size="sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>← Назад</Button>
            <span className="text-sm text-muted-foreground">{page} / {totalPages}</span>
            <Button variant="ghost" size="sm" disabled={page === totalPages} onClick={() => setPage((p) => p + 1)}>Вперёд →</Button>
          </div>
        )}
      </div>
    </div>
  );
}
