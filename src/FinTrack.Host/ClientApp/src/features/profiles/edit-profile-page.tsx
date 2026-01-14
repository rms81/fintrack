import { Link, useParams } from 'react-router';
import { ArrowLeft } from 'lucide-react';
import { useProfile } from '../../hooks';
import { Spinner } from '../../components/ui/spinner';
import { ProfileForm } from './profile-form';

export function EditProfilePage() {
  const { id } = useParams<{ id: string }>();
  const { data: profile, isLoading, error } = useProfile(id ?? '');

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error || !profile) {
    return (
      <div className="space-y-6">
        <Link
          to="/profiles"
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Profiles
        </Link>
        <div className="flex flex-col items-center justify-center py-12">
          <p className="text-red-500">Profile not found</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link
          to="/profiles"
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Profiles
        </Link>
      </div>

      <div>
        <h1 className="text-2xl font-bold">Profile Settings</h1>
        <p className="text-gray-500">Update your profile details</p>
      </div>

      <div className="max-w-lg">
        <ProfileForm profile={profile} />
      </div>
    </div>
  );
}
