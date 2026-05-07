import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getIncomingProposals, getOutgoingProposals } from '../api/proposals';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type {
  IncomingProposalDto,
  OutgoingProposalDto,
  PagedResponse,
  ProposalStatus,
} from '../types';
import { PROPOSAL_SOURCE_LABELS } from '../types';
import { Button, buttonVariants } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

const PAGE_SIZE = 20;

function StatusBadge({ status }: { status: ProposalStatus }) {
  if (status === 1) return <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Хочу</Badge>;
  if (status === 2) return <Badge className="bg-red-100 text-red-700 hover:bg-red-100">Не моё</Badge>;
  return <Badge variant="secondary">Ожидает</Badge>;
}

function getProposalTitle(p: IncomingProposalDto | OutgoingProposalDto): string | null {
  if (p.sourceType === 1) return p.catalogItemName;
  if (p.sourceType === 2) return p.wishlistItemName;
  return p.customTitle;
}

function getProposalImage(p: IncomingProposalDto | OutgoingProposalDto): string | null {
  if (p.sourceType === 1) return getImageUrl(p.catalogItemImagePath);
  if (p.sourceType === 2) return getImageUrl(p.wishlistItemImagePath);
  return getImageUrl(p.customImagePath);
}

function IncomingCard({ proposal }: { proposal: IncomingProposalDto }) {
  const imgUrl = getProposalImage(proposal);
  const title = getProposalTitle(proposal);

  return (
    <Link to={`/proposals/${proposal.id}`}>
      <Card className={`hover:shadow-md hover:-translate-y-0.5 transition-all ${!proposal.isViewedByRecipient ? 'ring-1 ring-primary/40' : ''}`}>
        <CardContent className="pt-4 flex items-start gap-4">
          <div className="w-14 h-14 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-2xl">
            {imgUrl
              ? <img src={imgUrl} alt="" className="w-full h-full object-cover" />
              : '🎁'
            }
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <Badge variant="outline" className="text-xs">{PROPOSAL_SOURCE_LABELS[proposal.sourceType]}</Badge>
              <StatusBadge status={proposal.status} />
              {!proposal.isViewedByRecipient && <Badge className="text-xs bg-primary text-primary-foreground">Новое</Badge>}
            </div>
            {proposal.status === 0
              ? <p className="text-sm text-muted-foreground italic">Нажми, чтобы узнать что это</p>
              : <p className="text-sm font-medium line-clamp-1">{title ?? 'Подарок'}</p>
            }
            {proposal.hintMessage && (
              <p className="text-xs text-muted-foreground italic mt-1 line-clamp-1">"{proposal.hintMessage}"</p>
            )}
            <p className="text-xs text-muted-foreground mt-1">
              {new Date(proposal.createdAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })}
            </p>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}

function OutgoingCard({ proposal }: { proposal: OutgoingProposalDto }) {
  const imgUrl = getProposalImage(proposal);
  const title = getProposalTitle(proposal);

  return (
    <Link to={`/proposals/${proposal.id}`}>
      <Card className="hover:shadow-md hover:-translate-y-0.5 transition-all">
        <CardContent className="pt-4 flex items-start gap-4">
          <div className="w-14 h-14 rounded-lg bg-muted flex-shrink-0 overflow-hidden flex items-center justify-center text-2xl">
            {imgUrl
              ? <img src={imgUrl} alt="" className="w-full h-full object-cover" />
              : '🎁'
            }
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <Avatar className="h-5 w-5">
                <AvatarImage src={getImageUrl(proposal.recipientAvatarUrl) ?? undefined} />
                <AvatarFallback className="text-[10px]">{proposal.recipientDisplayName[0]}</AvatarFallback>
              </Avatar>
              <span className="text-sm font-medium">{proposal.recipientDisplayName}</span>
              <StatusBadge status={proposal.status} />
            </div>
            <p className="text-sm text-muted-foreground line-clamp-1">{title ?? 'Подарок'}</p>
            {proposal.recipientComment && (
              <p className="text-xs text-muted-foreground italic mt-1 line-clamp-1">"{proposal.recipientComment}"</p>
            )}
            <p className="text-xs text-muted-foreground mt-1">
              {new Date(proposal.createdAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })}
            </p>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}

export default function ProposalsPage() {
  const toast = useToast();
  const [tab, setTab] = useState<'incoming' | 'outgoing'>('incoming');
  const [incoming, setIncoming] = useState<PagedResponse<IncomingProposalDto> | null>(null);
  const [outgoing, setOutgoing] = useState<PagedResponse<OutgoingProposalDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const load = (t: 'incoming' | 'outgoing', p: number) => {
    setLoading(true);
    const req = t === 'incoming'
      ? getIncomingProposals(p, PAGE_SIZE).then(setIncoming)
      : getOutgoingProposals(p, PAGE_SIZE).then(setOutgoing);
    req.catch((e) => toast.error(parseError(e))).finally(() => setLoading(false));
  };

  useEffect(() => { load(tab, page); }, [tab, page]);

  const switchTab = (t: 'incoming' | 'outgoing') => {
    setTab(t);
    setPage(1);
  };

  const data = tab === 'incoming' ? incoming : outgoing;
  const unviewedCount = incoming?.items.filter((p) => !p.isViewedByRecipient).length ?? 0;

  const tabCls = (t: 'incoming' | 'outgoing') =>
    `px-4 py-2 text-sm font-medium rounded-md transition-colors ${tab === t ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:text-foreground hover:bg-muted'}`;

  return (
    <div>
      <div className="flex items-center justify-between mb-6 gap-4">
        <h1 className="text-2xl font-extrabold tracking-tight">Предложения</h1>
        <Link to="/proposals/new" className={buttonVariants({ size: 'sm' })}>+ Предложить подарок</Link>
      </div>

      <div className="flex gap-2 mb-6">
        <button onClick={() => switchTab('incoming')} className={tabCls('incoming')}>
          Входящие
          {unviewedCount > 0 && (
            <span className="ml-2 inline-flex items-center justify-center bg-destructive text-destructive-foreground text-[10px] font-bold rounded-full min-w-[16px] h-4 px-1">
              {unviewedCount}
            </span>
          )}
        </button>
        <button onClick={() => switchTab('outgoing')} className={tabCls('outgoing')}>
          Исходящие
        </button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center min-h-[200px] text-muted-foreground">Загрузка...</div>
      ) : !data || data.items.length === 0 ? (
        <div className="text-center py-16">
          <div className="text-5xl mb-4">🎁</div>
          {tab === 'incoming' ? (
            <>
              <p className="font-semibold mb-1">Нет входящих предложений</p>
              <p className="text-sm text-muted-foreground">Друзья могут анонимно предлагать тебе идеи подарков</p>
            </>
          ) : (
            <>
              <p className="font-semibold mb-1">Нет исходящих предложений</p>
              <p className="text-sm text-muted-foreground mb-4">Предложи другу идею подарка анонимно</p>
              <Link to="/proposals/new" className={buttonVariants({ size: 'sm' })}>Предложить подарок</Link>
            </>
          )}
        </div>
      ) : (
        <>
          <div className="flex flex-col gap-3">
            {tab === 'incoming'
              ? (data as PagedResponse<IncomingProposalDto>).items.map((p) => <IncomingCard key={p.id} proposal={p} />)
              : (data as PagedResponse<OutgoingProposalDto>).items.map((p) => <OutgoingCard key={p.id} proposal={p} />)
            }
          </div>

          {(data.hasPreviousPage || data.hasNextPage) && (
            <div className="flex items-center justify-center gap-3 mt-6">
              <Button variant="ghost" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>← Назад</Button>
              <span className="text-sm text-muted-foreground">{page} / {Math.ceil(data.totalCount / PAGE_SIZE)}</span>
              <Button variant="ghost" size="sm" disabled={!data.hasNextPage} onClick={() => setPage((p) => p + 1)}>Вперёд →</Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
