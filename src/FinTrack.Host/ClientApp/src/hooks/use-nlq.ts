import { useMutation, useQuery } from '@tanstack/react-query';
import { nlqApi } from '../lib/api';

export const nlqKeys = {
  all: ['nlq'] as const,
  suggestions: (profileId: string) => [...nlqKeys.all, 'suggestions', profileId] as const,
};

export function useNlqMutation(profileId: string | undefined) {
  return useMutation({
    mutationFn: (question: string) => {
      if (!profileId) throw new Error('Profile ID is required');
      return nlqApi.query(profileId, question);
    },
  });
}

export function useNlqSuggestions(profileId: string | undefined) {
  return useQuery({
    queryKey: nlqKeys.suggestions(profileId ?? ''),
    queryFn: () => nlqApi.getSuggestions(profileId!),
    enabled: !!profileId,
  });
}
