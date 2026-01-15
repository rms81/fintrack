import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { accountsApi } from '../lib/api';
import type { CreateAccountRequest, UpdateAccountRequest } from '../lib/types';

export const accountKeys = {
  all: ['accounts'] as const,
  lists: () => [...accountKeys.all, 'list'] as const,
  list: (profileId: string) => [...accountKeys.lists(), { profileId }] as const,
  details: () => [...accountKeys.all, 'detail'] as const,
  detail: (profileId: string, id: string) =>
    [...accountKeys.details(), { profileId, id }] as const,
};

export function useAccounts(profileId: string | undefined) {
  return useQuery({
    queryKey: accountKeys.list(profileId ?? ''),
    queryFn: () => accountsApi.getAll(profileId!),
    enabled: !!profileId,
  });
}

export function useAccount(profileId: string, id: string) {
  return useQuery({
    queryKey: accountKeys.detail(profileId, id),
    queryFn: () => accountsApi.getById(profileId, id),
    enabled: !!profileId && !!id,
  });
}

export function useCreateAccount() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      profileId,
      data,
    }: {
      profileId: string;
      data: CreateAccountRequest;
    }) => accountsApi.create(profileId, data),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: accountKeys.list(profileId) });
    },
  });
}

export function useUpdateAccount() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      profileId,
      id,
      data,
    }: {
      profileId: string;
      id: string;
      data: UpdateAccountRequest;
    }) => accountsApi.update(profileId, id, data),
    onSuccess: (_, { profileId, id }) => {
      queryClient.invalidateQueries({ queryKey: accountKeys.list(profileId) });
      queryClient.invalidateQueries({
        queryKey: accountKeys.detail(profileId, id),
      });
    },
  });
}

export function useDeleteAccount() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, id }: { profileId: string; id: string }) =>
      accountsApi.delete(profileId, id),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: accountKeys.list(profileId) });
    },
  });
}
