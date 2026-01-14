import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router';
import { ChevronDown, Plus, User, Briefcase } from 'lucide-react';
import { useProfiles, useActiveProfile } from '../hooks';
import { ProfileType } from '../lib/types';
import { cn } from '../lib/utils';
import { Button } from './ui/button';
import { Spinner } from './ui/spinner';

export function ProfileSwitcher() {
  const navigate = useNavigate();
  const { data: profiles, isLoading } = useProfiles();
  const { activeProfileId, setActiveProfile } = useActiveProfile();

  const activeProfile = profiles?.find((p) => p.id === activeProfileId);

  // Auto-select first profile if none selected
  useEffect(() => {
    if (!isLoading && profiles && profiles.length > 0 && !activeProfileId) {
      setActiveProfile(profiles[0].id);
    }
  }, [profiles, activeProfileId, isLoading, setActiveProfile]);

  // Redirect to profile creation if no profiles exist
  useEffect(() => {
    if (!isLoading && profiles && profiles.length === 0) {
      navigate('/profiles/new');
    }
  }, [profiles, isLoading, navigate]);

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 text-sm text-gray-500">
        <Spinner size="sm" />
        Loading...
      </div>
    );
  }

  if (!profiles || profiles.length === 0) {
    return (
      <Link to="/profiles/new">
        <Button variant="outline" size="sm">
          <Plus className="h-4 w-4" />
          Create Profile
        </Button>
      </Link>
    );
  }

  return (
    <div className="relative group">
      <Button variant="outline" className="gap-2">
        {activeProfile?.type === ProfileType.Business ? (
          <Briefcase className="h-4 w-4" />
        ) : (
          <User className="h-4 w-4" />
        )}
        <span className="max-w-[120px] truncate">
          {activeProfile?.name ?? 'Select Profile'}
        </span>
        <ChevronDown className="h-4 w-4" />
      </Button>

      <div className="absolute right-0 top-full z-50 mt-1 hidden w-56 rounded-md border border-gray-200 bg-white py-1 shadow-lg group-hover:block">
        {profiles.map((profile) => (
          <button
            key={profile.id}
            onClick={() => setActiveProfile(profile.id)}
            className={cn(
              'flex w-full items-center gap-3 px-4 py-2 text-left text-sm transition-colors',
              profile.id === activeProfileId
                ? 'bg-blue-50 text-blue-700'
                : 'text-gray-700 hover:bg-gray-100'
            )}
          >
            {profile.type === ProfileType.Business ? (
              <Briefcase className="h-4 w-4" />
            ) : (
              <User className="h-4 w-4" />
            )}
            <span className="flex-1 truncate">{profile.name}</span>
            {profile.id === activeProfileId && (
              <span className="text-xs text-blue-600">Active</span>
            )}
          </button>
        ))}

        <div className="my-1 border-t border-gray-200" />

        <Link
          to="/profiles"
          className="flex w-full items-center gap-3 px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100"
        >
          Manage Profiles
        </Link>

        <Link
          to="/profiles/new"
          className="flex w-full items-center gap-3 px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100"
        >
          <Plus className="h-4 w-4" />
          New Profile
        </Link>
      </div>
    </div>
  );
}
