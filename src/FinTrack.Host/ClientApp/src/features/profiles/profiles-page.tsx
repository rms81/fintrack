import { Link } from 'react-router';
import { Plus, User } from 'lucide-react';
import { useProfiles } from '../../hooks';
import { Button } from '../../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Spinner } from '../../components/ui/spinner';
import { ProfileCard } from './profile-card';

export function ProfilesPage() {
  const { data: profiles, isLoading, error } = useProfiles();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center py-12">
        <p className="text-red-500">Failed to load profiles</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Profiles</h1>
          <p className="text-gray-500">
            Manage your personal and business profiles
          </p>
        </div>
        <Link to="/profiles/new">
          <Button>
            <Plus className="h-4 w-4" />
            New Profile
          </Button>
        </Link>
      </div>

      {profiles && profiles.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {profiles.map((profile) => (
            <ProfileCard key={profile.id} profile={profile} />
          ))}
        </div>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>No Profiles</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <User className="h-12 w-12 text-gray-400" />
              <p className="mt-4 text-sm text-gray-500">
                You haven't created any profiles yet. Create your first profile
                to start tracking your finances.
              </p>
              <Link to="/profiles/new" className="mt-4">
                <Button>
                  <Plus className="h-4 w-4" />
                  Create Profile
                </Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
