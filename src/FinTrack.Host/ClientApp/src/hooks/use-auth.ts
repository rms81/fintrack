import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { authApi } from '../lib/api';
import type { RegisterRequest, LoginRequest } from '../lib/types';

export function useCurrentUser() {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: authApi.getCurrentUser,
    retry: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

export function useRegister() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    onSuccess: (user) => {
      queryClient.setQueryData(['currentUser'], user);
    },
  });
}

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (user) => {
      queryClient.setQueryData(['currentUser'], user);
      // Invalidate all queries to refresh data with new auth
      queryClient.invalidateQueries();
    },
  });
}

export function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => authApi.logout(),
    onSuccess: () => {
      queryClient.setQueryData(['currentUser'], null);
      // Clear all cached data
      queryClient.clear();
    },
  });
}
