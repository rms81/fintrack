import { Spinner } from './ui/spinner';

export function PageLoading() {
  return (
    <div
      className="flex h-full min-h-[400px] items-center justify-center"
      role="status"
      aria-live="polite"
      aria-busy="true"
    >
      <Spinner size="lg" />
    </div>
  );
}
