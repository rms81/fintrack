import { Navigate, useLocation } from 'react-router';
import { useActiveProfile, useProfiles } from '../hooks';
import { Spinner } from './ui/spinner';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireProfile?: boolean;
}

export function ProtectedRoute({
  children,
  requireProfile = true,
}: ProtectedRouteProps) {
  const location = useLocation();
  const { data: profiles, isLoading } = useProfiles();
  const { activeProfileId } = useActiveProfile();

  // Show loading while fetching profiles
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <Spinner size="lg" />
          <p className="text-sm text-gray-500">Loading...</p>
        </div>
      </div>
    );
  }

  // If profile is required but none exist, redirect to create profile
  if (requireProfile && profiles && profiles.length === 0) {
    // Don't redirect if already on profile creation page
    if (location.pathname !== '/profiles/new') {
      return <Navigate to="/profiles/new" state={{ from: location }} replace />;
    }
  }

  // If profile is required but none is active, redirect to profiles
  if (
    requireProfile &&
    profiles &&
    profiles.length > 0 &&
    !activeProfileId
  ) {
    // Don't redirect if already on profiles page
    if (!location.pathname.startsWith('/profiles')) {
      return <Navigate to="/profiles" state={{ from: location }} replace />;
    }
  }

  return <>{children}</>;
}
