import { Link } from 'react-router';
import { User, Briefcase, MoreVertical, Pencil, Trash2, Check } from 'lucide-react';
import { useState } from 'react';
import type { Profile } from '../../lib/types';
import { ProfileType } from '../../lib/types';
import { formatDate } from '../../lib/utils';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { useActiveProfile, useDeleteProfile } from '../../hooks';

interface ProfileCardProps {
  profile: Profile;
}

export function ProfileCard({ profile }: ProfileCardProps) {
  const [showMenu, setShowMenu] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const { activeProfileId, setActiveProfile } = useActiveProfile();
  const deleteProfile = useDeleteProfile();

  const isActive = profile.id === activeProfileId;

  const handleDelete = async () => {
    await deleteProfile.mutateAsync(profile.id);
    if (isActive) {
      setActiveProfile(null);
    }
    setShowConfirm(false);
  };

  const handleSetActive = () => {
    setActiveProfile(profile.id);
    setShowMenu(false);
  };

  return (
    <Card className={isActive ? 'ring-2 ring-blue-500' : ''}>
      <CardHeader className="flex flex-row items-start justify-between space-y-0">
        <div className="flex items-center gap-3">
          <div
            className={`rounded-full p-2 ${
              profile.type === ProfileType.Business
                ? 'bg-purple-100'
                : 'bg-blue-100'
            }`}
          >
            {profile.type === ProfileType.Business ? (
              <Briefcase
                className={`h-5 w-5 ${
                  profile.type === ProfileType.Business
                    ? 'text-purple-600'
                    : 'text-blue-600'
                }`}
              />
            ) : (
              <User className="h-5 w-5 text-blue-600" />
            )}
          </div>
          <div>
            <CardTitle className="text-base">{profile.name}</CardTitle>
            <Badge
              variant="secondary"
              className={
                profile.type === ProfileType.Business
                  ? 'bg-purple-100 text-purple-700'
                  : ''
              }
            >
              {profile.type === ProfileType.Business ? 'Business' : 'Personal'}
            </Badge>
          </div>
        </div>

        <div className="relative">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => setShowMenu(!showMenu)}
          >
            <MoreVertical className="h-4 w-4" />
          </Button>

          {showMenu && (
            <>
              <div
                className="fixed inset-0 z-40"
                onClick={() => setShowMenu(false)}
              />
              <div className="absolute right-0 top-full z-50 mt-1 w-44 rounded-md border border-gray-200 bg-white py-1 shadow-lg">
                {!isActive && (
                  <button
                    onClick={handleSetActive}
                    className="flex w-full items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    <Check className="h-4 w-4" />
                    Set as Active
                  </button>
                )}
                <Link
                  to={`/profiles/${profile.id}/settings`}
                  className="flex w-full items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  onClick={() => setShowMenu(false)}
                >
                  <Pencil className="h-4 w-4" />
                  Edit
                </Link>
                <button
                  onClick={() => {
                    setShowMenu(false);
                    setShowConfirm(true);
                  }}
                  className="flex w-full items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-gray-100"
                >
                  <Trash2 className="h-4 w-4" />
                  Delete
                </button>
              </div>
            </>
          )}
        </div>
      </CardHeader>

      <CardContent>
        <div className="flex items-center justify-between">
          {isActive && (
            <Badge variant="default" className="bg-green-600">
              Active
            </Badge>
          )}
          <span className="text-xs text-gray-500 ml-auto">
            Created {formatDate(profile.createdAt)}
          </span>
        </div>
      </CardContent>

      {/* Delete Confirmation Dialog */}
      {showConfirm && (
        <>
          <div className="fixed inset-0 z-50 bg-gray-900/50" />
          <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-lg bg-white p-6 shadow-xl">
            <h3 className="text-lg font-semibold">Delete Profile</h3>
            <p className="mt-2 text-sm text-gray-500">
              Are you sure you want to delete "{profile.name}"? This action
              cannot be undone and will delete all accounts and transactions
              associated with this profile.
            </p>
            <div className="mt-4 flex justify-end gap-3">
              <Button
                variant="outline"
                onClick={() => setShowConfirm(false)}
                disabled={deleteProfile.isPending}
              >
                Cancel
              </Button>
              <Button
                variant="destructive"
                onClick={handleDelete}
                disabled={deleteProfile.isPending}
              >
                {deleteProfile.isPending ? 'Deleting...' : 'Delete'}
              </Button>
            </div>
          </div>
        </>
      )}
    </Card>
  );
}
