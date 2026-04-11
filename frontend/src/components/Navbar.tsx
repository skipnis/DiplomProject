import { useState, useEffect, useCallback } from 'react';
import { Link, NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getImageUrl } from '../api/client';
import { useTheme } from '../hooks/useTheme';
import { useNotificationsHub } from '../hooks/useNotificationsHub';
import { getUnreadCount } from '../api/notifications';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import type { NotificationDto } from '../types';

export default function Navbar() {
  const { user, logout } = useAuth();
  const location = useLocation();
  const { theme, toggle } = useTheme();
  const [menuOpen, setMenuOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    if (!user) return;
    getUnreadCount()
      .then(setUnreadCount)
      .catch(() => {});
  }, [user]);

  const handleNotification = useCallback((n: NotificationDto) => {
    if (!n.isRead) setUnreadCount((c) => c + 1);
  }, []);

  useNotificationsHub(!!user, handleNotification);

  if (location.pathname === '/onboarding') return null;

  const close = () => setMenuOpen(false);

  const navCls = ({ isActive }: { isActive: boolean }) =>
    `text-sm font-medium px-3 py-1.5 rounded-md transition-colors ${
      isActive
        ? 'text-primary bg-primary/10'
        : 'text-muted-foreground hover:text-foreground hover:bg-muted'
    }`;

  return (
    <nav className="sticky top-0 z-50 bg-background/90 backdrop-blur-md border-b h-16">
      <div className="max-w-[1100px] mx-auto h-full flex items-center justify-between px-6 gap-4">
        <Link
          to={user ? '/wishlists' : '/'}
          className="text-lg font-extrabold text-primary tracking-tight"
        >
          🎁 Wishapp
        </Link>

        <button
          className="md:hidden text-xl"
          onClick={() => setMenuOpen((o) => !o)}
          aria-label="Меню"
        >
          {menuOpen ? '✕' : '☰'}
        </button>

        <div className={`${menuOpen ? 'flex' : 'hidden'} md:flex items-center gap-1 absolute md:static top-16 left-0 right-0 bg-background md:bg-transparent border-b md:border-0 p-4 md:p-0 flex-col md:flex-row z-50`}>
          {user ? (
            <>
              <NavLink to="/wishlists" className={navCls} onClick={close}>Вишлисты</NavLink>
              <NavLink to="/catalog" end className={navCls} onClick={close}>Каталог</NavLink>
              <NavLink to="/catalog/collections" className={navCls} onClick={close}>Подборки</NavLink>
              <NavLink to="/friends" className={navCls} onClick={close}>Друзья</NavLink>
              <NavLink to="/reservations" className={navCls} onClick={close}>Резервации</NavLink>
              <NavLink to="/events" className={navCls} onClick={close}>События</NavLink>
              <NavLink
                to="/notifications"
                onClick={() => { setUnreadCount(0); close(); }}
                className={({ isActive }) =>
                  `relative text-sm font-medium px-3 py-1.5 rounded-md transition-colors ${
                    isActive
                      ? 'text-primary bg-primary/10'
                      : 'text-muted-foreground hover:text-foreground hover:bg-muted'
                  }`
                }
              >
                🔔
                {unreadCount > 0 && (
                  <span className="absolute -top-1 -right-1 bg-destructive text-destructive-foreground text-[10px] font-bold rounded-full min-w-[16px] h-4 flex items-center justify-center px-0.5 leading-none">
                    {unreadCount > 99 ? '99+' : unreadCount}
                  </span>
                )}
              </NavLink>
              <Link to="/profile" onClick={close} className="ml-1">
                <Avatar className="h-8 w-8">
                  <AvatarImage src={getImageUrl(user.avatarUrl) ?? user.avatarUrl ?? undefined} alt={user.username ?? undefined} />
                  <AvatarFallback className="bg-primary text-primary-foreground text-sm font-bold">
                    {user.username?.[0]?.toUpperCase() ?? '?'}
                  </AvatarFallback>
                </Avatar>
              </Link>
              <Button variant="ghost" size="sm" onClick={() => { logout(); close(); }}>Выйти</Button>
              <Button variant="ghost" size="sm" onClick={toggle}>{theme === 'dark' ? '☀️' : '🌙'}</Button>
            </>
          ) : (
            <>
              <NavLink to="/catalog" end className={navCls} onClick={close}>Каталог</NavLink>
              <NavLink to="/catalog/collections" className={navCls} onClick={close}>Подборки</NavLink>
              <Button variant="ghost" size="sm" onClick={toggle}>{theme === 'dark' ? '☀️' : '🌙'}</Button>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}
