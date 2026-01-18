import { Spinner } from './ui/spinner';

export function PageLoading() {
  return (
    <div className="flex h-full min-h-[400px] items-center justify-center">
      <Spinner size="lg" />
    </div>
  );
}
