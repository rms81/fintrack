import { useState } from 'react';
import { Download, FileJson, FileSpreadsheet } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Label } from '../../components/ui/label';
import { Input } from '../../components/ui/input';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useCategories } from '../../hooks/use-categories';
import { useJsonExportUrl, useCsvExportUrl, useDownload } from '../../hooks/use-export';
import type { JsonExportOptions, CsvExportOptions } from '../../lib/types';

export function ExportPanel() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? undefined);
  const { data: categories } = useCategories(activeProfileId ?? undefined);
  const { triggerDownload } = useDownload();

  // JSON export options
  const [jsonOptions, setJsonOptions] = useState<JsonExportOptions>({
    includeRules: true,
    includeFormats: true,
  });

  // CSV export options
  const [csvOptions, setCsvOptions] = useState<CsvExportOptions>({});

  const jsonUrl = useJsonExportUrl(activeProfileId ?? undefined, jsonOptions);
  const csvUrl = useCsvExportUrl(activeProfileId ?? undefined, csvOptions);

  const handleJsonExport = () => {
    if (jsonUrl) triggerDownload(jsonUrl);
  };

  const handleCsvExport = () => {
    if (csvUrl) triggerDownload(csvUrl);
  };

  if (!activeProfileId) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Please select a profile first</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* JSON Export */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileJson className="h-5 w-5" />
            Export as JSON
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-gray-600">
            Export your complete profile including accounts, categories, rules, import formats,
            and transactions. Use this for backups or migrating to another device.
          </p>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <Label htmlFor="jsonFromDate">From Date (Optional)</Label>
              <Input
                id="jsonFromDate"
                type="date"
                value={jsonOptions.fromDate ?? ''}
                onChange={(e) => setJsonOptions(prev => ({
                  ...prev,
                  fromDate: e.target.value || undefined
                }))}
              />
            </div>
            <div>
              <Label htmlFor="jsonToDate">To Date (Optional)</Label>
              <Input
                id="jsonToDate"
                type="date"
                value={jsonOptions.toDate ?? ''}
                onChange={(e) => setJsonOptions(prev => ({
                  ...prev,
                  toDate: e.target.value || undefined
                }))}
              />
            </div>
          </div>

          <div className="flex flex-wrap gap-4">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={jsonOptions.includeRules ?? true}
                onChange={(e) => setJsonOptions(prev => ({
                  ...prev,
                  includeRules: e.target.checked
                }))}
                className="rounded border-gray-300"
              />
              <span className="text-sm">Include Rules</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={jsonOptions.includeFormats ?? true}
                onChange={(e) => setJsonOptions(prev => ({
                  ...prev,
                  includeFormats: e.target.checked
                }))}
                className="rounded border-gray-300"
              />
              <span className="text-sm">Include Import Formats</span>
            </label>
          </div>

          <Button onClick={handleJsonExport} className="w-full sm:w-auto">
            <Download className="h-4 w-4 mr-2" />
            Download JSON Backup
          </Button>
        </CardContent>
      </Card>

      {/* CSV Export */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileSpreadsheet className="h-5 w-5" />
            Export as CSV
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-gray-600">
            Export transactions as a CSV file compatible with Excel, Google Sheets, or other
            spreadsheet applications. Great for analysis and reporting.
          </p>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <Label htmlFor="csvFromDate">From Date (Optional)</Label>
              <Input
                id="csvFromDate"
                type="date"
                value={csvOptions.fromDate ?? ''}
                onChange={(e) => setCsvOptions(prev => ({
                  ...prev,
                  fromDate: e.target.value || undefined
                }))}
              />
            </div>
            <div>
              <Label htmlFor="csvToDate">To Date (Optional)</Label>
              <Input
                id="csvToDate"
                type="date"
                value={csvOptions.toDate ?? ''}
                onChange={(e) => setCsvOptions(prev => ({
                  ...prev,
                  toDate: e.target.value || undefined
                }))}
              />
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <Label htmlFor="csvAccount">Account (Optional)</Label>
              <select
                id="csvAccount"
                value={csvOptions.accountId ?? ''}
                onChange={(e) => setCsvOptions(prev => ({
                  ...prev,
                  accountId: e.target.value || undefined
                }))}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              >
                <option value="">All Accounts</option>
                {accounts?.map(account => (
                  <option key={account.id} value={account.id}>
                    {account.name}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <Label htmlFor="csvCategory">Category (Optional)</Label>
              <select
                id="csvCategory"
                value={csvOptions.categoryId ?? ''}
                onChange={(e) => setCsvOptions(prev => ({
                  ...prev,
                  categoryId: e.target.value || undefined
                }))}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              >
                <option value="">All Categories</option>
                {categories?.filter(c => !c.parentId).map(category => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <Button onClick={handleCsvExport} variant="outline" className="w-full sm:w-auto">
            <Download className="h-4 w-4 mr-2" />
            Download CSV
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
