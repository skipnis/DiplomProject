import { useState, useEffect, useRef } from 'react';
import { GoogleLogin } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { googleSignIn, sendOtp, verifyOtp } from '../api/auth';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { Card, CardContent } from '@/components/ui/card';
import { z } from 'zod';

const emailSchema = z.string().email('Введите корректный email');

const RESEND_COOLDOWN = 60;

type Step = 'select' | 'email' | 'code';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

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
      navigate('/wishlists', { replace: true });
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const handleSendOtp = async () => {
    const parsed = emailSchema.safeParse(email);
    if (!parsed.success) {
      setEmailError(parsed.error.errors[0].message);
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
      navigate('/wishlists', { replace: true });
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
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={() => toast.error('Ошибка Google авторизации')}
                text="continue_with"
                shape="rectangular"
                size="large"
              />
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
              <input
                type="text"
                inputMode="numeric"
                maxLength={6}
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
