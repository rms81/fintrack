import { type ReactNode } from 'react';
import { Link } from 'react-router';
import {
  FileText,
  CreditCard,
  Tag,
  Receipt,
  Users,
  FolderOpen,
  type LucideIcon,
} from 'lucide-react';
import { cn } from '../../lib/utils';
import { Button } from './button';

interface EmptyStateProps {
  icon?: LucideIcon;
  title: string;
  description?: string;
  action?: {
    label: string;
    href?: string;
    onClick?: () => void;
  };
  className?: string;
  children?: ReactNode;
}

/**
 * Generic empty state component for when there's no data to display.
 */
export function EmptyState({
  icon: Icon = FolderOpen,
  title,
  description,
  action,
  className,
  children,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center rounded-lg border-2 border-dashed border-gray-200 p-12 text-center',
        'dark:border-gray-700',
        className
      )}
    >
      <div className="flex h-14 w-14 items-center justify-center rounded-full bg-gray-100 dark:bg-gray-800">
        <Icon className="h-7 w-7 text-gray-400 dark:text-gray-500" />
      </div>
      <h3 className="mt-4 text-lg font-medium text-gray-900 dark:text-gray-100">
        {title}
      </h3>
      {description && (
        <p className="mt-2 max-w-sm text-sm text-gray-500 dark:text-gray-400">
          {description}
        </p>
      )}
      {action && (
        <div className="mt-6">
          {action.href ? (
            <Link to={action.href}>
              <Button>{action.label}</Button>
            </Link>
          ) : (
            <Button onClick={action.onClick}>{action.label}</Button>
          )}
        </div>
      )}
      {children}
    </div>
  );
}

/**
 * Empty state for no transactions.
 */
export function EmptyTransactions() {
  return (
    <EmptyState
      icon={Receipt}
      title="No transactions yet"
      description="Import your first bank statement to start tracking your expenses."
      action={{ label: 'Import Transactions', href: '/import' }}
    />
  );
}

/**
 * Empty state for no accounts.
 */
export function EmptyAccounts() {
  return (
    <EmptyState
      icon={CreditCard}
      title="No accounts yet"
      description="Add your first bank account to start organizing your finances."
      action={{ label: 'Add Account', href: '/accounts/new' }}
    />
  );
}

/**
 * Empty state for no profiles.
 */
export function EmptyProfiles() {
  return (
    <EmptyState
      icon={Users}
      title="No profiles yet"
      description="Create a profile to separate personal and business finances."
      action={{ label: 'Create Profile', href: '/profiles/new' }}
    />
  );
}

/**
 * Empty state for no rules.
 */
export function EmptyRules() {
  return (
    <EmptyState
      icon={Tag}
      title="No categorization rules"
      description="Create rules to automatically categorize your transactions."
      action={{ label: 'Create Rule', href: '/rules' }}
    />
  );
}

/**
 * Empty state for no search results.
 */
export function EmptySearchResults({ onClear }: { onClear?: () => void }) {
  return (
    <EmptyState
      icon={FileText}
      title="No results found"
      description="Try adjusting your search or filter to find what you're looking for."
      action={onClear ? { label: 'Clear Filters', onClick: onClear } : undefined}
    />
  );
}
