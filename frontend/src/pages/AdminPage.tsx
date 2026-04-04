import { useEffect, useState } from 'react';
import {
  adminGetCategories, adminCreateCategory, adminUpdateCategory, adminDeleteCategory,
  adminGetAllItems, adminCreateItem, adminUpdateItem, adminDeleteItem,
  adminGetAllCollections, adminCreateCollection, adminUpdateCollection, adminDeleteCollection,
  adminAddItemToCollection, adminRemoveItemFromCollection, adminGetCollectionItems,
  adminUploadItemImage, adminUploadCollectionImage, adminParseUrl, adminSetItemPublished,
} from '../api/admin';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { catalogItemSchema, catalogCategorySchema, catalogCollectionSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { OCCASION_LABELS } from '../types';
import type { CatalogCategoryDto, CatalogCollectionAdminDto, CatalogItemDto, PagedResponse } from '../types';
import { getImageUrl } from '../api/client';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { FieldError } from '@/components/ui/field-error';

type ItemFormValues = { name: string; description: string; price: string; currency: string; imagePath: string; url: string; categoryId: string; isPublished: boolean };
type CollectionFormValues = { name: string; description: string; occasion: string; coverImagePath: string; order: string; isPublished: boolean };

function ItemForm({ form, setForm, categories, itemImageFile, setItemImageFile, externalImageUrl, setExternalImageUrl, onSubmit, submitLabel, onCancel, onParseUrl, parsing, errors, clearError }: {
  form: ItemFormValues; setForm: (f: ItemFormValues) => void; categories: CatalogCategoryDto[];
  itemImageFile: File | null; setItemImageFile: (f: File | null) => void;
  externalImageUrl: string | null; setExternalImageUrl: (u: string | null) => void;
  onSubmit: (e: React.FormEvent) => void; submitLabel: string; onCancel: () => void;
  onParseUrl: () => void; parsing: boolean;
  errors: FormErrors; clearError: (field: string) => void;
}) {
  const imagePreview = itemImageFile ? URL.createObjectURL(itemImageFile) : externalImageUrl ?? (form.imagePath ? getImageUrl(form.imagePath) : null);
  return (
    <Card className="mb-6">
      <CardContent className="pt-6">
        <form onSubmit={onSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label>Ссылка на товар</Label>
            <div className="flex gap-2">
              <Input
                value={form.url}
                onChange={(e) => { setForm({ ...form, url: e.target.value }); clearError('url'); }}
                placeholder="https://..."
                aria-invalid={!!errors.url}
              />
              <Button type="button" variant="secondary" onClick={onParseUrl} disabled={parsing || !form.url.trim()}>{parsing ? '...' : 'Загрузить'}</Button>
            </div>
            <FieldError message={errors.url} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <Label>Название *</Label>
              <Input
                value={form.name}
                onChange={(e) => { setForm({ ...form, name: e.target.value }); clearError('name'); }}
                aria-invalid={!!errors.name}
              />
              <FieldError message={errors.name} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Категория *</Label>
              <Select value={form.categoryId} onValueChange={(v) => { setForm({ ...form, categoryId: v ?? '' }); clearError('categoryId'); }}>
                <SelectTrigger aria-invalid={!!errors.categoryId}><SelectValue placeholder="Выберите..." /></SelectTrigger>
                <SelectContent>{categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}</SelectContent>
              </Select>
              <FieldError message={errors.categoryId} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Цена</Label>
              <Input
                type="number"
                value={form.price}
                onChange={(e) => { setForm({ ...form, price: e.target.value }); clearError('price'); }}
                aria-invalid={!!errors.price}
              />
              <FieldError message={errors.price} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Валюта</Label>
              <Input value="BYN" readOnly />
            </div>
            <div className="flex flex-col gap-1.5 col-span-2">
              <Label>Изображение</Label>
              {imagePreview && <img src={imagePreview} alt="" className="h-20 rounded object-cover mb-1" />}
              <input type="file" accept="image/*" onChange={(e) => { setItemImageFile(e.target.files?.[0] ?? null); setExternalImageUrl(null); }} />
            </div>
            <div className="flex flex-col gap-1.5 col-span-2">
              <Label>Описание</Label>
              <Textarea
                value={form.description}
                onChange={(e) => { setForm({ ...form, description: e.target.value }); clearError('description'); }}
                rows={3}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>
          </div>
          <div className="flex gap-2">
            <Button type="submit">{submitLabel}</Button>
            <Button type="button" variant="ghost" onClick={onCancel}>Отмена</Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

function CollectionForm({ form, setForm, occasions, collectionImageFile, setCollectionImageFile, isEdit, onSubmit, submitLabel, onCancel, errors, clearError }: {
  form: CollectionFormValues; setForm: (f: CollectionFormValues) => void; occasions: [string, string][];
  collectionImageFile: File | null; setCollectionImageFile: (f: File | null) => void;
  isEdit?: boolean; onSubmit: (e: React.FormEvent) => void; submitLabel: string; onCancel: () => void;
  errors: FormErrors; clearError: (field: string) => void;
}) {
  return (
    <Card className="mb-6">
      <CardContent className="pt-6">
        <form onSubmit={onSubmit} className="flex flex-col gap-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <Label>Название *</Label>
              <Input
                value={form.name}
                onChange={(e) => { setForm({ ...form, name: e.target.value }); clearError('name'); }}
                aria-invalid={!!errors.name}
              />
              <FieldError message={errors.name} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Повод</Label>
              <Select value={form.occasion || '__none__'} onValueChange={(v) => setForm({ ...form, occasion: v == null || v === '__none__' ? '' : v })}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">—</SelectItem>
                  {occasions.map(([key, label]) => <SelectItem key={key} value={key}>{label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Порядок *</Label>
              <Input
                type="number"
                value={form.order}
                onChange={(e) => { setForm({ ...form, order: e.target.value }); clearError('order'); }}
                aria-invalid={!!errors.order}
              />
              <FieldError message={errors.order} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Обложка</Label>
              {form.coverImagePath && !collectionImageFile && <img src={getImageUrl(form.coverImagePath) ?? undefined} alt="" className="h-14 rounded mb-1 object-cover" />}
              <input type="file" accept="image/*" onChange={(e) => setCollectionImageFile(e.target.files?.[0] ?? null)} />
            </div>
            <div className="flex flex-col gap-1.5 col-span-2">
              <Label>Описание</Label>
              <Textarea
                value={form.description}
                onChange={(e) => { setForm({ ...form, description: e.target.value }); clearError('description'); }}
                rows={2}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>
            {isEdit && (
              <div className="flex items-center gap-2 col-span-2">
                <input type="checkbox" id="colPublished" checked={form.isPublished} onChange={(e) => setForm({ ...form, isPublished: e.target.checked })} />
                <label htmlFor="colPublished" className="text-sm font-medium">Опубликована</label>
              </div>
            )}
          </div>
          <div className="flex gap-2">
            <Button type="submit">{submitLabel}</Button>
            <Button type="button" variant="ghost" onClick={onCancel}>Отмена</Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

export default function AdminPage() {
  return (
    <div>
      <div className="flex items-center justify-between mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Администрирование каталога</h1>
        <Button variant="ghost" size="sm" onClick={() => { localStorage.removeItem('admin_token'); window.location.href = '/admin/login'; }}>Выйти</Button>
      </div>
      <Tabs defaultValue="categories">
        <TabsList className="mb-6">
          <TabsTrigger value="categories">Категории</TabsTrigger>
          <TabsTrigger value="items">Товары</TabsTrigger>
          <TabsTrigger value="collections">Подборки</TabsTrigger>
        </TabsList>
        <TabsContent value="categories"><CategoriesTab /></TabsContent>
        <TabsContent value="items"><ItemsTab /></TabsContent>
        <TabsContent value="collections"><CollectionsTab /></TabsContent>
      </Tabs>
    </div>
  );
}

function CategoriesTab() {
  const toast = useToast();
  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [newName, setNewName] = useState('');
  const [newOrder, setNewOrder] = useState('0');
  const [newErrors, setNewErrors] = useState<FormErrors>({});
  const [editId, setEditId] = useState<string | null>(null);
  const [editName, setEditName] = useState('');
  const [editOrder, setEditOrder] = useState('0');
  const [editErrors, setEditErrors] = useState<FormErrors>({});

  const load = () => { setLoading(true); adminGetCategories().then(setCategories).catch((e) => toast.error(parseError(e))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = catalogCategorySchema.safeParse({ name: newName, order: newOrder });
    if (!result.success) { setNewErrors(parseZodErrors(result.error)); return; }
    setNewErrors({});
    try { await adminCreateCategory({ name: newName, order: Number(newOrder) }); setNewName(''); setNewOrder('0'); load(); } catch (e) { toast.error(parseError(e)); }
  };

  const handleUpdate = async (id: string) => {
    const result = catalogCategorySchema.safeParse({ name: editName, order: editOrder });
    if (!result.success) { setEditErrors(parseZodErrors(result.error)); return; }
    setEditErrors({});
    try { await adminUpdateCategory(id, { name: editName, order: Number(editOrder) }); setEditId(null); load(); } catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      <form onSubmit={handleCreate} className="flex gap-3 mb-6 items-start">
        <div className="flex flex-col gap-1.5 flex-1">
          <Label>Название *</Label>
          <Input value={newName} onChange={(e) => { setNewName(e.target.value); if (newErrors.name) setNewErrors((p) => ({ ...p, name: '' })); }} aria-invalid={!!newErrors.name} />
          <FieldError message={newErrors.name} />
        </div>
        <div className="flex flex-col gap-1.5 w-24">
          <Label>Порядок *</Label>
          <Input type="number" value={newOrder} onChange={(e) => { setNewOrder(e.target.value); if (newErrors.order) setNewErrors((p) => ({ ...p, order: '' })); }} aria-invalid={!!newErrors.order} />
          <FieldError message={newErrors.order} />
        </div>
        <Button type="submit" className="mt-6">Создать</Button>
      </form>
      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : (
        <Card>
          <CardContent className="p-0">
            <table className="w-full border-collapse text-sm">
              <thead><tr className="border-b"><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Название</th><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Порядок</th><th className="p-3"></th></tr></thead>
              <tbody>
                {categories.map((c) => (
                  <tr key={c.id} className="border-b last:border-0">
                    <td className="p-3">
                      {editId === c.id ? (
                        <>
                          <Input value={editName} onChange={(e) => { setEditName(e.target.value); if (editErrors.name) setEditErrors((p) => ({ ...p, name: '' })); }} className="h-7" aria-invalid={!!editErrors.name} />
                          <FieldError message={editErrors.name} />
                        </>
                      ) : c.name}
                    </td>
                    <td className="p-3">
                      {editId === c.id ? (
                        <>
                          <Input type="number" value={editOrder} onChange={(e) => { setEditOrder(e.target.value); if (editErrors.order) setEditErrors((p) => ({ ...p, order: '' })); }} className="h-7 w-20" aria-invalid={!!editErrors.order} />
                          <FieldError message={editErrors.order} />
                        </>
                      ) : c.order}
                    </td>
                    <td className="p-3 text-right">
                      {editId === c.id ? (
                        <div className="flex gap-2 justify-end">
                          <Button size="sm" onClick={() => handleUpdate(c.id)}>Сохранить</Button>
                          <Button size="sm" variant="ghost" onClick={() => { setEditId(null); setEditErrors({}); }}>Отмена</Button>
                        </div>
                      ) : (
                        <div className="flex gap-2 justify-end">
                          <Button size="sm" variant="ghost" onClick={() => { setEditId(c.id); setEditName(c.name); setEditOrder(String(c.order)); setEditErrors({}); }}>Изменить</Button>
                          <Button size="sm" variant="ghost" className="text-destructive" onClick={async () => { if (!confirm('Удалить?')) return; try { await adminDeleteCategory(c.id); load(); } catch (e) { toast.error(parseError(e)); } }}>Удалить</Button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function ItemsTab() {
  const toast = useToast();
  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [data, setData] = useState<PagedResponse<CatalogItemDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [editItem, setEditItem] = useState<CatalogItemDto | null>(null);
  const [form, setForm] = useState<ItemFormValues>({ name: '', description: '', price: '', currency: '0', imagePath: '', url: '', categoryId: '', isPublished: false });
  const [itemImageFile, setItemImageFile] = useState<File | null>(null);
  const [externalImageUrl, setExternalImageUrl] = useState<string | null>(null);
  const [parsing, setParsing] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});

  const clearError = (field: string) => { if (errors[field]) setErrors((p) => ({ ...p, [field]: '' })); };

  useEffect(() => { adminGetCategories().then(setCategories).catch(() => {}); }, []);
  const load = (p: number) => { setLoading(true); adminGetAllItems({ page: p }).then(setData).catch(() => {}).finally(() => setLoading(false)); };
  useEffect(() => { load(page); }, [page]);

  const resetForm = () => { setForm({ name: '', description: '', price: '', currency: '0', imagePath: '', url: '', categoryId: '', isPublished: false }); setItemImageFile(null); setExternalImageUrl(null); setErrors({}); };

  const handleParseUrl = async () => {
    if (!form.url.trim()) return;
    setParsing(true);
    try {
      const d = await adminParseUrl(form.url.trim());
      if (d.name) setForm((f) => ({ ...f, name: d.name! }));
      if (d.description) setForm((f) => ({ ...f, description: d.description! }));
      if (d.price != null) setForm((f) => ({ ...f, price: String(d.price) }));
      if (d.externalImageUrl) { setExternalImageUrl(d.externalImageUrl); setItemImageFile(null); }
    } catch (e) { toast.error(parseError(e)); }
    finally { setParsing(false); }
  };

  const validate = () => {
    const result = catalogItemSchema.safeParse({ name: form.name, description: form.description || undefined, url: form.url || undefined, price: form.price || undefined, categoryId: form.categoryId });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return false; }
    setErrors({});
    return true;
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    try {
      const newId = await adminCreateItem({ name: form.name, description: form.description || null, price: form.price ? Number(form.price) : null, currency: form.currency ? (Number(form.currency) as never) : null, imagePath: null, url: form.url || null, categoryId: form.categoryId });
      if (itemImageFile) await adminUploadItemImage(newId, itemImageFile);
      else if (externalImageUrl) await adminUploadItemImage(newId, externalImageUrl);
      resetForm(); setShowCreate(false); load(page);
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editItem || !validate()) return;
    try {
      await adminUpdateItem(editItem.id, { name: form.name, description: form.description || null, price: form.price ? Number(form.price) : null, currency: form.currency ? (Number(form.currency) as never) : null, imagePath: form.imagePath || null, url: form.url || null, categoryId: form.categoryId, isPublished: form.isPublished });
      if (itemImageFile) await adminUploadItemImage(editItem.id, itemImageFile);
      else if (externalImageUrl) await adminUploadItemImage(editItem.id, externalImageUrl);
      setEditItem(null); resetForm(); load(page);
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleCancel = () => { setShowCreate(false); setEditItem(null); resetForm(); };

  return (
    <div>
      {!showCreate && !editItem && <Button className="mb-4" onClick={() => { setShowCreate(true); resetForm(); }}>Добавить товар</Button>}
      {showCreate && <ItemForm form={form} setForm={setForm} categories={categories} itemImageFile={itemImageFile} setItemImageFile={setItemImageFile} externalImageUrl={externalImageUrl} setExternalImageUrl={setExternalImageUrl} onSubmit={handleCreate} submitLabel="Создать" onCancel={handleCancel} onParseUrl={handleParseUrl} parsing={parsing} errors={errors} clearError={clearError} />}
      {editItem && <ItemForm form={form} setForm={setForm} categories={categories} itemImageFile={itemImageFile} setItemImageFile={setItemImageFile} externalImageUrl={externalImageUrl} setExternalImageUrl={setExternalImageUrl} onSubmit={handleUpdate} submitLabel="Сохранить" onCancel={handleCancel} onParseUrl={handleParseUrl} parsing={parsing} errors={errors} clearError={clearError} />}

      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : !data || data.items.length === 0 ? <div className="text-muted-foreground text-sm">Нет товаров</div> : (
        <>
          <Card><CardContent className="p-0">
            <table className="w-full border-collapse text-sm">
              <thead><tr className="border-b">{['Название','Категория','Цена','Статус',''].map((h) => <th key={h} className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">{h}</th>)}</tr></thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id} className="border-b last:border-0">
                    <td className="p-3 font-medium">{item.name}</td>
                    <td className="p-3 text-muted-foreground">{item.categoryName}</td>
                    <td className="p-3">{item.price !== null ? `${item.price} ${item.currency ?? ''}` : '—'}</td>
                    <td className="p-3"><Badge variant={item.isPublished ? 'default' : 'secondary'}>{item.isPublished ? 'Опубликован' : 'Черновик'}</Badge></td>
                    <td className="p-3 text-right">
                      <div className="flex gap-1 justify-end">
                        <Button size="sm" variant="ghost" onClick={async () => { try { await adminSetItemPublished(item.id, !item.isPublished); load(page); } catch (e) { toast.error(parseError(e)); } }}>{item.isPublished ? 'Снять' : 'Опубликовать'}</Button>
                        <Button size="sm" variant="ghost" onClick={() => { setEditItem(item); setShowCreate(false); setForm({ name: item.name, description: item.description ?? '', price: item.price !== null ? String(item.price) : '', currency: '0', imagePath: item.imagePath ?? '', url: item.url ?? '', categoryId: item.categoryId, isPublished: item.isPublished }); setErrors({}); }}>Изменить</Button>
                        <Button size="sm" variant="ghost" className="text-destructive" onClick={async () => { if (!confirm('Удалить товар?')) return; try { await adminDeleteItem(item.id); load(page); } catch (e) { toast.error(parseError(e)); } }}>Удалить</Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent></Card>
          <div className="flex items-center justify-center gap-3 mt-4">
            <Button variant="ghost" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>Назад</Button>
            <span className="text-sm text-muted-foreground">{page} / {Math.ceil(data.totalCount / data.pageSize)}</span>
            <Button variant="ghost" size="sm" disabled={!data.hasNextPage} onClick={() => setPage((p) => p + 1)}>Вперёд</Button>
          </div>
        </>
      )}
    </div>
  );
}

function CollectionsTab() {
  const toast = useToast();
  const [collections, setCollections] = useState<CatalogCollectionAdminDto[]>([]);
  const [allItems, setAllItems] = useState<CatalogItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [editCollection, setEditCollection] = useState<CatalogCollectionAdminDto | null>(null);
  const [selectedCollection, setSelectedCollection] = useState<CatalogCollectionAdminDto | null>(null);
  const [addItemId, setAddItemId] = useState('');
  const [form, setForm] = useState<CollectionFormValues>({ name: '', description: '', occasion: '', coverImagePath: '', order: '0', isPublished: false });
  const [collectionImageFile, setCollectionImageFile] = useState<File | null>(null);
  const [errors, setErrors] = useState<FormErrors>({});
  const clearError = (field: string) => { if (errors[field]) setErrors((p) => ({ ...p, [field]: '' })); };
  const OCCASIONS = Object.entries(OCCASION_LABELS) as [string, string][];

  const load = () => { setLoading(true); adminGetAllCollections().then(setCollections).catch(() => {}).finally(() => setLoading(false)); };
  useEffect(() => { load(); adminGetAllItems({ pageSize: 200 }).then((r) => setAllItems(r.items)).catch(() => {}); }, []);

  const resetForm = () => { setForm({ name: '', description: '', occasion: '', coverImagePath: '', order: '0', isPublished: false }); setCollectionImageFile(null); setErrors({}); };
  const handleCancel = () => { setShowCreate(false); setEditCollection(null); resetForm(); };

  const validate = () => {
    const result = catalogCollectionSchema.safeParse({ name: form.name, description: form.description || undefined, order: form.order });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return false; }
    setErrors({});
    return true;
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    try {
      const newId = await adminCreateCollection({ name: form.name, description: form.description || null, occasion: form.occasion || null, coverImagePath: null, order: Number(form.order) });
      if (collectionImageFile) await adminUploadCollectionImage(newId, collectionImageFile);
      resetForm(); setShowCreate(false); load(); toast.success('Подборка создана');
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editCollection || !validate()) return;
    try {
      await adminUpdateCollection(editCollection.id, { name: form.name, description: form.description || null, occasion: form.occasion || null, coverImagePath: form.coverImagePath || null, order: Number(form.order), isPublished: form.isPublished });
      if (collectionImageFile) await adminUploadCollectionImage(editCollection.id, collectionImageFile);
      setEditCollection(null); resetForm(); load(); toast.success('Подборка обновлена');
    } catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      {!showCreate && !editCollection && <Button className="mb-4" onClick={() => { setShowCreate(true); resetForm(); }}>Создать подборку</Button>}
      {showCreate && <CollectionForm form={form} setForm={setForm} occasions={OCCASIONS} collectionImageFile={collectionImageFile} setCollectionImageFile={setCollectionImageFile} onSubmit={handleCreate} submitLabel="Создать" onCancel={handleCancel} errors={errors} clearError={clearError} />}
      {editCollection && <CollectionForm form={form} setForm={setForm} occasions={OCCASIONS} collectionImageFile={collectionImageFile} setCollectionImageFile={setCollectionImageFile} isEdit onSubmit={handleUpdate} submitLabel="Сохранить" onCancel={handleCancel} errors={errors} clearError={clearError} />}

      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : collections.length === 0 ? <div className="text-muted-foreground text-sm">Нет подборок</div> : (
        <Card><CardContent className="p-0">
          <table className="w-full border-collapse text-sm">
            <thead><tr className="border-b">{['Название','Повод','Товаров','Статус',''].map((h) => <th key={h} className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">{h}</th>)}</tr></thead>
            <tbody>
              {collections.map((c) => (
                <>
                  <tr key={c.id} className="border-b">
                    <td className="p-3 font-medium">{c.name}</td>
                    <td className="p-3 text-muted-foreground">{c.occasion ? (OCCASION_LABELS[c.occasion] ?? c.occasion) : '—'}</td>
                    <td className="p-3">{c.itemCount}</td>
                    <td className="p-3"><Badge variant={c.isPublished ? 'default' : 'secondary'}>{c.isPublished ? 'Опубликована' : 'Черновик'}</Badge></td>
                    <td className="p-3 text-right">
                      <div className="flex gap-1 justify-end">
                        <Button size="sm" variant="ghost" onClick={() => setSelectedCollection(selectedCollection?.id === c.id ? null : c)}>{selectedCollection?.id === c.id ? 'Свернуть' : 'Товары'}</Button>
                        <Button size="sm" variant="ghost" onClick={() => { setEditCollection(c); setShowCreate(false); setForm({ name: c.name, description: c.description ?? '', occasion: c.occasion ?? '', coverImagePath: c.coverImagePath ?? '', order: String(c.order), isPublished: c.isPublished }); setErrors({}); }}>Изменить</Button>
                        <Button size="sm" variant="ghost" className="text-destructive" onClick={async () => { if (!confirm('Удалить подборку?')) return; try { await adminDeleteCollection(c.id); if (selectedCollection?.id === c.id) setSelectedCollection(null); load(); } catch (e) { toast.error(parseError(e)); } }}>Удалить</Button>
                      </div>
                    </td>
                  </tr>
                  {selectedCollection?.id === c.id && (
                    <tr key={`${c.id}-items`}>
                      <td colSpan={5} className="p-4 bg-muted/30 border-b">
                        <div className="flex gap-2 mb-3">
                          <Select value={addItemId} onValueChange={(v) => setAddItemId(v ?? '')}>
                            <SelectTrigger className="flex-1"><SelectValue placeholder="Выберите товар..." /></SelectTrigger>
                            <SelectContent>{allItems.map((i) => <SelectItem key={i.id} value={i.id}>{i.name} ({i.categoryName})</SelectItem>)}</SelectContent>
                          </Select>
                          <Button size="sm" disabled={!addItemId} onClick={async () => { try { await adminAddItemToCollection(c.id, addItemId); setAddItemId(''); load(); toast.success('Товар добавлен'); } catch (e) { toast.error(parseError(e)); } }}>Добавить</Button>
                        </div>
                        <CollectionItemsList collectionId={c.id} onRemove={load} />
                      </td>
                    </tr>
                  )}
                </>
              ))}
            </tbody>
          </table>
        </CardContent></Card>
      )}
    </div>
  );
}

function CollectionItemsList({ collectionId, onRemove }: { collectionId: string; onRemove: () => void }) {
  const toast = useToast();
  const [items, setItems] = useState<CatalogItemDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { setLoading(true); adminGetCollectionItems(collectionId).then(setItems).catch(() => {}).finally(() => setLoading(false)); }, [collectionId]);

  if (loading) return <p className="text-sm text-muted-foreground">Загрузка...</p>;
  if (items.length === 0) return <p className="text-sm text-muted-foreground">Нет товаров в подборке</p>;

  return (
    <div className="flex flex-wrap gap-2">
      {items.map((item) => (
        <span key={item.id} className="flex items-center gap-1 bg-background border rounded-full px-3 py-0.5 text-xs">
          {item.name}
          <button className="text-destructive hover:text-destructive/70 font-bold" onClick={async () => { try { await adminRemoveItemFromCollection(collectionId, item.id); setItems((p) => p.filter((i) => i.id !== item.id)); onRemove(); toast.success('Товар удалён'); } catch (e) { toast.error(parseError(e)); } }}>×</button>
        </span>
      ))}
    </div>
  );
}
