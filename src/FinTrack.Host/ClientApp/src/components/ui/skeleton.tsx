import { cn } from '../../lib/utils';

interface SkeletonProps {
  className?: string;
  style?: React.CSSProperties;
}

/**
 * Base skeleton component with pulse animation.
 */
export function Skeleton({ className, style }: SkeletonProps) {
  return (
    <div
      className={cn('animate-pulse rounded-md bg-gray-200', className)}
      style={style}
      aria-hidden="true"
    />
  );
}

/**
 * Skeleton for text lines.
 */
export function SkeletonText({ className, lines = 1 }: SkeletonProps & { lines?: number }) {
  return (
    <div className={cn('space-y-2', className)} aria-hidden="true">
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton
          key={i}
          className={cn('h-4', i === lines - 1 && lines > 1 ? 'w-3/4' : 'w-full')}
        />
      ))}
    </div>
  );
}

/**
 * Skeleton for a card layout.
 */
export function SkeletonCard({ className }: SkeletonProps) {
  return (
    <div
      className={cn(
        'rounded-lg border border-gray-200 bg-white p-4 shadow-sm',
        className
      )}
      aria-hidden="true"
    >
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div className="flex-1 space-y-2">
          <Skeleton className="h-4 w-1/2" />
          <Skeleton className="h-3 w-3/4" />
        </div>
      </div>
    </div>
  );
}

/**
 * Skeleton for a list of items.
 */
export function SkeletonList({
  className,
  items = 5,
}: SkeletonProps & { items?: number }) {
  return (
    <div className={cn('space-y-3', className)} aria-hidden="true">
      {Array.from({ length: items }).map((_, i) => (
        <div
          key={i}
          className="flex items-center gap-4 rounded-lg border border-gray-200 bg-white p-4"
        >
          <Skeleton className="h-10 w-10 rounded" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-1/3" />
            <Skeleton className="h-3 w-1/2" />
          </div>
          <Skeleton className="h-6 w-20" />
        </div>
      ))}
    </div>
  );
}

/**
 * Skeleton for a table.
 */
export function SkeletonTable({
  className,
  rows = 5,
  columns = 4,
}: SkeletonProps & { rows?: number; columns?: number }) {
  return (
    <div
      className={cn('overflow-hidden rounded-lg border border-gray-200', className)}
      aria-hidden="true"
    >
      {/* Header */}
      <div className="flex gap-4 border-b border-gray-200 bg-gray-50 p-4">
        {Array.from({ length: columns }).map((_, i) => (
          <Skeleton key={i} className="h-4 flex-1" />
        ))}
      </div>
      {/* Rows */}
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <div
          key={rowIndex}
          className="flex gap-4 border-b border-gray-100 p-4 last:border-0"
        >
          {Array.from({ length: columns }).map((_, colIndex) => (
            <Skeleton key={colIndex} className="h-4 flex-1" />
          ))}
        </div>
      ))}
    </div>
  );
}

/**
 * Skeleton for a chart/graph area.
 */
export function SkeletonChart({ className }: SkeletonProps) {
  return (
    <div
      className={cn(
        'flex h-64 items-end justify-between gap-2 rounded-lg border border-gray-200 bg-white p-4',
        className
      )}
      aria-hidden="true"
    >
      {/* Bar chart skeleton */}
      {[40, 65, 45, 80, 55, 70, 50, 85, 60, 75, 45, 90].map((height, i) => (
        <Skeleton
          key={i}
          className="flex-1 rounded-t"
          style={{ height: `${height}%` }}
        />
      ))}
    </div>
  );
}

/**
 * Skeleton for a stat/metric card.
 */
export function SkeletonStat({ className }: SkeletonProps) {
  return (
    <div
      className={cn(
        'rounded-lg border border-gray-200 bg-white p-6',
        className
      )}
      aria-hidden="true"
    >
      <Skeleton className="mb-2 h-4 w-24" />
      <Skeleton className="mb-1 h-8 w-32" />
      <Skeleton className="h-3 w-16" />
    </div>
  );
}

/**
 * Skeleton for the dashboard layout.
 */
export function SkeletonDashboard() {
  return (
    <div className="space-y-6" aria-hidden="true">
      {/* Stats row */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <SkeletonStat />
        <SkeletonStat />
        <SkeletonStat />
        <SkeletonStat />
      </div>
      {/* Chart */}
      <SkeletonChart />
      {/* Recent transactions */}
      <div>
        <Skeleton className="mb-4 h-6 w-48" />
        <SkeletonList items={5} />
      </div>
    </div>
  );
}
