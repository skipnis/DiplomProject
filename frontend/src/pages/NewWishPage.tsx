import { useState, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { addWish, parseWishUrl, uploadWishImage } from '../api/wishes';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { wishSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { parseApiFieldErrors } from '../utils/errors';
import type { WishPriority, Currency } from '../types';
import { CURRENCY_LABELS, PRIORITY_LABELS } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { FieldError } from '@/components/ui/field-error';

export default function NewWishPage() {
  const { id: wishlistId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [currency, setCurrency] = useState<Currency>(0);
  const [priority, setPriority] = useState<WishPriority>(0);
  const [url, setUrl] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [externalImageUrl, setExternalImageUrl] = useState<string | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const [parsing, setParsing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleParseUrl = async () => {
    if (!url.trim()) return;
    setParsing(true);
    try {
      const data = await parseWishUrl(url.trim());
      if (data.name) setName(data.name);
      if (data.description) setDescription(data.description);
      if (data.price != null) setPrice(String(data.price));
      if (data.externalImageUrl) { setExternalImageUrl(data.externalImageUrl); setImageFile(null); setImagePreview(data.externalImageUrl); }
    } catch (e) { toast.error(parseError(e)); }
    finally { setParsing(false); }
  };

  const handleFileChange = useCallback((file: File) => {
    setImageFile(file);
    setExternalImageUrl(null);
    const reader = new FileReader();
    reader.onload = (e) => setImagePreview(e.target?.result as string);
    reader.readAsDataURL(file);
  }, []);

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    if (file?.type.startsWith('image/')) handleFileChange(file);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!wishlistId) return;
    const result = wishSchema.safeParse({ name, description: description || undefined, url: url || undefined, price: price || undefined });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    setErrors({});
    setSaving(true);
    try {
      const res = await addWish(wishlistId, { name, description: description || null, price: price ? Number(price) : null, currency: price ? currency : null, priority, url: url || null });
      try {
        if (imageFile) await uploadWishImage(wishlistId, res.wishId, imageFile);
        else if (externalImageUrl) await uploadWishImage(wishlistId, res.wishId, externalImageUrl);
      } catch { toast.warning('Желание добавлено, но изображение не удалось загрузить'); }
      navigate(`/wishlists/${wishlistId}`);
    } catch (e) {
      const fieldErrors = parseApiFieldErrors(e);
      if (fieldErrors) setErrors((prev) => ({ ...prev, ...fieldErrors }));
      else toast.error(parseError(e));
    } finally { setSaving(false); }
  };

  return (
    <div className="max-w-xl mx-auto">
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Новое желание</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="url">Ссылка на товар</Label>
              <div className="flex gap-2">
                <Input
                  id="url"
                  type="url"
                  value={url}
                  onChange={(e) => { setUrl(e.target.value); clearError('url'); }}
                  placeholder="https://..."
                  aria-invalid={!!errors.url}
                />
                <Button type="button" variant="secondary" onClick={handleParseUrl} disabled={parsing || !url.trim()}>
                  {parsing ? '...' : 'Загрузить'}
                </Button>
              </div>
              <FieldError message={errors.url} />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label>Изображение</Label>
              <div
                className={`border-2 border-dashed rounded-lg p-6 text-center cursor-pointer transition-colors ${dragOver ? 'border-primary bg-primary/5' : 'border-muted-foreground/30 hover:border-primary/50'}`}
                onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
                onDragLeave={() => setDragOver(false)}
                onDrop={handleDrop}
                onClick={() => fileInputRef.current?.click()}
              >
                {imagePreview ? (
                  <>
                    <img src={imagePreview} alt="preview" className="max-h-40 mx-auto mb-2 rounded" />
                    <p className="text-xs text-muted-foreground">Нажми для замены</p>
                  </>
                ) : (
                  <>
                    <p className="text-sm">📸 Перетащи или нажми для загрузки</p>
                    <p className="text-xs text-muted-foreground mt-1">PNG, JPG, WebP · до 10 МБ</p>
                  </>
                )}
              </div>
              <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={(e) => { const f = e.target.files?.[0]; if (f) handleFileChange(f); }} />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="name">Название *</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => { setName(e.target.value); clearError('name'); }}
                placeholder="Что хочешь?"
                aria-invalid={!!errors.name}
              />
              <FieldError message={errors.name} />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="description">Описание</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => { setDescription(e.target.value); clearError('description'); }}
                placeholder="Подробнее о желании..."
                rows={3}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="price">Цена</Label>
                <Input
                  id="price"
                  type="number"
                  value={price}
                  onChange={(e) => { setPrice(e.target.value); clearError('price'); }}
                  placeholder="0"
                  min="0"
                  step="0.01"
                  aria-invalid={!!errors.price}
                />
                <FieldError message={errors.price} />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label>Валюта</Label>
                <Select value={String(currency)} onValueChange={(v) => setCurrency(Number(v) as Currency)}>
                  <SelectTrigger><SelectValue>{CURRENCY_LABELS[currency]}</SelectValue></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">BYN</SelectItem>
                    <SelectItem value="1">RUB</SelectItem>
                    <SelectItem value="2">USD</SelectItem>
                    <SelectItem value="3">EUR</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="flex flex-col gap-1.5">
              <Label>Приоритет</Label>
              <Select value={String(priority)} onValueChange={(v) => setPriority(Number(v) as WishPriority)}>
                <SelectTrigger><SelectValue>{PRIORITY_LABELS[priority]}</SelectValue></SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">Без приоритета</SelectItem>
                  <SelectItem value="1">Неплохо бы</SelectItem>
                  <SelectItem value="2">Хочу</SelectItem>
                  <SelectItem value="3">Очень хочу</SelectItem>
                  <SelectItem value="4">Мечта</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex gap-2 justify-end mt-1">
              <Button type="button" variant="ghost" onClick={() => navigate(`/wishlists/${wishlistId}`)}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Добавить'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
