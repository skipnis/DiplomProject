import { useEffect, useState } from 'react';
import { getCatalogCollections, getCatalogCollection, addWishFromCatalog } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { useAuth } from '../context/AuthContext';
import { OCCASION_LABELS } from '../types';
import type { CatalogCollectionSummaryDto, CatalogItemDto, WishlistSummaryDto } from '../types';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

export default function CollectionsPage() {
  const toast = useToast();
  const { user } = useAuth();

  const [collections, setCollections] = useState<CatalogCollectionSummaryDto[]>([]);
  const [activeCollection, setActiveCollection] = useState<CatalogCollectionSummaryDto | null>(null);
  const [collectionItems, setCollectionItems] = useState<CatalogItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [collectionLoading, setCollectionLoading] = useState(false);
  const [addModal, setAddModal] = useState<CatalogItemDto | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    getCatalogCollections()
      .then(setCollections)
      .catch(() => toast.error('Ошибка загрузки подборок'))
      .finally(() => setLoading(false));
  }, []);

  const openCollection = async (c: CatalogCollectionSummaryDto) => {
    setActiveCollection(c);
    setCollectionLoading(true);
    try { const full = await getCatalogCollection(c.id); setCollectionItems(full.items); }
    catch { toast.error('Ошибка загрузки подборки'); }
    finally { setCollectionLoading(false); }
  };

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

  const ItemGrid = ({ items }: { items: CatalogItemDto[] }) => (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
      {items.map((item) => (
        <div key={item.id} className="rounded-xl border bg-card overflow-hidden flex flex-col hover:shadow-md transition-shadow">
          {item.imagePath
            ? <img src={getImageUrl(item.imagePath) ?? ''} alt={item.name} className="w-full h-36 object-contain bg-muted" />
            : <div className="w-full h-36 bg-muted flex items-center justify-center text-4xl">🛍️</div>
          }
          <div className="p-3 flex flex-col gap-1 flex-1">
            <div className="font-semibold text-sm leading-snug line-clamp-2">{item.name}</div>
            <div className="text-xs text-muted-foreground">{item.categoryName}</div>
            {item.price !== null && <div className="font-bold text-primary text-sm">{item.price} {item.currency}</div>}
            {item.url && <a href={item.url} target="_blank" rel="noopener noreferrer" className="text-xs text-muted-foreground hover:underline" onClick={(e) => e.stopPropagation()}>Перейти →</a>}
            <Button size="sm" className="mt-auto" onClick={() => openAddModal(item)}>В вишлист</Button>
          </div>
        </div>
      ))}
    </div>
  );

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
      <div>
        <div className="mb-7">
          <button className="text-sm text-muted-foreground hover:text-foreground mb-1" onClick={() => setActiveCollection(null)}>← Все подборки</button>
          <h1 className="text-2xl font-extrabold tracking-tight">{activeCollection.name}</h1>
        </div>
        {collectionLoading ? (
          <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
        ) : collectionItems.length === 0 ? (
          <div className="text-center py-12"><div className="text-4xl mb-3">🛍️</div><p className="text-muted-foreground">Нет товаров</p></div>
        ) : (
          <ItemGrid items={collectionItems} />
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
      {collections.length === 0 ? (
        <div className="text-center py-16"><div className="text-5xl mb-4">🎁</div><p className="text-muted-foreground">Подборок пока нет</p></div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          {collections.map((c) => {
            const occasionLabel = c.occasion ? OCCASION_LABELS[c.occasion] : null;
            return (
              <div key={c.id} className="rounded-xl border bg-card overflow-hidden cursor-pointer hover:shadow-md hover:-translate-y-0.5 transition-all" onClick={() => openCollection(c)}>
                {c.coverImagePath
                  ? <img src={getImageUrl(c.coverImagePath) ?? ''} alt={c.name} className="w-full h-36 object-cover" />
                  : <div className="w-full h-36 bg-muted flex items-center justify-center text-4xl">{occasionLabel ? occasionLabel.split(' ')[0] : '🎁'}</div>
                }
                <div className="p-3">
                  <div className="font-bold text-sm">{c.name}</div>
                  {occasionLabel && <Badge variant="secondary" className="mt-1 text-xs">{occasionLabel}</Badge>}
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
