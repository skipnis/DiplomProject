import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useGoogleLogin } from '@react-oauth/google';
import { useAuth } from '../context/AuthContext';
import { getMyWishlists } from '../api/wishlists';
import { connectGoogleCalendar, disconnectGoogleCalendar, deleteMyAccount, getUserProfile, getMyGiftProfile } from '../api/users';
import { syncAllEvents } from '../api/events';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { VISIBILITY_LABELS, getWishlistEmoji } from '../types';
import type { WishlistSummaryDto, UserProfile, GiftProfileDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog';

const LEVEL_COLORS: Record<number, string> = {
  1: 'bg-muted text-muted-foreground',
  2: 'bg-green-100 text-green-700',
  3: 'bg-blue-100 text-blue-700',
  4: 'bg-purple-100 text-purple-700',
  5: 'bg-orange-100 text-orange-700',
  6: 'bg-yellow-100 text-yellow-800',
};

function GiftProfileSection({ giftProfile }: { giftProfile: GiftProfileDto }) {
  const isMaxLevel = giftProfile.nextLevelThreshold === 2147483647;
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
          {giftProfile.hitRate > 0 && (
            <span><span className="font-bold">{Math.round(giftProfile.hitRate * 100)}%</span> <span className="text-muted-foreground">с бейджем</span></span>
          )}
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

export default function ProfilePage() {
  const { user, refreshUser, logout } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [stats, setStats] = useState<Pick<UserProfile, 'receivedCount' | 'giftedCount'> | null>(null);
  const [giftProfile, setGiftProfile] = useState<GiftProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [calendarLoading, setCalendarLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    if (!user) return;
    Promise.all([getMyWishlists(), getUserProfile(user.id), getMyGiftProfile()])
      .then(([fetchedWishlists, profile, fetchedGiftProfile]) => {
        setWishlists(fetchedWishlists);
        setStats({ receivedCount: profile.receivedCount, giftedCount: profile.giftedCount });
        setGiftProfile(fetchedGiftProfile);
      })
      .finally(() => setLoading(false));
  }, [user?.id]);

  const connectCalendar = useGoogleLogin({
    flow: 'auth-code',
    scope: 'https://www.googleapis.com/auth/calendar.events',
    onSuccess: async (response) => {
      setCalendarLoading(true);
      try {
        await connectGoogleCalendar(response.code);
        await refreshUser();
        await syncAllEvents().catch(() => {});
        toast.success('Google Calendar подключён');
      } catch (e) {
        toast.error(parseError(e));
      } finally {
        setCalendarLoading(false);
      }
    },
    onError: () => toast.error('Не удалось подключить Google Calendar'),
  });

  async function handleDeleteAccount() {
    setDeleteLoading(true);
    try {
      await deleteMyAccount();
      await logout();
      navigate('/');
    } catch (e) {
      toast.error(parseError(e));
      setDeleteLoading(false);
    }
  }

  async function handleDisconnectCalendar() {
    setCalendarLoading(true);
    try {
      await disconnectGoogleCalendar();
      await refreshUser();
      toast.success('Google Calendar отключён');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setCalendarLoading(false);
    }
  }

  if (!user) return null;

  return (
    <div>
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6 flex-wrap">
            <Avatar className="h-20 w-20">
              <AvatarImage src={getImageUrl(user.avatarUrl) ?? user.avatarUrl ?? undefined} alt={user.displayName} />
              <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
                {user.displayName[0].toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h1 className="text-xl font-extrabold tracking-tight">{user.displayName}</h1>
              {user.username && <div className="text-sm text-muted-foreground">@{user.username}</div>}
              <div className="text-sm text-muted-foreground">{user.email}</div>
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
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="profile">
        <TabsList className="mb-5 w-full">
          <TabsTrigger value="profile" className="flex-1">Профиль</TabsTrigger>
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
            </>
          )}
        </TabsContent>

        <TabsContent value="settings" className="space-y-4">
          <Card>
            <CardContent className="pt-6 space-y-3">
              <div className="font-semibold mb-1">Аккаунт</div>
              <Link to="/profile/edit" className={buttonVariants({ variant: 'secondary', size: 'sm' })}>Редактировать профиль</Link>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-between mb-3">
                <span className="font-semibold">Google Calendar</span>
                {user.isGoogleCalendarConnected && <Badge variant="secondary" className="bg-green-100 text-green-700">Подключён</Badge>}
              </div>
              {user.isGoogleCalendarConnected ? (
                <div className="flex items-center gap-3 flex-wrap">
                  <span className="text-sm text-muted-foreground">Синхронизация событий работает автоматически.</span>
                  <Button variant="ghost" size="sm" onClick={handleDisconnectCalendar} disabled={calendarLoading}>
                    {calendarLoading ? 'Отключение...' : 'Отключить'}
                  </Button>
                </div>
              ) : (
                <div className="flex flex-col gap-2">
                  <p className="text-sm text-muted-foreground">Подключите Google Calendar, чтобы синхронизировать события.</p>
                  <div>
                    <Button size="sm" onClick={() => connectCalendar()} disabled={calendarLoading}>
                      {calendarLoading ? 'Подключение...' : 'Подключить Google Calendar'}
                    </Button>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <AlertDialog>
                <AlertDialogTrigger render={<Button variant="destructive" size="sm" disabled={deleteLoading}>Удалить аккаунт</Button>} />
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Удалить аккаунт?</AlertDialogTitle>
                    <AlertDialogDescription>
                      Это действие необратимо. Все ваши вишлисты, желания и данные будут удалены навсегда.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Отмена</AlertDialogCancel>
                    <AlertDialogAction onClick={handleDeleteAccount} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                      Удалить
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
