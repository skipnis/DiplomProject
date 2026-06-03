import { useEffect, useState, useRef } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getMyWishlists, getMyFulfilledWishes } from '../api/wishlists';
import {
  deleteMyAccount, requestAccountDeletion, getUserProfile, getMyGiftProfile, getMyBlacklist,
  addBlacklistItem, deleteBlacklistItem, updateMyProfile, uploadAvatar, deleteAvatar,
} from '../api/users';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError, parseApiFieldErrors, ApiError } from '../utils/errors';
import { profileSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { VISIBILITY_LABELS, CURRENCY_LABELS, getWishlistEmoji } from '../types';
import type { WishlistSummaryDto, UserProfile, GiftProfileDto, FulfilledWishRecordDto, BlacklistItemDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { FieldError } from '@/components/ui/field-error';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { OTP_CODE_LENGTH } from '@/lib/utils';

const MAX_LEVEL_SENTINEL = 2147483647;

const LEVEL_COLORS: Record<number, string> = {
  1: 'bg-muted text-muted-foreground',
  2: 'bg-green-100 text-green-700',
  3: 'bg-blue-100 text-blue-700',
  4: 'bg-purple-100 text-purple-700',
  5: 'bg-orange-100 text-orange-700',
  6: 'bg-yellow-100 text-yellow-800',
};

function GiftProfileSection({ giftProfile }: { giftProfile: GiftProfileDto }) {
  const isMaxLevel = giftProfile.nextLevelThreshold === MAX_LEVEL_SENTINEL;
  const progressPercent = !isMaxLevel && giftProfile.nextLevelThreshold > 0
    ? Math.min(100, Math.round((giftProfile.giftsGiven / giftProfile.nextLevelThreshold) * 100))
    : 100;

  const earnedAchievements = giftProfile.achievements.filter((achievement) => achievement.isEarned);

  return (
    <div className="space-y-5">
      <div className="flex items-center gap-3 flex-wrap">
        <span className={`px-3 py-1 rounded-full text-sm font-bold ${LEVEL_COLORS[giftProfile.level] ?? 'bg-muted text-muted-foreground'}`}>
          {giftProfile.levelName}
        </span>
        <div className="flex gap-4 text-sm">
          <span><span className="font-bold">{giftProfile.giftsGiven}</span> <span className="text-muted-foreground">подарено</span></span>
          {earnedAchievements.length > 0 && (
            <span><span className="font-bold">{earnedAchievements.length}</span> <span className="text-muted-foreground">достижений</span></span>
          )}
        </div>
      </div>

      {!isMaxLevel && (
        <div>
          <div className="flex justify-between text-xs text-muted-foreground mb-1">
            <span>До следующего уровня</span>
            <span>{giftProfile.giftsGiven} / {giftProfile.nextLevelThreshold}</span>
          </div>
          <div className="w-full bg-muted rounded-full h-2">
            <div className="bg-primary h-2 rounded-full transition-all" style={{ width: `${progressPercent}%` }} />
          </div>
        </div>
      )}

      {earnedAchievements.length > 0 && (
        <div>
          <div className="text-sm font-semibold mb-2">Достижения</div>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
            {earnedAchievements.map((achievement) => (
              <div key={achievement.definitionId} className="flex items-start gap-2 p-2.5 rounded-lg border bg-card">
                <span className="text-xl">{achievement.emoji}</span>
                <div>
                  <div className="text-xs font-semibold">{achievement.name}</div>
                  <div className="text-xs text-muted-foreground">{achievement.description}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {giftProfile.badgesReceived.length > 0 && (
        <div>
          <div className="text-sm font-semibold mb-2">Полученные бейджи</div>
          <div className="flex flex-wrap gap-2">
            {giftProfile.badgesReceived.map((badgeCount) => (
              <span
                key={badgeCount.badgeType}
                className="flex items-center gap-1.5 px-3 py-1 rounded-full text-sm border border-primary/20 bg-primary/5 text-primary"
              >
                {badgeCount.emoji} {badgeCount.label}
                <span className="font-bold text-xs">×{badgeCount.count}</span>
              </span>
            ))}
          </div>
        </div>
      )}

      {giftProfile.giftsGiven === 0 && (
        <p className="text-sm text-muted-foreground">Подарите кому-нибудь желание — здесь появится ваш подарочный профиль.</p>
      )}
    </div>
  );
}

const BLACKLIST_PRESETS = [
  'Носки', 'Парфюм', 'Алкоголь', 'Цветы',
  'Книги по саморазвитию', 'Дешёвые безделушки', 'Сладкое', 'Подарочные сертификаты',
];

const MAX_BLACKLIST_ITEMS = 5;

export default function ProfilePage() {
  const { user, logout, refreshUser } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const toast = useToast();

  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [stats, setStats] = useState<Pick<UserProfile, 'receivedCount' | 'giftedCount'> | null>(null);
  const [giftProfile, setGiftProfile] = useState<GiftProfileDto | null>(null);
  const [fulfilledWishes, setFulfilledWishes] = useState<FulfilledWishRecordDto[]>([]);
  const [blacklistItems, setBlacklistItems] = useState<BlacklistItemDto[]>([]);
  const [blacklistInput, setBlacklistInput] = useState('');
  const [blacklistSaving, setBlacklistSaving] = useState(false);
  const [loading, setLoading] = useState(true);

  const [deleteLoading, setDeleteLoading] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteStep, setDeleteStep] = useState<'confirm' | 'code'>('confirm');
  const [deleteCode, setDeleteCode] = useState('');

  const [displayName, setDisplayName] = useState('');
  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [showFulfilledWishes, setShowFulfilledWishes] = useState(false);
  const [saving, setSaving] = useState(false);
  const [avatarUploading, setAvatarUploading] = useState(false);
  const [previewAvatarUrl, setPreviewAvatarUrl] = useState<string | null>(null);
  const [formErrors, setFormErrors] = useState<FormErrors>({});
  const avatarInputRef = useRef<HTMLInputElement>(null);

  const defaultTab = searchParams.get('tab') ?? 'profile';

  useEffect(() => {
    if (!user) return;
    Promise.all([getMyWishlists(), getUserProfile(user.id), getMyGiftProfile(), getMyFulfilledWishes(), getMyBlacklist()])
      .then(([fetchedWishlists, profile, fetchedGiftProfile, fetchedFulfilledWishes, fetchedBlacklist]) => {
        setWishlists(fetchedWishlists);
        setStats({ receivedCount: profile.receivedCount, giftedCount: profile.giftedCount });
        setGiftProfile(fetchedGiftProfile);
        setFulfilledWishes(fetchedFulfilledWishes);
        setBlacklistItems(fetchedBlacklist);
      })
      .finally(() => setLoading(false));
  }, [user?.id]);

  useEffect(() => {
    if (!user) return;
    setDisplayName(user.displayName);
    setUsername(user.username ?? '');
    setBio(user.bio ?? '');
    setBirthDate(user.birthDate ?? '');
    setShowFulfilledWishes(user.showFulfilledWishes);
  }, [user]);

  function clearFormError(field: string) {
    if (formErrors[field]) setFormErrors((prev) => ({ ...prev, [field]: '' }));
  }

  async function handleAvatarChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    const localPreview = URL.createObjectURL(file);
    setPreviewAvatarUrl(localPreview);
    setAvatarUploading(true);
    try {
      await uploadAvatar(file);
      setPreviewAvatarUrl(null);
      await refreshUser();
      toast.success('Аватар обновлён');
    } catch (err) {
      setPreviewAvatarUrl(null);
      toast.error(parseError(err));
    } finally {
      setAvatarUploading(false);
      URL.revokeObjectURL(localPreview);
      if (avatarInputRef.current) avatarInputRef.current.value = '';
    }
  }

  async function handleDeleteAvatar() {
    setAvatarUploading(true);
    try {
      await deleteAvatar();
      await refreshUser();
      setPreviewAvatarUrl(null);
      toast.success('Аватар удалён');
    } catch {
      toast.error('Не удалось удалить аватар');
    } finally {
      setAvatarUploading(false);
    }
  }

  async function handleSaveProfile(e: React.FormEvent) {
    e.preventDefault();
    const result = profileSchema.safeParse({ displayName, username, bio: bio || undefined });
    if (!result.success) { setFormErrors(parseZodErrors(result.error)); return; }
    setFormErrors({});
    setSaving(true);
    try {
      await updateMyProfile({ displayName, username, bio: bio || null, birthDate: birthDate || null, showFulfilledWishes });
      await refreshUser();
      toast.success('Профиль сохранён');
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setFormErrors((prev) => ({ ...prev, username: 'Этот username уже занят' }));
      } else {
        const fieldErrors = parseApiFieldErrors(err);
        if (fieldErrors) setFormErrors((prev) => ({ ...prev, ...fieldErrors }));
        else toast.error(parseError(err));
      }
    } finally {
      setSaving(false);
    }
  }

  async function handleAddBlacklistItem(title: string) {
    if (!title.trim() || blacklistItems.length >= MAX_BLACKLIST_ITEMS) return;
    if (blacklistItems.some((item) => item.title === title.trim())) return;
    setBlacklistSaving(true);
    try {
      const newItem = await addBlacklistItem(title.trim());
      setBlacklistItems((prev) => [...prev, newItem]);
      setBlacklistInput('');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setBlacklistSaving(false);
    }
  }

  async function handleRemoveBlacklistItem(itemId: string) {
    try {
      await deleteBlacklistItem(itemId);
      setBlacklistItems((prev) => prev.filter((item) => item.id !== itemId));
    } catch (e) {
      toast.error(parseError(e));
    }
  }

  async function handleRequestDeletion() {
    setDeleteLoading(true);
    try {
      await requestAccountDeletion();
      setDeleteStep('code');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setDeleteLoading(false);
    }
  }

  async function handleConfirmDelete() {
    if (deleteCode.length !== 6) return;
    setDeleteLoading(true);
    try {
      await deleteMyAccount(deleteCode);
      await logout();
      navigate('/');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setDeleteLoading(false);
    }
  }

  function handleShareProfile() {
    navigator.clipboard.writeText(`${window.location.origin}/users/${user!.id}`);
    toast.success('Ссылка скопирована');
  }

  function handleDeleteDialogOpenChange(open: boolean) {
    setDeleteDialogOpen(open);
    if (!open) {
      setDeleteStep('confirm');
      setDeleteCode('');
    }
  }

  if (!user) return null;

  return (
    <div>
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6 flex-wrap">
            <Avatar className="h-20 w-20">
              <AvatarImage src={getImageUrl(user.avatarPath) ?? user.avatarUrl ?? undefined} alt={user.displayName} />
              <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
                {user.displayName[0].toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h1 className="text-xl font-extrabold tracking-tight">{user.displayName}</h1>
              {user.username && <div className="text-sm text-muted-foreground">@{user.username}</div>}
              {user.bio && <div className="text-sm mt-1">{user.bio}</div>}
              {user.birthDate && (
                <div className="text-sm text-muted-foreground mt-1">
                  🎂 {new Date(user.birthDate).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}
                </div>
              )}
              {stats && (
                <div className="flex gap-4 mt-2 text-sm">
                  <span><span className="font-bold">{stats.receivedCount}</span> <span className="text-muted-foreground">получено</span></span>
                  <span><span className="font-bold">{stats.giftedCount}</span> <span className="text-muted-foreground">подарено</span></span>
                </div>
              )}
            </div>
            <Button variant="outline" onClick={handleShareProfile}>Поделиться</Button>
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue={defaultTab}>
        <TabsList className="mb-5 w-full">
          <TabsTrigger value="profile" className="flex-1">Профиль</TabsTrigger>
          <TabsTrigger value="history" className="flex-1">История</TabsTrigger>
          <TabsTrigger value="settings" className="flex-1">Настройки</TabsTrigger>
        </TabsList>

        <TabsContent value="profile" className="space-y-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Загрузка...</p>
          ) : (
            <>
              {giftProfile && (
                <Card>
                  <CardContent className="pt-6">
                    <div className="text-base font-bold mb-4">Подарочный профиль</div>
                    <GiftProfileSection giftProfile={giftProfile} />
                  </CardContent>
                </Card>
              )}

              <div>
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-bold">Мои вишлисты</h2>
                  <Link to="/wishlists" className={buttonVariants({ variant: 'ghost', size: 'sm' })}>Все вишлисты</Link>
                </div>
                {wishlists.length === 0 ? (
                  <div className="text-center py-12">
                    <div className="text-4xl mb-3">📋</div>
                    <p className="font-semibold mb-3">Нет вишлистов</p>
                    <Link to="/wishlists/new" className={buttonVariants()}>Создать первый</Link>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {wishlists.slice(0, 6).map((wishlist) => (
                      <Link key={wishlist.id} to={`/wishlists/${wishlist.id}`} className="block rounded-xl border bg-card p-5 hover:shadow-md hover:-translate-y-0.5 transition-all">
                        <div className="text-3xl mb-2">{getWishlistEmoji(wishlist)}</div>
                        <div className="font-bold text-sm">{wishlist.name}</div>
                        <div className="flex gap-3 mt-auto pt-2 text-xs text-muted-foreground flex-wrap">
                          <span>{wishlist.wishCount} желаний</span>
                          {wishlist.fulfilledWishCount > 0 && <span className="text-green-600 font-medium">✓ {wishlist.fulfilledWishCount} исполнено</span>}
                          <span>{VISIBILITY_LABELS[wishlist.visibility]}</span>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}
              </div>
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between mb-3">
                    <div>
                      <div className="text-base font-bold">Не хочу получать</div>
                      <div className="text-xs text-muted-foreground">Видят только твои друзья</div>
                    </div>
                    <span className="text-xs text-muted-foreground">{blacklistItems.length}/{MAX_BLACKLIST_ITEMS}</span>
                  </div>

                  {blacklistItems.length > 0 && (
                    <div className="flex flex-wrap gap-2 mb-3">
                      {blacklistItems.map((item) => (
                        <span
                          key={item.id}
                          className="flex items-center gap-1.5 px-3 py-1 rounded-full border border-destructive/30 bg-destructive/5 text-sm"
                        >
                          {item.title}
                          <button
                            type="button"
                            onClick={() => handleRemoveBlacklistItem(item.id)}
                            className="text-muted-foreground hover:text-destructive transition-colors ml-0.5"
                            aria-label="Удалить"
                          >
                            ✕
                          </button>
                        </span>
                      ))}
                    </div>
                  )}

                  {blacklistItems.length < MAX_BLACKLIST_ITEMS && (
                    <>
                      <div className="flex flex-wrap gap-1.5 mb-3">
                        {BLACKLIST_PRESETS.filter((preset) => !blacklistItems.some((item) => item.title === preset)).map((preset) => (
                          <button
                            key={preset}
                            type="button"
                            disabled={blacklistSaving}
                            onClick={() => handleAddBlacklistItem(preset)}
                            className="px-2.5 py-1 rounded-full border border-muted-foreground/30 text-xs hover:border-primary hover:bg-muted transition-colors"
                          >
                            + {preset}
                          </button>
                        ))}
                      </div>
                      <div className="flex gap-2">
                        <Input
                          placeholder="Своя позиция..."
                          value={blacklistInput}
                          onChange={(e) => setBlacklistInput(e.target.value)}
                          onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddBlacklistItem(blacklistInput))}
                          maxLength={100}
                          className="text-sm"
                        />
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          disabled={!blacklistInput.trim() || blacklistSaving}
                          onClick={() => handleAddBlacklistItem(blacklistInput)}
                        >
                          +
                        </Button>
                      </div>
                    </>
                  )}
                </CardContent>
              </Card>
            </>
          )}
        </TabsContent>

        <TabsContent value="history">
          {loading ? (
            <p className="text-sm text-muted-foreground">Загрузка...</p>
          ) : fulfilledWishes.length === 0 ? (
            <div className="text-center py-16">
              <div className="text-4xl mb-3">🎁</div>
              <p className="font-semibold mb-1">История пуста</p>
              <p className="text-sm text-muted-foreground">Здесь будут появляться исполненные желания — даже если вишлист удалят.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {fulfilledWishes.map((record) => {
                const imageUrl = getImageUrl(record.imagePath);
                return (
                  <Card key={record.id}>
                    <CardContent className="pt-5">
                      <div className="flex gap-4">
                        {imageUrl && (
                          <img
                            src={imageUrl}
                            alt={record.wishName}
                            className="w-16 h-16 rounded-lg object-cover flex-shrink-0 bg-muted"
                          />
                        )}
                        <div className="flex-1 min-w-0">
                          <div className="font-semibold text-sm truncate">{record.wishName}</div>
                          {record.wishDescription && (
                            <div className="text-xs text-muted-foreground mt-0.5 line-clamp-2">{record.wishDescription}</div>
                          )}
                          <div className="flex flex-wrap gap-x-4 gap-y-1 mt-2 text-xs text-muted-foreground">
                            {record.price != null && record.currency != null && (
                              <span className="font-semibold text-primary">
                                {record.price} {CURRENCY_LABELS[record.currency]}
                              </span>
                            )}
                            <span>из «{record.wishlistName}»</span>
                            {record.gifterDisplayName && (
                              <span>подарил <span className="font-medium text-foreground">{record.gifterDisplayName}</span></span>
                            )}
                          </div>
                          <div className="text-xs text-muted-foreground mt-1">
                            {new Date(record.fulfilledAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </TabsContent>

        <TabsContent value="settings" className="space-y-6">
          <Card>
            <CardContent className="pt-6">
              <div className="font-semibold mb-5">Аккаунт</div>
              <form onSubmit={handleSaveProfile} className="flex flex-col gap-4">
                <div className="flex flex-col gap-1.5">
                  <Label>Фото профиля</Label>
                  <div className="flex items-center gap-4">
                    <Avatar className="h-16 w-16">
                      <AvatarImage
                        src={previewAvatarUrl ?? getImageUrl(user.avatarPath) ?? user.avatarUrl ?? undefined}
                        alt={user.displayName}
                      />
                      <AvatarFallback className="bg-primary text-primary-foreground text-xl font-bold">
                        {user.displayName[0].toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex gap-2 flex-wrap">
                      <input
                        ref={avatarInputRef}
                        type="file"
                        accept="image/*"
                        className="hidden"
                        onChange={handleAvatarChange}
                      />
                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        disabled={avatarUploading}
                        onClick={() => avatarInputRef.current?.click()}
                      >
                        {avatarUploading ? 'Загрузка...' : 'Загрузить фото'}
                      </Button>
                      {user.avatarPath && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          disabled={avatarUploading}
                          onClick={handleDeleteAvatar}
                        >
                          Удалить
                        </Button>
                      )}
                    </div>
                  </div>
                </div>

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="displayName">Имя</Label>
                  <Input
                    id="displayName"
                    value={displayName}
                    onChange={(e) => { setDisplayName(e.target.value); clearFormError('displayName'); }}
                    placeholder="Алексей Кипнис"
                    aria-invalid={!!formErrors.displayName}
                  />
                  <FieldError message={formErrors.displayName} />
                </div>

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="username">Username</Label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground select-none">@</span>
                    <Input
                      id="username"
                      value={username}
                      onChange={(e) => { setUsername(e.target.value); clearFormError('username'); }}
                      placeholder="alexkipnis"
                      className="pl-7"
                      aria-invalid={!!formErrors.username}
                    />
                  </div>
                  <FieldError message={formErrors.username} />
                </div>

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="bio">О себе</Label>
                  <Textarea
                    id="bio"
                    value={bio}
                    onChange={(e) => { setBio(e.target.value); clearFormError('bio'); }}
                    placeholder="Расскажи о себе..."
                    rows={3}
                    aria-invalid={!!formErrors.bio}
                  />
                  <FieldError message={formErrors.bio} />
                </div>

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="birthDate">Дата рождения</Label>
                  <Input id="birthDate" type="date" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
                </div>

                <label className="flex items-center gap-3 cursor-pointer select-none">
                  <input
                    type="checkbox"
                    checked={showFulfilledWishes}
                    onChange={(e) => setShowFulfilledWishes(e.target.checked)}
                    className="w-4 h-4 rounded border-input accent-primary"
                  />
                  <span className="text-sm font-medium leading-none">Показывать исполненные желания в профиле</span>
                </label>

                <div className="flex justify-end">
                  <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</Button>
                </div>
              </form>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6 flex items-center justify-between gap-4">
              <p className="text-sm text-muted-foreground">Удаление аккаунта необратимо — все данные будут уничтожены.</p>
              <Button variant="destructive" size="sm" className="shrink-0" onClick={() => setDeleteDialogOpen(true)}>
                Удалить аккаунт
              </Button>
            </CardContent>
          </Card>

          <Dialog open={deleteDialogOpen} onOpenChange={handleDeleteDialogOpenChange}>
            <DialogContent>
              {deleteStep === 'confirm' ? (
                <>
                  <DialogHeader>
                    <DialogTitle>Удалить аккаунт?</DialogTitle>
                    <DialogDescription>
                      Это действие необратимо. Все вишлисты, желания и данные будут удалены навсегда.
                      На ваш email придёт код подтверждения.
                    </DialogDescription>
                  </DialogHeader>
                  <DialogFooter>
                    <Button variant="outline" onClick={() => handleDeleteDialogOpenChange(false)}>
                      Отмена
                    </Button>
                    <Button variant="destructive" onClick={handleRequestDeletion} disabled={deleteLoading}>
                      {deleteLoading ? 'Отправка...' : 'Получить код'}
                    </Button>
                  </DialogFooter>
                </>
              ) : (
                <>
                  <DialogHeader>
                    <DialogTitle>Введите код подтверждения</DialogTitle>
                    <DialogDescription>
                      Код отправлен на {user.email}. Введите его, чтобы подтвердить удаление.
                    </DialogDescription>
                  </DialogHeader>
                  <Input
                    value={deleteCode}
                    onChange={(e) => setDeleteCode(e.target.value)}
                    placeholder="000000"
                    maxLength={OTP_CODE_LENGTH}
                    inputMode="numeric"
                    autoFocus
                  />
                  <DialogFooter>
                    <Button variant="outline" onClick={() => handleDeleteDialogOpenChange(false)}>
                      Отмена
                    </Button>
                    <Button
                      variant="destructive"
                      onClick={handleConfirmDelete}
                      disabled={deleteLoading || deleteCode.length !== 6}
                    >
                      {deleteLoading ? 'Удаление...' : 'Удалить аккаунт'}
                    </Button>
                  </DialogFooter>
                </>
              )}
            </DialogContent>
          </Dialog>
        </TabsContent>
      </Tabs>
    </div>
  );
}
