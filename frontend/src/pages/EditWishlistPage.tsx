import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getWishlist, updateWishlist, getWishlistMembers, addWishlistMembers, removeWishlistMember } from '../api/wishlists';
import { getMyEvents, linkWishlist } from '../api/events';
import { getFriends } from '../api/friends';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { wishlistSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { parseApiFieldErrors } from '../utils/errors';
import { VISIBILITY_LABELS } from '../types';
import type { WishlistVisibility, EventDto, FriendInfo } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { FieldError } from '@/components/ui/field-error';

const EMOJIS = ['🎁', '🎂', '🎮', '👗', '📚', '🏠', '✈️', '💄', '🎵', '🍕', '⚽', '🌸', '💻', '📷', '🎨'];

export default function EditWishlistPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [emoji, setEmoji] = useState('🎁');
  const [visibility, setVisibility] = useState<WishlistVisibility>(1);
  const [isSystem, setIsSystem] = useState(false);
  const [events, setEvents] = useState<EventDto[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<string>('');
  const [originalEventId, setOriginalEventId] = useState<string>('');
  const [friends, setFriends] = useState<FriendInfo[]>([]);
  const [memberUserIds, setMemberUserIds] = useState<Set<string>>(new Set());
  const [friendFilter, setFriendFilter] = useState('');
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    if (!id) return;
    Promise.all([getWishlist(id), getMyEvents(1, 100), getFriends(1, 100), getWishlistMembers(id)])
      .then(([wl, eventsRes, friendsRes, members]) => {
        setName(wl.name);
        setDescription(wl.description ?? '');
        setEmoji(wl.emoji ?? '🎁');
        setVisibility(wl.visibility);
        setIsSystem(wl.isSystem);
        setEvents(eventsRes.items);
        const linked = eventsRes.items.find((e) => e.linkedWishlistId === id);
        const linkedId = linked?.id ?? '';
        setSelectedEventId(linkedId);
        setOriginalEventId(linkedId);
        setFriends(friendsRes.items);
        setMemberUserIds(new Set(members.map((m) => m.userId)));
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleAddFriend = async (friend: FriendInfo) => {
    if (!id) return;
    try {
      await addWishlistMembers(id, [{ userId: friend.userId, role: 1 }]);
      setMemberUserIds((prev) => new Set([...prev, friend.userId]));
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleRemoveFriend = async (userId: string) => {
    if (!id) return;
    try {
      await removeWishlistMember(id, userId);
      setMemberUserIds((prev) => { const next = new Set(prev); next.delete(userId); return next; });
    } catch (e) { toast.error(parseError(e)); }
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
    try {
      await updateWishlist(id, { name, description: description || null, emoji, visibility });

      if (!isSystem && selectedEventId !== originalEventId) {
        if (originalEventId) await linkWishlist(originalEventId, null);
        if (selectedEventId) await linkWishlist(selectedEventId, id);
      }

      navigate(`/wishlists/${id}`);
    } catch (e) {
      const fieldErrors = parseApiFieldErrors(e);
      if (fieldErrors) setErrors((prev) => ({ ...prev, ...fieldErrors }));
      else toast.error(parseError(e));
    } finally { setSaving(false); }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  return (
    <div className="max-w-xl mx-auto">
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Редактировать вишлист</h1>
      </div>

      <Card>
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
              <>
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
                {visibility === 2 && (
                  <div className="flex flex-col gap-2">
                    <Label>Участники</Label>
                    <Input
                      placeholder="Поиск по друзьям..."
                      value={friendFilter}
                      onChange={(e) => setFriendFilter(e.target.value)}
                    />
                    {(() => {
                      const available = friends.filter(
                        (f) => !memberUserIds.has(f.userId) && f.username.toLowerCase().includes(friendFilter.toLowerCase()),
                      );
                      const added = friends.filter((f) => memberUserIds.has(f.userId));
                      return (
                        <>
                          {available.length > 0 && (
                            <Card>
                              <CardContent className="p-2 flex flex-col gap-1">
                                {available.map((f) => (
                                  <button key={f.userId} type="button" className="flex items-center gap-2 p-2 rounded-md hover:bg-muted text-left" onClick={() => handleAddFriend(f)}>
                                    <Avatar className="h-7 w-7">
                                      <AvatarImage src={getImageUrl(f.avatarUrl) ?? undefined} />
                                      <AvatarFallback className="text-xs">{f.username[0].toUpperCase()}</AvatarFallback>
                                    </Avatar>
                                    <span className="text-sm font-medium flex-1">{f.username}</span>
                                    <span className="text-xs text-primary font-semibold">+ Добавить</span>
                                  </button>
                                ))}
                              </CardContent>
                            </Card>
                          )}
                          {friends.length === 0 && (
                            <p className="text-sm text-muted-foreground">Нет друзей для добавления</p>
                          )}
                          {added.length > 0 && (
                            <div className="flex flex-wrap gap-1">
                              {added.map((f) => (
                                <span key={f.userId} className="flex items-center gap-1 bg-primary/10 text-primary rounded-full px-3 py-0.5 text-xs font-medium">
                                  {f.username}
                                  <button type="button" className="font-bold hover:text-primary/70" onClick={() => handleRemoveFriend(f.userId)}>×</button>
                                </span>
                              ))}
                            </div>
                          )}
                        </>
                      );
                    })()}
                  </div>
                )}
                <div className="flex flex-col gap-1.5">
                  <Label>Событие</Label>
                  <Select value={selectedEventId} onValueChange={(v) => setSelectedEventId(v ?? '')}>
                    <SelectTrigger>
                      <SelectValue>
                        {selectedEventId
                          ? (events.find((ev) => ev.id === selectedEventId)?.title ?? selectedEventId)
                          : <span className="text-muted-foreground">Не привязан</span>}
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="">Не привязан</SelectItem>
                      {events.map((ev) => (
                        <SelectItem key={ev.id} value={ev.id}>{ev.title}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </>
            )}
            <div className="flex gap-2 justify-end">
              <Button type="button" variant="ghost" onClick={() => navigate(`/wishlists/${id}`)}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
