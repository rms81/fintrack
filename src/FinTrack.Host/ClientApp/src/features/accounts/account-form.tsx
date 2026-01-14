import { useState } from 'react';
import { useNavigate } from 'react-router';
import type { Account, CreateAccountRequest, UpdateAccountRequest } from '../../lib/types';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Select } from '../../components/ui/select';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { useActiveProfile, useCreateAccount, useUpdateAccount } from '../../hooks';

interface AccountFormProps {
  account?: Account;
}

const currencies = ['EUR', 'USD', 'GBP', 'CHF', 'PLN', 'CZK'];

export function AccountForm({ account }: AccountFormProps) {
  const navigate = useNavigate();
  const { activeProfileId } = useActiveProfile();
  const createAccount = useCreateAccount();
  const updateAccount = useUpdateAccount();

  const [name, setName] = useState(account?.name ?? '');
  const [bankName, setBankName] = useState(account?.bankName ?? '');
  const [currency, setCurrency] = useState(account?.currency ?? 'EUR');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const isEditing = !!account;
  const isPending = createAccount.isPending || updateAccount.isPending;

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

    if (!validate() || !activeProfileId) return;

    try {
      if (isEditing) {
        const data: UpdateAccountRequest = {
          name: name.trim(),
          bankName: bankName.trim() || null,
          currency,
        };
        await updateAccount.mutateAsync({
          profileId: activeProfileId,
          id: account.id,
          data,
        });
      } else {
        const data: CreateAccountRequest = {
          name: name.trim(),
          bankName: bankName.trim() || undefined,
          currency,
        };
        await createAccount.mutateAsync({
          profileId: activeProfileId,
          data,
        });
      }
      navigate('/accounts');
    } catch (error) {
      // Error is handled by TanStack Query
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Edit Account' : 'New Account'}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Account Name *</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g., Main Checking"
              error={!!errors.name}
            />
            {errors.name && (
              <p className="text-sm text-red-500">{errors.name}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="bankName">Bank Name (optional)</Label>
            <Input
              id="bankName"
              value={bankName}
              onChange={(e) => setBankName(e.target.value)}
              placeholder="e.g., Chase, Bank of America"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="currency">Currency</Label>
            <Select
              id="currency"
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
            >
              {currencies.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </Select>
          </div>
        </CardContent>
        <CardFooter className="justify-end gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate('/accounts')}
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
                : 'Create Account'}
          </Button>
        </CardFooter>
      </Card>
    </form>
  );
}
