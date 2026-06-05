import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { getWish, addGiftBadges } from '../api/wishes';
import { getFulfilledBadgeDefinitions } from '../api/catalog';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { WishDto, FulfilledBadgeDefinitionDto } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

export default function RateGifterPage() {
  const { id: wishlistId, wishId } = useParams<{ id: string; wishId: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [wish, setWish] = useState<WishDto | null>(null);
  const [definitions, setDefinitions] = useState<FulfilledBadgeDefinitionDto[]>([]);
  const [selected, setSelected] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!wishlistId || !wishId) return;
    Promise.all([getWish(wishlistId, wishId), getFulfilledBadgeDefinitions()])
      .then(([fetchedWish, fetchedDefinitions]) => {
        if (!fetchedWish.isFulfilled || !fetchedWish.fulfilledByReserverId || fetchedWish.hasGiftBadges) {
          navigate(`/wishlists/${wishlistId}/wishes/${wishId}`, { replace: true });
          return;
        }
        setWish(fetchedWish);
        setDefinitions(fetchedDefinitions.filter((def) => def.isActive));
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [wishlistId, wishId]);

  const toggleBadge = (badgeType: number) => {
    setSelected((prev) => {
      if (prev.includes(badgeType)) return prev.filter((b) => b !== badgeType);
      if (prev.length >= 3) return prev;
      return [...prev, badgeType];
    });
  };

  const handleSubmit = async () => {
    if (!wishlistId || !wishId || selected.length === 0) return;
    setSubmitting(true);
    try {
      await addGiftBadges(wishlistId, wishId, selected);
      toast.success('Спасибо за оценку!');
      navigate(`/wishlists/${wishlistId}/wishes/${wishId}`);
    } catch (e) {
      toast.error(parseError(e));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!wish) return null;

  return (
    <div className="max-w-xl mx-auto">
      <div className="flex items-center justify-between mb-7 gap-3">
        <Link to={`/wishlists/${wishlistId}/wishes/${wishId}`} className={buttonVariants({ variant: 'ghost', size: 'sm' })}>← Назад</Link>
      </div>

      <Card>
        <CardContent className="pt-6">
          <h1 className="text-xl font-extrabold tracking-tight mb-1">Оцените подарок</h1>
          {wish.fulfilledByDisplayName && (
            <p className="text-muted-foreground text-sm mb-6">
              Подарок от <span className="font-medium text-foreground">{wish.fulfilledByDisplayName}</span> — «{wish.name}»
            </p>
          )}

          <p className="text-sm text-muted-foreground mb-4">
            Выберите до 3 характеристик, которые лучше всего описывают впечатление от этого подарка.
          </p>

          <div className="flex flex-wrap gap-2 mb-6">
            {definitions.map((def) => {
              const isSelected = selected.includes(def.id);
              const isDisabled = !isSelected && selected.length >= 3;
              return (
                <button
                  key={def.id}
                  onClick={() => !isDisabled && toggleBadge(def.id)}
                  className={[
                    'px-3 py-1.5 rounded-full text-sm border transition-all',
                    isSelected
                      ? 'border-primary bg-primary/10 text-primary font-semibold'
                      : isDisabled
                        ? 'border-border bg-background text-muted-foreground opacity-40 cursor-not-allowed'
                        : 'border-border bg-background text-foreground cursor-pointer hover:border-primary/60',
                  ].join(' ')}
                >
                  {def.emoji} {def.label}
                </button>
              );
            })}
          </div>

          {selected.length >= 3 && (
            <p className="text-xs text-muted-foreground mb-4">Максимум 3 бейджа</p>
          )}

          <div className="flex gap-2">
            <Button
              disabled={selected.length === 0 || submitting}
              onClick={handleSubmit}
            >
              {submitting ? 'Отправка...' : 'Отправить оценку'}
            </Button>
            <Button variant="ghost" onClick={() => navigate(`/wishlists/${wishlistId}/wishes/${wishId}`)}>
              Пропустить
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
