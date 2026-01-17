import { cn } from '../../lib/utils';

interface ProgressProps {
  value: number;
  max?: number;
  className?: string;
  showLabel?: boolean;
  size?: 'sm' | 'default' | 'lg';
}

const sizeClasses = {
  sm: 'h-1',
  default: 'h-2',
  lg: 'h-3',
};

/**
 * Progress bar component.
 */
export function Progress({
  value,
  max = 100,
  className,
  showLabel = false,
  size = 'default',
}: ProgressProps) {
  const safeMax = max > 0 ? max : 1;
  const clampedValue = Math.min(safeMax, Math.max(0, value));
  const percentage = Math.min(100, Math.max(0, (clampedValue / safeMax) * 100));

  return (
    <div className={cn('w-full', className)}>
      <div
        className={cn(
          'w-full overflow-hidden rounded-full bg-gray-200',
          sizeClasses[size]
        )}
        role="progressbar"
        aria-valuenow={clampedValue}
        aria-valuemin={0}
        aria-valuemax={safeMax}
      >
        <div
          className="h-full rounded-full bg-blue-600 transition-all duration-300 ease-out"
          style={{ width: `${percentage}%` }}
        />
      </div>
      {showLabel && (
        <p className="mt-1 text-right text-xs text-gray-500">
          {Math.round(percentage)}%
        </p>
      )}
    </div>
  );
}

interface StepsProgressProps {
  currentStep: number;
  totalSteps: number;
  labels?: string[];
  className?: string;
}

/**
 * Multi-step progress indicator.
 */
export function StepsProgress({
  currentStep,
  totalSteps,
  labels,
  className,
}: StepsProgressProps) {
  const clampedCurrentStep = Math.min(totalSteps, Math.max(1, currentStep));

  return (
    <div className={cn('w-full', className)}>
      <div
        className="flex items-center justify-between"
        role="progressbar"
        aria-label="Multi-step progress"
        aria-valuenow={clampedCurrentStep}
        aria-valuemin={1}
        aria-valuemax={totalSteps}
        aria-valuetext={`Step ${clampedCurrentStep} of ${totalSteps}`}
      >
        {Array.from({ length: totalSteps }).map((_, i) => {
          const stepNumber = i + 1;
          const isCompleted = stepNumber < clampedCurrentStep;
          const isCurrent = stepNumber === clampedCurrentStep;

          return (
            <div key={i} className="flex flex-1 items-center">
              {/* Step circle */}
              <div
                className={cn(
                  'flex h-8 w-8 items-center justify-center rounded-full text-sm font-medium',
                  isCompleted && 'bg-blue-600 text-white',
                  isCurrent && 'border-2 border-blue-600 bg-white text-blue-600',
                  !isCompleted && !isCurrent && 'border-2 border-gray-300 bg-white text-gray-400'
                )}
              >
                {isCompleted ? (
                  <svg
                    className="h-4 w-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M5 13l4 4L19 7"
                    />
                  </svg>
                ) : (
                  stepNumber
                )}
              </div>
              {/* Connector line */}
              {i < totalSteps - 1 && (
                <div
                  className={cn(
                    'mx-2 h-0.5 flex-1',
                    stepNumber < clampedCurrentStep ? 'bg-blue-600' : 'bg-gray-300'
                  )}
                />
              )}
            </div>
          );
        })}
      </div>
      {/* Labels */}
      {labels && labels.length > 0 && (
        <div className="mt-2 flex justify-between">
          {labels.map((label, i) => (
            <span
              key={i}
              className={cn(
                'text-xs',
                i + 1 <= clampedCurrentStep ? 'text-blue-600' : 'text-gray-400'
              )}
            >
              {label}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
