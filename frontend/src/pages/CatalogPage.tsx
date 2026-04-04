import { useEffect, useState } from 'react';
import { getCatalogCategories, getCatalogItems, addWishFromCatalog } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { useAuth } from '../context/AuthContext';
import type { CatalogCategoryDto, CatalogItemDto, PagedResponse, WishlistSummaryDto } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { FieldError } from '@/components/ui/field-error';

export default function CatalogPage() {
  const toast = useToast();
  const { user } = useAuth();

  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [data, setData] = useState<PagedResponse<CatalogItemDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>();
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [minPrice, setMinPrice] = useState<number | undefined>();
  const [maxPrice, setMaxPrice] = useState<number | undefined>();
  const [addModal, setAddModal] = useState<CatalogItemDto | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);

  const priceError = minPrice != null && maxPrice != null && maxPrice < minPrice
    ? 'Максимальная цена не может быть меньше минимальной'
    : undefined;

  const load = (p: number) => {
    if (priceError) return;
    setLoading(true);
    getCatalogItems({ categoryId: selectedCategory, search, minPrice, maxPrice, page: p })
      .then(setData)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => { getCatalogCategories().then(setCategories).catch(() => {}); }, []);
  useEffect(() => { setPage(1); load(1); }, [selectedCategory, search, minPrice, maxPrice]);
  useEffect(() => { if (page > 1) load(page); }, [page]);

  const handleSearch = (e: React.FormEvent) => { e.preventDefault(); setSearch(searchInput); };

  const openAddModal = async (item: CatalogItemDto) => {
    if (!user) { toast.error('Войдите в аккаунт'); return; }
    setAddModal(item);
    const lists = await getMyWishlists().catch(() => []);
    setWishlists(lists);
    if (lists.length > 0) setSelectedWishlistId(lists[0].id);
  };

  const handleAddToWishlist = async () => {
    if (!addModal || !selectedWishlistId) return;
    setAdding(true);
    try { await addWishFromCatalog(selectedWishlistId, addModal.id); toast.success('Желание добавлено в вишлист'); setAddModal(null); }
    catch (e) { toast.error(parseError(e)); }
    finally { setAdding(false); }
  };

  return (
    <div>
      <form onSubmit={handleSearch} className="flex gap-2 mb-6">
        <Input className="max-w-lg" placeholder="Поиск в каталоге..." value={searchInput} onChange={(e) => setSearchInput(e.target.value)} />
        <Button type="submit">Найти</Button>
      </form>

      <div className="flex gap-6">
        <aside className="w-48 flex-shrink-0">
          <div className="mb-4">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">Категории</p>
            <div className="flex flex-col gap-1">
              <button
                className={`text-sm px-3 py-1.5 rounded-md text-left transition-colors ${!selectedCategory ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-muted text-foreground'}`}
                onClick={() => setSelectedCategory(undefined)}
              >
                Все
              </button>
              {categories.map((c) => (
                <button
                  key={c.id}
                  className={`text-sm px-3 py-1.5 rounded-md text-left transition-colors ${selectedCategory === c.id ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-muted text-foreground'}`}
                  onClick={() => setSelectedCategory(c.id)}
                >
                  {c.name}
                </button>
              ))}
            </div>
          </div>
          <div>
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">Цена</p>
            <div className="flex flex-col gap-2">
              <Input type="number" placeholder="От" value={minPrice ?? ''} onChange={(e) => setMinPrice(e.target.value ? Number(e.target.value) : undefined)} aria-invalid={!!priceError} />
              <Input type="number" placeholder="До" value={maxPrice ?? ''} onChange={(e) => setMaxPrice(e.target.value ? Number(e.target.value) : undefined)} aria-invalid={!!priceError} />
              <FieldError message={priceError} />
            </div>
          </div>
        </aside>

        <div className="flex-1 min-w-0">
          {loading ? (
            <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
          ) : !data?.items.length ? (
            <div className="text-center py-16">
              <div className="text-4xl mb-3">🛍️</div>
              <p className="text-muted-foreground">Ничего не найдено</p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                {data.items.map((item) => (
                  <div key={item.id} className="rounded-xl border bg-card overflow-hidden flex flex-col hover:shadow-md transition-shadow">
                    {item.imagePath
                      ? <img src={getImageUrl(item.imagePath) ?? ''} alt={item.name} className="w-full h-36 object-contain bg-muted" />
                      : <div className="w-full h-36 bg-muted flex items-center justify-center text-4xl">🛍️</div>
                    }
                    <div className="p-3 flex flex-col gap-1 flex-1">
                      <div className="font-semibold text-sm leading-snug line-clamp-2">{item.name}</div>
                      <div className="text-xs text-muted-foreground">{item.categoryName}</div>
                      {item.price !== null && <div className="font-bold text-primary text-sm">{item.price} {item.currency}</div>}
                      {item.url && (
                        <a href={item.url} target="_blank" rel="noopener noreferrer" className="text-xs text-muted-foreground hover:underline" onClick={(e) => e.stopPropagation()}>Перейти →</a>
                      )}
                      <Button size="sm" className="mt-auto" onClick={() => openAddModal(item)}>В вишлист</Button>
                    </div>
                  </div>
                ))}
              </div>
              {data && (
                <div className="flex items-center justify-center gap-3 mt-6">
                  <Button variant="ghost" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>← Назад</Button>
                  <span className="text-sm text-muted-foreground">{page} / {Math.ceil(data.totalCount / data.pageSize)}</span>
                  <Button variant="ghost" size="sm" disabled={!data.hasNextPage} onClick={() => setPage((p) => p + 1)}>Вперёд →</Button>
                </div>
              )}
            </>
          )}
        </div>
      </div>

      <Dialog open={!!addModal} onOpenChange={() => setAddModal(null)}>
        <DialogContent className="max-w-sm">
          <DialogTitle>Добавить в вишлист</DialogTitle>
          <p className="text-sm text-muted-foreground">{addModal?.name}</p>
          {wishlists.length === 0
            ? <p className="text-sm text-muted-foreground">Нет вишлистов</p>
            : <Select value={selectedWishlistId} onValueChange={(v) => setSelectedWishlistId(v ?? '')}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {wishlists.map((w) => <SelectItem key={w.id} value={w.id}>{w.name}</SelectItem>)}
                </SelectContent>
              </Select>

          }
          <div className="flex gap-2 justify-end">
            <Button variant="ghost" onClick={() => setAddModal(null)}>Отмена</Button>
            <Button disabled={adding || !selectedWishlistId} onClick={handleAddToWishlist}>{adding ? 'Добавление...' : 'Добавить'}</Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
