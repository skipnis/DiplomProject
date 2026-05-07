import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  adminGetCategories, adminCreateCategory, adminUpdateCategory, adminDeleteCategory,
  adminGetAllItems, adminCreateItem, adminUpdateItem, adminDeleteItem,
  adminGetAllCollections, adminCreateCollection, adminUpdateCollection, adminDeleteCollection,
  adminAddItemToCollection, adminRemoveItemFromCollection, adminGetCollectionItems, adminUpdateCollectionItemDescription,
  adminUploadItemImage, adminUploadCollectionImage, adminParseUrl, adminSetItemPublished,
  adminSetCategoryPublished, adminSetCollectionPublished,
  adminBatchImportItems, type BatchImportItemResult,
  adminGetOccasions, adminCreateOccasion, adminUpdateOccasion, adminDeleteOccasion,
  adminGetCatalogBadgeDefinitions, adminCreateCatalogBadgeDefinition, adminUpdateCatalogBadgeDefinition, adminDeleteCatalogBadgeDefinition,
  adminGetFulfilledBadgeDefinitions, adminCreateFulfilledBadgeDefinition, adminUpdateFulfilledBadgeDefinition, adminDeleteFulfilledBadgeDefinition,
  adminGetAchievementDefinitions, adminCreateAchievementDefinition, adminUpdateAchievementDefinition, adminDeleteAchievementDefinition,
  ADMIN_TOKEN_KEY,
} from '../api/admin';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { catalogItemSchema, catalogCategorySchema, catalogCollectionSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { parseApiFieldErrors } from '../utils/errors';
import type { AchievementDefinitionAdminDto, AchievementRuleType, CatalogBadgeDefinitionDto, CatalogCategoryDto, CatalogCollectionAdminDto, CatalogItemDto, FulfilledBadgeDefinitionDto, OccasionDto, PagedResponse } from '../types';
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

type ItemFormValues = { name: string; description: string; price: string; currency: string; imagePath: string; url: string; categoryId: string; isPublished: boolean; occasionIds: string[] };
type CollectionFormValues = { name: string; description: string; occasionId: string; coverImagePath: string; order: string; isPublished: boolean };

function ItemForm({ form, setForm, categories, occasions, itemImageFile, setItemImageFile, externalImageUrl, setExternalImageUrl, onSubmit, submitLabel, onCancel, onParseUrl, parsing, errors, clearError }: {
  form: ItemFormValues; setForm: (f: ItemFormValues) => void; categories: CatalogCategoryDto[]; occasions: OccasionDto[];
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
                <SelectTrigger aria-invalid={!!errors.categoryId}>
                  <SelectValue>
                    {form.categoryId
                      ? (categories.find((c) => c.id === form.categoryId)?.name ?? form.categoryId)
                      : <span className="text-muted-foreground">Выберите...</span>}
                  </SelectValue>
                </SelectTrigger>
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
            {occasions.length > 0 && (
              <div className="flex flex-col gap-1.5 col-span-2">
                <Label>Поводы</Label>
                <div className="flex flex-wrap gap-2">
                  {occasions.map((o) => {
                    const selected = form.occasionIds.includes(o.id);
                    return (
                      <button
                        key={o.id}
                        type="button"
                        onClick={() => setForm({ ...form, occasionIds: selected ? form.occasionIds.filter((id) => id !== o.id) : [...form.occasionIds, o.id] })}
                        className={`px-3 py-1 rounded-full text-sm border transition-colors ${selected ? 'bg-primary text-primary-foreground border-primary' : 'bg-background border-border text-foreground hover:bg-muted'}`}
                      >
                        {o.label}
                      </button>
                    );
                  })}
                </div>
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

function CollectionForm({ form, setForm, occasions, collectionImageFile, setCollectionImageFile, onSubmit, submitLabel, onCancel, errors, clearError }: {
  form: CollectionFormValues; setForm: (f: CollectionFormValues) => void; occasions: OccasionDto[];
  collectionImageFile: File | null; setCollectionImageFile: (f: File | null) => void;
  onSubmit: (e: React.FormEvent) => void; submitLabel: string; onCancel: () => void;
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
              <Select value={form.occasionId || '__none__'} onValueChange={(v) => setForm({ ...form, occasionId: v == null || v === '__none__' ? '' : v })}>
                <SelectTrigger>
                  <SelectValue>
                    {form.occasionId
                      ? (occasions.find((o) => o.id === form.occasionId)?.label ?? '—')
                      : '—'}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">—</SelectItem>
                  {occasions.map((o) => <SelectItem key={o.id} value={o.id}>{o.label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Порядок *</Label>
              <Input
                type="number"
                min={1}
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
  const [activeTab, setActiveTab] = useState('categories');
  const [filterCategoryId, setFilterCategoryId] = useState<string | undefined>(undefined);

  const openItemsByCategory = (categoryId: string) => {
    setFilterCategoryId(categoryId);
    setActiveTab('items');
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Администрирование каталога</h1>
        <Button variant="ghost" size="sm" onClick={() => { localStorage.removeItem(ADMIN_TOKEN_KEY); window.location.href = '/admin/login'; }}>Выйти</Button>
      </div>
      <Tabs value={activeTab} onValueChange={(v) => { setActiveTab(v); if (v !== 'items') setFilterCategoryId(undefined); }}>
        <TabsList className="mb-6 flex-wrap h-auto gap-1">
          <TabsTrigger value="categories">Категории</TabsTrigger>
          <TabsTrigger value="items">Товары</TabsTrigger>
          <TabsTrigger value="occasions">Поводы</TabsTrigger>
          <TabsTrigger value="collections">Подборки</TabsTrigger>
          <TabsTrigger value="catalog-badges">Бейджи каталога</TabsTrigger>
          <TabsTrigger value="fulfilled-badges">Бейджи подарков</TabsTrigger>
          <TabsTrigger value="achievements">Достижения</TabsTrigger>
        </TabsList>
        <TabsContent value="categories"><CategoriesTab onOpenItems={openItemsByCategory} /></TabsContent>
        <TabsContent value="items"><ItemsTab initialCategoryId={filterCategoryId} /></TabsContent>
        <TabsContent value="occasions"><OccasionsTab /></TabsContent>
        <TabsContent value="collections"><CollectionsTab /></TabsContent>
        <TabsContent value="catalog-badges"><CatalogBadgesTab /></TabsContent>
        <TabsContent value="fulfilled-badges"><FulfilledBadgesTab /></TabsContent>
        <TabsContent value="achievements"><AchievementsTab /></TabsContent>
      </Tabs>
    </div>
  );
}

function CategoriesTab({ onOpenItems }: { onOpenItems: (categoryId: string) => void }) {
  const toast = useToast();
  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [newName, setNewName] = useState('');
  const [newOrder, setNewOrder] = useState('1');
  const [newErrors, setNewErrors] = useState<FormErrors>({});
  const [editId, setEditId] = useState<string | null>(null);
  const [editName, setEditName] = useState('');
  const [editOrder, setEditOrder] = useState('1');
  const [editErrors, setEditErrors] = useState<FormErrors>({});

  const load = () => { setLoading(true); adminGetCategories().then(setCategories).catch((e) => toast.error(parseError(e))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = catalogCategorySchema.safeParse({ name: newName, order: newOrder });
    if (!result.success) { setNewErrors(parseZodErrors(result.error)); return; }
    setNewErrors({});
    try { await adminCreateCategory({ name: newName, order: Number(newOrder) }); setNewName(''); setNewOrder('1'); load(); }
    catch (e) { const fe = parseApiFieldErrors(e); if (fe) setNewErrors((p) => ({ ...p, ...fe })); else toast.error(parseError(e)); }
  };

  const handleUpdate = async (id: string) => {
    const result = catalogCategorySchema.safeParse({ name: editName, order: editOrder });
    if (!result.success) { setEditErrors(parseZodErrors(result.error)); return; }
    setEditErrors({});
    try { await adminUpdateCategory(id, { name: editName, order: Number(editOrder) }); setEditId(null); load(); }
    catch (e) { const fe = parseApiFieldErrors(e); if (fe) setEditErrors((p) => ({ ...p, ...fe })); else toast.error(parseError(e)); }
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
          <Input type="number" min={1} value={newOrder} onChange={(e) => { setNewOrder(e.target.value); if (newErrors.order) setNewErrors((p) => ({ ...p, order: '' })); }} aria-invalid={!!newErrors.order} />
          <FieldError message={newErrors.order} />
        </div>
        <Button type="submit" className="mt-6">Создать</Button>
      </form>
      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : (
        <Card>
          <CardContent className="p-0">
            <table className="w-full border-collapse text-sm">
              <thead><tr className="border-b"><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Название</th><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Порядок</th><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Статус</th><th className="p-3"></th></tr></thead>
              <tbody>
                {categories.map((c) => (
                  <tr key={c.id} className="border-b last:border-0">
                    <td className="p-3">
                      {editId === c.id ? (
                        <>
                          <Input value={editName} onChange={(e) => { setEditName(e.target.value); if (editErrors.name) setEditErrors((p) => ({ ...p, name: '' })); }} className="h-7" aria-invalid={!!editErrors.name} />
                          <FieldError message={editErrors.name} />
                        </>
                      ) : (
                        <button
                          className="hover:underline text-left"
                          onClick={() => onOpenItems(c.id)}
                        >
                          {c.name}
                        </button>
                      )}
                    </td>
                    <td className="p-3">
                      {editId === c.id ? (
                        <>
                          <Input type="number" min={1} value={editOrder} onChange={(e) => { setEditOrder(e.target.value); if (editErrors.order) setEditErrors((p) => ({ ...p, order: '' })); }} className="h-7 w-20" aria-invalid={!!editErrors.order} />
                          <FieldError message={editErrors.order} />
                        </>
                      ) : c.order}
                    </td>
                    <td className="p-3">
                      {editId !== c.id && <Badge variant={c.isPublished ? 'default' : 'secondary'}>{c.isPublished ? 'Опубликована' : 'Черновик'}</Badge>}
                    </td>
                    <td className="p-3 text-right">
                      {editId === c.id ? (
                        <div className="flex gap-2 justify-end">
                          <Button size="sm" onClick={() => handleUpdate(c.id)}>Сохранить</Button>
                          <Button size="sm" variant="ghost" onClick={() => { setEditId(null); setEditErrors({}); }}>Отмена</Button>
                        </div>
                      ) : (
                        <div className="flex gap-2 justify-end">
                          <Button size="sm" variant="ghost" onClick={async () => { try { await adminSetCategoryPublished(c.id, !c.isPublished); load(); } catch (e) { toast.error(parseError(e)); } }}>{c.isPublished ? 'Скрыть' : 'Опубликовать'}</Button>
                          <Button size="sm" variant="ghost" onClick={() => onOpenItems(c.id)}>Открыть</Button>
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

function ItemsTab({ initialCategoryId }: { initialCategoryId?: string }) {
  const toast = useToast();
  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [occasions, setOccasions] = useState<OccasionDto[]>([]);
  const [data, setData] = useState<PagedResponse<CatalogItemDto> | null>(null);
  const [page, setPage] = useState(1);
  const [categoryFilter, setCategoryFilter] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [editItem, setEditItem] = useState<CatalogItemDto | null>(null);
  const [form, setForm] = useState<ItemFormValues>({ name: '', description: '', price: '', currency: '0', imagePath: '', url: '', categoryId: '', isPublished: false, occasionIds: [] });
  const [itemImageFile, setItemImageFile] = useState<File | null>(null);
  const [externalImageUrl, setExternalImageUrl] = useState<string | null>(null);
  const [parsing, setParsing] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const [showBatchImport, setShowBatchImport] = useState(false);
  const [batchUrls, setBatchUrls] = useState<string[]>(['']);
  const [batchCategoryId, setBatchCategoryId] = useState('');
  const [batchImporting, setBatchImporting] = useState(false);
  const [batchResultMap, setBatchResultMap] = useState<Map<string, BatchImportItemResult> | null>(null);

  const clearError = (field: string) => { if (errors[field]) setErrors((p) => ({ ...p, [field]: '' })); };

  useEffect(() => {
    adminGetCategories().then((cats) => {
      setCategories(cats);
      if (initialCategoryId) setCategoryFilter(initialCategoryId);
    }).catch(() => {});
    adminGetOccasions().then(setOccasions).catch(() => {});
  }, []);
  const load = (p: number, catId?: string) => { setLoading(true); adminGetAllItems({ page: p, categoryId: catId || undefined }).then(setData).catch(() => {}).finally(() => setLoading(false)); };
  useEffect(() => { load(page, categoryFilter); }, [page, categoryFilter]);

  const resetForm = () => { setForm({ name: '', description: '', price: '', currency: '0', imagePath: '', url: '', categoryId: '', isPublished: false, occasionIds: [] }); setItemImageFile(null); setExternalImageUrl(null); setErrors({}); };

  const handleBatchImport = async () => {
    const urls = batchUrls.map((url) => url.trim()).filter(Boolean);
    if (!urls.length || !batchCategoryId) return;
    setBatchImporting(true);
    setBatchResultMap(null);
    try {
      const results = await adminBatchImportItems({ urls, categoryId: batchCategoryId });
      setBatchResultMap(new Map(results.map((r) => [r.url, r])));
      load(page, categoryFilter);
    } catch (error) {
      toast.error(parseError(error));
    } finally {
      setBatchImporting(false);
    }
  };

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
      const newId = await adminCreateItem({ name: form.name, description: form.description || null, price: form.price ? Number(form.price) : null, currency: form.currency ? (Number(form.currency) as never) : null, imagePath: null, url: form.url || null, categoryId: form.categoryId, occasionIds: form.occasionIds });
      if (itemImageFile) await adminUploadItemImage(newId, itemImageFile);
      else if (externalImageUrl) await adminUploadItemImage(newId, externalImageUrl);
      resetForm(); setShowCreate(false); load(page);
    } catch (e) { const fe = parseApiFieldErrors(e); if (fe) setErrors((p) => ({ ...p, ...fe })); else toast.error(parseError(e)); }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editItem || !validate()) return;
    try {
      await adminUpdateItem(editItem.id, { name: form.name, description: form.description || null, price: form.price ? Number(form.price) : null, currency: form.currency ? (Number(form.currency) as never) : null, imagePath: form.imagePath || null, url: form.url || null, categoryId: form.categoryId, isPublished: form.isPublished, occasionIds: form.occasionIds });
      if (itemImageFile) await adminUploadItemImage(editItem.id, itemImageFile);
      else if (externalImageUrl) await adminUploadItemImage(editItem.id, externalImageUrl);
      setEditItem(null); resetForm(); load(page);
    } catch (e) { const fe = parseApiFieldErrors(e); if (fe) setErrors((p) => ({ ...p, ...fe })); else toast.error(parseError(e)); }
  };

  const handleCancel = () => { setShowCreate(false); setEditItem(null); resetForm(); };

  return (
    <div>
      {!showCreate && !editItem && (
        <>
          <div className="flex items-center gap-3 mb-4">
            <Button onClick={() => { setShowCreate(true); resetForm(); }}>Добавить товар</Button>
            <Button variant="outline" onClick={() => { setShowBatchImport((v) => !v); setBatchResultMap(null); setBatchUrls(['']); }}>
              {showBatchImport ? 'Скрыть импорт' : 'Импортировать по ссылкам'}
            </Button>
            <Select value={categoryFilter} onValueChange={(v) => { setCategoryFilter(v ?? ''); setPage(1); }}>
              <SelectTrigger className="w-52 h-9">
                <SelectValue>
                  {categoryFilter
                    ? (categories.find((c) => c.id === categoryFilter)?.name ?? categoryFilter)
                    : <span className="text-muted-foreground">Все категории</span>}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">Все категории</SelectItem>
                {categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
              </SelectContent>
            </Select>
            {categoryFilter && <Button variant="ghost" size="sm" onClick={() => { setCategoryFilter(''); setPage(1); }}>Сбросить</Button>}
          </div>
          {showBatchImport && (
            <Card className="mb-4">
              <CardContent className="pt-4 flex flex-col gap-3">
                <div className="flex flex-col gap-2">
                  <Label>Ссылки (до 50)</Label>
                  {batchUrls.map((url, index) => {
                    const result = batchResultMap?.get(url.trim());
                    return (
                      <div key={index} className="flex items-center gap-2">
                        <Input
                          placeholder="https://example.com/product"
                          value={url}
                          onChange={(e) => {
                            const next = [...batchUrls];
                            next[index] = e.target.value;
                            setBatchUrls(next);
                            if (batchResultMap) setBatchResultMap(null);
                          }}
                          className={
                            result?.status === 'Success' ? 'border-green-400 focus-visible:ring-green-300' :
                            result?.status === 'Partial'  ? 'border-yellow-400 focus-visible:ring-yellow-300' :
                            result?.status === 'Failed'   ? 'border-red-400 focus-visible:ring-red-300' : ''
                          }
                        />
                        {result && (
                          <div className="shrink-0 flex flex-col items-end gap-1 min-w-[90px]">
                            {result.status === 'Success' && <Badge className="bg-green-100 text-green-800 border border-green-200 shadow-none">Успех</Badge>}
                            {result.status === 'Partial' && (
                              <div className="flex flex-col items-end gap-1">
                                <Badge className="bg-yellow-100 text-yellow-800 border border-yellow-300 shadow-none">Частично</Badge>
                                {result.missingFields.length > 0 && (
                                  <div className="flex gap-1 flex-wrap justify-end">
                                    {result.missingFields.map((field) => (
                                      <span key={field} className="text-xs bg-yellow-200 text-yellow-900 rounded px-1.5 py-0.5">
                                        {MISSING_FIELD_LABELS[field] ?? field}
                                      </span>
                                    ))}
                                  </div>
                                )}
                              </div>
                            )}
                            {result.status === 'Failed' && (
                              <div className="flex flex-col items-end gap-1">
                                <Badge variant="destructive">Ошибка</Badge>
                                {result.errorMessage && <span className="text-xs text-red-600 text-right max-w-[180px]">{result.errorMessage}</span>}
                              </div>
                            )}
                            {result.itemId && (
                              <Button
                                size="sm"
                                variant="ghost"
                                className="h-5 px-2 text-xs"
                                onClick={() => {
                                  const found = data?.items.find((item) => item.id === result.itemId) ?? null;
                                  if (found) {
                                    setEditItem(found);
                                    setShowCreate(false);
                                    setForm({ name: found.name, description: found.description ?? '', price: found.price !== null ? String(found.price) : '', currency: '0', imagePath: found.imagePath ?? '', url: found.url ?? '', categoryId: found.categoryId, isPublished: found.isPublished, occasionIds: found.occasions.map((o) => o.id) });
                                    setErrors({});
                                    setShowBatchImport(false);
                                  }
                                }}
                              >
                                Редактировать
                              </Button>
                            )}
                          </div>
                        )}
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          className="shrink-0 h-9 w-9 p-0 text-muted-foreground hover:text-destructive"
                          onClick={() => setBatchUrls(batchUrls.length > 1 ? batchUrls.filter((_, i) => i !== index) : [''])}
                        >
                          ✕
                        </Button>
                      </div>
                    );
                  })}
                  {batchUrls.length < 50 && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="self-start text-muted-foreground"
                      onClick={() => setBatchUrls([...batchUrls, ''])}
                    >
                      + Добавить ссылку
                    </Button>
                  )}
                </div>
                <div className="flex gap-3 items-center">
                  <div className="flex flex-col gap-1.5 flex-1">
                    <Label>Категория *</Label>
                    <Select value={batchCategoryId} onValueChange={(v) => setBatchCategoryId(v ?? '')}>
                      <SelectTrigger>
                        <SelectValue>
                          {batchCategoryId
                            ? (categories.find((c) => c.id === batchCategoryId)?.name ?? batchCategoryId)
                            : <span className="text-muted-foreground">Выберите...</span>}
                        </SelectValue>
                      </SelectTrigger>
                      <SelectContent>
                        {categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button
                    className="mt-5"
                    onClick={handleBatchImport}
                    disabled={batchImporting || !batchCategoryId || !batchUrls.some((u) => u.trim())}
                  >
                    {batchImporting ? 'Импортируем...' : 'Импортировать'}
                  </Button>
                </div>
                {batchResultMap && (
                  <p className="text-sm text-muted-foreground">
                    Результат: {[...batchResultMap.values()].filter((r) => r.status === 'Success').length} успешно,{' '}
                    {[...batchResultMap.values()].filter((r) => r.status === 'Partial').length} частично,{' '}
                    {[...batchResultMap.values()].filter((r) => r.status === 'Failed').length} ошибок
                  </p>
                )}
              </CardContent>
            </Card>
          )}
        </>
      )}
      {showCreate && <ItemForm form={form} setForm={setForm} categories={categories} occasions={occasions} itemImageFile={itemImageFile} setItemImageFile={setItemImageFile} externalImageUrl={externalImageUrl} setExternalImageUrl={setExternalImageUrl} onSubmit={handleCreate} submitLabel="Создать" onCancel={handleCancel} onParseUrl={handleParseUrl} parsing={parsing} errors={errors} clearError={clearError} />}
      {editItem && <ItemForm form={form} setForm={setForm} categories={categories} occasions={occasions} itemImageFile={itemImageFile} setItemImageFile={setItemImageFile} externalImageUrl={externalImageUrl} setExternalImageUrl={setExternalImageUrl} onSubmit={handleUpdate} submitLabel="Сохранить" onCancel={handleCancel} onParseUrl={handleParseUrl} parsing={parsing} errors={errors} clearError={clearError} />}

      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : !data || data.items.length === 0 ? <div className="text-muted-foreground text-sm">Нет товаров</div> : (
        <>
          <Card><CardContent className="p-0">
            <table className="w-full border-collapse text-sm">
              <thead><tr className="border-b">{['Название','Категория','Цена','Статус',''].map((h) => <th key={h} className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">{h}</th>)}</tr></thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id} className="border-b last:border-0">
                    <td className="p-3 font-medium"><Link to={`/catalog/items/${item.id}`} target="_blank" className="hover:underline">{item.name}</Link></td>
                    <td className="p-3 text-muted-foreground">{item.categoryName}</td>
                    <td className="p-3">{item.price !== null ? `${item.price} ${item.currency ?? ''}` : '—'}</td>
                    <td className="p-3"><Badge variant={item.isPublished ? 'default' : 'secondary'}>{item.isPublished ? 'Опубликован' : 'Черновик'}</Badge></td>
                    <td className="p-3 text-right">
                      <div className="flex gap-1 justify-end">
                        <Button size="sm" variant="ghost" onClick={async () => { try { await adminSetItemPublished(item.id, !item.isPublished); load(page); } catch (e) { toast.error(parseError(e)); } }}>{item.isPublished ? 'Снять' : 'Опубликовать'}</Button>
                        <Button size="sm" variant="ghost" onClick={() => { setEditItem(item); setShowCreate(false); setForm({ name: item.name, description: item.description ?? '', price: item.price !== null ? String(item.price) : '', currency: '0', imagePath: item.imagePath ?? '', url: item.url ?? '', categoryId: item.categoryId, isPublished: item.isPublished, occasionIds: item.occasions.map((o) => o.id) }); setErrors({}); }}>Изменить</Button>
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
  const [occasions, setOccasions] = useState<OccasionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [editCollection, setEditCollection] = useState<CatalogCollectionAdminDto | null>(null);
  const [selectedCollection, setSelectedCollection] = useState<CatalogCollectionAdminDto | null>(null);
  const [addItemId, setAddItemId] = useState('');
  const [addItemDescription, setAddItemDescription] = useState('');
  const [form, setForm] = useState<CollectionFormValues>({ name: '', description: '', occasionId: '', coverImagePath: '', order: '1', isPublished: false });
  const [collectionImageFile, setCollectionImageFile] = useState<File | null>(null);
  const [errors, setErrors] = useState<FormErrors>({});
  const clearError = (field: string) => { if (errors[field]) setErrors((p) => ({ ...p, [field]: '' })); };

  const load = () => { setLoading(true); adminGetAllCollections().then(setCollections).catch(() => {}).finally(() => setLoading(false)); };
  useEffect(() => {
    load();
    adminGetAllItems({ pageSize: 200 }).then((r) => setAllItems(r.items)).catch(() => {});
    adminGetOccasions().then(setOccasions).catch(() => {});
  }, []);

  const resetForm = () => { setForm({ name: '', description: '', occasionId: '', coverImagePath: '', order: '1', isPublished: false }); setCollectionImageFile(null); setErrors({}); };
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
      const newId = await adminCreateCollection({ name: form.name, description: form.description || null, occasionId: form.occasionId || null, coverImagePath: null, order: Number(form.order) });
      if (collectionImageFile) await adminUploadCollectionImage(newId, collectionImageFile);
      resetForm(); setShowCreate(false); load(); toast.success('Подборка создана');
    } catch (e) { const fe = parseApiFieldErrors(e); if (fe) setErrors((p) => ({ ...p, ...fe })); else toast.error(parseError(e)); }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editCollection || !validate()) return;
    try {
      await adminUpdateCollection(editCollection.id, { name: form.name, description: form.description || null, occasionId: form.occasionId || null, coverImagePath: form.coverImagePath || null, order: Number(form.order), isPublished: form.isPublished });
      if (collectionImageFile) await adminUploadCollectionImage(editCollection.id, collectionImageFile);
      setEditCollection(null); resetForm(); load(); toast.success('Подборка обновлена');
    } catch (e) { const fe = parseApiFieldErrors(e); if (fe) setErrors((p) => ({ ...p, ...fe })); else toast.error(parseError(e)); }
  };

  return (
    <div>
      {!showCreate && !editCollection && <Button className="mb-4" onClick={() => { setShowCreate(true); resetForm(); }}>Создать подборку</Button>}
      {showCreate && <CollectionForm form={form} setForm={setForm} occasions={occasions} collectionImageFile={collectionImageFile} setCollectionImageFile={setCollectionImageFile} onSubmit={handleCreate} submitLabel="Создать" onCancel={handleCancel} errors={errors} clearError={clearError} />}
      {editCollection && <CollectionForm form={form} setForm={setForm} occasions={occasions} collectionImageFile={collectionImageFile} setCollectionImageFile={setCollectionImageFile} onSubmit={handleUpdate} submitLabel="Сохранить" onCancel={handleCancel} errors={errors} clearError={clearError} />}

      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : collections.length === 0 ? <div className="text-muted-foreground text-sm">Нет подборок</div> : (
        <Card><CardContent className="p-0">
          <table className="w-full border-collapse text-sm">
            <thead><tr className="border-b">{['Название','Повод','Товаров','Статус',''].map((h) => <th key={h} className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">{h}</th>)}</tr></thead>
            <tbody>
              {collections.map((c) => (
                <>
                  <tr key={c.id} className="border-b">
                    <td className="p-3 font-medium">
                      <Link to={`/collections/${c.id}`} className="hover:underline">{c.name}</Link>
                    </td>
                    <td className="p-3 text-muted-foreground">{c.occasion?.label ?? '—'}</td>
                    <td className="p-3">{c.itemCount}</td>
                    <td className="p-3"><Badge variant={c.isPublished ? 'default' : 'secondary'}>{c.isPublished ? 'Опубликована' : 'Черновик'}</Badge></td>
                    <td className="p-3 text-right">
                      <div className="flex gap-1 justify-end">
                        <Button size="sm" variant="ghost" onClick={async () => { try { await adminSetCollectionPublished(c.id, !c.isPublished); load(); } catch (e) { toast.error(parseError(e)); } }}>{c.isPublished ? 'Снять' : 'Опубликовать'}</Button>
                        <Button size="sm" variant="ghost" onClick={() => setSelectedCollection(selectedCollection?.id === c.id ? null : c)}>{selectedCollection?.id === c.id ? 'Свернуть' : 'Товары'}</Button>
                        <Button size="sm" variant="ghost" onClick={() => { setEditCollection(c); setShowCreate(false); setForm({ name: c.name, description: c.description ?? '', occasionId: c.occasion?.id ?? '', coverImagePath: c.coverImagePath ?? '', order: String(c.order), isPublished: c.isPublished }); setErrors({}); }}>Изменить</Button>
                        <Button size="sm" variant="ghost" className="text-destructive" onClick={async () => { if (!confirm('Удалить подборку?')) return; try { await adminDeleteCollection(c.id); if (selectedCollection?.id === c.id) setSelectedCollection(null); load(); } catch (e) { toast.error(parseError(e)); } }}>Удалить</Button>
                      </div>
                    </td>
                  </tr>
                  {selectedCollection?.id === c.id && (
                    <tr key={`${c.id}-items`}>
                      <td colSpan={5} className="p-4 bg-muted/30 border-b">
                        <div className="flex flex-col gap-2 mb-3">
                          <div className="flex gap-2">
                            <Select value={addItemId} onValueChange={(v) => setAddItemId(v ?? '')}>
                              <SelectTrigger className="flex-1">
                                <SelectValue>
                                  {addItemId
                                    ? (allItems.find((i) => i.id === addItemId)?.name ?? addItemId)
                                    : <span className="text-muted-foreground">Выберите товар...</span>}
                                </SelectValue>
                              </SelectTrigger>
                              <SelectContent>{allItems.map((i) => <SelectItem key={i.id} value={i.id}>{i.name} ({i.categoryName})</SelectItem>)}</SelectContent>
                            </Select>
                            <Button size="sm" disabled={!addItemId} onClick={async () => { try { await adminAddItemToCollection(c.id, addItemId, addItemDescription || undefined); setAddItemId(''); setAddItemDescription(''); load(); toast.success('Товар добавлен'); } catch (e) { toast.error(parseError(e)); } }}>Добавить</Button>
                          </div>
                          <input
                            className="border rounded px-3 py-1.5 text-sm w-full bg-background"
                            placeholder="Описание товара в подборке (необязательно)"
                            value={addItemDescription}
                            onChange={(e) => setAddItemDescription(e.target.value)}
                          />
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

function OccasionsTab() {
  const toast = useToast();
  const [occasions, setOccasions] = useState<OccasionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [newKey, setNewKey] = useState('');
  const [newLabel, setNewLabel] = useState('');
  const [newOrder, setNewOrder] = useState('1');
  const [editId, setEditId] = useState<string | null>(null);
  const [editKey, setEditKey] = useState('');
  const [editLabel, setEditLabel] = useState('');
  const [editOrder, setEditOrder] = useState('1');

  const load = () => { setLoading(true); adminGetOccasions().then(setOccasions).catch((e) => toast.error(parseError(e))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newKey.trim() || !newLabel.trim()) return;
    try {
      await adminCreateOccasion({ key: newKey.trim(), label: newLabel.trim(), order: Number(newOrder) });
      setNewKey(''); setNewLabel(''); setNewOrder('1'); load();
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleUpdate = async (id: string) => {
    if (!editKey.trim() || !editLabel.trim()) return;
    try {
      await adminUpdateOccasion(id, { key: editKey.trim(), label: editLabel.trim(), order: Number(editOrder) });
      setEditId(null); load();
    } catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      <form onSubmit={handleCreate} className="flex gap-3 mb-6 items-start flex-wrap">
        <div className="flex flex-col gap-1.5 flex-1 min-w-32">
          <Label>Ключ *</Label>
          <Input placeholder="birthday" value={newKey} onChange={(e) => setNewKey(e.target.value)} />
        </div>
        <div className="flex flex-col gap-1.5 flex-1 min-w-40">
          <Label>Название *</Label>
          <Input placeholder="🎂 День рождения" value={newLabel} onChange={(e) => setNewLabel(e.target.value)} />
        </div>
        <div className="flex flex-col gap-1.5 w-24">
          <Label>Порядок *</Label>
          <Input type="number" min={1} value={newOrder} onChange={(e) => setNewOrder(e.target.value)} />
        </div>
        <Button type="submit" className="mt-6" disabled={!newKey.trim() || !newLabel.trim()}>Создать</Button>
      </form>
      {loading ? <div className="text-muted-foreground text-sm">Загрузка...</div> : (
        <Card>
          <CardContent className="p-0">
            <table className="w-full border-collapse text-sm">
              <thead><tr className="border-b"><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Ключ</th><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Название</th><th className="p-3 text-left text-xs font-semibold text-muted-foreground uppercase">Порядок</th><th className="p-3"></th></tr></thead>
              <tbody>
                {occasions.map((o) => (
                  <tr key={o.id} className="border-b last:border-0">
                    <td className="p-3 font-mono text-xs">
                      {editId === o.id
                        ? <Input value={editKey} onChange={(e) => setEditKey(e.target.value)} className="h-7 font-mono text-xs" />
                        : o.key}
                    </td>
                    <td className="p-3">
                      {editId === o.id
                        ? <Input value={editLabel} onChange={(e) => setEditLabel(e.target.value)} className="h-7" />
                        : o.label}
                    </td>
                    <td className="p-3">
                      {editId === o.id
                        ? <Input type="number" min={1} value={editOrder} onChange={(e) => setEditOrder(e.target.value)} className="h-7 w-20" />
                        : o.order}
                    </td>
                    <td className="p-3 text-right">
                      {editId === o.id ? (
                        <div className="flex gap-2 justify-end">
                          <Button size="sm" onClick={() => handleUpdate(o.id)}>Сохранить</Button>
                          <Button size="sm" variant="ghost" onClick={() => setEditId(null)}>Отмена</Button>
                        </div>
                      ) : (
                        <div className="flex gap-2 justify-end">
                          <Button size="sm" variant="ghost" onClick={() => { setEditId(o.id); setEditKey(o.key); setEditLabel(o.label); setEditOrder(String(o.order)); }}>Изменить</Button>
                          <Button size="sm" variant="ghost" className="text-destructive" onClick={async () => { if (!confirm('Удалить повод?')) return; try { await adminDeleteOccasion(o.id); load(); } catch (e) { toast.error(parseError(e)); } }}>Удалить</Button>
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

const MISSING_FIELD_LABELS: Record<string, string> = {
  Description: 'Описание',
  Price: 'Цена',
  Image: 'Изображение',
};


function CollectionItemsList({ collectionId, onRemove }: { collectionId: string; onRemove: () => void }) {
  const toast = useToast();
  const [items, setItems] = useState<CatalogItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingDescId, setEditingDescId] = useState<string | null>(null);
  const [editingDesc, setEditingDesc] = useState('');

  useEffect(() => { setLoading(true); adminGetCollectionItems(collectionId).then(setItems).catch(() => {}).finally(() => setLoading(false)); }, [collectionId]);

  const startEditDesc = (item: CatalogItemDto) => { setEditingDescId(item.id); setEditingDesc(item.collectionItemDescription ?? ''); };
  const saveDesc = async (itemId: string) => {
    try {
      await adminUpdateCollectionItemDescription(collectionId, itemId, editingDesc.trim() || null);
      setItems((prev) => prev.map((i) => i.id === itemId ? { ...i, collectionItemDescription: editingDesc.trim() || null } : i));
      setEditingDescId(null);
      toast.success('Описание сохранено');
    } catch (e) { toast.error(parseError(e)); }
  };

  if (loading) return <p className="text-sm text-muted-foreground">Загрузка...</p>;
  if (items.length === 0) return <p className="text-sm text-muted-foreground">Нет товаров в подборке</p>;

  return (
    <div className="flex flex-col gap-1.5">
      {items.map((item) => (
        <div key={item.id} className="flex items-center gap-2 bg-background border rounded-lg px-3 py-1.5 text-sm">
          <span className="flex-1 font-medium truncate">{item.name}</span>
          {editingDescId === item.id ? (
            <div className="flex items-center gap-1.5 flex-1">
              <input
                autoFocus
                className="border rounded px-2 py-0.5 text-xs flex-1 bg-background"
                value={editingDesc}
                onChange={(e) => setEditingDesc(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') saveDesc(item.id); if (e.key === 'Escape') setEditingDescId(null); }}
                placeholder="Описание в подборке..."
              />
              <button className="text-xs text-primary hover:text-primary/70" onClick={() => saveDesc(item.id)}>✓</button>
              <button className="text-xs text-muted-foreground hover:text-foreground" onClick={() => setEditingDescId(null)}>✕</button>
            </div>
          ) : (
            <button className="text-xs text-muted-foreground hover:text-foreground truncate max-w-48 text-left" onClick={() => startEditDesc(item)}>
              {item.collectionItemDescription ?? <span className="italic">+ описание</span>}
            </button>
          )}
          <button className="text-destructive hover:text-destructive/70 font-bold text-sm ml-1 shrink-0" onClick={async () => { try { await adminRemoveItemFromCollection(collectionId, item.id); setItems((p) => p.filter((i) => i.id !== item.id)); onRemove(); toast.success('Товар удалён'); } catch (e) { toast.error(parseError(e)); } }}>×</button>
        </div>
      ))}
    </div>
  );
}

type BadgeDefForm = { label: string; isActive: boolean };
const emptyBadgeForm: BadgeDefForm = { label: '', isActive: true };

function CatalogBadgesTab() {
  const toast = useToast();
  const [items, setItems] = useState<CatalogBadgeDefinitionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingItem, setEditingItem] = useState<CatalogBadgeDefinitionDto | null>(null);
  const [form, setForm] = useState<BadgeDefForm>(emptyBadgeForm);
  const [showForm, setShowForm] = useState(false);

  const load = () => { setLoading(true); adminGetCatalogBadgeDefinitions().then(setItems).catch(() => {}).finally(() => setLoading(false)); };
  useEffect(load, []);

  const openCreate = () => { setEditingItem(null); setForm(emptyBadgeForm); setShowForm(true); };
  const openEdit = (item: CatalogBadgeDefinitionDto) => { setEditingItem(item); setForm({ label: item.label, isActive: item.isActive }); setShowForm(true); };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = { label: form.label.trim(), isActive: form.isActive };
    try {
      if (editingItem) { await adminUpdateCatalogBadgeDefinition(editingItem.id, payload); toast.success('Бейдж обновлён'); }
      else { await adminCreateCatalogBadgeDefinition(payload); toast.success('Бейдж создан'); }
      setShowForm(false); load();
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Удалить бейдж каталога?')) return;
    try { await adminDeleteCatalogBadgeDefinition(id); load(); toast.success('Удалено'); }
    catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="font-bold text-lg">Бейджи каталога</h2>
        <Button size="sm" onClick={openCreate}>+ Добавить</Button>
      </div>
      {showForm && (
        <Card className="mb-4">
          <CardContent className="pt-4">
            <form onSubmit={handleSubmit} className="flex flex-col gap-3">
              <div className="flex flex-col gap-1"><Label>Метка *</Label><Input value={form.label} onChange={(e) => setForm({ ...form, label: e.target.value })} required /></div>
              <div className="flex items-center gap-2">
                <input type="checkbox" id="cb-cat-active" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                <Label htmlFor="cb-cat-active">Активен</Label>
              </div>
              <div className="flex gap-2"><Button type="submit" size="sm">{editingItem ? 'Сохранить' : 'Создать'}</Button><Button type="button" variant="ghost" size="sm" onClick={() => setShowForm(false)}>Отмена</Button></div>
            </form>
          </CardContent>
        </Card>
      )}
      {loading ? <p className="text-sm text-muted-foreground">Загрузка...</p> : (
        <div className="flex flex-col gap-2">
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between border rounded-lg px-4 py-2 bg-card">
              <div className="flex items-center gap-3">
                <span className="text-lg">{item.emoji}</span>
                <span className="text-sm font-medium">{item.label}</span>
                {!item.isActive && <Badge variant="secondary" className="text-xs">Неактивен</Badge>}
              </div>
              <div className="flex gap-1">
                <Button size="sm" variant="ghost" onClick={() => openEdit(item)}>✎</Button>
                <Button size="sm" variant="ghost" className="text-destructive" onClick={() => handleDelete(item.id)}>✕</Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function FulfilledBadgesTab() {
  const toast = useToast();
  const [items, setItems] = useState<FulfilledBadgeDefinitionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingItem, setEditingItem] = useState<FulfilledBadgeDefinitionDto | null>(null);
  const [form, setForm] = useState<BadgeDefForm>(emptyBadgeForm);
  const [showForm, setShowForm] = useState(false);

  const load = () => { setLoading(true); adminGetFulfilledBadgeDefinitions().then(setItems).catch(() => {}).finally(() => setLoading(false)); };
  useEffect(load, []);

  const openCreate = () => { setEditingItem(null); setForm(emptyBadgeForm); setShowForm(true); };
  const openEdit = (item: FulfilledBadgeDefinitionDto) => { setEditingItem(item); setForm({ label: item.label, isActive: item.isActive }); setShowForm(true); };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = { label: form.label.trim(), isActive: form.isActive };
    try {
      if (editingItem) { await adminUpdateFulfilledBadgeDefinition(editingItem.id, payload); toast.success('Бейдж обновлён'); }
      else { await adminCreateFulfilledBadgeDefinition(payload); toast.success('Бейдж создан'); }
      setShowForm(false); load();
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Удалить бейдж подарков?')) return;
    try { await adminDeleteFulfilledBadgeDefinition(id); load(); toast.success('Удалено'); }
    catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="font-bold text-lg">Бейджи подарков</h2>
        <Button size="sm" onClick={openCreate}>+ Добавить</Button>
      </div>
      {showForm && (
        <Card className="mb-4">
          <CardContent className="pt-4">
            <form onSubmit={handleSubmit} className="flex flex-col gap-3">
              <div className="flex flex-col gap-1"><Label>Метка *</Label><Input value={form.label} onChange={(e) => setForm({ ...form, label: e.target.value })} required /></div>
              <div className="flex items-center gap-2">
                <input type="checkbox" id="cb-ful-active" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                <Label htmlFor="cb-ful-active">Активен</Label>
              </div>
              <div className="flex gap-2"><Button type="submit" size="sm">{editingItem ? 'Сохранить' : 'Создать'}</Button><Button type="button" variant="ghost" size="sm" onClick={() => setShowForm(false)}>Отмена</Button></div>
            </form>
          </CardContent>
        </Card>
      )}
      {loading ? <p className="text-sm text-muted-foreground">Загрузка...</p> : (
        <div className="flex flex-col gap-2">
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between border rounded-lg px-4 py-2 bg-card">
              <div className="flex items-center gap-3">
                <span className="text-lg">{item.emoji}</span>
                <span className="text-sm font-medium">{item.label}</span>
                {!item.isActive && <Badge variant="secondary" className="text-xs">Неактивен</Badge>}
              </div>
              <div className="flex gap-1">
                <Button size="sm" variant="ghost" onClick={() => openEdit(item)}>✎</Button>
                <Button size="sm" variant="ghost" className="text-destructive" onClick={() => handleDelete(item.id)}>✕</Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

type AchievementForm = { name: string; description: string; emoji: string; ruleType: string; linkedBadgeTypeId: string; threshold: string; order: string; isActive: boolean };
const emptyAchievementForm: AchievementForm = { name: '', description: '', emoji: '', ruleType: '1', linkedBadgeTypeId: '', threshold: '3', order: '1', isActive: true };

function AchievementsTab() {
  const toast = useToast();
  const [items, setItems] = useState<AchievementDefinitionAdminDto[]>([]);
  const [fulfilledBadges, setFulfilledBadges] = useState<FulfilledBadgeDefinitionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingItem, setEditingItem] = useState<AchievementDefinitionAdminDto | null>(null);
  const [form, setForm] = useState<AchievementForm>(emptyAchievementForm);
  const [showForm, setShowForm] = useState(false);

  const load = () => {
    setLoading(true);
    Promise.all([adminGetAchievementDefinitions(), adminGetFulfilledBadgeDefinitions()])
      .then(([achievements, badges]) => { setItems(achievements); setFulfilledBadges(badges); })
      .catch(() => {})
      .finally(() => setLoading(false));
  };
  useEffect(load, []);

  const openCreate = () => { setEditingItem(null); setForm(emptyAchievementForm); setShowForm(true); };
  const openEdit = (item: AchievementDefinitionAdminDto) => {
    setEditingItem(item);
    setForm({ name: item.name, description: item.description, emoji: item.emoji, ruleType: String(item.ruleType), linkedBadgeTypeId: item.linkedBadgeTypeId != null ? String(item.linkedBadgeTypeId) : '', threshold: String(item.threshold), order: String(item.order), isActive: item.isActive });
    setShowForm(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      name: form.name.trim(), description: form.description.trim(), emoji: form.emoji.trim(),
      ruleType: Number(form.ruleType) as AchievementRuleType,
      linkedBadgeTypeId: form.ruleType === '1' && form.linkedBadgeTypeId ? Number(form.linkedBadgeTypeId) : null,
      threshold: Number(form.threshold), order: Number(form.order), isActive: form.isActive,
    };
    try {
      if (editingItem) { await adminUpdateAchievementDefinition(editingItem.id, payload); toast.success('Достижение обновлено'); }
      else { await adminCreateAchievementDefinition(payload); toast.success('Достижение создано'); }
      setShowForm(false); load();
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Удалить достижение?')) return;
    try { await adminDeleteAchievementDefinition(id); load(); toast.success('Удалено'); }
    catch (e) { toast.error(parseError(e)); }
  };

  const badgeLabelById = new Map(fulfilledBadges.map((b) => [b.id, b.label]));

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="font-bold text-lg">Достижения</h2>
        <Button size="sm" onClick={openCreate}>+ Добавить</Button>
      </div>
      {showForm && (
        <Card className="mb-4">
          <CardContent className="pt-4">
            <form onSubmit={handleSubmit} className="flex flex-col gap-3">
              <div className="grid grid-cols-3 gap-3">
                <div className="flex flex-col gap-1 col-span-2"><Label>Название *</Label><Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required /></div>
                <div className="flex flex-col gap-1"><Label>Эмодзи</Label><Input value={form.emoji} onChange={(e) => setForm({ ...form, emoji: e.target.value })} /></div>
              </div>
              <div className="flex flex-col gap-1"><Label>Описание</Label><Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></div>
              <div className="grid grid-cols-3 gap-3">
                <div className="flex flex-col gap-1">
                  <Label>Правило</Label>
                  <Select value={form.ruleType} onValueChange={(value) => setForm({ ...form, ruleType: value ?? '1' })}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">Конкретный бейдж</SelectItem>
                      <SelectItem value="2">Уникальные типы</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                {form.ruleType === '1' && (
                  <div className="flex flex-col gap-1">
                    <Label>Бейдж</Label>
                    <Select value={form.linkedBadgeTypeId} onValueChange={(value) => setForm({ ...form, linkedBadgeTypeId: value ?? '' })}>
                      <SelectTrigger><SelectValue placeholder="Выберите..." /></SelectTrigger>
                      <SelectContent>{fulfilledBadges.map((badge) => <SelectItem key={badge.id} value={String(badge.id)}>{badge.label}</SelectItem>)}</SelectContent>
                    </Select>
                  </div>
                )}
                <div className="flex flex-col gap-1"><Label>Порог</Label><Input type="number" value={form.threshold} onChange={(e) => setForm({ ...form, threshold: e.target.value })} /></div>
              </div>
              <div className="flex items-center gap-4">
                <div className="flex flex-col gap-1"><Label>Порядок</Label><Input type="number" value={form.order} className="w-24" onChange={(e) => setForm({ ...form, order: e.target.value })} /></div>
                <div className="flex items-center gap-2 mt-4">
                  <input type="checkbox" id="cb-ach-active" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                  <Label htmlFor="cb-ach-active">Активно</Label>
                </div>
              </div>
              <div className="flex gap-2"><Button type="submit" size="sm">{editingItem ? 'Сохранить' : 'Создать'}</Button><Button type="button" variant="ghost" size="sm" onClick={() => setShowForm(false)}>Отмена</Button></div>
            </form>
          </CardContent>
        </Card>
      )}
      {loading ? <p className="text-sm text-muted-foreground">Загрузка...</p> : (
        <div className="flex flex-col gap-2">
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between border rounded-lg px-4 py-3 bg-card">
              <div className="flex items-center gap-3">
                <span className="text-xl">{item.emoji}</span>
                <div>
                  <div className="text-sm font-semibold">{item.name}</div>
                  <div className="text-xs text-muted-foreground">{item.description}</div>
                  <div className="text-xs text-muted-foreground mt-0.5">
                    {item.ruleType === 1
                      ? `Бейдж: ${badgeLabelById.get(item.linkedBadgeTypeId!) ?? `#${item.linkedBadgeTypeId}`}`
                      : 'Уникальные типы'
                    } · Порог: {item.threshold}
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-2">
                {!item.isActive && <Badge variant="secondary" className="text-xs">Неактивно</Badge>}
                <Button size="sm" variant="ghost" onClick={() => openEdit(item)}>✎</Button>
                <Button size="sm" variant="ghost" className="text-destructive" onClick={() => handleDelete(item.id)}>✕</Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
