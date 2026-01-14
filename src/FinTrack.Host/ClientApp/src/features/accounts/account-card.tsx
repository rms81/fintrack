import { Link } from 'react-router';
import { CreditCard, MoreVertical, Pencil, Trash2 } from 'lucide-react';
import { useState } from 'react';
import type { Account } from '../../lib/types';
import { formatDate } from '../../lib/utils';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { useActiveProfile, useDeleteAccount } from '../../hooks';

interface AccountCardProps {
  account: Account;
}

export function AccountCard({ account }: AccountCardProps) {
  const [showMenu, setShowMenu] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const { activeProfileId } = useActiveProfile();
  const deleteAccount = useDeleteAccount();

  const handleDelete = async () => {
    if (!activeProfileId) return;
    await deleteAccount.mutateAsync({
      profileId: activeProfileId,
      id: account.id,
    });
    setShowConfirm(false);
  };

  return (
    <Card className="relative">
      <CardHeader className="flex flex-row items-start justify-between space-y-0">
        <div className="flex items-center gap-3">
          <div className="rounded-full bg-blue-100 p-2">
            <CreditCard className="h-5 w-5 text-blue-600" />
          </div>
          <div>
            <CardTitle className="text-base">{account.name}</CardTitle>
            {account.bankName && (
              <p className="text-sm text-gray-500">{account.bankName}</p>
            )}
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
              <div className="absolute right-0 top-full z-50 mt-1 w-40 rounded-md border border-gray-200 bg-white py-1 shadow-lg">
                <Link
                  to={`/accounts/${account.id}`}
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
          <Badge variant="secondary">{account.currency}</Badge>
          <span className="text-xs text-gray-500">
            Created {formatDate(account.createdAt)}
          </span>
        </div>
      </CardContent>

      {/* Delete Confirmation Dialog */}
      {showConfirm && (
        <>
          <div className="fixed inset-0 z-50 bg-gray-900/50" />
          <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-lg bg-white p-6 shadow-xl">
            <h3 className="text-lg font-semibold">Delete Account</h3>
            <p className="mt-2 text-sm text-gray-500">
              Are you sure you want to delete "{account.name}"? This action
              cannot be undone and will delete all associated transactions.
            </p>
            <div className="mt-4 flex justify-end gap-3">
              <Button
                variant="outline"
                onClick={() => setShowConfirm(false)}
                disabled={deleteAccount.isPending}
              >
                Cancel
              </Button>
              <Button
                variant="destructive"
                onClick={handleDelete}
                disabled={deleteAccount.isPending}
              >
                {deleteAccount.isPending ? 'Deleting...' : 'Delete'}
              </Button>
            </div>
          </div>
        </>
      )}
    </Card>
  );
}
