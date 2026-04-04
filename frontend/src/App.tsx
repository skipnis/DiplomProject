import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ToastProvider } from './components/Toast';
import { Toaster } from '@/components/ui/sonner';
import Navbar from './components/Navbar';
import LoginPage from './pages/LoginPage';
import ProfilePage from './pages/ProfilePage';
import EditProfilePage from './pages/EditProfilePage';
import UserProfilePage from './pages/UserProfilePage';
import FriendsPage from './pages/FriendsPage';
import WishlistsPage from './pages/WishlistsPage';
import NewWishlistPage from './pages/NewWishlistPage';
import WishlistPage from './pages/WishlistPage';
import EditWishlistPage from './pages/EditWishlistPage';
import NewWishPage from './pages/NewWishPage';
import WishPage from './pages/WishPage';
import EditWishPage from './pages/EditWishPage';
import ReservationsPage from './pages/ReservationsPage';
import EventsPage from './pages/EventsPage';
import NewEventPage from './pages/NewEventPage';
import EventPage from './pages/EventPage';
import EditEventPage from './pages/EditEventPage';
import OnboardingPage from './pages/OnboardingPage';
import CatalogPage from './pages/CatalogPage';
import CollectionsPage from './pages/CollectionsPage';
import AdminLoginPage from './pages/AdminLoginPage';
import AdminPage from './pages/AdminPage';
import SharedWishPage from './pages/SharedWishPage';
import type { ReactNode } from 'react';

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID as string;

function AdminRoute({ children }: { children: ReactNode }) {
  const adminToken = localStorage.getItem('admin_token');
  if (!adminToken) return <Navigate to="/admin/login" replace />;
  return <>{children}</>;
}

function PrivateRoute({ children, skipOnboarding = false }: { children: ReactNode; skipOnboarding?: boolean }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="flex items-center justify-center min-h-screen text-muted-foreground">Загрузка...</div>;
  if (!user) return <Navigate to="/login" replace />;
  if (!skipOnboarding && !user.isOnboarded) return <Navigate to="/onboarding" replace />;
  return <>{children}</>;
}

function AppRoutes() {
  const { user, loading } = useAuth();

  if (loading) return <div className="flex items-center justify-center min-h-screen text-muted-foreground">Загрузка...</div>;

  return (
    <>
      <Navbar />
      <main className="max-w-[1100px] mx-auto px-6 py-8">
        <Routes>
          <Route path="/login" element={user ? <Navigate to="/wishlists" replace /> : <LoginPage />} />
          <Route path="/profile" element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
          <Route path="/profile/edit" element={<PrivateRoute><EditProfilePage /></PrivateRoute>} />
          <Route path="/users/:id" element={<UserProfilePage />} />
          <Route path="/friends" element={<PrivateRoute><FriendsPage /></PrivateRoute>} />
          <Route path="/wishlists" element={<PrivateRoute><WishlistsPage /></PrivateRoute>} />
          <Route path="/wishlists/new" element={<PrivateRoute><NewWishlistPage /></PrivateRoute>} />
          <Route path="/wishlists/:id" element={<WishlistPage />} />
          <Route path="/wishlists/:id/edit" element={<PrivateRoute><EditWishlistPage /></PrivateRoute>} />
          <Route path="/wishlists/:id/wishes/new" element={<PrivateRoute><NewWishPage /></PrivateRoute>} />
          <Route path="/wishlists/:id/wishes/:wishId" element={<WishPage />} />
          <Route path="/wishlists/:id/wishes/:wishId/edit" element={<PrivateRoute><EditWishPage /></PrivateRoute>} />
          <Route path="/reservations" element={<PrivateRoute><ReservationsPage /></PrivateRoute>} />
          <Route path="/events" element={<PrivateRoute><EventsPage /></PrivateRoute>} />
          <Route path="/events/new" element={<PrivateRoute><NewEventPage /></PrivateRoute>} />
          <Route path="/events/:id" element={<PrivateRoute><EventPage /></PrivateRoute>} />
          <Route path="/events/:id/edit" element={<PrivateRoute><EditEventPage /></PrivateRoute>} />
          <Route path="/onboarding" element={<PrivateRoute skipOnboarding><OnboardingPage /></PrivateRoute>} />
          <Route path="/catalog" element={<CatalogPage />} />
          <Route path="/catalog/collections" element={<CollectionsPage />} />
          <Route path="/admin/login" element={<AdminLoginPage />} />
          <Route path="/admin" element={<AdminRoute><AdminPage /></AdminRoute>} />
          <Route path="/share/:token" element={<SharedWishPage />} />
          <Route path="/" element={<Navigate to="/wishlists" replace />} />
        </Routes>
      </main>
    </>
  );
}

export default function App() {
  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <BrowserRouter>
        <AuthProvider>
          <ToastProvider>
            <AppRoutes />
            <Toaster richColors position="top-right" />
          </ToastProvider>
        </AuthProvider>
      </BrowserRouter>
    </GoogleOAuthProvider>
  );
}
