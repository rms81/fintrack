import { Link, useParams } from 'react-router';
import { ArrowLeft } from 'lucide-react';
import { useActiveProfile, useAccount } from '../../hooks';
import { Spinner } from '../../components/ui/spinner';
import { AccountForm } from './account-form';

export function EditAccountPage() {
  const { id } = useParams<{ id: string }>();
  const { activeProfileId } = useActiveProfile();
  const { data: account, isLoading, error } = useAccount(
    activeProfileId ?? '',
    id ?? ''
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error || !account) {
    return (
      <div className="space-y-6">
        <Link
          to="/accounts"
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Accounts
        </Link>
        <div className="flex flex-col items-center justify-center py-12">
          <p className="text-red-500">Account not found</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link
          to="/accounts"
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Accounts
        </Link>
      </div>

      <div>
        <h1 className="text-2xl font-bold">Edit Account</h1>
        <p className="text-gray-500">Update account details</p>
      </div>

      <div className="max-w-lg">
        <AccountForm account={account} />
      </div>
    </div>
  );
}
