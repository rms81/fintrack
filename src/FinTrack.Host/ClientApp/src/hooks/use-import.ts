import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { importApi } from '../lib/api';
import type { CsvFormatConfig } from '../lib/types';
import { transactionKeys } from './use-transactions';

export const importKeys = {
  all: ['imports'] as const,
  sessions: () => [...importKeys.all, 'sessions'] as const,
  sessionList: (accountId: string) => [...importKeys.sessions(), accountId] as const,
};

export function useImportSessions(accountId: string | undefined) {
  return useQuery({
    queryKey: importKeys.sessionList(accountId ?? ''),
    queryFn: () => importApi.getSessions(accountId!),
    enabled: !!accountId,
  });
}

export function useUploadCsv() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ accountId, file }: { accountId: string; file: File }) =>
      importApi.upload(accountId, file),
    onSuccess: (_, { accountId }) => {
      queryClient.invalidateQueries({ queryKey: importKeys.sessionList(accountId) });
    },
  });
}

export function usePreviewImport() {
  return useMutation({
    mutationFn: ({ sessionId, formatOverride }: { sessionId: string; formatOverride?: CsvFormatConfig }) =>
      importApi.preview(sessionId, formatOverride),
  });
}

export function useConfirmImport() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      sessionId,
      formatOverride,
      skipDuplicates = true,
    }: {
      sessionId: string;
      formatOverride?: CsvFormatConfig;
      skipDuplicates?: boolean;
    }) => importApi.confirm(sessionId, formatOverride, skipDuplicates),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: importKeys.all });
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() });
    },
  });
}
