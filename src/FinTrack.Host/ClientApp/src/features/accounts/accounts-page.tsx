import { Link } from 'react-router';
import { Plus, CreditCard } from 'lucide-react';
import { useActiveProfile, useAccounts } from '../../hooks';
import { Button } from '../../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Spinner } from '../../components/ui/spinner';
import { AccountCard } from './account-card';

export function AccountsPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts, isLoading, error } = useAccounts(activeProfileId ?? '');

  if (!activeProfileId) {
    return (
      <div className="flex flex-col items-center justify-center py-12">
        <p className="text-gray-500">Please select a profile first</p>
        <Link to="/profiles" className="mt-4">
          <Button>Select Profile</Button>
        </Link>
      </div>
    );
  }

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
        <p className="text-red-500">Failed to load accounts</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Accounts</h1>
          <p className="text-gray-500">Manage your bank accounts</p>
        </div>
        <Link to="/accounts/new">
          <Button>
            <Plus className="h-4 w-4" />
            Add Account
          </Button>
        </Link>
      </div>

      {accounts && accounts.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {accounts.map((account) => (
            <AccountCard key={account.id} account={account} />
          ))}
        </div>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>No Accounts</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <CreditCard className="h-12 w-12 text-gray-400" />
              <p className="mt-4 text-sm text-gray-500">
                You haven't added any accounts yet. Add your first account to
                start tracking transactions.
              </p>
              <Link to="/accounts/new" className="mt-4">
                <Button>
                  <Plus className="h-4 w-4" />
                  Add Account
                </Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
