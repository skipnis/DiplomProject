import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getUserProfile } from '../api/users';
import { getUserWishlists } from '../api/wishlists';
import { sendFriendRequest, acceptFriendRequest, removeFriend, getFriends, getFriendshipRequests } from '../api/friends';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { VISIBILITY_LABELS, getWishlistEmoji } from '../types';
import type { UserProfile, WishlistSummaryDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

type FriendStatus = 'none' | 'friends' | 'request_sent' | 'request_received';

export default function UserProfilePage() {
  const { id } = useParams<{ id: string }>();
  const { user: me } = useAuth();
  const toast = useToast();

  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [friendStatus, setFriendStatus] = useState<FriendStatus>('none');
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([getUserProfile(id), getUserWishlists(id)])
      .then(([p, wl]) => { setProfile(p); setWishlists(wl); })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));

    if (me && id !== me.id) {
      Promise.all([getFriends(), getFriendshipRequests()])
        .then(([friends, requests]) => {
          if (friends.items.some((f) => f.userId === id)) setFriendStatus('friends');
          else if (requests.items.some((r) => r.userId === id)) setFriendStatus('request_received');
        })
        .catch(() => {});
    }
  }, [id, me]);

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
              <AvatarImage src={getImageUrl(profile.avatarUrl) ?? profile.avatarUrl ?? undefined} alt={profile.username} />
              <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
                {profile.username[0].toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h1 className="text-xl font-extrabold tracking-tight">{profile.username}</h1>
              {profile.bio && <div className="text-sm mt-1">{profile.bio}</div>}
            </div>
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
            {isMe && <Link to="/profile/edit" className={buttonVariants({ variant: 'secondary' })}>Редактировать</Link>}
          </div>
        </CardContent>
      </Card>

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
          {wishlists.map((w) => (
            <Link key={w.id} to={`/wishlists/${w.id}`} className="block rounded-xl border bg-card p-5 hover:shadow-md hover:-translate-y-0.5 transition-all">
              <div className="text-3xl mb-2">{getWishlistEmoji(w)}</div>
              <div className="font-bold text-sm">{w.name}</div>
              <div className="flex gap-3 mt-2 text-xs text-muted-foreground">
                <span>{w.wishCount} желаний</span>
                <span>{VISIBILITY_LABELS[w.visibility]}</span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
