import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { importApi, importFormatsApi } from '../lib/api';
import type { CsvFormatConfig, CreateImportFormatRequest, UpdateImportFormatRequest } from '../lib/types';
import { transactionKeys } from './use-transactions';

export const importKeys = {
  all: ['imports'] as const,
  sessions: () => [...importKeys.all, 'sessions'] as const,
  sessionList: (accountId: string) => [...importKeys.sessions(), accountId] as const,
  formats: () => [...importKeys.all, 'formats'] as const,
  formatList: (profileId: string) => [...importKeys.formats(), profileId] as const,
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

// Import Format hooks
export function useImportFormats(profileId: string | undefined) {
  return useQuery({
    queryKey: importKeys.formatList(profileId ?? ''),
    queryFn: () => importFormatsApi.getAll(profileId!),
    enabled: !!profileId,
  });
}

export function useCreateImportFormat() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ profileId, data }: { profileId: string; data: CreateImportFormatRequest }) =>
      importFormatsApi.create(profileId, data),
    onSuccess: (_, { profileId }) => {
      queryClient.invalidateQueries({ queryKey: importKeys.formatList(profileId) });
    },
  });
}

export function useUpdateImportFormat() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateImportFormatRequest }) =>
      importFormatsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: importKeys.formats() });
    },
  });
}

export function useDeleteImportFormat() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => importFormatsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: importKeys.formats() });
    },
  });
}
