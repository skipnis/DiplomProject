import { useState, useEffect, useRef } from 'react';
import { GoogleLogin } from '@react-oauth/google';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { googleSignIn, sendOtp, verifyOtp } from '../api/auth';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { Card, CardContent } from '@/components/ui/card';
import { z } from 'zod';
import { OTP_CODE_LENGTH } from '@/lib/utils';

const emailSchema = z.string().email('Введите корректный email');

const RESEND_COOLDOWN = 60;

type Step = 'select' | 'email' | 'code';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const toast = useToast();
  const redirectPath = searchParams.get('redirect') ?? '/wishlists';

  const [step, setStep] = useState<Step>('select');
  const [email, setEmail] = useState('');
  const [emailError, setEmailError] = useState('');
  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [cooldown, setCooldown] = useState(0);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);

  const startCooldown = () => {
    setCooldown(RESEND_COOLDOWN);
    intervalRef.current = setInterval(() => {
      setCooldown(prev => {
        if (prev <= 1) {
          clearInterval(intervalRef.current!);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const handleGoogleSuccess = async (credential: { credential?: string }) => {
    if (!credential.credential) return;
    try {
      await googleSignIn(credential.credential);
      await login();
      navigate(redirectPath, { replace: true });
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const handleSendOtp = async () => {
    const parsed = emailSchema.safeParse(email);
    if (!parsed.success) {
      setEmailError(parsed.error.issues[0].message);
      return;
    }
    setEmailError('');
    setLoading(true);
    try {
      await sendOtp(email);
      setStep('code');
      startCooldown();
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async () => {
    setLoading(true);
    try {
      await verifyOtp(email, code);
      await login();
      navigate(redirectPath, { replace: true });
    } catch (e) {
      toast.error(parseError(e));
      setCode('');
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    if (cooldown > 0) return;
    setLoading(true);
    try {
      await sendOtp(email);
      startCooldown();
      toast.success?.('Код отправлен повторно');
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-[calc(100vh-4rem)]">
      <Card className="w-full max-w-sm">
        <CardContent className="flex flex-col items-center gap-6 pt-8 pb-8">
          <div className="text-6xl">🎁</div>
          <div className="text-center">
            <h1 className="text-2xl font-extrabold tracking-tight">Wishapp</h1>
            <p className="text-sm text-muted-foreground mt-1">Создавай вишлисты, делись желаниями с друзьями</p>
          </div>

          {step === 'select' && (
            <div className="flex flex-col gap-3 w-full">
              <div className="relative w-full h-10">
                <button
                  type="button"
                  className="absolute inset-0 flex items-center justify-center gap-2 w-full border border-border rounded-md text-sm font-medium bg-background hover:bg-accent transition-colors pointer-events-none"
                >
                  <svg width="18" height="18" viewBox="0 0 18 18" xmlns="http://www.w3.org/2000/svg">
                    <path d="M17.64 9.2c0-.637-.057-1.251-.164-1.84H9v3.481h4.844c-.209 1.125-.843 2.078-1.796 2.717v2.258h2.908c1.702-1.567 2.684-3.874 2.684-6.615z" fill="#4285F4"/>
                    <path d="M9 18c2.43 0 4.467-.806 5.956-2.184l-2.908-2.258c-.806.54-1.837.86-3.048.86-2.344 0-4.328-1.584-5.036-3.711H.957v2.332A8.997 8.997 0 0 0 9 18z" fill="#34A853"/>
                    <path d="M3.964 10.707A5.41 5.41 0 0 1 3.682 9c0-.593.102-1.17.282-1.707V4.961H.957A8.996 8.996 0 0 0 0 9c0 1.452.348 2.827.957 4.039l3.007-2.332z" fill="#FBBC05"/>
                    <path d="M9 3.58c1.321 0 2.508.454 3.44 1.345l2.582-2.58C13.463.891 11.426 0 9 0A8.997 8.997 0 0 0 .957 4.961L3.964 6.293C4.672 4.166 6.656 3.58 9 3.58z" fill="#EA4335"/>
                  </svg>
                  Продолжить с Google
                </button>
                <div className="absolute inset-0 opacity-0 overflow-hidden [&>div]:!w-full [&>div]:!h-full">
                  <GoogleLogin
                    onSuccess={handleGoogleSuccess}
                    onError={() => toast.error('Ошибка Google авторизации')}
                    shape="rectangular"
                    size="large"
                    width="400"
                  />
                </div>
              </div>
              <div className="flex items-center gap-2">
                <div className="flex-1 h-px bg-border" />
                <span className="text-xs text-muted-foreground">или</span>
                <div className="flex-1 h-px bg-border" />
              </div>
              <button
                onClick={() => setStep('email')}
                className="w-full border border-border rounded-md py-2 text-sm font-medium hover:bg-accent transition-colors"
              >
                Войти по email
              </button>

            </div>
          )}

          {step === 'email' && (
            <div className="flex flex-col gap-3 w-full">
              <div className="flex flex-col gap-1">
                <input
                  type="email"
                  placeholder="Email"
                  value={email}
                  onChange={e => { setEmail(e.target.value); setEmailError(''); }}
                  onKeyDown={e => e.key === 'Enter' && handleSendOtp()}
                  className="border border-border rounded-md px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
                  autoFocus
                />
                {emailError && <p className="text-xs text-destructive">{emailError}</p>}
              </div>
              <button
                onClick={handleSendOtp}
                disabled={loading}
                className="w-full bg-primary text-primary-foreground rounded-md py-2 text-sm font-medium hover:bg-primary/90 disabled:opacity-50 transition-colors"
              >
                {loading ? 'Отправляем...' : 'Получить код'}
              </button>
              <button
                onClick={() => setStep('select')}
                className="text-xs text-muted-foreground hover:underline"
              >
                Назад
              </button>
            </div>
          )}

          {step === 'code' && (
            <div className="flex flex-col gap-3 w-full">
              <p className="text-sm text-center text-muted-foreground">
                Код отправлен на <span className="font-medium text-foreground">{email}</span>
              </p>
              <p className="text-xs text-center text-muted-foreground">
                Если письмо с кодом не пришло, проверьте папку «Спам» или «Промоакции»
              </p>
              <input
                type="text"
                inputMode="numeric"
                maxLength={OTP_CODE_LENGTH}
                placeholder="000000"
                value={code}
                onChange={e => setCode(e.target.value.replace(/\D/g, ''))}
                onKeyDown={e => e.key === 'Enter' && code.length === 6 && handleVerifyOtp()}
                className="border border-border rounded-md px-3 py-2 text-sm text-center tracking-widest outline-none focus:ring-2 focus:ring-ring"
                autoFocus
              />
              <button
                onClick={handleVerifyOtp}
                disabled={loading || code.length !== 6}
                className="w-full bg-primary text-primary-foreground rounded-md py-2 text-sm font-medium hover:bg-primary/90 disabled:opacity-50 transition-colors"
              >
                {loading ? 'Проверяем...' : 'Войти'}
              </button>
              <button
                onClick={handleResend}
                disabled={cooldown > 0 || loading}
                className="text-xs text-muted-foreground hover:underline disabled:opacity-50"
              >
                {cooldown > 0 ? `Отправить повторно через ${cooldown}с` : 'Отправить код повторно'}
              </button>
              <button
                onClick={() => { setStep('email'); setCode(''); }}
                className="text-xs text-muted-foreground hover:underline"
              >
                Изменить email
              </button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
