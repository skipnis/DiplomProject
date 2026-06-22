import { useEffect, useRef, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getUserProfile, getUserGiftProfile, getUserFulfilledWishes, getUserBlacklist } from '../api/users';
import { getUserWishlists } from '../api/wishlists';
import { sendFriendRequest, acceptFriendRequest, removeFriend, getFriends, getFriendshipRequests } from '../api/friends';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { VISIBILITY_LABELS, getWishlistEmoji } from '../types';
import type { UserProfile, WishlistSummaryDto, GiftProfileDto, FulfilledWishItem, BlacklistItemDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

type FriendStatus = 'none' | 'friends' | 'request_sent' | 'request_received';

function formatBirthDate(dateStr: string): string {
  const [, month, day] = dateStr.split('-').map(Number);
  return new Date(2000, month - 1, day).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' });
}

const LEVEL_COLORS: Record<number, string> = {
  1: 'bg-muted text-muted-foreground',
  2: 'bg-green-100 text-green-700',
  3: 'bg-blue-100 text-blue-700',
  4: 'bg-purple-100 text-purple-700',
  5: 'bg-orange-100 text-orange-700',
  6: 'bg-yellow-100 text-yellow-800',
};

function GiftProfileSection({ giftProfile }: { giftProfile: GiftProfileDto }) {
  const earnedAchievements = giftProfile.achievements.filter((achievement) => achievement.isEarned);

  if (giftProfile.giftsGiven === 0) return null;

  return (
    <Card className="mb-6">
      <CardContent className="pt-6 space-y-4">
        <div className="text-base font-bold">Подарочный профиль</div>

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
      </CardContent>
    </Card>
  );
}

export default function UserProfilePage() {
  const { id } = useParams<{ id: string }>();
  const { user: me } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [giftProfile, setGiftProfile] = useState<GiftProfileDto | null>(null);
  const [fulfilledWishes, setFulfilledWishes] = useState<FulfilledWishItem[]>([]);
  const [friendBlacklist, setFriendBlacklist] = useState<BlacklistItemDto[] | null>(null);
  const [friendStatus, setFriendStatus] = useState<FriendStatus>('none');
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const carouselRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!id) return;
    Promise.all([getUserProfile(id), getUserWishlists(id), getUserGiftProfile(id), getUserFulfilledWishes(id)])
      .then(([fetchedProfile, fetchedWishlists, fetchedGiftProfile, fetchedFulfilledWishes]) => {
        setProfile(fetchedProfile);
        setWishlists(fetchedWishlists);
        setGiftProfile(fetchedGiftProfile);
        setFulfilledWishes(fetchedFulfilledWishes);
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));

    if (me && id !== me.id) {
      Promise.all([getFriends(), getFriendshipRequests()])
        .then(([friends, requests]) => {
          const isFriend = friends.items.some((friend) => friend.userId === id);
          if (isFriend) {
            setFriendStatus('friends');
            getUserBlacklist(id).then(setFriendBlacklist).catch(() => {});
          } else if (requests.items.some((request) => request.userId === id)) {
            setFriendStatus('request_received');
          }
        })
        .catch(() => {});
    }
  }, [id, me]);

  const handleProposeGift = () => {
    if (!me) { toast.warning('Войдите в аккаунт, чтобы предложить подарок'); return; }
    if (!profile) return;
    navigate('/proposals/new', {
      state: {
        recipient: {
          userId: id,
          username: profile.displayName,
          avatarUrl: profile.avatarUrl,
        },
      },
    });
  };

  const handleShareProfile = () => {
    navigator.clipboard.writeText(`${window.location.origin}/users/${id}`);
    toast.success('Ссылка скопирована');
  };

  const handleFriendAction = async () => {
    if (!id) return;
    setActionLoading(true);
    try {
      if (friendStatus === 'none') { await sendFriendRequest(id); setFriendStatus('request_sent'); toast.success('Заявка отправлена'); }
      else if (friendStatus === 'friends') { await removeFriend(id); setFriendStatus('none'); toast.success('Пользователь удалён из друзей'); }
      else if (friendStatus === 'request_received') { await acceptFriendRequest(id); setFriendStatus('friends'); toast.success('Заявка принята'); }
      else if (friendStatus === 'request_sent') { await removeFriend(id); setFriendStatus('none'); }
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!profile) return <div className="text-center py-12 text-muted-foreground">Пользователь не найден</div>;

  const isMe = me?.id === id;

  return (
    <div>
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6 flex-wrap">
            <Avatar className="h-20 w-20">
              <AvatarImage src={getImageUrl(profile.avatarUrl) ?? profile.avatarUrl ?? undefined} alt={profile.displayName} />
              <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
                {profile.displayName[0].toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h1 className="text-xl font-extrabold tracking-tight">{profile.displayName}</h1>
              {profile.username && <div className="text-sm text-muted-foreground">@{profile.username}</div>}
              {profile.bio && <div className="text-sm mt-1">{profile.bio}</div>}
              {profile.birthDate && (
                <div className="text-sm text-muted-foreground mt-1">
                  🎂 {formatBirthDate(profile.birthDate)}
                </div>
              )}
              <div className="flex gap-4 mt-2 text-sm">
                <span><span className="font-bold">{profile.receivedCount}</span> <span className="text-muted-foreground">получено</span></span>
                <span><span className="font-bold">{profile.giftedCount}</span> <span className="text-muted-foreground">подарено</span></span>
              </div>
              {giftProfile && giftProfile.giftsGiven > 0 && (
                <div className="mt-2">
                  <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${LEVEL_COLORS[giftProfile.level] ?? 'bg-muted text-muted-foreground'}`}>
                    {giftProfile.levelName}
                  </span>
                </div>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" onClick={handleShareProfile}>Поделиться</Button>
              {!isMe && (
                <Button variant="secondary" onClick={handleProposeGift}>🎁 Предложить подарок</Button>
              )}
              {me && !isMe && (
                <Button
                  variant={friendStatus === 'friends' ? 'destructive' : friendStatus === 'request_sent' ? 'ghost' : 'default'}
                  onClick={handleFriendAction}
                  disabled={actionLoading}
                >
                  {friendStatus === 'none' && '+ Добавить в друзья'}
                  {friendStatus === 'friends' && 'Убрать из друзей'}
                  {friendStatus === 'request_sent' && 'Заявка отправлена'}
                  {friendStatus === 'request_received' && '✓ Принять заявку'}
                </Button>
              )}
              {isMe && <Link to="/profile?tab=settings" className={buttonVariants({ variant: 'secondary' })}>Редактировать</Link>}
            </div>
          </div>
        </CardContent>
      </Card>

      {giftProfile && <GiftProfileSection giftProfile={giftProfile} />}

      {friendBlacklist && friendBlacklist.length > 0 && (
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className="text-base font-bold mb-3">Не хочет получать</div>
            <div className="flex flex-wrap gap-2">
              {friendBlacklist.map((item) => (
                <span
                  key={item.id}
                  className="px-3 py-1 rounded-full border border-destructive/30 bg-destructive/5 text-sm"
                >
                  {item.title}
                </span>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {fulfilledWishes.length > 0 && (
        <div className="mb-6">
          <div className="text-base font-bold mb-3">Исполненные желания</div>
          <div
            ref={carouselRef}
            className="flex gap-3 overflow-x-auto pb-2 snap-x snap-mandatory scrollbar-thin"
            style={{ scrollbarWidth: 'thin' }}
          >
            {fulfilledWishes.map((wish) => (
              <div
                key={wish.id}
                className="snap-start shrink-0 w-40 rounded-xl border bg-card overflow-hidden"
              >
                {wish.imagePath ? (
                  <img
                    src={getImageUrl(wish.imagePath) ?? wish.imagePath}
                    alt={wish.wishName}
                    className="w-full h-28 object-cover"
                  />
                ) : (
                  <div className="w-full h-28 bg-muted flex items-center justify-center text-3xl">🎁</div>
                )}
                <div className="p-2">
                  <div className="text-xs font-semibold line-clamp-2">{wish.wishName}</div>
                  <div className="text-xs text-muted-foreground mt-0.5 truncate">{wish.wishlistName}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="flex items-center justify-between mb-5">
        <h2 className="text-lg font-bold">Вишлисты</h2>
      </div>

      {wishlists.length === 0 ? (
        <div className="text-center py-12">
          <div className="text-4xl mb-3">📋</div>
          <p className="text-muted-foreground">Нет публичных вишлистов</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {wishlists.map((wishlist) => (
            <Link key={wishlist.id} to={`/wishlists/${wishlist.id}`} className="block rounded-xl border bg-card p-5 hover:shadow-md hover:-translate-y-0.5 transition-all">
              <div className="text-3xl mb-2">{getWishlistEmoji(wishlist)}</div>
              <div className="font-bold text-sm">{wishlist.name}</div>
              <div className="flex gap-3 mt-2 text-xs text-muted-foreground">
                <span>{wishlist.wishCount} желаний</span>
                {wishlist.fulfilledWishCount > 0 && <span className="text-green-600 font-medium">✓ {wishlist.fulfilledWishCount} исполнено</span>}
                <span>{VISIBILITY_LABELS[wishlist.visibility]}</span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
