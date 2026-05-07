import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { updateMyProfile, uploadAvatar } from '../api/users';
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

export default function OnboardingPage() {
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
    if (user) setDisplayName(user.displayName);
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
    } catch (err) {
      setPreviewAvatarUrl(null);
      toast.error(parseError(err));
    } finally {
      setAvatarUploading(false);
      URL.revokeObjectURL(localPreview);
      if (avatarInputRef.current) avatarInputRef.current.value = '';
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = profileSchema.safeParse({ displayName, username, bio: bio || undefined });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    setErrors({});
    setSaving(true);
    try {
      await updateMyProfile({ displayName, username, bio: bio || null, birthDate: birthDate || null, showFulfilledWishes: true });
      await refreshUser();
      navigate('/wishlists');
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

  const currentAvatarSrc = previewAvatarUrl
    ?? getImageUrl(user?.avatarUrl)
    ?? user?.avatarUrl
    ?? undefined;

  return (
    <div className="flex items-center justify-center min-h-[calc(100vh-4rem)] py-8">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="text-5xl mb-3">🎁</div>
          <h1 className="text-2xl font-extrabold tracking-tight">Добро пожаловать в Wishapp</h1>
          <p className="text-muted-foreground text-sm mt-1">Расскажи немного о себе, чтобы друзья могли тебя найти</p>
        </div>
        <Card>
          <CardContent className="pt-6">
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
              <div className="flex flex-col items-center gap-3">
                <Avatar className="h-20 w-20">
                  <AvatarImage src={currentAvatarSrc} alt={user?.displayName} />
                  <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
                    {displayName[0]?.toUpperCase() ?? '?'}
                  </AvatarFallback>
                </Avatar>
                <input
                  ref={avatarInputRef}
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={handleAvatarChange}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  disabled={avatarUploading}
                  onClick={() => avatarInputRef.current?.click()}
                >
                  {avatarUploading ? 'Загрузка...' : 'Загрузить фото'}
                </Button>
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="displayName">Имя *</Label>
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
                <Label htmlFor="username">Username *</Label>
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
              <Button type="submit" className="w-full" disabled={saving || avatarUploading}>
                {saving ? 'Сохранение...' : 'Продолжить'}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
