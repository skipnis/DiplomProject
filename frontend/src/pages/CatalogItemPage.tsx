import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getCatalogItem, addWishFromCatalog, voteCatalogItemBadge, unvoteCatalogItemBadge, getCatalogBadgeDefinitions } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { CatalogItemDto, CatalogItemBadgeDto, WishlistSummaryDto, CatalogBadgeDefinitionDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';

function BadgeButton({ def, myVote, voteCount, isLoggedIn, onToggle }: {
  def: CatalogBadgeDefinitionDto;
  myVote: boolean;
  voteCount: number;
  isLoggedIn: boolean;
  onToggle: () => void;
}) {
  const [hovered, setHovered] = useState(false);

  return (
    <div className="relative" onMouseEnter={() => setHovered(true)} onMouseLeave={() => setHovered(false)}>
      <button
        onClick={() => isLoggedIn ? onToggle() : undefined}
        className={[
          'flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm border transition-all',
          myVote
            ? 'border-primary bg-primary/10 text-primary font-semibold'
            : 'border-border bg-background text-muted-foreground',
          isLoggedIn && !myVote ? 'cursor-pointer hover:border-primary/50 hover:text-foreground hover:bg-muted/50' : '',
          !isLoggedIn ? 'cursor-default opacity-60' : '',
          myVote ? 'cursor-pointer' : '',
        ].join(' ')}
      >
        <span>{def.emoji}</span>
        <span className="hidden sm:inline">{def.label}</span>
        {voteCount > 0 && (
          <span className={`text-xs font-bold ${myVote ? 'text-primary' : 'text-muted-foreground'}`}>{voteCount}</span>
        )}
      </button>
      {hovered && def.description && (
        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 z-10 pointer-events-none animate-in fade-in zoom-in-95 duration-150">
          <div className="bg-popover text-popover-foreground text-xs rounded-xl px-3 py-2 shadow-md max-w-48 text-center whitespace-normal border border-border/50">
            {def.description}
          </div>
          <div className="w-2 h-2 bg-popover border-r border-b border-border/50 rotate-45 mx-auto -mt-1" />
        </div>
      )}
    </div>
  );
}

function BadgeVoting({ badges, definitions, onToggle, isLoggedIn }: {
  badges: CatalogItemBadgeDto[];
  definitions: CatalogBadgeDefinitionDto[];
  onToggle: (badgeType: number) => void;
  isLoggedIn: boolean;
}) {
  const badgeMap = new Map(badges.map((badge) => [badge.badgeType, badge]));
  const activeDefinitions = definitions.filter((def) => def.isActive);

  if (activeDefinitions.length === 0) return null;

  return (
    <div>
      <div className="text-sm font-semibold mb-3">Как подарок эта вещь...</div>
      <div className="flex flex-wrap gap-2">
        {activeDefinitions.map((def) => {
          const badge = badgeMap.get(def.id);
          return (
            <BadgeButton
              key={def.id}
              def={def}
              myVote={badge?.myVote ?? false}
              voteCount={badge?.voteCount ?? 0}
              isLoggedIn={isLoggedIn}
              onToggle={() => onToggle(def.id)}
            />
          );
        })}
      </div>
      {!isLoggedIn && (
        <p className="text-xs text-muted-foreground mt-2">Войдите в аккаунт, чтобы оценить идею подарка</p>
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
  const [badgeDefinitions, setBadgeDefinitions] = useState<CatalogBadgeDefinitionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [addModal, setAddModal] = useState(false);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlistId, setSelectedWishlistId] = useState('');
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([getCatalogItem(id), getCatalogBadgeDefinitions()])
      .then(([fetchedItem, definitions]) => {
        setItem(fetchedItem);
        setBadgeDefinitions(definitions);
      })
      .catch((e) => { toast.error(parseError(e)); navigate('/catalog'); })
      .finally(() => setLoading(false));
  }, [id]);

  const handleToggleBadge = async (badgeType: number) => {
    if (!user || !item) return;
    const existingBadge = item.badges.find((badge) => badge.badgeType === badgeType);
    const myVote = existingBadge?.myVote ?? false;
    try {
      if (myVote) {
        await unvoteCatalogItemBadge(item.id, badgeType);
        setItem((prev) => {
          if (!prev) return prev;
          return {
            ...prev,
            badges: prev.badges.map((badge) =>
              badge.badgeType === badgeType
                ? { ...badge, myVote: false, voteCount: badge.voteCount - 1 }
                : badge
            ),
          };
        });
      } else {
        await voteCatalogItemBadge(item.id, badgeType);
        setItem((prev) => {
          if (!prev) return prev;
          const definition = badgeDefinitions.find((def) => def.id === badgeType);
          const updated = prev.badges.map((badge) =>
            badge.badgeType === badgeType
              ? { ...badge, myVote: true, voteCount: badge.voteCount + 1 }
              : badge
          );
          if (!prev.badges.find((badge) => badge.badgeType === badgeType)) {
            updated.push({
              badgeType,
              emoji: definition?.emoji ?? '',
              slug: definition?.slug ?? '',
              label: definition?.label ?? '',
              myVote: true,
              voteCount: 1,
            });
          }
          return { ...prev, badges: updated };
        });
      }
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

          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-muted text-muted-foreground border mb-3">{item.categoryName}</span>
          <h1 className="text-2xl font-extrabold tracking-tight mb-3">{item.name}</h1>

          {item.price != null && (
            <div className="font-bold text-primary text-xl mb-4">
              {item.price.toLocaleString('ru-RU')} {item.currency ?? 'руб.'}
            </div>
          )}

          {item.description && (
            <div className="mb-5">
              <div className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-1.5">Описание</div>
              <p className="text-muted-foreground text-sm leading-relaxed">{item.description}</p>
            </div>
          )}

          {item.wishCount > 0 && (
            <div className="text-sm text-muted-foreground mb-4">Добавили {item.wishCount} раз</div>
          )}

          <Separator className="my-5" />

          <BadgeVoting
            badges={item.badges}
            definitions={badgeDefinitions}
            onToggle={handleToggleBadge}
            isLoggedIn={!!user}
          />

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
