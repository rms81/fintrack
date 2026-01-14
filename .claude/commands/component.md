# Create React Component

Create a new React component for the FinTrack frontend application.

## Arguments
- `$ARGUMENTS` - The component specification (e.g., "TransactionList in features/transactions", "Button in components")

## Instructions

When the user runs `/component <ComponentName> in <path>`, create a complete React component including:

1. **Component file** with:
   - TypeScript with proper typing
   - Functional component using React 19 patterns
   - Tailwind CSS 4 classes for styling
   - Props interface

2. **Barrel export** (update or create index.ts)

3. **Test file** (if in features/ directory)

## Code Templates

### Feature Component (with data fetching)
```tsx
// src/FinTrack.Host/ClientApp/src/features/transactions/TransactionList.tsx
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { TransactionCard } from './TransactionCard';
import { Skeleton } from '@/components/ui/skeleton';

interface TransactionListProps {
  profileId: string;
  accountId?: string;
  className?: string;
}

export function TransactionList({ 
  profileId, 
  accountId,
  className 
}: TransactionListProps) {
  const { data, isLoading, error } = useQuery({
    queryKey: ['transactions', profileId, accountId],
    queryFn: () => api.transactions.list(profileId, { accountId }),
  });

  if (isLoading) {
    return (
      <div className={className}>
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-16 mb-2" />
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-red-500 p-4">
        Failed to load transactions
      </div>
    );
  }

  return (
    <div className={className}>
      {data?.items.map((transaction) => (
        <TransactionCard 
          key={transaction.id} 
          transaction={transaction} 
        />
      ))}
    </div>
  );
}
```

### UI Component (presentational)
```tsx
// src/FinTrack.Host/ClientApp/src/components/ui/CategoryBadge.tsx
import { cn } from '@/lib/utils';

interface CategoryBadgeProps {
  name: string;
  color?: string;
  className?: string;
}

export function CategoryBadge({ 
  name, 
  color = '#6b7280',
  className 
}: CategoryBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium',
        className
      )}
      style={{ 
        backgroundColor: `${color}20`,
        color: color 
      }}
    >
      {name}
    </span>
  );
}
```

### Form Component
```tsx
// src/FinTrack.Host/ClientApp/src/features/profiles/CreateProfileForm.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  type: z.enum(['Personal', 'Business']),
});

type FormData = z.infer<typeof schema>;

interface CreateProfileFormProps {
  onSuccess?: () => void;
}

export function CreateProfileForm({ onSuccess }: CreateProfileFormProps) {
  const queryClient = useQueryClient();
  
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { type: 'Personal' },
  });

  const mutation = useMutation({
    mutationFn: api.profiles.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      onSuccess?.();
    },
  });

  const onSubmit = (data: FormData) => {
    mutation.mutate(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <Input
          {...register('name')}
          placeholder="Profile name"
          aria-invalid={!!errors.name}
        />
        {errors.name && (
          <p className="text-sm text-red-500 mt-1">{errors.name.message}</p>
        )}
      </div>

      <div>
        <Select {...register('type')}>
          <option value="Personal">Personal</option>
          <option value="Business">Business</option>
        </Select>
      </div>

      <Button type="submit" disabled={mutation.isPending}>
        {mutation.isPending ? 'Creating...' : 'Create Profile'}
      </Button>
    </form>
  );
}
```

### Hook (for reusable logic)
```tsx
// src/FinTrack.Host/ClientApp/src/features/transactions/useTransactionFilters.ts
import { useState, useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router';

interface TransactionFilters {
  categoryId?: string;
  accountId?: string;
  fromDate?: string;
  toDate?: string;
  search?: string;
}

export function useTransactionFilters() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters = useMemo<TransactionFilters>(() => ({
    categoryId: searchParams.get('categoryId') ?? undefined,
    accountId: searchParams.get('accountId') ?? undefined,
    fromDate: searchParams.get('fromDate') ?? undefined,
    toDate: searchParams.get('toDate') ?? undefined,
    search: searchParams.get('search') ?? undefined,
  }), [searchParams]);

  const setFilter = useCallback((key: keyof TransactionFilters, value: string | undefined) => {
    setSearchParams(prev => {
      if (value) {
        prev.set(key, value);
      } else {
        prev.delete(key);
      }
      return prev;
    });
  }, [setSearchParams]);

  const clearFilters = useCallback(() => {
    setSearchParams({});
  }, [setSearchParams]);

  return { filters, setFilter, clearFilters };
}
```

Now create the component for: $ARGUMENTS
