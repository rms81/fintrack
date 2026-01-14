import { useCallback, useSyncExternalStore } from 'react';

const STORAGE_KEY = 'fintrack-active-profile';

function getSnapshot(): string | null {
  return localStorage.getItem(STORAGE_KEY);
}

function getServerSnapshot(): string | null {
  return null;
}

function subscribe(callback: () => void): () => void {
  const handleStorage = (e: StorageEvent) => {
    if (e.key === STORAGE_KEY) {
      callback();
    }
  };

  window.addEventListener('storage', handleStorage);

  // Custom event for same-tab updates
  window.addEventListener('active-profile-change', callback);

  return () => {
    window.removeEventListener('storage', handleStorage);
    window.removeEventListener('active-profile-change', callback);
  };
}

export function useActiveProfile() {
  const activeProfileId = useSyncExternalStore(
    subscribe,
    getSnapshot,
    getServerSnapshot
  );

  const setActiveProfile = useCallback((profileId: string | null) => {
    if (profileId === null) {
      localStorage.removeItem(STORAGE_KEY);
    } else {
      localStorage.setItem(STORAGE_KEY, profileId);
    }
    // Dispatch custom event for same-tab updates
    window.dispatchEvent(new Event('active-profile-change'));
  }, []);

  return {
    activeProfileId,
    setActiveProfile,
  };
}
