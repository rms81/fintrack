import { Link, useLocation } from 'react-router';
import {
  LayoutDashboard,
  CreditCard,
  Receipt,
  Upload,
  Filter,
  PieChart,
  Settings,
  X,
} from 'lucide-react';
import { cn } from '../../lib/utils';
import { Button } from '../ui/button';

interface SidebarProps {
  open?: boolean;
  onClose?: () => void;
}

const navigation = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Accounts', href: '/accounts', icon: CreditCard },
  { name: 'Transactions', href: '/transactions', icon: Receipt },
  { name: 'Import', href: '/import', icon: Upload },
  { name: 'Rules', href: '/rules', icon: Filter },
  { name: 'Reports', href: '/reports', icon: PieChart },
  { name: 'Settings', href: '/settings', icon: Settings },
];

export function Sidebar({ open, onClose }: SidebarProps) {
  const location = useLocation();

  return (
    <>
      {/* Mobile overlay */}
      {open && (
        <div
          className="fixed inset-0 z-40 bg-gray-900/50 lg:hidden"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 w-64 transform border-r border-gray-200 bg-white transition-transform lg:static lg:translate-x-0',
          open ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        <div className="flex h-16 items-center justify-between px-4 lg:hidden">
          <span className="text-xl font-bold text-blue-600">FinTrack</span>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        </div>

        <nav className="flex flex-col gap-1 p-4">
          {navigation.map((item) => {
            const isActive = location.pathname === item.href;
            return (
              <Link
                key={item.name}
                to={item.href}
                onClick={onClose}
                className={cn(
                  'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-blue-50 text-blue-700'
                    : 'text-gray-700 hover:bg-gray-100'
                )}
              >
                <item.icon className="h-5 w-5" />
                {item.name}
              </Link>
            );
          })}
        </nav>
      </aside>
    </>
  );
}
