import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { updateMyProfile } from '../api/users';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';

export default function EditProfilePage() {
  const { user, refreshUser } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (user) {
      setUsername(user.username);
      setBio(user.bio ?? '');
      setBirthDate(user.birthDate ?? '');
    }
  }, [user]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await updateMyProfile({ username, bio: bio || null, birthDate: birthDate || null });
      await refreshUser();
      toast.success('Профиль сохранён');
      navigate('/profile');
    } catch (e) {
      toast.error(parseError(e));
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
              <Label htmlFor="username">Имя пользователя</Label>
              <Input id="username" value={username} onChange={(e) => setUsername(e.target.value)} required placeholder="username" />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="bio">О себе</Label>
              <Textarea id="bio" value={bio} onChange={(e) => setBio(e.target.value)} placeholder="Расскажи о себе..." rows={3} />
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
