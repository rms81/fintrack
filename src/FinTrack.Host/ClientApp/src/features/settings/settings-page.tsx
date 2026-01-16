import { useState } from 'react';
import { Settings, Download, Upload } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { ExportPanel, ImportPanel } from '../export';

type SettingsTab = 'export' | 'import' | 'general';

export function SettingsPage() {
  const [activeTab, setActiveTab] = useState<SettingsTab>('export');

  const tabs: { id: SettingsTab; label: string; icon: React.ReactNode }[] = [
    { id: 'export', label: 'Export Data', icon: <Download className="h-4 w-4" /> },
    { id: 'import', label: 'Import Backup', icon: <Upload className="h-4 w-4" /> },
    { id: 'general', label: 'General', icon: <Settings className="h-4 w-4" /> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Settings</h1>
        <p className="text-gray-500">Configure your preferences and manage data</p>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex gap-4" aria-label="Tabs">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-2 py-3 px-1 border-b-2 text-sm font-medium transition-colors ${
                activeTab === tab.id
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'export' && <ExportPanel />}
      {activeTab === 'import' && <ImportPanel />}
      {activeTab === 'general' && (
        <Card>
          <CardHeader>
            <CardTitle>General Settings</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Settings className="h-12 w-12 text-gray-400" />
              <h3 className="mt-4 text-lg font-medium">Coming Soon</h3>
              <p className="mt-2 text-sm text-gray-500 max-w-md">
                Additional settings for currency display, date formats, and
                other preferences will be available in a future update.
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
