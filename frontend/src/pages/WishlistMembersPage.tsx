import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getWishlist, getWishlistMembers, addWishlistMembers, removeWishlistMember, updateMemberRole } from '../api/wishlists';
import { getUserProfile } from '../api/users';
import { getFriends } from '../api/friends';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { ROLE_LABELS } from '../types';
import type { WishlistMemberDto, WishlistMemberRole, UserProfile, FriendInfo } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';

export default function WishlistMembersPage() {
  const { id } = useParams<{ id: string }>();
  const { user: me } = useAuth();
  const toast = useToast();

  const [wishlistName, setWishlistName] = useState('');
  const [members, setMembers] = useState<WishlistMemberDto[]>([]);
  const [memberProfiles, setMemberProfiles] = useState<Record<string, UserProfile>>({});
  const [friends, setFriends] = useState<FriendInfo[]>([]);
  const [friendFilter, setFriendFilter] = useState('');
  const [customRoleEdits, setCustomRoleEdits] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    Promise.all([getWishlist(id), getWishlistMembers(id), getFriends()])
      .then(async ([wl, mems, friendList]) => {
        setWishlistName(wl.name);
        setMembers(mems);
        setFriends(friendList.items);
        const roleEdits: Record<string, string> = {};
        mems.forEach((m) => { roleEdits[m.userId] = m.customRoleName ?? ''; });
        setCustomRoleEdits(roleEdits);
        const profiles: Record<string, UserProfile> = {};
        await Promise.all(mems.map(async (m) => {
          try { profiles[m.userId] = await getUserProfile(m.userId); } catch { /* ignore */ }
        }));
        setMemberProfiles(profiles);
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  const handleAddMember = async (friend: FriendInfo) => {
    if (!id) return;
    try {
      await addWishlistMembers(id, [{ userId: friend.userId, role: 1 }]);
      setMembers((prev) => [...prev, { userId: friend.userId, role: 1, customRoleName: null, joinedAt: new Date().toISOString() }]);
      setCustomRoleEdits((prev) => ({ ...prev, [friend.userId]: '' }));
      setMemberProfiles((prev) => ({ ...prev, [friend.userId]: { id: friend.userId, displayName: friend.username, username: null, avatarUrl: friend.avatarUrl, bio: null, receivedCount: 0, giftedCount: 0 } }));
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleRemoveMember = async (userId: string) => {
    if (!id) return;
    try { await removeWishlistMember(id, userId); setMembers((prev) => prev.filter((m) => m.userId !== userId)); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleRoleChange = async (userId: string, role: WishlistMemberRole) => {
    if (!id) return;
    const member = members.find((m) => m.userId === userId);
    const customRoleName = member?.customRoleName ?? null;
    try { await updateMemberRole(id, userId, role, customRoleName); setMembers((prev) => prev.map((m) => m.userId === userId ? { ...m, role } : m)); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleCustomRoleBlur = async (userId: string) => {
    if (!id) return;
    const member = members.find((m) => m.userId === userId);
    if (!member) return;
    const customRoleName = customRoleEdits[userId]?.trim() || null;
    if (customRoleName === (member.customRoleName ?? null)) return;
    try {
      await updateMemberRole(id, userId, member.role, customRoleName);
      setMembers((prev) => prev.map((m) => m.userId === userId ? { ...m, customRoleName } : m));
    } catch (e) { toast.error(parseError(e)); }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  const available = friends.filter((f) => !members.some((m) => m.userId === f.userId) && f.username.toLowerCase().includes(friendFilter.toLowerCase()));

  return (
    <div className="max-w-xl mx-auto">
      <div className="mb-7">
        <Link to={`/wishlists/${id}`} className="text-sm text-muted-foreground hover:text-foreground">← {wishlistName}</Link>
        <h1 className="text-2xl font-extrabold tracking-tight mt-1">Участники</h1>
      </div>

      <Card>
        <CardContent className="pt-6">
          <Input placeholder="Поиск по друзьям..." value={friendFilter} onChange={(e) => setFriendFilter(e.target.value)} className="mb-3" />
          {available.length > 0 && (
            <div className="flex flex-col gap-1 mb-4">
              {available.map((f) => (
                <button key={f.userId} type="button" className="flex items-center gap-2 p-2 rounded-md hover:bg-muted text-left cursor-pointer" onClick={() => handleAddMember(f)}>
                  <Avatar className="h-7 w-7">
                    <AvatarImage src={getImageUrl(f.avatarUrl) ?? undefined} />
                    <AvatarFallback className="text-xs">{f.username[0].toUpperCase()}</AvatarFallback>
                  </Avatar>
                  <span className="text-sm font-medium flex-1">{f.username}</span>
                  <span className="text-xs text-primary font-semibold">+ Добавить</span>
                </button>
              ))}
            </div>
          )}
          <div className="flex flex-col gap-2">
            {members.map((m) => {
              const p = memberProfiles[m.userId];
              const isMe = m.userId === me?.id;
              return (
                <div key={m.userId} className="flex flex-col gap-1 py-1">
                  <div className="flex items-center gap-3">
                    <Avatar className="h-8 w-8">
                      <AvatarImage src={getImageUrl(p?.avatarUrl) ?? undefined} />
                      <AvatarFallback className="text-xs">{(p?.displayName ?? m.userId)[0].toUpperCase()}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1 text-sm font-semibold">
                      <Link to={`/users/${m.userId}`} className="hover:underline">{p?.displayName ?? m.userId.slice(0, 8) + '…'}</Link>
                    </div>
                    {m.role === 2 ? (
                      <Badge variant="secondary">{ROLE_LABELS[2]}</Badge>
                    ) : (
                      <Select value={String(m.role)} onValueChange={(v) => handleRoleChange(m.userId, Number(v) as WishlistMemberRole)} disabled={isMe}>
                        <SelectTrigger className="w-28 h-7 text-xs"><SelectValue /></SelectTrigger>
                        <SelectContent>
                          {([0, 1] as WishlistMemberRole[]).map((r) => <SelectItem key={r} value={String(r)}>{ROLE_LABELS[r]}</SelectItem>)}
                        </SelectContent>
                      </Select>
                    )}
                    {!isMe && m.role !== 2 && <Button variant="destructive" size="sm" onClick={() => handleRemoveMember(m.userId)}>Удалить</Button>}
                  </div>
                  {m.role !== 2 && !isMe && (
                    <div className="pl-11">
                      <Input
                        className="h-6 text-xs"
                        placeholder="Кастомная роль..."
                        value={customRoleEdits[m.userId] ?? ''}
                        onChange={(e) => setCustomRoleEdits((prev) => ({ ...prev, [m.userId]: e.target.value }))}
                        onBlur={() => handleCustomRoleBlur(m.userId)}
                        onKeyDown={(e) => { if (e.key === 'Enter') (e.target as HTMLInputElement).blur(); }}
                      />
                    </div>
                  )}
                </div>
              );
            })}
          </div>
          {members.length === 0 && available.length === 0 && (
            <p className="text-sm text-muted-foreground text-center py-4">Нет друзей для добавления</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
