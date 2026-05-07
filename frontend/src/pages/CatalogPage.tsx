import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getCatalogCategories, getCatalogItems, getCatalogOccasions, getCatalogPriceRange, addWishFromCatalog } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { useAuth } from '../context/AuthContext';
import type { CatalogCategoryDto, CatalogItemDto, OccasionDto, PagedResponse, WishlistSummaryDto } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Slider } from '@/components/ui/slider';

function ItemBadges({ item }: { item: CatalogItemDto }) {
  const topBadge = item.badges
    .filter((badge) => badge.voteCount > 0)
    .sort((a, b) => b.voteCount - a.voteCount)[0];

  if (!topBadge) return null;

  return (
    <span className="text-xs px-1.5 py-0.5 rounded-full bg-primary/8 text-primary border border-primary/20 leading-tight">
      {topBadge.emoji} {topBadge.label}
    </span>
  );
}

export default function CatalogPage() {
  const toast = useToast();
  const { user } = useAuth();

  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [occasions, setOccasions] = useState<OccasionDto[]>([]);
  const [selectedOccasionIds, setSelectedOccasionIds] = useState<string[]>([]);
  const [data, setData] = useState<PagedResponse<CatalogItemDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>();
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const FALLBACK_MAX_PRICE = 100000;
  const [maxCatalogPrice, setMaxCatalogPrice] = useState<number>(FALLBACK_MAX_PRICE);
  const [priceRange, setPriceRange] = useState<[number, number]>([1, FALLBACK_MAX_PRICE]);
  const priceFilterActive = priceRange[0] > 1 || priceRange[1] < maxCatalogPrice;
  const minPrice = priceFilterActive ? priceRange[0] : undefined;
  const maxPrice = priceFilterActive ? priceRange[1] : undefined;
  const [addModal, setAddModal] = useState<CatalogItemDto | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);

  const load = (p: number) => {
    setLoading(true);
    getCatalogItems({ categoryId: selectedCategory, search, minPrice, maxPrice, occasionIds: selectedOccasionIds.length ? selectedOccasionIds : undefined, page: p })
      .then(setData)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    getCatalogCategories().then(setCategories).catch(() => {});
    getCatalogOccasions().then(setOccasions).catch(() => {});
    getCatalogPriceRange().then(({ max }) => {
      if (max > 0) {
        setMaxCatalogPrice(max);
        setPriceRange([1, max]);
      }
    }).catch(() => {});
  }, []);
  useEffect(() => { setPage(1); load(1); }, [selectedCategory, search, priceRange, selectedOccasionIds]);
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
            <div className="flex flex-col gap-3">
              <Slider
                min={1}
                max={maxCatalogPrice}
                step={50}
                value={priceRange}
                onValueChange={(v) => { const r = v as number[]; setPriceRange([r[0], r[1]]); }}
              />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>{priceRange[0].toLocaleString('ru-RU')} р.</span>
                <span>{priceRange[1].toLocaleString('ru-RU')} р.</span>
              </div>
            </div>
          </div>
          {occasions.length > 0 && (
            <div className="mt-4">
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">Повод</p>
              <div className="flex flex-col gap-1">
                {occasions.map((o) => {
                  const selected = selectedOccasionIds.includes(o.id);
                  return (
                    <button
                      key={o.id}
                      className={`text-sm px-3 py-1.5 rounded-md text-left transition-colors ${selected ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-muted text-foreground'}`}
                      onClick={() => setSelectedOccasionIds(selected ? selectedOccasionIds.filter((id) => id !== o.id) : [...selectedOccasionIds, o.id])}
                    >
                      {o.label}
                    </button>
                  );
                })}
                {selectedOccasionIds.length > 0 && (
                  <button className="text-xs text-muted-foreground text-left px-3 mt-1 hover:underline" onClick={() => setSelectedOccasionIds([])}>Сбросить</button>
                )}
              </div>
            </div>
          )}
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
                    <Link to={`/catalog/items/${item.id}`} className="flex flex-col flex-1">
                      {item.imagePath
                        ? <img src={getImageUrl(item.imagePath) ?? ''} alt={item.name} className="w-full h-36 object-contain bg-muted" />
                        : <div className="w-full h-36 bg-muted flex items-center justify-center text-4xl">🛍️</div>
                      }
                      <div className="px-3 pt-2.5 pb-3 flex flex-col gap-1.5 flex-1">
                        <div className="font-semibold text-sm leading-snug line-clamp-2">{item.name}</div>
                        <div className="flex items-center justify-between gap-1">
                          {item.price !== null && (
                            <span className="font-bold text-primary text-sm whitespace-nowrap">{item.price} {item.currency}</span>
                          )}
                        </div>
                        <div className="flex items-center justify-between mt-auto pt-1">
                          <ItemBadges item={item} />
                          {item.wishCount > 0 && (
                            <span className="text-xs text-muted-foreground">♥ {item.wishCount}</span>
                          )}
                        </div>
                      </div>
                    </Link>
                    <div className="px-3 pb-3">
                      <Button size="sm" className="w-full" onClick={(e) => { e.preventDefault(); openAddModal(item); }}>В вишлист</Button>
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
                <SelectTrigger>
                  <SelectValue>
                    {selectedWishlistId
                      ? (wishlists.find((w) => w.id === selectedWishlistId)?.name ?? selectedWishlistId)
                      : <span className="text-muted-foreground">Выберите вишлист...</span>}
                  </SelectValue>
                </SelectTrigger>
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
