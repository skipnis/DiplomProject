import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { updateMyProfile } from '../api/users';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { profileSchema, parseZodErrors, type FormErrors } from '../lib/schemas';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { FieldError } from '@/components/ui/field-error';

export default function OnboardingPage() {
  const { user, refreshUser } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    if (user) setUsername(user.username);
  }, [user]);

  const clearError = (field: string) => {
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = profileSchema.safeParse({ username, bio: bio || undefined });
    if (!result.success) { setErrors(parseZodErrors(result.error)); return; }
    setErrors({});
    setSaving(true);
    try {
      await updateMyProfile({ username, bio: bio || null, birthDate: birthDate || null });
      await refreshUser();
      navigate('/wishlists');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setSaving(false);
    }
  };

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
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="username">Имя пользователя *</Label>
                <Input
                  id="username"
                  value={username}
                  onChange={(e) => { setUsername(e.target.value); clearError('username'); }}
                  placeholder="username"
                  aria-invalid={!!errors.username}
                />
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
              <Button type="submit" className="w-full" disabled={saving}>
                {saving ? 'Сохранение...' : 'Продолжить'}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
