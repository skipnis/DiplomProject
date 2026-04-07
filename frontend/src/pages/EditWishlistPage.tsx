import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getWishlist, updateWishlist, getWishlistMembers, addWishlistMembers, removeWishlistMember, updateMemberRole } from '../api/wishlists';
import { getUserProfile } from '../api/users';
import { getFriends } from '../api/friends';
import { getImageUrl } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { wishlistSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { parseApiFieldErrors } from '../utils/errors';
import { ROLE_LABELS, VISIBILITY_LABELS } from '../types';
import type { WishlistVisibility, WishlistMemberDto, WishlistMemberRole, UserProfile, FriendInfo } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { FieldError } from '@/components/ui/field-error';

const EMOJIS = ['🎁', '🎂', '🎮', '👗', '📚', '🏠', '✈️', '💄', '🎵', '🍕', '⚽', '🌸', '💻', '📷', '🎨'];

export default function EditWishlistPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user: me } = useAuth();
  const toast = useToast();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [emoji, setEmoji] = useState('🎁');
  const [visibility, setVisibility] = useState<WishlistVisibility>(1);
  const [isSystem, setIsSystem] = useState(false);
  const [members, setMembers] = useState<WishlistMemberDto[]>([]);
  const [memberProfiles, setMemberProfiles] = useState<Record<string, UserProfile>>({});
  const [friends, setFriends] = useState<FriendInfo[]>([]);
  const [friendFilter, setFriendFilter] = useState('');
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    if (!id) return;
    Promise.all([getWishlist(id), getWishlistMembers(id), getFriends()])
      .then(async ([wl, mems, friendList]) => {
        setName(wl.name);
        setDescription(wl.description ?? '');
        setEmoji(wl.emoji ?? '🎁');
        setVisibility(wl.visibility);
        setIsSystem(wl.isSystem);
        setMembers(mems);
        setFriends(friendList.items);
        const profiles: Record<string, UserProfile> = {};
        await Promise.all(mems.map(async (m) => {
          try { profiles[m.userId] = await getUserProfile(m.userId); } catch { /* ignore */ }
        }));
        setMemberProfiles(profiles);
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleAddMember = async (friend: FriendInfo) => {
    if (!id) return;
    try {
      await addWishlistMembers(id, [{ userId: friend.userId, role: 1 }]);
      setMembers((prev) => [...prev, { userId: friend.userId, role: 1, customRoleName: null, joinedAt: new Date().toISOString() }]);
      setMemberProfiles((prev) => ({ ...prev, [friend.userId]: { id: friend.userId, username: friend.username, avatarUrl: friend.avatarUrl, bio: null } }));
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleRemoveMember = async (userId: string) => {
    if (!id) return;
    try { await removeWishlistMember(id, userId); setMembers((prev) => prev.filter((m) => m.userId !== userId)); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleRoleChange = async (userId: string, role: WishlistMemberRole) => {
    if (!id) return;
    try { await updateMemberRole(id, userId, role, null); setMembers((prev) => prev.map((m) => m.userId === userId ? { ...m, role } : m)); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    if (!isSystem) {
      const result = wishlistSchema.safeParse({ name, description: description || undefined });
      if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    }
    setErrors({});
    setSaving(true);
    try { await updateWishlist(id, { name, description: description || null, emoji, visibility }); navigate(`/wishlists/${id}`); }
    catch (e) {
      const fieldErrors = parseApiFieldErrors(e);
      if (fieldErrors) setErrors((prev) => ({ ...prev, ...fieldErrors }));
      else toast.error(parseError(e));
    } finally { setSaving(false); }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  const available = friends.filter((f) => !members.some((m) => m.userId === f.userId) && f.username.toLowerCase().includes(friendFilter.toLowerCase()));

  return (
    <div className="max-w-xl mx-auto">
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Редактировать вишлист</h1>
      </div>

      <Card className="mb-6">
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            {!isSystem && (
              <>
                <div className="flex flex-col gap-1.5">
                  <Label>Эмодзи</Label>
                  <div className="flex flex-wrap gap-1">
                    {EMOJIS.map((e) => (
                      <button key={e} type="button" className={`text-2xl p-1.5 rounded-md border-2 transition-colors ${emoji === e ? 'border-primary' : 'border-transparent hover:border-muted'}`} onClick={() => setEmoji(e)}>{e}</button>
                    ))}
                  </div>
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="name">Название *</Label>
                  <Input
                    id="name"
                    value={name}
                    onChange={(e) => { setName(e.target.value); clearError('name'); }}
                    aria-invalid={!!errors.name}
                  />
                  <FieldError message={errors.name} />
                </div>
              </>
            )}
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="description">Описание</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => { setDescription(e.target.value); clearError('description'); }}
                rows={2}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>
            {!isSystem && (
              <div className="flex flex-col gap-1.5">
                <Label>Видимость</Label>
                <Select value={String(visibility)} onValueChange={(v) => setVisibility(Number(v) as WishlistVisibility)}>
                  <SelectTrigger><SelectValue>{VISIBILITY_LABELS[visibility]}</SelectValue></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">🌍 Публичный</SelectItem>
                    <SelectItem value="1">👥 Для друзей</SelectItem>
                    <SelectItem value="2">👤 Избранные друзья</SelectItem>
                    <SelectItem value="3">🔒 Приватный</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            )}
            <div className="flex gap-2 justify-end">
              <Button type="button" variant="ghost" onClick={() => navigate(`/wishlists/${id}`)}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {!isSystem && <Card>
        <CardContent className="pt-6">
          <h2 className="font-semibold mb-4">Участники</h2>
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
                <div key={m.userId} className="flex items-center gap-3 py-1">
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={getImageUrl(p?.avatarUrl) ?? undefined} />
                    <AvatarFallback className="text-xs">{(p?.username ?? m.userId)[0].toUpperCase()}</AvatarFallback>
                  </Avatar>
                  <div className="flex-1 text-sm font-semibold">{p?.username ?? m.userId.slice(0, 8) + '…'}</div>
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
              );
            })}
          </div>
        </CardContent>
      </Card>}
    </div>
  );
}
