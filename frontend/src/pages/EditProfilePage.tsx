import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { updateMyProfile, uploadAvatar, deleteAvatar } from '../api/users';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { profileSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { parseApiFieldErrors, ApiError } from '../utils/errors';
import { getImageUrl } from '../api/client';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { FieldError } from '@/components/ui/field-error';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

export default function EditProfilePage() {
  const { user, refreshUser } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const [displayName, setDisplayName] = useState('');
  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [saving, setSaving] = useState(false);
  const [avatarUploading, setAvatarUploading] = useState(false);
  const [previewAvatarUrl, setPreviewAvatarUrl] = useState<string | null>(null);
  const [errors, setErrors] = useState<FormErrors>({});
  const avatarInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (user) {
      setDisplayName(user.displayName);
      setUsername(user.username ?? '');
      setBio(user.bio ?? '');
      setBirthDate(user.birthDate ?? '');
    }
  }, [user]);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const localPreview = URL.createObjectURL(file);
    setPreviewAvatarUrl(localPreview);
    setAvatarUploading(true);

    try {
      await uploadAvatar(file);
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
  };

  const handleDeleteAvatar = async () => {
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
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = profileSchema.safeParse({ displayName, username, bio: bio || undefined });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    setErrors({});
    setSaving(true);
    try {
      await updateMyProfile({ displayName, username, bio: bio || null, birthDate: birthDate || null });
      await refreshUser();
      toast.success('Профиль сохранён');
      navigate('/profile');
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        setErrors((prev) => ({ ...prev, username: 'Этот username уже занят' }));
      } else {
        const fieldErrors = parseApiFieldErrors(e);
        if (fieldErrors) setErrors((prev) => ({ ...prev, ...fieldErrors }));
        else toast.error(parseError(e));
      }
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-xl mx-auto">
      <div className="flex items-center justify-between mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Редактировать профиль</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label>Фото профиля</Label>
              <div className="flex items-center gap-4">
                <Avatar className="h-16 w-16">
                  <AvatarImage
                    src={previewAvatarUrl ?? getImageUrl(user?.avatarUrl) ?? user?.avatarUrl ?? undefined}
                    alt={user?.displayName}
                  />
                  <AvatarFallback className="bg-primary text-primary-foreground text-xl font-bold">
                    {user?.displayName[0].toUpperCase()}
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
                  {user?.avatarUrl && (
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
                onChange={(e) => { setDisplayName(e.target.value); clearError('displayName'); }}
                placeholder="Алексей Кипнис"
                aria-invalid={!!errors.displayName}
              />
              <FieldError message={errors.displayName} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="username">Username</Label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground select-none">@</span>
                <Input
                  id="username"
                  value={username}
                  onChange={(e) => { setUsername(e.target.value); clearError('username'); }}
                  placeholder="alexkipnis"
                  className="pl-7"
                  aria-invalid={!!errors.username}
                />
              </div>
              <FieldError message={errors.username} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="bio">О себе</Label>
              <Textarea
                id="bio"
                value={bio}
                onChange={(e) => { setBio(e.target.value); clearError('bio'); }}
                placeholder="Расскажи о себе..."
                rows={3}
                aria-invalid={!!errors.bio}
              />
              <FieldError message={errors.bio} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="birthDate">Дата рождения</Label>
              <Input id="birthDate" type="date" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
            </div>
            <div className="flex gap-2 justify-end">
              <Button type="button" variant="ghost" onClick={() => navigate('/profile')}>Отмена</Button>
              <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
