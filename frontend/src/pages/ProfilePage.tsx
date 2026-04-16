import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useGoogleLogin } from '@react-oauth/google';
import { useAuth } from '../context/AuthContext';
import { getMyWishlists } from '../api/wishlists';
import { connectGoogleCalendar, disconnectGoogleCalendar, deleteMyAccount, getUserProfile } from '../api/users';
import { syncAllEvents } from '../api/events';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { VISIBILITY_LABELS, getWishlistEmoji } from '../types';
import type { WishlistSummaryDto, UserProfile } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
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

export default function ProfilePage() {
  const { user, refreshUser, logout } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [stats, setStats] = useState<Pick<UserProfile, 'receivedCount' | 'giftedCount'> | null>(null);
  const [loading, setLoading] = useState(true);
  const [calendarLoading, setCalendarLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    if (!user) return;
    Promise.all([getMyWishlists(), getUserProfile(user.id)])
      .then(([wl, profile]) => { setWishlists(wl); setStats({ receivedCount: profile.receivedCount, giftedCount: profile.giftedCount }); })
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
            <div className="flex gap-2">
              <Link to="/profile/edit" className={buttonVariants({ variant: 'secondary' })}>Редактировать</Link>
              <AlertDialog>
                <AlertDialogTrigger render={<Button variant="destructive" disabled={deleteLoading}>Удалить аккаунт</Button>} />
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
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="mb-6">
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

      <div className="flex items-center justify-between mb-5">
        <h2 className="text-lg font-bold">Мои вишлисты</h2>
        <Link to="/wishlists" className={buttonVariants({ variant: 'ghost', size: 'sm' })}>Все вишлисты</Link>
      </div>

      {loading ? (
        <p className="text-sm text-muted-foreground">Загрузка...</p>
      ) : wishlists.length === 0 ? (
        <div className="text-center py-12">
          <div className="text-4xl mb-3">📋</div>
          <p className="font-semibold mb-3">Нет вишлистов</p>
          <Link to="/wishlists/new" className={buttonVariants()}>Создать первый</Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {wishlists.slice(0, 6).map((w) => (
            <Link key={w.id} to={`/wishlists/${w.id}`} className="block rounded-xl border bg-card p-5 hover:shadow-md hover:-translate-y-0.5 transition-all">
              <div className="text-3xl mb-2">{getWishlistEmoji(w)}</div>
              <div className="font-bold text-sm">{w.name}</div>
              <div className="flex gap-3 mt-auto pt-2 text-xs text-muted-foreground flex-wrap">
                <span>{w.wishCount} желаний</span>
                {w.fulfilledWishCount > 0 && <span className="text-green-600 font-medium">✓ {w.fulfilledWishCount} исполнено</span>}
                <span>{VISIBILITY_LABELS[w.visibility]}</span>
              </div>
            </Link>
          ))}
        </div>
      )}

    </div>
  );
}
