import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../lib/api';
import type { DashboardFilter } from '../lib/types';

export const dashboardKeys = {
  all: ['dashboard'] as const,
  summary: (profileId: string, filter: DashboardFilter) => [...dashboardKeys.all, 'summary', profileId, filter] as const,
  spendingByCategory: (profileId: string, filter: DashboardFilter) => [...dashboardKeys.all, 'spending-by-category', profileId, filter] as const,
  spendingOverTime: (profileId: string, filter: DashboardFilter, granularity: string) => [...dashboardKeys.all, 'spending-over-time', profileId, filter, granularity] as const,
  topMerchants: (profileId: string, filter: DashboardFilter, limit: number) => [...dashboardKeys.all, 'top-merchants', profileId, filter, limit] as const,
};

export function useDashboardSummary(profileId: string | undefined, filter: DashboardFilter = {}) {
  return useQuery({
    queryKey: dashboardKeys.summary(profileId ?? '', filter),
    queryFn: () => dashboardApi.getSummary(profileId!, filter),
    enabled: !!profileId,
  });
}

export function useSpendingByCategory(profileId: string | undefined, filter: DashboardFilter = {}) {
  return useQuery({
    queryKey: dashboardKeys.spendingByCategory(profileId ?? '', filter),
    queryFn: () => dashboardApi.getSpendingByCategory(profileId!, filter),
    enabled: !!profileId,
  });
}

export function useSpendingOverTime(profileId: string | undefined, filter: DashboardFilter = {}, granularity = 'month') {
  return useQuery({
    queryKey: dashboardKeys.spendingOverTime(profileId ?? '', filter, granularity),
    queryFn: () => dashboardApi.getSpendingOverTime(profileId!, filter, granularity),
    enabled: !!profileId,
  });
}

export function useTopMerchants(profileId: string | undefined, filter: DashboardFilter = {}, limit = 10) {
  return useQuery({
    queryKey: dashboardKeys.topMerchants(profileId ?? '', filter, limit),
    queryFn: () => dashboardApi.getTopMerchants(profileId!, filter, limit),
    enabled: !!profileId,
  });
}
