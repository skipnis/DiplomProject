import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { createEvent, syncToGoogleCalendar } from '../api/events';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';

export default function NewEventPage() {
  const navigate = useNavigate();
  const toast = useToast();
  const { user } = useAuth();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [date, setDate] = useState('');
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    try {
      const result = await createEvent({ title: title.trim(), description: description.trim() || null, date });
      if (user?.isGoogleCalendarConnected) await syncToGoogleCalendar(result.id).catch(() => {});
      navigate(`/events/${result.id}`);
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="max-w-lg mx-auto">
      <div className="mb-7">
        <button className="text-sm text-muted-foreground hover:text-foreground mb-1" onClick={() => navigate('/events')}>← События</button>
        <h1 className="text-2xl font-extrabold tracking-tight">Новое событие</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="title">Название *</Label>
              <Input id="title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="День рождения Ани" maxLength={200} required />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="date">Дата *</Label>
              <Input id="date" type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="description">Описание</Label>
              <Textarea id="description" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Заметки..." rows={3} maxLength={2000} />
            </div>
            <div className="flex gap-2 justify-end mt-1">
              <Button type="button" variant="ghost" onClick={() => navigate('/events')}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Создание...' : 'Создать'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
