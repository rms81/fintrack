import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/layout';
import { ProtectedRoute } from './components/protected-route';
import { ErrorBoundary } from './components/error-boundary';
import { ToastProvider } from './components/ui/toast';
import { PageLoading } from './components/page-loading';

// Lazy load all page components for code splitting
const LoginPage = lazy(() =>
  import('./features/auth/login-page').then((m) => ({ default: m.LoginPage }))
);
const RegisterPage = lazy(() =>
  import('./features/auth/register-page').then((m) => ({
    default: m.RegisterPage,
  }))
);
const NewProfilePage = lazy(() =>
  import('./features/profiles/new-profile-page').then((m) => ({
    default: m.NewProfilePage,
  }))
);
const DashboardPage = lazy(() =>
  import('./features/dashboard/dashboard-page').then((m) => ({
    default: m.DashboardPage,
  }))
);
const AccountsPage = lazy(() =>
  import('./features/accounts/accounts-page').then((m) => ({
    default: m.AccountsPage,
  }))
);
const NewAccountPage = lazy(() =>
  import('./features/accounts/new-account-page').then((m) => ({
    default: m.NewAccountPage,
  }))
);
const EditAccountPage = lazy(() =>
  import('./features/accounts/edit-account-page').then((m) => ({
    default: m.EditAccountPage,
  }))
);
const ProfilesPage = lazy(() =>
  import('./features/profiles/profiles-page').then((m) => ({
    default: m.ProfilesPage,
  }))
);
const EditProfilePage = lazy(() =>
  import('./features/profiles/edit-profile-page').then((m) => ({
    default: m.EditProfilePage,
  }))
);
const TransactionsPage = lazy(() =>
  import('./features/transactions/transactions-page').then((m) => ({
    default: m.TransactionsPage,
  }))
);
const ImportPage = lazy(() =>
  import('./features/import/import-page').then((m) => ({
    default: m.ImportPage,
  }))
);
const RulesPage = lazy(() =>
  import('./features/rules/rules-page').then((m) => ({ default: m.RulesPage }))
);
const AskPage = lazy(() =>
  import('./features/ask/ask-page').then((m) => ({ default: m.AskPage }))
);
const ReportsPage = lazy(() =>
  import('./features/reports/reports-page').then((m) => ({
    default: m.ReportsPage,
  }))
);
const SettingsPage = lazy(() =>
  import('./features/settings/settings-page').then((m) => ({
    default: m.SettingsPage,
  }))
);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60, // 1 minute
      retry: 1,
    },
  },
});

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <BrowserRouter>
            <Suspense fallback={<PageLoading />}>
              <Routes>
                {/* Auth routes */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />

                {/* Public routes */}
                <Route path="/profiles/new" element={<NewProfilePage />} />

                {/* Protected routes with layout */}
                <Route
                  element={
                    <ProtectedRoute>
                      <Layout />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<DashboardPage />} />
                  <Route path="accounts" element={<AccountsPage />} />
                  <Route path="accounts/new" element={<NewAccountPage />} />
                  <Route path="accounts/:id" element={<EditAccountPage />} />
                  <Route path="profiles" element={<ProfilesPage />} />
                  <Route path="profiles/:id/settings" element={<EditProfilePage />} />
                  <Route path="transactions" element={<TransactionsPage />} />
                  <Route path="import" element={<ImportPage />} />
                  <Route path="rules" element={<RulesPage />} />
                  <Route path="ask" element={<AskPage />} />
                  <Route path="reports" element={<ReportsPage />} />
                  <Route path="settings" element={<SettingsPage />} />
                </Route>
              </Routes>
            </Suspense>
          </BrowserRouter>
        </ToastProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;
