import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getFriends, getFriendshipRequests, removeFriend, acceptFriendRequest, declineFriendRequest, sendFriendRequest } from '../api/friends';
import { searchUsers } from '../api/users';
import { getImageUrl } from '../api/client';
import { useToast } from '../components/Toast';
import { parseError } from '../utils/errors';
import type { FriendInfo, FriendshipRequest, UserSearchResult } from '../types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

const PAGE_SIZE = 20;

function UserRow({ avatarUrl, name, userId, children }: { avatarUrl: string | null; name: string; userId: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-3 p-3 rounded-lg border bg-card">
      <Avatar className="h-9 w-9">
        <AvatarImage src={getImageUrl(avatarUrl) ?? undefined} />
        <AvatarFallback className="text-sm font-semibold">{name[0].toUpperCase()}</AvatarFallback>
      </Avatar>
      <div className="flex-1 min-w-0">
        <Link to={`/users/${userId}`} className="text-sm font-semibold hover:underline">{name}</Link>
      </div>
      {children}
    </div>
  );
}

export default function FriendsPage() {
  const toast = useToast();

  const [friends, setFriends] = useState<FriendInfo[]>([]);
  const [friendPage, setFriendPage] = useState(1);
  const [friendTotalPages, setFriendTotalPages] = useState(1);
  const [friendTotalCount, setFriendTotalCount] = useState(0);
  const [friendsLoading, setFriendsLoading] = useState(true);

  const [requests, setRequests] = useState<FriendshipRequest[]>([]);
  const [requestPage, setRequestPage] = useState(1);
  const [requestTotalPages, setRequestTotalPages] = useState(1);
  const [requestTotalCount, setRequestTotalCount] = useState(0);
  const [requestsLoading, setRequestsLoading] = useState(true);

  const [outgoing, setOutgoing] = useState<FriendshipRequest[]>([]);
  const [outgoingPage, setOutgoingPage] = useState(1);
  const [outgoingTotalPages, setOutgoingTotalPages] = useState(1);
  const [outgoingTotalCount, setOutgoingTotalCount] = useState(0);
  const [outgoingLoading, setOutgoingLoading] = useState(true);

  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<UserSearchResult[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);

  const loadFriends = async (page: number) => {
    setFriendsLoading(true);
    try {
      const res = await getFriends(page, PAGE_SIZE);
      setFriends(res.items);
      setFriendTotalPages(Math.ceil(res.totalCount / res.pageSize));
      setFriendTotalCount(res.totalCount);
    } catch (e) { toast.error(parseError(e)); }
    finally { setFriendsLoading(false); }
  };

  const loadRequests = async (page: number) => {
    setRequestsLoading(true);
    try {
      const res = await getFriendshipRequests('Pending', false, page, PAGE_SIZE);
      setRequests(res.items);
      setRequestTotalPages(Math.ceil(res.totalCount / res.pageSize));
      setRequestTotalCount(res.totalCount);
    } catch (e) { toast.error(parseError(e)); }
    finally { setRequestsLoading(false); }
  };

  const loadOutgoing = async (page: number) => {
    setOutgoingLoading(true);
    try {
      const res = await getFriendshipRequests('Pending', true, page, PAGE_SIZE);
      setOutgoing(res.items);
      setOutgoingTotalPages(Math.ceil(res.totalCount / res.pageSize));
      setOutgoingTotalCount(res.totalCount);
    } catch (e) { toast.error(parseError(e)); }
    finally { setOutgoingLoading(false); }
  };

  useEffect(() => { loadFriends(friendPage); }, [friendPage]);
  useEffect(() => { loadRequests(requestPage); }, [requestPage]);
  useEffect(() => { loadOutgoing(outgoingPage); }, [outgoingPage]);

  const handleSearch = async () => {
    if (!searchQuery.trim()) return;
    setSearchLoading(true);
    try { const res = await searchUsers(searchQuery.trim()); setSearchResults(res.items); }
    catch (e) { toast.error(parseError(e)); }
    finally { setSearchLoading(false); }
  };

  const handleRemoveFriend = async (userId: string) => {
    try { await removeFriend(userId); setFriends((p) => p.filter((f) => f.userId !== userId)); setFriendTotalCount((n) => n - 1); toast.success('Пользователь удалён из друзей'); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleAccept = async (userId: string) => {
    try {
      await acceptFriendRequest(userId);
      const accepted = requests.find((r) => r.userId === userId);
      setRequests((p) => p.filter((r) => r.userId !== userId));
      setRequestTotalCount((n) => n - 1);
      if (accepted) { setFriends((p) => [...p, { userId: accepted.userId, username: accepted.username, avatarUrl: accepted.avatarUrl }]); setFriendTotalCount((n) => n + 1); }
      toast.success('Заявка принята');
    } catch (e) { toast.error(parseError(e)); }
  };

  const handleDecline = async (userId: string) => {
    try { await declineFriendRequest(userId); setRequests((p) => p.filter((r) => r.userId !== userId)); setRequestTotalCount((n) => n - 1); toast.success('Заявка отклонена'); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleCancelRequest = async (userId: string) => {
    try { await removeFriend(userId); setOutgoing((p) => p.filter((r) => r.userId !== userId)); setOutgoingTotalCount((n) => n - 1); toast.success('Заявка отменена'); }
    catch (e) { toast.error(parseError(e)); }
  };

  const handleSendRequest = async (userId: string) => {
    try { await sendFriendRequest(userId); setSearchResults((p) => p.filter((u) => u.id !== userId)); toast.success('Заявка отправлена'); }
    catch (e) { toast.error(parseError(e)); }
  };

  return (
    <div>
      <div className="mb-7">
        <h1 className="text-2xl font-extrabold tracking-tight">Друзья</h1>
      </div>

      <Tabs defaultValue="friends">
        <TabsList className="mb-6">
          <TabsTrigger value="friends">Друзья {friendTotalCount > 0 && `(${friendTotalCount})`}</TabsTrigger>
          <TabsTrigger value="requests">Заявки {(requestTotalCount + outgoingTotalCount) > 0 && `(${requestTotalCount + outgoingTotalCount})`}</TabsTrigger>
          <TabsTrigger value="search">Поиск</TabsTrigger>
        </TabsList>

        <TabsContent value="friends">
          {friendsLoading ? <p className="text-sm text-muted-foreground">Загрузка...</p> :
            friends.length === 0 ? (
              <div className="text-center py-12">
                <div className="text-4xl mb-3">👥</div>
                <p className="font-semibold mb-1">Пока нет друзей</p>
                <p className="text-sm text-muted-foreground">Найди друзей в поиске</p>
              </div>
            ) : (
              <>
                <div className="flex flex-col gap-2">
                  {friends.map((f) => (
                    <UserRow key={f.userId} avatarUrl={f.avatarUrl} name={f.username} userId={f.userId}>
                      <Button variant="destructive" size="sm" onClick={() => handleRemoveFriend(f.userId)}>Удалить</Button>
                    </UserRow>
                  ))}
                </div>
                {friendTotalPages > 1 && (
                  <div className="flex items-center justify-center gap-3 mt-4">
                    <Button variant="ghost" size="sm" disabled={friendPage === 1} onClick={() => setFriendPage((p) => p - 1)}>← Назад</Button>
                    <span className="text-sm text-muted-foreground">{friendPage} / {friendTotalPages}</span>
                    <Button variant="ghost" size="sm" disabled={friendPage === friendTotalPages} onClick={() => setFriendPage((p) => p + 1)}>Вперёд →</Button>
                  </div>
                )}
              </>
            )
          }
        </TabsContent>

        <TabsContent value="requests">
          <div className="flex flex-col gap-6">
            <div>
              <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-3">Входящие {requestTotalCount > 0 && `(${requestTotalCount})`}</h2>
              {requestsLoading ? <p className="text-sm text-muted-foreground">Загрузка...</p> :
                requests.length === 0 ? (
                  <p className="text-sm text-muted-foreground">Нет входящих заявок</p>
                ) : (
                  <>
                    <div className="flex flex-col gap-2">
                      {requests.map((r) => (
                        <UserRow key={r.friendshipId} avatarUrl={r.avatarUrl} name={r.username} userId={r.userId}>
                          <div className="flex gap-2">
                            <Button size="sm" onClick={() => handleAccept(r.userId)}>Принять</Button>
                            <Button size="sm" variant="ghost" onClick={() => handleDecline(r.userId)}>Отклонить</Button>
                          </div>
                        </UserRow>
                      ))}
                    </div>
                    {requestTotalPages > 1 && (
                      <div className="flex items-center justify-center gap-3 mt-4">
                        <Button variant="ghost" size="sm" disabled={requestPage === 1} onClick={() => setRequestPage((p) => p - 1)}>← Назад</Button>
                        <span className="text-sm text-muted-foreground">{requestPage} / {requestTotalPages}</span>
                        <Button variant="ghost" size="sm" disabled={requestPage === requestTotalPages} onClick={() => setRequestPage((p) => p + 1)}>Вперёд →</Button>
                      </div>
                    )}
                  </>
                )
              }
            </div>

            <div>
              <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-3">Исходящие {outgoingTotalCount > 0 && `(${outgoingTotalCount})`}</h2>
              {outgoingLoading ? <p className="text-sm text-muted-foreground">Загрузка...</p> :
                outgoing.length === 0 ? (
                  <p className="text-sm text-muted-foreground">Нет исходящих заявок</p>
                ) : (
                  <>
                    <div className="flex flex-col gap-2">
                      {outgoing.map((r) => (
                        <UserRow key={r.friendshipId} avatarUrl={r.avatarUrl} name={r.username} userId={r.userId}>
                          <Button size="sm" variant="ghost" onClick={() => handleCancelRequest(r.userId)}>Отменить</Button>
                        </UserRow>
                      ))}
                    </div>
                    {outgoingTotalPages > 1 && (
                      <div className="flex items-center justify-center gap-3 mt-4">
                        <Button variant="ghost" size="sm" disabled={outgoingPage === 1} onClick={() => setOutgoingPage((p) => p - 1)}>← Назад</Button>
                        <span className="text-sm text-muted-foreground">{outgoingPage} / {outgoingTotalPages}</span>
                        <Button variant="ghost" size="sm" disabled={outgoingPage === outgoingTotalPages} onClick={() => setOutgoingPage((p) => p + 1)}>Вперёд →</Button>
                      </div>
                    )}
                  </>
                )
              }
            </div>
          </div>
        </TabsContent>

        <TabsContent value="search">
          <div className="flex gap-2 mb-4">
            <Input
              placeholder="Поиск по имени..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            />
            <Button onClick={handleSearch} disabled={searchLoading}>{searchLoading ? '...' : 'Найти'}</Button>
          </div>
          <div className="flex flex-col gap-2">
            {searchResults.map((u) => (
              <UserRow key={u.id} avatarUrl={u.avatarUrl} name={u.displayName} userId={u.id}>
                <Button size="sm" onClick={() => handleSendRequest(u.id)}>+ Добавить</Button>
              </UserRow>
            ))}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
