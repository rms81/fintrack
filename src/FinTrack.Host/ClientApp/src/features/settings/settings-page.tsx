import { Settings } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';

export function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Settings</h1>
        <p className="text-gray-500">Configure your preferences</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Coming in Phase 5</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <Settings className="h-12 w-12 text-gray-400" />
            <h3 className="mt-4 text-lg font-medium">Application Settings</h3>
            <p className="mt-2 text-sm text-gray-500 max-w-md">
              Configure categorization rules, export your data, set up
              preferences for currency display, and manage your account
              settings.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
