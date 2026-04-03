import { GoogleLogin } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { googleSignIn } from '../api/auth';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { Card, CardContent } from '@/components/ui/card';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const handleSuccess = async (credential: { credential?: string }) => {
    if (!credential.credential) return;
    try {
      await googleSignIn(credential.credential);
      await login();
      navigate('/wishlists', { replace: true });
    } catch (e) {
      toast.error(parseError(e));
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
          <GoogleLogin
            onSuccess={handleSuccess}
            onError={() => toast.error('Ошибка Google авторизации')}
            text="continue_with"
            shape="rectangular"
            size="large"
          />
        </CardContent>
      </Card>
    </div>
  );
}
