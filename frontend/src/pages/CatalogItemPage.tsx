import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getCatalogItem, addWishFromCatalog, rateCatalogItem, unrateCatalogItem } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { CatalogItemDto, WishlistSummaryDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';

function StarRating({ item, onRate }: { item: CatalogItemDto; onRate: (value: number | null) => void }) {
  const [hover, setHover] = useState<number | null>(null);
  const displayed = hover ?? item.myRating ?? 0;

  return (
    <div className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          className={`text-2xl leading-none transition-colors ${displayed >= star ? 'text-yellow-400' : 'text-muted-foreground/30'}`}
          onMouseEnter={() => setHover(star)}
          onMouseLeave={() => setHover(null)}
          onClick={() => onRate(item.myRating === star ? null : star)}
        >
          ★
        </button>
      ))}
      {item.ratingCount > 0 && (
        <span className="text-sm text-muted-foreground ml-1">
          {item.averageRating?.toFixed(1)} ({item.ratingCount})
        </span>
      )}
    </div>
  );
}

export default function CatalogItemPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const toast = useToast();

  const [item, setItem] = useState<CatalogItemDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [addModal, setAddModal] = useState(false);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    if (!id) return;
    getCatalogItem(id)
      .then(setItem)
      .catch((e) => { toast.error(parseError(e)); navigate('/catalog'); })
      .finally(() => setLoading(false));
  }, [id]);

  const handleRate = async (value: number | null) => {
    if (!user) { toast.error('Войдите в аккаунт'); return; }
    if (!item) return;
    try {
      if (value === null) {
        await unrateCatalogItem(item.id);
      } else {
        await rateCatalogItem(item.id, value);
      }
      setItem((prev) => prev ? { ...prev, myRating: value } : prev);
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const openAddModal = async () => {
    if (!user) { toast.error('Войдите в аккаунт'); return; }
    const lists = await getMyWishlists().catch(() => []);
    setWishlists(lists);
    if (lists.length > 0) setSelectedWishlistId(lists[0].id);
    setAddModal(true);
  };

  const handleAddToWishlist = async () => {
    if (!item || !selectedWishlistId) return;
    setAdding(true);
    try {
      await addWishFromCatalog(selectedWishlistId, item.id);
      toast.success('Желание добавлено в вишлист');
      setAddModal(false);
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setAdding(false);
    }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!item) return null;

  const imageUrl = getImageUrl(item.imagePath);

  return (
    <div className="max-w-2xl mx-auto">
      <div className="mb-6">
        <Link to="/catalog" className={buttonVariants({ variant: 'ghost', size: 'sm' })}>← Каталог</Link>
      </div>

      <Card>
        <CardContent className="pt-6">
          {imageUrl
            ? <img src={imageUrl} alt={item.name} className="w-full max-h-80 object-contain rounded-lg mb-6 bg-muted" />
            : <div className="w-full h-60 bg-muted rounded-lg flex items-center justify-center text-5xl mb-6">🛍️</div>
          }

          <div className="text-xs text-muted-foreground mb-1">{item.categoryName}</div>
          <h1 className="text-2xl font-extrabold tracking-tight mb-3">{item.name}</h1>

          {item.price != null && (
            <div className="font-bold text-primary text-xl mb-4">
              {item.price} {item.currency ?? ''}
            </div>
          )}

          {item.description && (
            <p className="text-muted-foreground text-sm mb-5 leading-relaxed">{item.description}</p>
          )}

          <StarRating item={item} onRate={handleRate} />
          {item.wishCount > 0 && (
            <div className="text-sm text-muted-foreground mt-1">{item.wishCount} в вишлистах</div>
          )}

          <Separator className="my-5" />

          <div className="flex flex-wrap gap-2">
            <Button onClick={openAddModal}>В вишлист</Button>
            {item.url && (
              <a href={item.url} target="_blank" rel="noopener noreferrer" className={buttonVariants({ variant: 'secondary' })}>
                🔗 Перейти к товару
              </a>
            )}
          </div>
        </CardContent>
      </Card>

      <Dialog open={addModal} onOpenChange={() => setAddModal(false)}>
        <DialogContent className="max-w-sm">
          <DialogTitle>Добавить в вишлист</DialogTitle>
          <p className="text-sm text-muted-foreground">{item.name}</p>
          {wishlists.length === 0
            ? <p className="text-sm text-muted-foreground">Нет вишлистов</p>
            : <Select value={selectedWishlistId} onValueChange={(value) => setSelectedWishlistId(value ?? '')}>
                <SelectTrigger>
                  <SelectValue>
                    {selectedWishlistId
                      ? (wishlists.find((wishlist) => wishlist.id === selectedWishlistId)?.name ?? selectedWishlistId)
                      : <span className="text-muted-foreground">Выберите вишлист...</span>}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {wishlists.map((wishlist) => <SelectItem key={wishlist.id} value={wishlist.id}>{wishlist.name}</SelectItem>)}
                </SelectContent>
              </Select>
          }
          <div className="flex gap-2 justify-end">
            <Button variant="ghost" onClick={() => setAddModal(false)}>Отмена</Button>
            <Button disabled={adding || !selectedWishlistId} onClick={handleAddToWishlist}>
              {adding ? 'Добавление...' : 'Добавить'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
