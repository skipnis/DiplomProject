import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getEvent, updateEvent } from '../api/events';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { eventSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { FieldError } from '@/components/ui/field-error';

export default function EditEventPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [date, setDate] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    if (!id) return;
    getEvent(id)
      .then((ev) => { setTitle(ev.title); setDescription(ev.description ?? ''); setDate(ev.date); })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!id) return;
    const result = eventSchema.safeParse({ title, description: description || undefined, date });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    setErrors({});
    setSaving(true);
    try {
      await updateEvent(id, { title: title.trim(), description: description.trim() || null, date });
      navigate(`/events/${id}`);
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  return (
    <div className="max-w-lg mx-auto">
      <div className="mb-7">
        <button className="text-sm text-muted-foreground hover:text-foreground mb-1" onClick={() => navigate(`/events/${id}`)}>← Событие</button>
        <h1 className="text-2xl font-extrabold tracking-tight">Редактировать событие</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="title">Название *</Label>
              <Input
                id="title"
                value={title}
                onChange={(e) => { setTitle(e.target.value); clearError('title'); }}
                maxLength={200}
                aria-invalid={!!errors.title}
              />
              <FieldError message={errors.title} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="date">Дата *</Label>
              <Input
                id="date"
                type="date"
                value={date}
                onChange={(e) => { setDate(e.target.value); clearError('date'); }}
                aria-invalid={!!errors.date}
              />
              <FieldError message={errors.date} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="description">Описание</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => { setDescription(e.target.value); clearError('description'); }}
                rows={3}
                maxLength={2000}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>
            <div className="flex gap-2 justify-end mt-1">
              <Button type="button" variant="ghost" onClick={() => navigate(`/events/${id}`)}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
