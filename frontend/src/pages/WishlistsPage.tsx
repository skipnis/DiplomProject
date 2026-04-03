import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getMyWishlists, deleteWishlist } from '../api/wishlists';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import { VISIBILITY_LABELS } from '../types';
import type { WishlistSummaryDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';

const SYSTEM_NAMES: Record<string, string> = {
  Hidden: '👁️ Скрытый',
  Blacklist: '🚫 Чёрный список',
};

export default function WishlistsPage() {
  const navigate = useNavigate();
  const toast = useToast();
  const [wishlists, setWishlists] = useState<WishlistSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyWishlists()
      .then(setWishlists)
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, []);

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.preventDefault();
    if (!confirm('Удалить вишлист?')) return;
    try {
      await deleteWishlist(id);
      setWishlists((prev) => prev.filter((w) => w.id !== id));
    } catch (e) {
      toast.error(parseError(e));
    }
  };

  const regular = wishlists.filter((w) => !w.isSystem);
  const system = wishlists.filter((w) => w.isSystem);

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-7 gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">Мои вишлисты</h1>
          <p className="text-sm text-muted-foreground mt-0.5">{regular.length} вишлист(ов)</p>
        </div>
        <Link to="/wishlists/new" className={buttonVariants()}>+ Создать</Link>
      </div>

      {regular.length === 0 ? (
        <div className="text-center py-16">
          <div className="text-5xl mb-4">🎁</div>
          <p className="font-semibold mb-1">Нет вишлистов</p>
          <p className="text-sm text-muted-foreground mb-4">Создай свой первый вишлист</p>
          <Link to="/wishlists/new" className={buttonVariants()}>Создать вишлист</Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {regular.map((w) => (
            <div
              key={w.id}
              className="relative rounded-xl border bg-card p-5 cursor-pointer hover:shadow-md hover:-translate-y-0.5 transition-all"
              onClick={() => navigate(`/wishlists/${w.id}`)}
            >
              <div className="text-3xl mb-2">{w.emoji || '📋'}</div>
              <div className="font-bold text-sm">{w.name}</div>
              {w.description && <div className="text-xs text-muted-foreground truncate mt-0.5">{w.description}</div>}
              <div className="flex gap-3 mt-2 text-xs text-muted-foreground">
                <span>{w.wishCount} желаний</span>
                <span>{VISIBILITY_LABELS[w.visibility]}</span>
              </div>
              <div className="flex gap-1 mt-3" onClick={(e) => e.stopPropagation()}>
                <Link to={`/wishlists/${w.id}/edit`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>Изменить</Link>
                <Button variant="destructive" size="sm" onClick={(e) => handleDelete(e, w.id)}>Удалить</Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {system.length > 0 && (
        <>
          <Separator className="my-6" />
          <h2 className="text-base font-semibold mb-4">Системные</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {system.map((w) => (
              <Link key={w.id} to={`/wishlists/${w.id}`} className="block rounded-xl border bg-card p-5 hover:shadow-md hover:-translate-y-0.5 transition-all">
                <div className="text-3xl mb-2">{w.emoji || '⚙️'}</div>
                <div className="font-bold text-sm">{SYSTEM_NAMES[w.name] || w.name}</div>
                <div className="text-xs text-muted-foreground mt-2">{w.wishCount} желаний</div>
              </Link>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
