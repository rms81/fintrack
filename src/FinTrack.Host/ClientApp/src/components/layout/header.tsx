import { Menu, LogOut } from 'lucide-react';
import { useNavigate } from 'react-router';
import { Button } from '../ui/button';
import { ProfileSwitcher } from '../profile-switcher';
import { useCurrentUser, useLogout } from '../../hooks';

interface HeaderProps {
  onMenuClick?: () => void;
}

export function Header({ onMenuClick }: HeaderProps) {
  const navigate = useNavigate();
  const { data: user } = useCurrentUser();
  const logout = useLogout();

  const handleLogout = async () => {
    await logout.mutateAsync();
    navigate('/login', { replace: true });
  };

  return (
    <header className="sticky top-0 z-40 border-b border-gray-200 bg-white">
      <div className="flex h-16 items-center gap-4 px-4 sm:px-6">
        <Button
          variant="ghost"
          size="icon"
          className="lg:hidden"
          onClick={onMenuClick}
        >
          <Menu className="h-5 w-5" />
          <span className="sr-only">Toggle menu</span>
        </Button>

        <div className="flex items-center gap-2">
          <span className="text-xl font-bold text-blue-600">FinTrack</span>
        </div>

        <div className="flex-1" />

        <ProfileSwitcher />

        <div className="flex items-center gap-3 border-l border-gray-200 pl-3">
          {user && (
            <span className="text-sm text-gray-600 hidden sm:block">
              {user.displayName || user.email}
            </span>
          )}
          <Button
            variant="ghost"
            size="icon"
            onClick={handleLogout}
            disabled={logout.isPending}
            title="Sign out"
          >
            <LogOut className="h-5 w-5" />
            <span className="sr-only">Sign out</span>
          </Button>
        </div>
      </div>
    </header>
  );
}
