import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { categoriesApi } from '../lib/api';
import type { CreateCategoryRequest, UpdateCategoryRequest } from '../lib/types';

export const categoryKeys = {
  all: ['categories'] as const,
  lists: () => [...categoryKeys.all, 'list'] as const,
  list: (profileId: string) => [...categoryKeys.lists(), profileId] as const,
  details: () => [...categoryKeys.all, 'detail'] as const,
  detail: (profileId: string, id: string) => [...categoryKeys.details(), profileId, id] as const,
};

export function useCategories(profileId: string | undefined) {
  return useQuery({
    queryKey: categoryKeys.list(profileId ?? ''),
    queryFn: () => categoriesApi.getAll(profileId!),
    enabled: !!profileId,
  });
}

export function useCategory(profileId: string | undefined, id: string | undefined) {
  return useQuery({
    queryKey: categoryKeys.detail(profileId ?? '', id ?? ''),
    queryFn: () => categoriesApi.getById(profileId!, id!),
    enabled: !!profileId && !!id,
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, data }: { profileId: string; data: CreateCategoryRequest }) =>
      categoriesApi.create(profileId, data),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.list(profileId) });
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, id, data }: { profileId: string; id: string; data: UpdateCategoryRequest }) =>
      categoriesApi.update(profileId, id, data),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.list(profileId) });
    },
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, id }: { profileId: string; id: string }) =>
      categoriesApi.delete(profileId, id),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.list(profileId) });
    },
  });
}
