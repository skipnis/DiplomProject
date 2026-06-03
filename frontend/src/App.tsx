import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ToastProvider } from './components/Toast';
import { Toaster } from '@/components/ui/sonner';
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import PageLoader from './components/PageLoader';
import type { ReactNode } from 'react';

const LoginPage = lazy(() => import('./pages/LoginPage'));
const ProfilePage = lazy(() => import('./pages/ProfilePage'));
const UserProfilePage = lazy(() => import('./pages/UserProfilePage'));
const FriendsPage = lazy(() => import('./pages/FriendsPage'));
const WishlistsPage = lazy(() => import('./pages/WishlistsPage'));
const NewWishlistPage = lazy(() => import('./pages/NewWishlistPage'));
const WishlistPage = lazy(() => import('./pages/WishlistPage'));
const EditWishlistPage = lazy(() => import('./pages/EditWishlistPage'));
const WishlistMembersPage = lazy(() => import('./pages/WishlistMembersPage'));
const NewWishPage = lazy(() => import('./pages/NewWishPage'));
const WishPage = lazy(() => import('./pages/WishPage'));
const EditWishPage = lazy(() => import('./pages/EditWishPage'));
const ReservationsPage = lazy(() => import('./pages/ReservationsPage'));
const ProposalsPage = lazy(() => import('./pages/ProposalsPage'));
const ProposalDetailPage = lazy(() => import('./pages/ProposalDetailPage'));
const CreateProposalPage = lazy(() => import('./pages/CreateProposalPage'));
const EventsPage = lazy(() => import('./pages/EventsPage'));
const NewEventPage = lazy(() => import('./pages/NewEventPage'));
const EventPage = lazy(() => import('./pages/EventPage'));
const EditEventPage = lazy(() => import('./pages/EditEventPage'));
const OnboardingPage = lazy(() => import('./pages/OnboardingPage'));
const CatalogPage = lazy(() => import('./pages/CatalogPage'));
const CatalogItemPage = lazy(() => import('./pages/CatalogItemPage'));
const CollectionsPage = lazy(() => import('./pages/CollectionsPage'));
const AdminLoginPage = lazy(() => import('./pages/AdminLoginPage'));
const AdminPage = lazy(() => import('./pages/AdminPage'));
const NotificationsPage = lazy(() => import('./pages/NotificationsPage'));
const SharedWishPage = lazy(() => import('./pages/SharedWishPage'));
const LandingPage = lazy(() => import('./pages/LandingPage'));

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID as string;

function AdminRoute({ children }: { children: ReactNode }) {
  const adminToken = localStorage.getItem('admin_token');
  if (!adminToken) return <Navigate to="/admin/login" replace />;
  return <>{children}</>;
}

function PrivateRoute({ children, skipOnboarding = false }: { children: ReactNode; skipOnboarding?: boolean }) {
  const { user, loading } = useAuth();
  if (loading) return <PageLoader />;
  if (!user) return <Navigate to="/" replace />;
  if (!skipOnboarding && !user.isOnboarded) return <Navigate to="/onboarding" replace />;
  return <>{children}</>;
}

function AppRoutes() {
  const { user, loading } = useAuth();

  if (loading) return <PageLoader />;

  return (
    <>
      <Navbar />
      <main className="max-w-[1100px] mx-auto px-6 py-8">
        <Suspense fallback={<PageLoader />}>
          <Routes>
            <Route path="/" element={user ? <Navigate to="/wishlists" replace /> : <LandingPage />} />
            <Route path="/about" element={<LandingPage />} />
            <Route path="/login" element={user ? <Navigate to="/wishlists" replace /> : <LoginPage />} />
            <Route path="/profile" element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
            <Route path="/users/:id" element={<UserProfilePage />} />
            <Route path="/friends" element={<PrivateRoute><FriendsPage /></PrivateRoute>} />
            <Route path="/wishlists" element={<PrivateRoute><WishlistsPage /></PrivateRoute>} />
            <Route path="/wishlists/new" element={<PrivateRoute><NewWishlistPage /></PrivateRoute>} />
            <Route path="/wishlists/:id" element={<WishlistPage />} />
            <Route path="/wishlists/:id/edit" element={<PrivateRoute><EditWishlistPage /></PrivateRoute>} />
            <Route path="/wishlists/:id/members" element={<PrivateRoute><WishlistMembersPage /></PrivateRoute>} />
            <Route path="/wishlists/:id/wishes/new" element={<PrivateRoute><NewWishPage /></PrivateRoute>} />
            <Route path="/wishlists/:id/wishes/:wishId" element={<WishPage />} />
            <Route path="/wishlists/:id/wishes/:wishId/edit" element={<PrivateRoute><EditWishPage /></PrivateRoute>} />
            <Route path="/reservations" element={<PrivateRoute><ReservationsPage /></PrivateRoute>} />
            <Route path="/proposals" element={<PrivateRoute><ProposalsPage /></PrivateRoute>} />
            <Route path="/proposals/new" element={<PrivateRoute><CreateProposalPage /></PrivateRoute>} />
            <Route path="/proposals/:id" element={<PrivateRoute><ProposalDetailPage /></PrivateRoute>} />
            <Route path="/events" element={<PrivateRoute><EventsPage /></PrivateRoute>} />
            <Route path="/events/new" element={<PrivateRoute><NewEventPage /></PrivateRoute>} />
            <Route path="/events/:id" element={<PrivateRoute><EventPage /></PrivateRoute>} />
            <Route path="/events/:id/edit" element={<PrivateRoute><EditEventPage /></PrivateRoute>} />
            <Route path="/onboarding" element={<PrivateRoute skipOnboarding><OnboardingPage /></PrivateRoute>} />
            <Route path="/catalog" element={<CatalogPage />} />
            <Route path="/catalog/items/:id" element={<CatalogItemPage />} />
            <Route path="/catalog/collections" element={<CollectionsPage />} />
            <Route path="/admin/login" element={<AdminLoginPage />} />
            <Route path="/admin" element={<AdminRoute><AdminPage /></AdminRoute>} />
            <Route path="/notifications" element={<PrivateRoute><NotificationsPage /></PrivateRoute>} />
            <Route path="/share/:token" element={<SharedWishPage />} />
          </Routes>
        </Suspense>
      </main>
      <Footer />
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
