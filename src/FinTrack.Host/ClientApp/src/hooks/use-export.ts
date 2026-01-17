import { useMutation, useQueryClient } from '@tanstack/react-query';
import { exportApi } from '../lib/api';
import type { JsonExportOptions, CsvExportOptions, JsonImportConfirmRequest } from '../lib/types';
import { profileKeys } from './use-profiles';

export const exportKeys = {
  all: ['export'] as const,
};

// Export URL generators (no React Query needed - these are direct downloads)
export function useJsonExportUrl(profileId: string | undefined, options: JsonExportOptions = {}) {
  if (!profileId) return null;
  return exportApi.getJsonExportUrl(profileId, options);
}

export function useCsvExportUrl(profileId: string | undefined, options: CsvExportOptions = {}) {
  if (!profileId) return null;
  return exportApi.getCsvExportUrl(profileId, options);
}

// JSON Import hooks
export function usePreviewJsonImport() {
  return useMutation({
    mutationFn: (file: File) => exportApi.previewJsonImport(file),
  });
}

export function useConfirmJsonImport() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: JsonImportConfirmRequest) => exportApi.confirmJsonImport(data),
    onSuccess: () => {
      // Invalidate profiles since a new one was created
      queryClient.invalidateQueries({ queryKey: profileKeys.lists() });
    },
  });
}

// Utility hook for triggering file download
export function useDownload() {
  const triggerDownload = (url: string) => {
    // Create a temporary anchor element and click it to trigger download
    const link = document.createElement('a');
    link.href = url;
    link.download = ''; // Let the server set the filename via Content-Disposition
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return { triggerDownload };
}
