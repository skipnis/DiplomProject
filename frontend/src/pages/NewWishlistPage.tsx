import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { createWishlist } from '../api/wishlists';
import { createEvent, linkWishlist } from '../api/events';
import { getFriends } from '../api/friends';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { wishlistSchema, eventSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { parseApiFieldErrors } from '../utils/errors';
import { VISIBILITY_LABELS } from '../types';
import type { WishlistVisibility, WishlistMemberRole, FriendInfo } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { FieldError } from '@/components/ui/field-error';
import { EmojiPickerPopover } from '../components/EmojiPickerPopover';

const EMOJIS = ['🎁', '🎂', '🎮', '👗', '📚', '🏠', '✈️', '💄', '🎵', '🍕', '⚽', '🌸', '💻', '📷', '🎨'];

export default function NewWishlistPage() {
  const navigate = useNavigate();
  const toast = useToast();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [emoji, setEmoji] = useState('🎁');
  const [visibility, setVisibility] = useState<WishlistVisibility>(1);
  const [friends, setFriends] = useState<FriendInfo[]>([]);
  const [selectedFriends, setSelectedFriends] = useState<FriendInfo[]>([]);
  const [friendFilter, setFriendFilter] = useState('');
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [surpriseMode, setSurpriseMode] = useState(false);
  const [saving, setSaving] = useState(false);

  const closeEmojiPicker = useCallback(() => setShowEmojiPicker(false), []);
  const [createEventEnabled, setCreateEventEnabled] = useState(false);
  const [eventTitle, setEventTitle] = useState('');
  const [eventDate, setEventDate] = useState('');
  const [eventDescription, setEventDescription] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    getFriends(1, 100).then((res) => setFriends(res.items)).catch(() => {});
  }, []);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const addFriend = (friend: FriendInfo) => {
    setSelectedFriends((prev) => [...prev, friend]);
  };

  const removeFriend = (userId: string) => {
    setSelectedFriends((prev) => prev.filter((f) => f.userId !== userId));
  };

  const availableFriends = friends.filter(
    (f) =>
      !selectedFriends.some((s) => s.userId === f.userId) &&
      f.username.toLowerCase().includes(friendFilter.toLowerCase()),
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const wlResult = wishlistSchema.safeParse({ name, description: description || undefined });
    const evResult = createEventEnabled
      ? eventSchema.safeParse({ title: eventTitle, description: eventDescription || undefined, date: eventDate })
      : null;

    const combined: FormErrors = {};
    if (!wlResult.success) Object.assign(combined, parseZodErrors(wlResult.error));
    if (evResult && !evResult.success) {
      const evErrors = parseZodErrors(evResult.error);
      for (const [k, v] of Object.entries(evErrors)) combined[`event_${k}`] = v;
    }
    if (Object.keys(combined).length > 0) { setErrors(combined); return; }
    setErrors({});

    setSaving(true);
    try {
      const members = selectedFriends.map((f) => ({ userId: f.userId, role: 1 as WishlistMemberRole }));
      const wishlist = await createWishlist({ name, description: description || null, emoji, visibility, isSurpriseModeEnabled: surpriseMode, members: members.length > 0 ? members : undefined });
      if (createEventEnabled && eventTitle.trim() && eventDate) {
        const event = await createEvent({ title: eventTitle.trim(), description: eventDescription.trim() || null, date: eventDate });
        await linkWishlist(event.id, wishlist.id);
      }
      navigate(`/wishlists/${wishlist.id}`);
    } catch (e) {
      const fieldErrors = parseApiFieldErrors(e);
      if (fieldErrors) setErrors((prev) => ({ ...prev, ...fieldErrors }));
      else toast.error(parseError(e));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-xl mx-auto">
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Новый вишлист</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label>Эмодзи</Label>
              <div className="flex flex-wrap gap-1 items-center">
                <span className={`text-2xl p-1.5 rounded-md border-2 transition-colors ${!EMOJIS.includes(emoji) ? 'border-primary' : 'border-transparent'}`}>{emoji}</span>
                <span className="w-px h-6 bg-border mx-1" />
                {EMOJIS.map((e) => (
                  <button
                    key={e}
                    type="button"
                    className={`text-2xl p-1.5 rounded-md border-2 transition-colors ${emoji === e ? 'border-primary' : 'border-transparent hover:border-muted'}`}
                    onClick={() => setEmoji(e)}
                  >
                    {e}
                  </button>
                ))}
                <button
                  type="button"
                  className="text-sm px-2.5 py-1.5 rounded-md border-2 border-transparent hover:border-muted text-muted-foreground hover:text-foreground transition-colors"
                  onClick={() => setShowEmojiPicker((v) => !v)}
                >
                  Ещё...
                </button>
                <EmojiPickerPopover open={showEmojiPicker} onClose={closeEmojiPicker} onSelect={setEmoji} />
              </div>
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="name">Название *</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => { setName(e.target.value); clearError('name'); }}
                placeholder="Мой вишлист"
                aria-invalid={!!errors.name}
              />
              <FieldError message={errors.name} />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="description">Описание</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => { setDescription(e.target.value); clearError('description'); }}
                placeholder="Описание вишлиста..."
                rows={2}
                aria-invalid={!!errors.description}
              />
              <FieldError message={errors.description} />
            </div>

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
                {availableFriends.length > 0 && (
                  <Card>
                    <CardContent className="p-2 flex flex-col gap-1">
                      {availableFriends.map((f) => (
                        <button key={f.userId} type="button" className="flex items-center gap-2 p-2 rounded-md hover:bg-muted text-left" onClick={() => addFriend(f)}>
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
                {selectedFriends.length > 0 && (
                  <div className="flex flex-wrap gap-1">
                    {selectedFriends.map((f) => (
                      <span key={f.userId} className="flex items-center gap-1 bg-primary/10 text-primary rounded-full px-3 py-0.5 text-xs font-medium">
                        {f.username}
                        <button type="button" className="font-bold hover:text-primary/70" onClick={() => removeFriend(f.userId)}>×</button>
                      </span>
                    ))}
                  </div>
                )}
              </div>
            )}

            <div className="flex flex-col gap-1">
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" checked={surpriseMode} onChange={(e) => setSurpriseMode(e.target.checked)} />
                <span className="text-sm font-medium">🎁 Режим сюрприза — не показывать кто что забронировал</span>
              </label>
              <p className="text-xs text-muted-foreground pl-6">Задаётся только при создании и не может быть изменён</p>
            </div>

            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={createEventEnabled} onChange={(e) => { setCreateEventEnabled(e.target.checked); if (e.target.checked && !eventTitle) setEventTitle(name); }} />
              <span className="text-sm font-medium">Создать событие и привязать к вишлисту</span>
            </label>

            {createEventEnabled && (
              <Card>
                <CardContent className="pt-4 flex flex-col gap-3">
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="eventTitle">Название события *</Label>
                    <Input
                      id="eventTitle"
                      value={eventTitle}
                      onChange={(e) => { setEventTitle(e.target.value); clearError('event_title'); }}
                      placeholder="День рождения Ани"
                      maxLength={200}
                      aria-invalid={!!errors.event_title}
                    />
                    <FieldError message={errors.event_title} />
                  </div>
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="eventDate">Дата *</Label>
                    <Input
                      id="eventDate"
                      type="date"
                      value={eventDate}
                      onChange={(e) => { setEventDate(e.target.value); clearError('event_date'); }}
                      aria-invalid={!!errors.event_date}
                    />
                    <FieldError message={errors.event_date} />
                  </div>
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="eventDescription">Описание события</Label>
                    <Textarea
                      id="eventDescription"
                      value={eventDescription}
                      onChange={(e) => { setEventDescription(e.target.value); clearError('event_description'); }}
                      rows={2}
                      maxLength={2000}
                      aria-invalid={!!errors.event_description}
                    />
                    <FieldError message={errors.event_description} />
                  </div>
                </CardContent>
              </Card>
            )}

            <div className="flex gap-2 justify-end mt-1">
              <Button type="button" variant="ghost" onClick={() => navigate('/wishlists')}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Создание...' : 'Создать'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
