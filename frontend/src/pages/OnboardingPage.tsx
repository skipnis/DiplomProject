import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { updateMyProfile, uploadAvatar, addBlacklistItem, checkUsernameAvailability } from '../api/users';
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
import { QUOTES, pickRandom } from '../lib/quotes';

const BLACKLIST_PRESETS = [
  'Носки',
  'Парфюм',
  'Алкоголь',
  'Цветы',
  'Книги по саморазвитию',
  'Дешёвые безделушки',
  'Сладкое',
  'Подарочные сертификаты',
];

const MAX_BLACKLIST_ITEMS = 5;

export default function OnboardingPage() {
  const { user, refreshUser } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const [step, setStep] = useState(1);
  const [showFinalScreen, setShowFinalScreen] = useState(false);
  const [randomQuote] = useState(() => pickRandom(QUOTES));

  const [displayName, setDisplayName] = useState('');
  const [username, setUsername] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});

  const [bio, setBio] = useState('');
  const [avatarUploading, setAvatarUploading] = useState(false);
  const [previewAvatarUrl, setPreviewAvatarUrl] = useState<string | null>(null);
  const avatarInputRef = useRef<HTMLInputElement>(null);

  const [birthDate, setBirthDate] = useState('');

  const [blacklistItems, setBlacklistItems] = useState<string[]>([]);
  const [blacklistInput, setBlacklistInput] = useState('');

  const [saving, setSaving] = useState(false);

  const validateBirthDate = (date: string): string | null => {
    const parsed = new Date(date);
    const minAgeDate = new Date();
    minAgeDate.setFullYear(minAgeDate.getFullYear() - 6);
    const maxAgeDate = new Date();
    maxAgeDate.setFullYear(maxAgeDate.getFullYear() - 120);
    if (parsed > minAgeDate) return 'Возраст должен быть не менее 6 лет';
    if (parsed < maxAgeDate) return 'Введите корректную дату рождения';
    return null;
  };

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
      setPreviewAvatarUrl(null);
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

  const [checkingUsername, setCheckingUsername] = useState(false);

  const handleStep1Next = async () => {
    const result = profileSchema.safeParse({ displayName, username, bio: bio || undefined });
    if (!result.success) {
      const parsed = parseZodErrors(result.error);
      const step1Errors: FormErrors = {};
      if (parsed.displayName) step1Errors.displayName = parsed.displayName;
      if (parsed.username) step1Errors.username = parsed.username;
      if (Object.keys(step1Errors).length > 0) { setErrors(step1Errors); return; }
    }
    setErrors({});
    setCheckingUsername(true);
    try {
      await checkUsernameAvailability(username);
      setStep(2);
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setErrors({ username: 'Это имя пользователя уже занято' });
      } else {
        toast.error(parseError(err));
      }
    } finally {
      setCheckingUsername(false);
    }
  };

  const togglePreset = (preset: string) => {
    if (blacklistItems.includes(preset)) {
      setBlacklistItems((prev) => prev.filter((item) => item !== preset));
    } else if (blacklistItems.length < MAX_BLACKLIST_ITEMS) {
      setBlacklistItems((prev) => [...prev, preset]);
    }
  };

  const addCustomItem = () => {
    const trimmed = blacklistInput.trim();
    if (!trimmed || blacklistItems.length >= MAX_BLACKLIST_ITEMS) return;
    if (blacklistItems.includes(trimmed)) return;
    setBlacklistItems((prev) => [...prev, trimmed]);
    setBlacklistInput('');
  };

  const removeItem = (item: string) => {
    setBlacklistItems((prev) => prev.filter((i) => i !== item));
  };

  const handleStep3Next = () => {
    if (birthDate) {
      const dateError = validateBirthDate(birthDate);
      if (dateError) {
        setErrors((prev) => ({ ...prev, birthDate: dateError }));
        return;
      }
    }
    setErrors((prev) => ({ ...prev, birthDate: '' }));
    setStep(4);
  };

  const handleFinish = async () => {
    setSaving(true);
    try {
      await updateMyProfile({
        displayName,
        username,
        bio: bio || null,
        birthDate: birthDate || null,
        showFulfilledWishes: true,
      });

      if (blacklistItems.length > 0) {
        await Promise.all(blacklistItems.map((title) => addBlacklistItem(title)));
      }

      await refreshUser();
      setShowFinalScreen(true);
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setErrors({ username: 'Это имя пользователя уже занято' });
        setStep(1);
      } else {
        const fieldErrors = parseApiFieldErrors(err);
        if (fieldErrors) {
          setErrors((prev) => ({ ...prev, ...fieldErrors }));
          if (fieldErrors.birthDate) setStep(3);
          else setStep(1);
        } else {
          toast.error(parseError(err));
        }
      }
    } finally {
      setSaving(false);
    }
  };

  const currentAvatarSrc =
    previewAvatarUrl ?? getImageUrl(user?.avatarPath) ?? user?.avatarUrl ?? undefined;

  if (showFinalScreen) {
    return (
      <div className="flex items-center justify-center min-h-[calc(100vh-4rem)] py-8">
        <div className="w-full max-w-md text-center">
          <div className="text-5xl mb-4">🎉</div>
          <h1 className="text-2xl font-extrabold tracking-tight mb-2">Добро пожаловать!</h1>
          <p className="text-muted-foreground text-base italic mb-8">«{randomQuote}»</p>
          <Button className="w-full mb-3" size="lg" onClick={() => navigate('/wishlists/new')}>
            Создать первый вишлист
          </Button>
          <Button variant="ghost" className="w-full" onClick={() => navigate('/wishlists')}>
            Позже
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-[calc(100vh-4rem)] py-8">
      <div className="w-full max-w-md">
        <div className="text-center mb-6">
          <div className="text-4xl mb-2">🎁</div>
          <h1 className="text-2xl font-extrabold tracking-tight">Добро пожаловать в Wishapp</h1>
        </div>

        <div className="flex gap-1 mb-6">
          {[1, 2, 3, 4].map((s) => (
            <div
              key={s}
              className={`h-1 flex-1 rounded-full transition-colors ${step >= s ? 'bg-primary' : 'bg-muted'}`}
            />
          ))}
        </div>

        <Card>
          <CardContent className="pt-6">
            {step === 1 && (
              <div className="flex flex-col gap-4">
                <div>
                  <h2 className="font-semibold text-lg mb-1">Как тебя зовут?</h2>
                  <p className="text-sm text-muted-foreground">Это имя увидят твои друзья</p>
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
                  <Label htmlFor="username">Имя пользователя *</Label>
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
                <Button className="w-full mt-2" disabled={checkingUsername} onClick={handleStep1Next}>
                  {checkingUsername ? 'Проверка...' : 'Далее →'}
                </Button>
              </div>
            )}

            {step === 2 && (
              <div className="flex flex-col gap-4">
                <div>
                  <h2 className="font-semibold text-lg mb-1">Расскажи о себе</h2>
                  <p className="text-sm text-muted-foreground">Необязательно — можно добавить позже</p>
                </div>
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
                  <Label htmlFor="bio">О себе</Label>
                  <Textarea
                    id="bio"
                    value={bio}
                    onChange={(e) => setBio(e.target.value)}
                    placeholder="Расскажи о себе..."
                    rows={3}
                  />
                </div>
                <div className="flex gap-2 mt-2">
                  <Button variant="outline" className="flex-none" onClick={() => setStep(1)}>
                    ← Назад
                  </Button>
                  <Button className="flex-1" disabled={avatarUploading} onClick={() => setStep(3)}>
                    Далее →
                  </Button>
                </div>
              </div>
            )}

            {step === 3 && (
              <div className="flex flex-col gap-4">
                <div>
                  <h2 className="font-semibold text-lg mb-1">Когда твой день рождения?</h2>
                  <p className="text-sm text-muted-foreground">Друзья смогут напомнить себе о нём</p>
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="birthDate">Дата рождения</Label>
                  <Input
                    id="birthDate"
                    type="date"
                    value={birthDate}
                    onChange={(e) => { setBirthDate(e.target.value); clearError('birthDate'); }}
                    aria-invalid={!!errors.birthDate}
                    className="pr-3"
                  />
                  <FieldError message={errors.birthDate} />
                </div>
                <div className="flex gap-2 mt-2">
                  <Button variant="outline" className="flex-none" onClick={() => setStep(2)}>
                    ← Назад
                  </Button>
                  <Button className="flex-1" onClick={handleStep3Next}>
                    Далее →
                  </Button>
                </div>
              </div>
            )}

            {step === 4 && (
              <div className="flex flex-col gap-4">
                <div>
                  <h2 className="font-semibold text-lg mb-1">Что ты точно не хочешь?</h2>
                  <p className="text-sm text-muted-foreground">
                    До 5 позиций — друзья увидят это в твоём профиле
                  </p>
                </div>

                <div className="flex flex-wrap gap-2">
                  {BLACKLIST_PRESETS.map((preset) => {
                    const isSelected = blacklistItems.includes(preset);
                    const isDisabled = !isSelected && blacklistItems.length >= MAX_BLACKLIST_ITEMS;
                    return (
                      <button
                        key={preset}
                        type="button"
                        disabled={isDisabled}
                        onClick={() => togglePreset(preset)}
                        className={`px-3 py-1.5 rounded-full border text-sm transition-colors ${
                          isSelected
                            ? 'border-primary bg-primary/10 text-primary font-medium'
                            : isDisabled
                            ? 'border-muted text-muted-foreground cursor-not-allowed opacity-50'
                            : 'border-muted-foreground/30 hover:border-primary hover:bg-muted'
                        }`}
                      >
                        {preset}
                      </button>
                    );
                  })}
                </div>

                {blacklistItems.filter((item) => !BLACKLIST_PRESETS.includes(item)).map((item) => (
                  <div key={item} className="flex items-center gap-2">
                    <span className="px-3 py-1.5 rounded-full border border-primary bg-primary/10 text-primary text-sm font-medium">
                      {item}
                    </span>
                    <button
                      type="button"
                      onClick={() => removeItem(item)}
                      className="text-muted-foreground hover:text-foreground text-xs"
                    >
                      ✕
                    </button>
                  </div>
                ))}

                <div className="flex gap-2">
                  <Input
                    placeholder="Своя позиция..."
                    value={blacklistInput}
                    onChange={(e) => setBlacklistInput(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), addCustomItem())}
                    disabled={blacklistItems.length >= MAX_BLACKLIST_ITEMS}
                    maxLength={100}
                  />
                  <Button
                    type="button"
                    variant="outline"
                    onClick={addCustomItem}
                    disabled={!blacklistInput.trim() || blacklistItems.length >= MAX_BLACKLIST_ITEMS}
                  >
                    +
                  </Button>
                </div>

                <p className="text-xs text-muted-foreground text-right">{blacklistItems.length}/{MAX_BLACKLIST_ITEMS}</p>

                <div className="flex gap-2 mt-2">
                  <Button
                    variant="outline"
                    className="flex-none"
                    disabled={saving}
                    onClick={() => setStep(3)}
                  >
                    ← Назад
                  </Button>
                  <Button
                    className="flex-1"
                    disabled={saving}
                    onClick={() => handleFinish()}
                  >
                    {saving ? 'Сохранение...' : 'Завершить'}
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
