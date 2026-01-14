import { BrowserRouter, Routes, Route } from 'react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/layout';
import { ProtectedRoute } from './components/protected-route';
import { DashboardPage } from './features/dashboard';
import { AccountsPage, NewAccountPage, EditAccountPage } from './features/accounts';
import { ProfilesPage, NewProfilePage, EditProfilePage } from './features/profiles';
import { ImportPage } from './features/import';
import { RulesPage } from './features/rules';
import { ReportsPage } from './features/reports';
import { SettingsPage } from './features/settings';

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
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
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
            <Route path="import" element={<ImportPage />} />
            <Route path="rules" element={<RulesPage />} />
            <Route path="reports" element={<ReportsPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
