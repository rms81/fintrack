import { Menu } from 'lucide-react';
import { Button } from '../ui/button';
import { ProfileSwitcher } from '../profile-switcher';

interface HeaderProps {
  onMenuClick?: () => void;
}

export function Header({ onMenuClick }: HeaderProps) {
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
      </div>
    </header>
  );
}
