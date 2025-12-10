import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './auth';
import { ProtectedRoute } from './components/auth';
import { Layout } from './components/layout/Layout';
import { ThemeProvider } from './contexts';
import { AdminLayout } from './layouts';
import { HomePage } from './pages/HomePage';
import { NotFoundPage } from './pages/NotFoundPage';
import { PrizesPage } from './pages/PrizesPage';
import {
  Awards,
  CompetitionDetail,
  Competitions,
  Dashboard,
  PrizeCreate,
  PrizeDetail,
  PrizePoolCreate,
  PrizePoolDetail,
  PrizePools,
  Redemptions,
  Registrations,
} from './pages/admin';
import { AuthCallback, SilentCallback } from './pages/auth';
import { Redeem, Register, Status } from './pages/public';

export const App: React.FC = () => {
  // Get the base path from Vite config (set during build)
  const basename = import.meta.env.BASE_URL.replace(/\/$/, '') || '/';

  return (
    <ThemeProvider>
      <AuthProvider>
        <BrowserRouter basename={basename}>
          <Routes>
            {/* Auth Callback Routes */}
            <Route path="/auth/callback" element={<AuthCallback />} />
            <Route path="/auth/silent-callback" element={<SilentCallback />} />

            {/* Public Routes */}
            <Route path="/" element={<Layout />}>
              <Route index element={<HomePage />} />
              <Route path="prizes" element={<PrizesPage />} />
            </Route>

            {/* Public Registration (standalone page) */}
            <Route path="/register/:competitionId" element={<Register />} />

            {/* Public Redemption (standalone page) */}
            <Route path="/redeem" element={<Redeem />} />

            {/* Public Status Check (standalone page) */}
            <Route path="/status" element={<Status />} />

            {/* Admin Routes - Protected */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute>
                  <AdminLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="dashboard" replace />} />
              <Route path="dashboard" element={<Dashboard />} />
              <Route path="competitions" element={<Competitions />} />
              <Route path="competitions/:id" element={<CompetitionDetail />} />
              <Route path="competitions/:id/edit" element={<CompetitionDetail />} />
              <Route path="competitions/new" element={<div>New Competition</div>} />
              <Route path="prize-pools" element={<PrizePools />} />
              <Route path="prize-pools/:id" element={<PrizePoolDetail />} />
              <Route path="prize-pools/new" element={<PrizePoolCreate />} />
              <Route path="prizes" element={<div>Prizes List</div>} />
              <Route path="prizes/new" element={<PrizeCreate />} />
              <Route path="prizes/:id" element={<PrizeDetail />} />
              <Route path="awards" element={<Awards />} />
              <Route path="awards/:id" element={<div>Award Detail</div>} />
              <Route path="awards/new" element={<div>New Award</div>} />
              <Route path="registrations" element={<Registrations />} />
              <Route path="redemptions" element={<Redemptions />} />
              <Route path="reports" element={<div>Reports</div>} />
            </Route>

            {/* 404 */}
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
};
