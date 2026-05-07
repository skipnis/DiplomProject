import { useEffect, useState } from 'react';
import { getCatalogCollections, getCatalogCollection, getCatalogOccasions, addWishFromCatalog } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { useAuth } from '../context/AuthContext';
import type { CatalogCollectionSummaryDto, CatalogItemSummaryDto, OccasionDto, WishlistSummaryDto } from '../types';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

export default function CollectionsPage() {
  const toast = useToast();
  const { user } = useAuth();

  const [collections, setCollections] = useState<CatalogCollectionSummaryDto[]>([]);
  const [occasions, setOccasions] = useState<OccasionDto[]>([]);
  const [occasionFilter, setOccasionFilter] = useState<string | null>(null);
  const [activeCollection, setActiveCollection] = useState<CatalogCollectionSummaryDto | null>(null);
  const [collectionItems, setCollectionItems] = useState<CatalogItemSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [collectionLoading, setCollectionLoading] = useState(false);
  const [addModal, setAddModal] = useState<CatalogItemSummaryDto | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    Promise.all([getCatalogCollections(), getCatalogOccasions()])
      .then(([cols, occ]) => { setCollections(cols); setOccasions(occ); })
      .catch(() => toast.error('Ошибка загрузки подборок'))
      .finally(() => setLoading(false));
  }, []);

  const visibleCollections = occasionFilter
    ? collections.filter((c) => c.occasion?.key === occasionFilter)
    : collections;

  const openCollection = async (c: CatalogCollectionSummaryDto) => {
    setActiveCollection(c);
    setCollectionLoading(true);
    try { const full = await getCatalogCollection(c.id); setCollectionItems(full.items); }
    catch { toast.error('Ошибка загрузки подборки'); }
    finally { setCollectionLoading(false); }
  };

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

  const AddModal = () => (
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
              <SelectContent>{wishlists.map((w) => <SelectItem key={w.id} value={w.id}>{w.name}</SelectItem>)}</SelectContent>
            </Select>
        }
        <div className="flex gap-2 justify-end">
          <Button variant="ghost" onClick={() => setAddModal(null)}>Отмена</Button>
          <Button disabled={adding || !selectedWishlistId} onClick={handleAddToWishlist}>{adding ? 'Добавление...' : 'Добавить'}</Button>
        </div>
      </DialogContent>
    </Dialog>
  );

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  if (activeCollection) {
    return (
      <div className="max-w-2xl mx-auto">
        <button className="text-sm text-muted-foreground hover:text-foreground mb-6" onClick={() => setActiveCollection(null)}>← Все подборки</button>

        <div className="mb-8">
          {activeCollection.coverImagePath && (
            <img
              src={getImageUrl(activeCollection.coverImagePath) ?? ''}
              alt={activeCollection.name}
              className="w-full h-52 object-cover rounded-2xl mb-5"
            />
          )}
          {activeCollection.occasion && (
            <Badge variant="secondary" className="mb-2">{activeCollection.occasion.label}</Badge>
          )}
          <h1 className="text-3xl font-extrabold tracking-tight mb-3">{activeCollection.name}</h1>
          {activeCollection.description && (
            <p className="text-muted-foreground leading-relaxed">{activeCollection.description}</p>
          )}
        </div>

        {collectionLoading ? (
          <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
        ) : collectionItems.length === 0 ? (
          <div className="text-center py-12"><div className="text-4xl mb-3">🛍️</div><p className="text-muted-foreground">Нет товаров</p></div>
        ) : (
          <div className="flex flex-col gap-5">
            {collectionItems.map((item) => (
              <div key={item.id} className="rounded-2xl border bg-card overflow-hidden flex flex-col sm:flex-row hover:shadow-md transition-shadow">
                {item.imagePath
                  ? <img src={getImageUrl(item.imagePath) ?? ''} alt={item.name} className="w-full sm:w-40 h-44 sm:h-auto object-contain bg-muted shrink-0" />
                  : <div className="w-full sm:w-40 h-44 sm:h-auto bg-muted flex items-center justify-center text-4xl shrink-0">🛍️</div>
                }
                <div className="p-4 flex flex-col flex-1 gap-2">
                  <div>
                    <div className="font-bold text-base leading-snug">{item.name}</div>
                    <div className="text-xs text-muted-foreground mt-0.5">{item.categoryName}</div>
                  </div>
                  {item.collectionItemDescription && (
                    <p className="text-sm text-muted-foreground leading-relaxed">{item.collectionItemDescription}</p>
                  )}
                  <div className="mt-auto flex items-center justify-between gap-3 flex-wrap">
                    <div className="flex items-center gap-3">
                      {item.price !== null && <span className="font-bold text-primary">{item.price} {item.currency}</span>}
                      {item.url && <a href={item.url} target="_blank" rel="noopener noreferrer" className="text-xs text-muted-foreground hover:underline">Перейти →</a>}
                    </div>
                    <Button size="sm" onClick={() => openAddModal(item)}>В вишлист</Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
        <AddModal />
      </div>
    );
  }

  return (
    <div>
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Подборки</h1>
      </div>
      {occasions.length > 0 && (
        <div className="flex flex-wrap gap-2 mb-6">
          <button
            onClick={() => setOccasionFilter(null)}
            className={`px-3 py-1.5 rounded-full text-sm font-medium border transition-colors ${
              occasionFilter === null
                ? 'bg-primary text-primary-foreground border-primary'
                : 'bg-transparent text-muted-foreground border-border hover:text-foreground'
            }`}
          >
            Все
          </button>
          {occasions.map((occasion) => (
            <button
              key={occasion.key}
              onClick={() => setOccasionFilter(occasionFilter === occasion.key ? null : occasion.key)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium border transition-colors ${
                occasionFilter === occasion.key
                  ? 'bg-primary text-primary-foreground border-primary'
                  : 'bg-transparent text-muted-foreground border-border hover:text-foreground'
              }`}
            >
              {occasion.label}
            </button>
          ))}
        </div>
      )}
      {visibleCollections.length === 0 ? (
        <div className="text-center py-16"><div className="text-5xl mb-4">🎁</div><p className="text-muted-foreground">Подборок пока нет</p></div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          {visibleCollections.map((c) => {
            const label = c.occasion?.label ?? null;
            return (
              <div key={c.id} className="rounded-xl border bg-card overflow-hidden cursor-pointer hover:shadow-md hover:-translate-y-0.5 transition-all flex flex-col" onClick={() => openCollection(c)}>
                {c.coverImagePath
                  ? <img src={getImageUrl(c.coverImagePath) ?? ''} alt={c.name} className="w-full h-36 object-cover" />
                  : <div className="w-full h-36 bg-muted flex items-center justify-center text-4xl">{label ? label.split(' ')[0] : '🎁'}</div>
                }
                <div className="p-3 flex flex-col flex-1">
                  <div className="font-bold text-sm leading-snug line-clamp-2 min-h-[2.5rem]">{c.name}</div>
                  <div className="mt-1 min-h-[1.5rem]">
                    {label && <Badge variant="secondary" className="text-xs">{label}</Badge>}
                  </div>
                  <div className="text-xs text-muted-foreground mt-1">{c.itemCount} товаров</div>
                </div>
              </div>
            );
          })}
        </div>
      )}
      <AddModal />
    </div>
  );
}
