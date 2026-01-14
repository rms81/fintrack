import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { rulesApi } from '../lib/api';
import type { CreateRuleRequest, UpdateRuleRequest, TestRulesRequest } from '../lib/types';
import { transactionKeys } from './use-transactions';

export const ruleKeys = {
  all: ['rules'] as const,
  lists: () => [...ruleKeys.all, 'list'] as const,
  list: (profileId: string) => [...ruleKeys.lists(), profileId] as const,
  details: () => [...ruleKeys.all, 'detail'] as const,
  detail: (id: string) => [...ruleKeys.details(), id] as const,
};

export function useRules(profileId: string | undefined) {
  return useQuery({
    queryKey: ruleKeys.list(profileId ?? ''),
    queryFn: () => rulesApi.getAll(profileId!),
    enabled: !!profileId,
  });
}

export function useRule(id: string | undefined) {
  return useQuery({
    queryKey: ruleKeys.detail(id ?? ''),
    queryFn: () => rulesApi.getById(id!),
    enabled: !!id,
  });
}

export function useCreateRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, data }: { profileId: string; data: CreateRuleRequest }) =>
      rulesApi.create(profileId, data),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: ruleKeys.list(profileId) });
    },
  });
}

export function useUpdateRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRuleRequest }) =>
      rulesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ruleKeys.lists() });
    },
  });
}

export function useDeleteRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => rulesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ruleKeys.lists() });
    },
  });
}

export function useTestRules() {
  return useMutation({
    mutationFn: ({ profileId, data }: { profileId: string; data: TestRulesRequest }) =>
      rulesApi.test(profileId, data),
  });
}

export function useApplyRules() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, onlyUncategorized = true }: { profileId: string; onlyUncategorized?: boolean }) =>
      rulesApi.apply(profileId, onlyUncategorized),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() });
    },
  });
}
