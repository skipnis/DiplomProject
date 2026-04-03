import { useState } from 'react';
import { Button } from '@/components/ui/button';
import type { EventDto } from '../types';

interface Props {
  events: EventDto[];
  selectedDate: string | null;
  onDateSelect: (date: string | null) => void;
}

const WEEKDAYS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];
const MONTHS = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'];

function toYMD(date: Date): string {
  return date.toISOString().slice(0, 10);
}

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getMondayBasedWeekday(date: Date): number {
  return (date.getDay() + 6) % 7;
}

export default function EventCalendar({ events, selectedDate, onDateSelect }: Props) {
  const today = new Date();
  const [viewYear, setViewYear] = useState(today.getFullYear());
  const [viewMonth, setViewMonth] = useState(today.getMonth());

  const eventDates = new Set(events.map((e) => e.date));
  const daysInMonth = getDaysInMonth(viewYear, viewMonth);
  const firstWeekday = getMondayBasedWeekday(new Date(viewYear, viewMonth, 1));
  const todayStr = toYMD(today);

  const cells: (number | null)[] = [
    ...Array(firstWeekday).fill(null),
    ...Array.from({ length: daysInMonth }, (_, i) => i + 1),
  ];
  while (cells.length % 7 !== 0) cells.push(null);

  function prevMonth() {
    if (viewMonth === 0) { setViewYear(y => y - 1); setViewMonth(11); }
    else setViewMonth(m => m - 1);
  }

  function nextMonth() {
    if (viewMonth === 11) { setViewYear(y => y + 1); setViewMonth(0); }
    else setViewMonth(m => m + 1);
  }

  function handleDayClick(day: number) {
    const dateStr = `${viewYear}-${String(viewMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    onDateSelect(selectedDate === dateStr ? null : dateStr);
  }

  return (
    <div className="rounded-xl border bg-card p-4">
      <div className="flex items-center justify-between mb-3">
        <Button variant="ghost" size="sm" onClick={prevMonth} type="button">‹</Button>
        <span className="font-semibold text-sm">{MONTHS[viewMonth]} {viewYear}</span>
        <Button variant="ghost" size="sm" onClick={nextMonth} type="button">›</Button>
      </div>

      <div className="grid grid-cols-7 gap-0.5">
        {WEEKDAYS.map((d) => (
          <div key={d} className="text-center text-xs text-muted-foreground font-medium py-1">{d}</div>
        ))}
        {cells.map((day, i) => {
          if (!day) return <div key={`e-${i}`} />;
          const dateStr = `${viewYear}-${String(viewMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
          const hasEvent = eventDates.has(dateStr);
          const isToday = dateStr === todayStr;
          const isSelected = dateStr === selectedDate;

          return (
            <button
              key={dateStr}
              type="button"
              onClick={() => handleDayClick(day)}
              className={`relative flex flex-col items-center justify-center h-9 w-full rounded-md text-sm font-medium transition-colors ${
                isSelected
                  ? 'bg-primary text-primary-foreground'
                  : isToday
                  ? 'bg-primary/10 text-primary font-bold'
                  : 'hover:bg-muted text-foreground'
              }`}
            >
              <span>{day}</span>
              {hasEvent && (
                <span className={`absolute bottom-1 left-1/2 -translate-x-1/2 h-1 w-1 rounded-full ${isSelected ? 'bg-primary-foreground' : 'bg-primary'}`} />
              )}
            </button>
          );
        })}
      </div>
    </div>
  );
}
