import { useEffect, useState, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getWish, updateWish, uploadWishImage, deleteWishImage } from '../api/wishes';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { wishSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import type { WishPriority, Currency } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { FieldError } from '@/components/ui/field-error';

export default function EditWishPage() {
  const { id: wishlistId, wishId } = useParams<{ id: string; wishId: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [currency, setCurrency] = useState<Currency>(0);
  const [priority, setPriority] = useState<WishPriority>(0);
  const [url, setUrl] = useState('');
  const [currentImagePath, setCurrentImagePath] = useState<string | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const [removeImage, setRemoveImage] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!wishlistId || !wishId) return;
    getWish(wishlistId, wishId)
      .then((w) => {
        setName(w.name);
        setDescription(w.description ?? '');
        setPrice(w.price != null ? String(w.price) : '');
        setCurrency(w.currency ?? 0);
        setPriority(w.priority);
        setUrl(w.url ?? '');
        setCurrentImagePath(w.imagePath);
        if (w.imagePath) setImagePreview(getImageUrl(w.imagePath));
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [wishlistId, wishId]);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleFileChange = useCallback((file: File) => {
    setImageFile(file);
    setRemoveImage(false);
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
    if (!wishlistId || !wishId) return;
    const result = wishSchema.safeParse({ name, description: description || undefined, url: url || undefined, price: price || undefined });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    setErrors({});
    setSaving(true);
    try {
      await updateWish(wishlistId, wishId, { name, description: description || null, price: price ? Number(price) : null, currency: price ? currency : null, priority, url: url || null });
      if (removeImage && currentImagePath) await deleteWishImage(wishlistId, wishId);
      else if (imageFile) {
        try { await uploadWishImage(wishlistId, wishId, imageFile); }
        catch { toast.error('Желание сохранено, но изображение не удалось загрузить'); }
      }
      navigate(`/wishlists/${wishlistId}/wishes/${wishId}`);
    } catch (e) { toast.error(parseError(e)); }
    finally { setSaving(false); }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  return (
    <div className="max-w-xl mx-auto">
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Редактировать желание</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
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
              {(imagePreview || currentImagePath) && !removeImage && (
                <Button type="button" variant="destructive" size="sm" className="w-fit" onClick={() => { setRemoveImage(true); setImageFile(null); setImagePreview(null); }}>
                  Удалить изображение
                </Button>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="name">Название *</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => { setName(e.target.value); clearError('name'); }}
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
                rows={3}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="url">Ссылка</Label>
              <Input
                id="url"
                type="url"
                value={url}
                onChange={(e) => { setUrl(e.target.value); clearError('url'); }}
                placeholder="https://..."
                aria-invalid={!!errors.url}
              />
              <FieldError message={errors.url} />
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
                  <SelectTrigger><SelectValue /></SelectTrigger>
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
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">Без приоритета</SelectItem>
                  <SelectItem value="1">🟢 Низкий</SelectItem>
                  <SelectItem value="2">🟡 Средний</SelectItem>
                  <SelectItem value="3">🔴 Высокий</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex gap-2 justify-end mt-1">
              <Button type="button" variant="ghost" onClick={() => navigate(`/wishlists/${wishlistId}/wishes/${wishId}`)}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
