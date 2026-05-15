import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getCatalogCategories, getCatalogItems, getCatalogOccasions, getCatalogPriceRange, addWishFromCatalog } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { useAuth } from '../context/AuthContext';
import type { CatalogCategoryDto, CatalogItemSummaryDto, OccasionDto, PagedResponse, WishlistSummaryDto } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Slider } from '@/components/ui/slider';
import { Bookmark } from 'lucide-react';

function ItemBadges({ item }: { item: CatalogItemSummaryDto }) {
  const topBadge = item.badges
    .filter((badge) => badge.voteCount > 0)
    .sort((a, b) => b.voteCount - a.voteCount)[0];

  if (!topBadge) return null;

  return (
    <span className="absolute bottom-2 left-2 flex items-center gap-1 px-2 py-0.5 rounded-full bg-white/90 dark:bg-black/70 text-xs font-medium shadow-sm">
      {topBadge.emoji}<span className="hidden sm:inline"> {topBadge.label}</span>
    </span>
  );
}

export default function CatalogPage() {
  const toast = useToast();
  const { user } = useAuth();

  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [occasions, setOccasions] = useState<OccasionDto[]>([]);
  const [selectedOccasionIds, setSelectedOccasionIds] = useState<string[]>([]);
  const [data, setData] = useState<PagedResponse<CatalogItemSummaryDto> | null>(null);
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
  const [addModal, setAddModal] = useState<CatalogItemSummaryDto | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);

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

  const openAddModal = async (item: CatalogItemSummaryDto) => {
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

  const hasActiveFilters = priceFilterActive || !!selectedCategory || selectedOccasionIds.length > 0;

  const filterPanelContent = (
    <div className="flex flex-col gap-4">
      <div>
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">Категории</p>
        <div className="flex flex-col gap-1">
          <button
            className={`text-sm px-3 py-1.5 rounded-md text-left transition-colors ${!selectedCategory ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-muted text-foreground'}`}
            onClick={() => setSelectedCategory(undefined)}
          >
            Все
          </button>
          {categories.map((category) => (
            <button
              key={category.id}
              className={`text-sm px-3 py-1.5 rounded-md text-left transition-colors ${selectedCategory === category.id ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-muted text-foreground'}`}
              onClick={() => setSelectedCategory(category.id)}
            >
              {category.name}
            </button>
          ))}
        </div>
      </div>

      {occasions.length > 0 && (
        <div>
          <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">Повод</p>
          <div className="flex flex-col gap-1">
            {occasions
              .filter((occasion) => occasion.label.toLowerCase() !== 'другое')
              .map((occasion) => {
                const selected = selectedOccasionIds.includes(occasion.id);
                return (
                  <button
                    key={occasion.id}
                    className={`text-sm px-3 py-1.5 rounded-md text-left transition-colors ${selected ? 'bg-primary/10 text-primary font-medium' : 'hover:bg-muted text-foreground'}`}
                    onClick={() => setSelectedOccasionIds(selected ? selectedOccasionIds.filter((occasionId) => occasionId !== occasion.id) : [...selectedOccasionIds, occasion.id])}
                  >
                    {occasion.label}
                  </button>
                );
              })}
            {selectedOccasionIds.length > 0 && (
              <button className="text-xs text-muted-foreground text-left px-3 mt-1 hover:underline" onClick={() => setSelectedOccasionIds([])}>Сбросить</button>
            )}
          </div>
        </div>
      )}

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
    </div>
  );

  return (
    <div>
      <form onSubmit={handleSearch} className="flex gap-2 mb-6">
        <Button
          type="button"
          variant="outline"
          size="sm"
          className={`md:hidden flex-shrink-0 ${hasActiveFilters ? 'border-primary text-primary' : ''}`}
          onClick={() => setFiltersOpen(true)}
        >
          Фильтры{hasActiveFilters ? ' •' : ''}
        </Button>
        <Input className="max-w-lg" placeholder="Поиск в каталоге..." value={searchInput} onChange={(e) => setSearchInput(e.target.value)} />
        <Button type="submit">Найти</Button>
      </form>

      {filtersOpen && (
        <div
          className="fixed inset-0 bg-black/40 z-40 md:hidden"
          onClick={() => setFiltersOpen(false)}
        />
      )}
      <div className={`fixed left-0 top-0 h-full w-72 bg-background border-r z-50 shadow-xl overflow-y-auto p-4 transition-transform duration-200 md:hidden ${filtersOpen ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="flex items-center justify-between mb-5">
          <span className="font-semibold">Фильтры</span>
          <button onClick={() => setFiltersOpen(false)} className="text-muted-foreground hover:text-foreground text-lg leading-none">✕</button>
        </div>
        {filterPanelContent}
      </div>

      <div className="flex gap-6">
        <aside className="hidden md:block w-48 flex-shrink-0">
          {filterPanelContent}
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
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                {data.items.map((item) => (
                  <div key={item.id} className="rounded-xl border bg-card overflow-hidden flex flex-col hover:shadow-md transition-shadow">
                    <Link to={`/catalog/items/${item.id}`} className="flex flex-col flex-1">
                      <div className="relative">
                        {item.imagePath
                          ? <img src={getImageUrl(item.imagePath) ?? ''} alt={item.name} className="w-full h-36 object-contain bg-muted" />
                          : <div className="w-full h-36 bg-muted flex items-center justify-center text-4xl">🛍️</div>
                        }
                        {item.wishCount > 0 && (
                          <span className="absolute top-2 right-2 flex items-center gap-1 px-2 py-0.5 rounded-full bg-white/90 dark:bg-black/70 text-xs font-medium text-foreground/80 shadow-sm">
                            <Bookmark className="h-3 w-3" /> {item.wishCount}
                          </span>
                        )}
                        <ItemBadges item={item} />
                      </div>
                      <div className="px-3 pt-2.5 pb-3 flex flex-col gap-1.5 flex-1">
                        <div className="font-semibold text-sm leading-snug line-clamp-2">{item.name}</div>
                        {item.price !== null && (
                          <span className="font-bold text-primary text-sm">{item.price.toLocaleString('ru-RU')} руб.</span>
                        )}
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
