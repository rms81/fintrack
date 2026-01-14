import { Link } from 'react-router';
import { ArrowLeft } from 'lucide-react';
import { useProfiles } from '../../hooks';
import { ProfileForm } from './profile-form';

export function NewProfilePage() {
  const { data: profiles } = useProfiles();
  const isFirstProfile = !profiles || profiles.length === 0;

  return (
    <div className="space-y-6">
      {!isFirstProfile && (
        <div className="flex items-center gap-4">
          <Link
            to="/profiles"
            className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Profiles
          </Link>
        </div>
      )}

      <div>
        <h1 className="text-2xl font-bold">
          {isFirstProfile ? 'Welcome to FinTrack!' : 'New Profile'}
        </h1>
        <p className="text-gray-500">
          {isFirstProfile
            ? "Let's create your first profile to get started"
            : 'Create a new profile to track finances separately'}
        </p>
      </div>

      <div className="max-w-lg">
        <ProfileForm redirectTo={isFirstProfile ? '/' : '/profiles'} />
      </div>
    </div>
  );
}
