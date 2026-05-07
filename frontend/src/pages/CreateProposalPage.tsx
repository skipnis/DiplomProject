import { useEffect, useRef, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { createProposal, uploadProposalImage } from '../api/proposals';
import { getFriends } from '../api/friends';
import { getCatalogItems, getCatalogCategories } from '../api/catalog';
import { getMyWishlists } from '../api/wishlists';
import { getWishes } from '../api/wishes';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type {
  CatalogItemSummaryDto,
  FriendInfo,
  PagedResponse,
  WishSummaryDto,
  WishlistSummaryDto,
  CatalogCategoryDto,
} from '../types';
import { PROPOSAL_SOURCE_LABELS } from '../types';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

const HINT_PRESETS = [
  'Это то, о чём ты давно мечтаешь 🌟',
  'Я думаю, тебе это очень понравится 🎁',
  'Это напомнит тебе о чём-то важном 💭',
];

const ALIAS_PRESETS = [
  'Твой друг',
  'Твой тайный поклонник',
  'Твой добрый ангел',
  'Один умный человек',
  'Кто-то кто о тебе думает',
  'Твой Санта',
];

type SelectedGift =
  | { type: 1; item: CatalogItemSummaryDto }
  | { type: 2; wish: WishSummaryDto }
  | { type: 3; title: string; description: string; imageFile: File | null; imagePreview: string | null };

function CatalogPickerDialog({
  open,
  onClose,
  onSelect,
}: {
  open: boolean;
  onClose: () => void;
  onSelect: (item: CatalogItemSummaryDto) => void;
}) {
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [categories, setCategories] = useState<CatalogCategoryDto[]>([]);
  const [items, setItems] = useState<PagedResponse<CatalogItemSummaryDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const toast = useToast();

  useEffect(() => {
    getCatalogCategories().then(setCategories).catch(() => {});
  }, []);

  useEffect(() => {
    if (!open) return;
    setLoading(true);
    getCatalogItems({ search: search || undefined, categoryId: categoryId || undefined, page, pageSize: 12 })
      .then(setItems)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [open, search, categoryId, page]);

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Выбрать из каталога</DialogTitle>
        </DialogHeader>
        <div className="flex gap-2 mb-3">
          <Input
            placeholder="Поиск..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="flex-1"
          />
          <Select value={categoryId} onValueChange={(v) => { setCategoryId(v == null || v === 'all' ? '' : v); setPage(1); }}>
            <SelectTrigger className="w-40">
              <SelectValue placeholder="Категория" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Все</SelectItem>
              {categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
            </SelectContent>
          </Select>
        </div>
        {loading ? (
          <div className="text-center py-8 text-muted-foreground">Загрузка...</div>
        ) : !items || items.items.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">Ничего не найдено</div>
        ) : (
          <>
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
              {items.items.map((item) => (
                <button
                  key={item.id}
                  onClick={() => onSelect(item)}
                  className="text-left rounded-lg border p-2 hover:bg-muted hover:shadow-sm transition-all"
                >
                  <div className="w-full aspect-square rounded-md bg-muted mb-2 overflow-hidden">
                    {item.imagePath
                      ? <img src={getImageUrl(item.imagePath)!} alt={item.name} className="w-full h-full object-cover" />
                      : <div className="w-full h-full flex items-center justify-center text-2xl">📦</div>
                    }
                  </div>
                  <p className="text-xs font-medium line-clamp-2">{item.name}</p>
                  {item.price != null && <p className="text-xs text-muted-foreground">{item.price} {item.currency}</p>}
                </button>
              ))}
            </div>
            {(items.hasPreviousPage || items.hasNextPage) && (
              <div className="flex justify-center gap-2 mt-3">
                <Button variant="ghost" size="sm" disabled={!items.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>← Назад</Button>
                <Button variant="ghost" size="sm" disabled={!items.hasNextPage} onClick={() => setPage((p) => p + 1)}>Вперёд →</Button>
              </div>
            )}
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}

function WishlistPickerDialog({
  open,
  onClose,
  onSelect,
}: {
  open: boolean;
  onClose: () => void;
  onSelect: (wish: WishSummaryDto) => void;
}) {
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [selectedWishlist, setSelectedWishlist] = useState<WishlistSummaryDto | null>(null);
  const [wishes, setWishes] = useState<WishSummaryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const toast = useToast();

  useEffect(() => {
    if (!open) return;
    setLoading(true);
    getMyWishlists()
      .then((all) => setWishlists(all.filter((w) => !w.isSystem && w.wishCount > 0)))
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [open]);

  const selectWishlist = (wishlist: WishlistSummaryDto) => {
    setSelectedWishlist(wishlist);
    setLoading(true);
    getWishes(wishlist.id)
      .then((res) => setWishes(res.items))
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  };

  const handleClose = () => {
    setSelectedWishlist(null);
    setWishes([]);
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && handleClose()}>
      <DialogContent className="max-w-lg max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {selectedWishlist ? (
              <div className="flex items-center gap-2">
                <button onClick={() => setSelectedWishlist(null)} className="text-muted-foreground hover:text-foreground text-sm">← Назад</button>
                <span>{selectedWishlist.name}</span>
              </div>
            ) : 'Выбрать из вишлиста'}
          </DialogTitle>
        </DialogHeader>
        {loading ? (
          <div className="text-center py-8 text-muted-foreground">Загрузка...</div>
        ) : !selectedWishlist ? (
          wishlists.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Нет вишлистов с желаниями</div>
          ) : (
            <div className="flex flex-col gap-2">
              {wishlists.map((w) => (
                <button
                  key={w.id}
                  onClick={() => selectWishlist(w)}
                  className="text-left rounded-lg border px-4 py-3 hover:bg-muted transition-colors flex items-center justify-between"
                >
                  <span className="font-medium">{w.name}</span>
                  <span className="text-xs text-muted-foreground">{w.wishCount} желаний</span>
                </button>
              ))}
            </div>
          )
        ) : (
          wishes.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Нет желаний</div>
          ) : (
            <div className="flex flex-col gap-2">
              {wishes.map((wish) => (
                <button
                  key={wish.id}
                  onClick={() => onSelect(wish)}
                  className="text-left rounded-lg border px-3 py-2 hover:bg-muted transition-colors flex items-center gap-3"
                >
                  <div className="w-10 h-10 rounded-md bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-lg">
                    {wish.imagePath
                      ? <img src={getImageUrl(wish.imagePath)!} alt={wish.name} className="w-full h-full object-cover" />
                      : '🎁'
                    }
                  </div>
                  <span className="text-sm font-medium line-clamp-2">{wish.name}</span>
                </button>
              ))}
            </div>
          )
        )}
      </DialogContent>
    </Dialog>
  );
}

function CustomIdeaDialog({
  open,
  onClose,
  onSelect,
}: {
  open: boolean;
  onClose: () => void;
  onSelect: (title: string, description: string, file: File | null, preview: string | null) => void;
}) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setImageFile(file);
    const reader = new FileReader();
    reader.onload = (ev) => setImagePreview(ev.target?.result as string);
    reader.readAsDataURL(file);
  };

  const handleSubmit = () => {
    if (!title.trim()) return;
    onSelect(title.trim(), description.trim(), imageFile, imagePreview);
    setTitle('');
    setDescription('');
    setImageFile(null);
    setImagePreview(null);
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Своя идея подарка</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <Label>Название *</Label>
            <Input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Что подарить?"
              maxLength={200}
            />
          </div>
          <div>
            <Label>Описание</Label>
            <Textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Подробности..."
              maxLength={2000}
              rows={3}
            />
          </div>
          <div>
            <Label>Фото (необязательно)</Label>
            {imagePreview && (
              <img src={imagePreview} alt="" className="rounded-md w-full max-h-40 object-cover mb-2" />
            )}
            <Button variant="outline" size="sm" onClick={() => fileRef.current?.click()}>
              {imagePreview ? 'Изменить фото' : 'Добавить фото'}
            </Button>
            <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={handleFile} />
          </div>
          <Button onClick={handleSubmit} disabled={!title.trim()} className="w-full">
            Готово
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

export default function CreateProposalPage() {
  const navigate = useNavigate();
  const toast = useToast();

  const [step, setStep] = useState(1);
  const [friends, setFriends] = useState<FriendInfo[]>([]);
  const [friendsLoading, setFriendsLoading] = useState(true);
  const [friendSearch, setFriendSearch] = useState('');
  const [selectedFriend, setSelectedFriend] = useState<FriendInfo | null>(null);
  const [selectedGift, setSelectedGift] = useState<SelectedGift | null>(null);
  const [hintMessage, setHintMessage] = useState<string | null>(null);
  const [customHint, setCustomHint] = useState('');
  const [showCustomHint, setShowCustomHint] = useState(false);
  const [senderAlias, setSenderAlias] = useState<string | null>(null);
  const [customAlias, setCustomAlias] = useState('');
  const [showCustomAlias, setShowCustomAlias] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const [catalogOpen, setCatalogOpen] = useState(false);
  const [wishlistOpen, setWishlistOpen] = useState(false);
  const [customOpen, setCustomOpen] = useState(false);

  useEffect(() => {
    getFriends(1, 100)
      .then((res) => setFriends(res.items))
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setFriendsLoading(false));
  }, []);

  const filteredFriends = friends.filter((f) =>
    f.username.toLowerCase().includes(friendSearch.toLowerCase()),
  );

  const getGiftPreview = () => {
    if (!selectedGift) return null;
    if (selectedGift.type === 1) return { title: selectedGift.item.name, imgUrl: getImageUrl(selectedGift.item.imagePath) };
    if (selectedGift.type === 2) return { title: selectedGift.wish.name, imgUrl: getImageUrl(selectedGift.wish.imagePath) };
    return { title: selectedGift.title, imgUrl: selectedGift.imagePreview };
  };

  const handleSubmit = async () => {
    if (!selectedFriend || !selectedGift) return;
    setSubmitting(true);
    try {
      const proposalId = await createProposal({
        recipientId: selectedFriend.userId,
        sourceType: selectedGift.type,
        catalogItemId: selectedGift.type === 1 ? selectedGift.item.id : null,
        wishlistItemId: selectedGift.type === 2 ? selectedGift.wish.id : null,
        customTitle: selectedGift.type === 3 ? selectedGift.title : null,
        customDescription: selectedGift.type === 3 ? selectedGift.description || null : null,
        hintMessage: hintMessage || null,
        senderAlias: senderAlias || null,
      });

      if (selectedGift.type === 3 && selectedGift.imageFile) {
        await uploadProposalImage(proposalId, selectedGift.imageFile);
      }

      navigate('/proposals');
    } catch (e) {
      toast.error(parseError(e));
      setSubmitting(false);
    }
  };

  const giftPreview = getGiftPreview();

  return (
    <div className="max-w-lg mx-auto">
      <div className="flex items-center gap-3 mb-6">
        <Link to="/proposals" className="text-sm text-muted-foreground hover:text-foreground">← Назад</Link>
        <h1 className="text-xl font-bold">Предложить подарок</h1>
      </div>

      <div className="flex gap-1 mb-8">
        {[1, 2, 3, 4].map((s) => (
          <div
            key={s}
            className={`h-1 flex-1 rounded-full transition-colors ${step >= s ? 'bg-primary' : 'bg-muted'}`}
          />
        ))}
      </div>

      {step === 1 && (
        <div>
          <h2 className="font-semibold mb-4">Шаг 1 — Кому предложить?</h2>
          <Input
            placeholder="Поиск друзей..."
            value={friendSearch}
            onChange={(e) => setFriendSearch(e.target.value)}
            className="mb-3"
          />
          {friendsLoading ? (
            <div className="text-center py-8 text-muted-foreground">Загрузка...</div>
          ) : filteredFriends.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <p>Нет друзей</p>
              <Link to="/friends" className="text-primary text-sm hover:underline">Добавить друга</Link>
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              {filteredFriends.map((f) => (
                <button
                  key={f.userId}
                  onClick={() => { setSelectedFriend(f); setStep(2); }}
                  className={`flex items-center gap-3 rounded-lg border px-4 py-3 text-left transition-colors hover:bg-muted ${selectedFriend?.userId === f.userId ? 'border-primary bg-primary/5' : ''}`}
                >
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={getImageUrl(f.avatarUrl) ?? undefined} />
                    <AvatarFallback>{f.username[0].toUpperCase()}</AvatarFallback>
                  </Avatar>
                  <span className="font-medium">{f.username}</span>
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {step === 2 && (
        <div>
          <h2 className="font-semibold mb-4">Шаг 2 — Что подарить?</h2>
          {selectedGift ? (
            <div>
              <Card className="mb-4">
                <CardContent className="pt-4 flex items-center gap-4">
                  <div className="w-14 h-14 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-2xl">
                    {giftPreview?.imgUrl
                      ? <img src={giftPreview.imgUrl} alt="" className="w-full h-full object-cover" />
                      : '🎁'
                    }
                  </div>
                  <div className="flex-1 min-w-0">
                    <Badge variant="outline" className="text-xs mb-1">{PROPOSAL_SOURCE_LABELS[selectedGift.type]}</Badge>
                    <p className="font-medium line-clamp-2">{giftPreview?.title}</p>
                  </div>
                  <Button variant="ghost" size="sm" onClick={() => setSelectedGift(null)}>Изменить</Button>
                </CardContent>
              </Card>
              <Button onClick={() => setStep(3)} className="w-full">Далее →</Button>
            </div>
          ) : (
            <div className="flex flex-col gap-3">
              <Button variant="outline" className="w-full py-6 text-base" onClick={() => setCatalogOpen(true)}>
                📦 Из каталога
              </Button>
              <Button variant="outline" className="w-full py-6 text-base" onClick={() => setWishlistOpen(true)}>
                📋 Из моего вишлиста
              </Button>
              <Button variant="outline" className="w-full py-6 text-base" onClick={() => setCustomOpen(true)}>
                ✏️ Своя идея
              </Button>
            </div>
          )}

          <CatalogPickerDialog
            open={catalogOpen}
            onClose={() => setCatalogOpen(false)}
            onSelect={(item) => { setSelectedGift({ type: 1, item }); setCatalogOpen(false); }}
          />
          <WishlistPickerDialog
            open={wishlistOpen}
            onClose={() => setWishlistOpen(false)}
            onSelect={(wish) => { setSelectedGift({ type: 2, wish }); setWishlistOpen(false); }}
          />
          <CustomIdeaDialog
            open={customOpen}
            onClose={() => setCustomOpen(false)}
            onSelect={(title, description, file, preview) => {
              setSelectedGift({ type: 3, title, description, imageFile: file, imagePreview: preview });
              setCustomOpen(false);
            }}
          />

          {!selectedGift && (
            <Button variant="ghost" className="w-full mt-3 text-muted-foreground" onClick={() => setStep(1)}>
              ← Назад
            </Button>
          )}
        </div>
      )}

      {step === 3 && (
        <div>
          <h2 className="font-semibold mb-4">Шаг 3 — Записка (необязательно)</h2>

          <p className="text-sm font-medium mb-2">Подписать как</p>
          <div className="flex flex-col gap-2 mb-4">
            {ALIAS_PRESETS.map((preset) => (
              <button
                key={preset}
                onClick={() => { setSenderAlias(preset); setShowCustomAlias(false); setCustomAlias(''); }}
                className={`rounded-lg border px-4 py-3 text-sm text-left transition-colors hover:bg-muted ${senderAlias === preset && !showCustomAlias ? 'border-primary bg-primary/5' : ''}`}
              >
                {preset}
              </button>
            ))}
            <button
              onClick={() => { setShowCustomAlias(true); setSenderAlias(null); }}
              className={`rounded-lg border px-4 py-3 text-sm text-left transition-colors hover:bg-muted ${showCustomAlias ? 'border-primary bg-primary/5' : ''}`}
            >
              ✏️ Написать своё
            </button>
          </div>

          {showCustomAlias && (
            <div className="mb-4">
              <input
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                value={customAlias}
                onChange={(e) => { setCustomAlias(e.target.value); setSenderAlias(e.target.value || null); }}
                placeholder="Твой псевдоним..."
                maxLength={100}
              />
              <p className="text-xs text-amber-600 mt-1">
                Не упоминай детали, которые тебя выдадут
              </p>
            </div>
          )}

          <p className="text-sm font-medium mb-2 mt-2">Записка</p>
          <div className="flex flex-col gap-2 mb-4">
            {HINT_PRESETS.map((preset) => (
              <button
                key={preset}
                onClick={() => { setHintMessage(preset); setShowCustomHint(false); setCustomHint(''); }}
                className={`rounded-lg border px-4 py-3 text-sm text-left transition-colors hover:bg-muted ${hintMessage === preset && !showCustomHint ? 'border-primary bg-primary/5' : ''}`}
              >
                {preset}
              </button>
            ))}
            <button
              onClick={() => { setShowCustomHint(true); setHintMessage(null); }}
              className={`rounded-lg border px-4 py-3 text-sm text-left transition-colors hover:bg-muted ${showCustomHint ? 'border-primary bg-primary/5' : ''}`}
            >
              ✏️ Написать своё
            </button>
          </div>

          {showCustomHint && (
            <div className="mb-4">
              <Textarea
                value={customHint}
                onChange={(e) => { setCustomHint(e.target.value); setHintMessage(e.target.value || null); }}
                placeholder="Твоя записка..."
                maxLength={500}
                rows={3}
              />
              <p className="text-xs text-amber-600 mt-1">
                Получатель не узнает кто ты — не упоминай детали, которые тебя выдадут
              </p>
            </div>
          )}

          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => { setHintMessage(null); setStep(4); }}>
              Пропустить
            </Button>
            <Button className="flex-1" onClick={() => setStep(4)} disabled={showCustomHint && !customHint.trim()}>
              Далее →
            </Button>
          </div>
          <Button variant="ghost" className="w-full mt-2 text-muted-foreground" onClick={() => setStep(2)}>← Назад</Button>
        </div>
      )}

      {step === 4 && selectedFriend && selectedGift && (
        <div>
          <h2 className="font-semibold mb-4">Шаг 4 — Подтверждение</h2>
          <Card className="mb-6">
            <CardContent className="pt-4 space-y-4">
              <div className="flex items-center gap-3">
                <Avatar className="h-10 w-10">
                  <AvatarImage src={getImageUrl(selectedFriend.avatarUrl) ?? undefined} />
                  <AvatarFallback>{selectedFriend.username[0].toUpperCase()}</AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-medium">{selectedFriend.username}</p>
                  <p className="text-xs text-muted-foreground">Получатель</p>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-xl">
                  {giftPreview?.imgUrl
                    ? <img src={giftPreview.imgUrl} alt="" className="w-full h-full object-cover" />
                    : '🎁'
                  }
                </div>
                <div>
                  <Badge variant="outline" className="text-xs mb-1">{PROPOSAL_SOURCE_LABELS[selectedGift.type]}</Badge>
                  <p className="text-sm font-medium">{giftPreview?.title}</p>
                </div>
              </div>

              {hintMessage && (
                <div className="bg-muted/50 rounded-lg p-3">
                  <p className="text-xs text-muted-foreground mb-1">Записка:</p>
                  <p className="text-sm italic">"{hintMessage}"</p>
                </div>
              )}
            </CardContent>
          </Card>

          <Button onClick={handleSubmit} disabled={submitting} className="w-full">
            {submitting ? 'Отправляем...' : 'Отправить анонимно'}
          </Button>
          <Button variant="ghost" className="w-full mt-2 text-muted-foreground" onClick={() => setStep(3)}>← Назад</Button>
        </div>
      )}
    </div>
  );
}
