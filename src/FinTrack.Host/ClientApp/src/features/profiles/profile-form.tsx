import { useState } from 'react';
import { useNavigate } from 'react-router';
import type { Profile, CreateProfileRequest, UpdateProfileRequest } from '../../lib/types';
import { ProfileType } from '../../lib/types';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Select } from '../../components/ui/select';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { useCreateProfile, useUpdateProfile, useActiveProfile } from '../../hooks';

interface ProfileFormProps {
  profile?: Profile;
  redirectTo?: string;
}

export function ProfileForm({ profile, redirectTo = '/profiles' }: ProfileFormProps) {
  const navigate = useNavigate();
  const createProfile = useCreateProfile();
  const updateProfile = useUpdateProfile();
  const { setActiveProfile } = useActiveProfile();

  const [name, setName] = useState(profile?.name ?? '');
  const [type, setType] = useState<ProfileType>(profile?.type ?? ProfileType.Personal);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const isEditing = !!profile;
  const isPending = createProfile.isPending || updateProfile.isPending;

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!name.trim()) {
      newErrors.name = 'Name is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    try {
      if (isEditing) {
        const data: UpdateProfileRequest = {
          name: name.trim(),
          type,
        };
        await updateProfile.mutateAsync({
          id: profile.id,
          data,
        });
      } else {
        const data: CreateProfileRequest = {
          name: name.trim(),
          type,
        };
        const newProfile = await createProfile.mutateAsync(data);
        // Set as active profile if this is the first profile
        setActiveProfile(newProfile.id);
      }
      navigate(redirectTo);
    } catch {
      // Error is handled by TanStack Query
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Edit Profile' : 'Create Profile'}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Profile Name *</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g., Personal Finance, Business Expenses"
              error={!!errors.name}
            />
            {errors.name && (
              <p className="text-sm text-red-500">{errors.name}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="type">Profile Type</Label>
            <Select
              id="type"
              value={type.toString()}
              onChange={(e) => setType(parseInt(e.target.value) as ProfileType)}
            >
              <option value={ProfileType.Personal}>Personal</option>
              <option value={ProfileType.Business}>Business</option>
            </Select>
            <p className="text-xs text-gray-500">
              {type === ProfileType.Business
                ? 'For tracking business expenses and income'
                : 'For tracking personal finances'}
            </p>
          </div>
        </CardContent>
        <CardFooter className="justify-end gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(redirectTo)}
            disabled={isPending}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={isPending}>
            {isPending
              ? isEditing
                ? 'Saving...'
                : 'Creating...'
              : isEditing
                ? 'Save Changes'
                : 'Create Profile'}
          </Button>
        </CardFooter>
      </Card>
    </form>
  );
}
