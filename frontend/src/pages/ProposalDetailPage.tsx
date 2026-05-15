import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { getProposalDetail, reactToProposal } from '../api/proposals';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { ProposalDetailDto } from '../types';
import { PROPOSAL_SOURCE_LABELS, PROPOSAL_STATUS_LABELS } from '../types';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
}

export default function ProposalDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [proposal, setProposal] = useState<ProposalDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [revealLevel, setRevealLevel] = useState(0);
  const [reacting, setReacting] = useState(false);
  const [reactedStatus, setReactedStatus] = useState<1 | 2 | null>(null);

  useEffect(() => {
    if (!id) return;
    getProposalDetail(id)
      .then((p) => {
        setProposal(p);
        if (p.status !== 0) setRevealLevel(2);
      })
      .catch((e) => toast.error(parseError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  const handleReact = async (status: 1 | 2) => {
    if (!id || reacting) return;
    setReacting(true);
    setReactedStatus(status);
    try {
      await reactToProposal(id, { status, comment: null });
      setProposal((prev) => prev ? { ...prev, status, reactedAt: new Date().toISOString() } : prev);
      setTimeout(() => navigate('/proposals'), 750);
    } catch (e) {
      toast.error(parseError(e));
      setReacting(false);
      setReactedStatus(null);
    }
  };

  if (loading) return <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>;
  if (!proposal) return null;

  const getTitle = () => {
    if (proposal.sourceType === 1) return proposal.catalogItemName;
    if (proposal.sourceType === 2) return proposal.wishlistItemName;
    return proposal.customTitle;
  };

  const getDescription = () => {
    if (proposal.sourceType === 1) return null;
    if (proposal.sourceType === 2) return proposal.wishlistItemDescription;
    return proposal.customDescription;
  };

  const getImagePath = () => {
    if (proposal.sourceType === 1) return proposal.catalogItemImagePath;
    if (proposal.sourceType === 2) return proposal.wishlistItemImagePath;
    return proposal.customImagePath;
  };

  const title = getTitle();
  const description = getDescription();
  const imgUrl = getImageUrl(getImagePath());

  if (proposal.isOwnProposal) {
    return (
      <div>
        <div className="flex items-center gap-3 mb-6">
          <Link to="/proposals" className="text-sm text-muted-foreground hover:text-foreground">← Назад</Link>
          <h1 className="text-xl font-bold">Предложение подарка</h1>
        </div>

        <Card>
          <CardContent className="pt-6 space-y-4">
            <div className="flex items-center gap-3">
              <Avatar className="h-10 w-10">
                <AvatarImage src={getImageUrl(proposal.recipientAvatarUrl) ?? undefined} />
                <AvatarFallback>{proposal.recipientDisplayName?.[0] ?? '?'}</AvatarFallback>
              </Avatar>
              <div>
                <p className="font-medium">{proposal.recipientDisplayName}</p>
                <p className="text-xs text-muted-foreground">Получатель</p>
              </div>
            </div>

            <div className="flex gap-2 flex-wrap">
              <Badge variant="outline">{PROPOSAL_SOURCE_LABELS[proposal.sourceType]}</Badge>
              {proposal.status === 0 && <Badge variant="secondary">{PROPOSAL_STATUS_LABELS[0]}</Badge>}
              {proposal.status === 1 && <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Хочу</Badge>}
              {proposal.status === 2 && <Badge className="bg-red-100 text-red-700 hover:bg-red-100">Не моё</Badge>}
            </div>

            {imgUrl && <img src={imgUrl} alt="" className="rounded-lg w-full max-h-64 object-cover" />}
            {title && <p className="font-semibold text-lg">{title}</p>}
            {description && <p className="text-sm text-muted-foreground">{description}</p>}

            {proposal.catalogItemId && proposal.sourceType === 1 && (
              <Link to={`/catalog/items/${proposal.catalogItemId}`} className="text-sm text-primary hover:underline">
                Посмотреть в каталоге →
              </Link>
            )}

            {proposal.hintMessage && (
              <div className="bg-muted/50 rounded-lg p-3">
                <p className="text-xs text-muted-foreground mb-1">Твоя записка:</p>
                <p className="text-sm italic">"{proposal.hintMessage}"</p>
              </div>
            )}

            {proposal.recipientComment && (
              <div className="bg-muted/50 rounded-lg p-3">
                <p className="text-xs text-muted-foreground mb-1">Комментарий получателя:</p>
                <p className="text-sm italic">"{proposal.recipientComment}"</p>
              </div>
            )}

            <p className="text-xs text-muted-foreground">Отправлено {formatDate(proposal.createdAt)}</p>
            {proposal.reactedAt && <p className="text-xs text-muted-foreground">Ответ получен {formatDate(proposal.reactedAt)}</p>}
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <Link to="/proposals" className="text-sm text-muted-foreground hover:text-foreground">← Назад</Link>
        <h1 className="text-xl font-bold">Тебе предложили подарок</h1>
      </div>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="flex items-center justify-between flex-wrap gap-2">
            <p className="text-sm text-muted-foreground">от: <span className="font-medium">{proposal.senderAlias}</span></p>
          </div>

          {proposal.hintMessage && (
            <div className="bg-muted/50 rounded-lg p-3">
              <p className="text-xs text-muted-foreground mb-1">Записка от дарящего:</p>
              <p className="text-sm italic">"{proposal.hintMessage}"</p>
            </div>
          )}

          {title && <p className="font-semibold text-xl">{title}</p>}

          {description && revealLevel < 1 && proposal.status === 0 && (
            <Button variant="outline" onClick={() => setRevealLevel(1)}>Показать описание</Button>
          )}

          {description && revealLevel >= 1 && (
            <p className="text-sm text-muted-foreground">{description}</p>
          )}

          {imgUrl && revealLevel < 2 && proposal.status === 0 && (
            <Button variant="outline" onClick={() => setRevealLevel(2)}>Показать фото</Button>
          )}

          {imgUrl && revealLevel >= 2 && (
            <img src={imgUrl} alt="" className="rounded-lg w-full max-h-64 object-cover" />
          )}

          {proposal.status === 0 && (
            <div className="flex gap-3 pt-2">
              <Button
                className={`flex-1 transition-all ${reactedStatus === 1 ? 'scale-105' : ''}`}
                onClick={() => handleReact(1)}
                disabled={reacting}
              >
                Хочу
              </Button>
              <Button
                variant="outline"
                className={`flex-1 transition-all ${reactedStatus === 2 ? 'scale-105' : ''}`}
                onClick={() => handleReact(2)}
                disabled={reacting}
              >
                Не моё
              </Button>
            </div>
          )}

          {proposal.status !== 0 && (
            <div className="pt-2">
              {proposal.status === 1 && <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Ты ответил: Хочу</Badge>}
              {proposal.status === 2 && <Badge className="bg-red-100 text-red-700 hover:bg-red-100">Ты ответил: Не моё</Badge>}
              {proposal.reactedAt && <p className="text-xs text-muted-foreground mt-1">{formatDate(proposal.reactedAt)}</p>}
            </div>
          )}

          <p className="text-xs text-muted-foreground">Получено {formatDate(proposal.createdAt)}</p>
        </CardContent>
      </Card>
    </div>
  );
}
