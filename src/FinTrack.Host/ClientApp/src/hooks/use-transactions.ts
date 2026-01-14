import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { transactionsApi } from '../lib/api';
import type { TransactionFilter, UpdateTransactionRequest } from '../lib/types';

export const transactionKeys = {
  all: ['transactions'] as const,
  lists: () => [...transactionKeys.all, 'list'] as const,
  list: (profileId: string, filter: TransactionFilter) => [...transactionKeys.lists(), profileId, filter] as const,
  details: () => [...transactionKeys.all, 'detail'] as const,
  detail: (id: string) => [...transactionKeys.details(), id] as const,
};

export function useTransactions(profileId: string | undefined, filter: TransactionFilter = {}) {
  return useQuery({
    queryKey: transactionKeys.list(profileId ?? '', filter),
    queryFn: () => transactionsApi.getAll(profileId!, filter),
    enabled: !!profileId,
  });
}

export function useTransaction(id: string | undefined) {
  return useQuery({
    queryKey: transactionKeys.detail(id ?? ''),
    queryFn: () => transactionsApi.getById(id!),
    enabled: !!id,
  });
}

export function useUpdateTransaction() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTransactionRequest }) =>
      transactionsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() });
    },
  });
}

export function useDeleteTransaction() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => transactionsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() });
    },
  });
}
