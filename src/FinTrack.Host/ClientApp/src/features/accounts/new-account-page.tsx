import { Link } from 'react-router';
import { ArrowLeft } from 'lucide-react';
import { AccountForm } from './account-form';

export function NewAccountPage() {
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
        <h1 className="text-2xl font-bold">Add Account</h1>
        <p className="text-gray-500">Create a new bank account to track</p>
      </div>

      <div className="max-w-lg">
        <AccountForm />
      </div>
    </div>
  );
}
