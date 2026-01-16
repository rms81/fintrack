import { useState, useCallback } from 'react';
import { Upload, CheckCircle, AlertCircle, Loader2, FileJson } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { usePreviewJsonImport, useConfirmJsonImport } from '../../hooks/use-export';
import type { JsonImportPreviewResponse, JsonImportResult } from '../../lib/types';

type ImportStep = 'upload' | 'preview' | 'complete';

const ProfileTypeLabels: Record<number, string> = {
  0: 'Personal',
  1: 'Business',
};

export function ImportPanel() {
  const [step, setStep] = useState<ImportStep>('upload');
  const [previewData, setPreviewData] = useState<JsonImportPreviewResponse | null>(null);
  const [importResult, setImportResult] = useState<JsonImportResult | null>(null);
  const [profileName, setProfileName] = useState('');
  const [importRules, setImportRules] = useState(true);
  const [importFormats, setImportFormats] = useState(true);

  const previewMutation = usePreviewJsonImport();
  const confirmMutation = useConfirmJsonImport();

  const handleFileSelect = useCallback(async (file: File) => {
    try {
      const result = await previewMutation.mutateAsync(file);
      setPreviewData(result);
      setProfileName(result.profileName + ' (Imported)');
      setStep('preview');
    } catch (error) {
      console.error('Preview failed:', error);
    }
  }, [previewMutation]);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    const file = e.dataTransfer.files[0];
    if (file && file.name.endsWith('.json')) {
      handleFileSelect(file);
    }
  }, [handleFileSelect]);

  const handleConfirmImport = async () => {
    if (!previewData || !profileName.trim()) return;

    try {
      const result = await confirmMutation.mutateAsync({
        sessionId: previewData.sessionId,
        profileName: profileName.trim(),
        importRules,
        importFormats,
      });
      setImportResult(result);
      setStep('complete');
    } catch (error) {
      console.error('Import failed:', error);
    }
  };

  const resetImport = () => {
    setStep('upload');
    setPreviewData(null);
    setImportResult(null);
    setProfileName('');
    setImportRules(true);
    setImportFormats(true);
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileJson className="h-5 w-5" />
            Import from JSON Backup
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-gray-600 mb-4">
            Import a previously exported FinTrack JSON backup. This will create a new profile
            with all the imported data. Your existing profiles will not be affected.
          </p>

          {/* Step 1: Upload */}
          {step === 'upload' && (
            <div
              onDragOver={(e) => e.preventDefault()}
              onDrop={handleDrop}
              className="border-2 border-dashed border-gray-300 rounded-lg p-12 text-center hover:border-blue-500 transition-colors"
            >
              {previewMutation.isPending ? (
                <div className="flex flex-col items-center">
                  <Loader2 className="h-12 w-12 text-blue-500 animate-spin" />
                  <p className="mt-4 text-sm text-gray-600">Analyzing backup file...</p>
                </div>
              ) : (
                <>
                  <Upload className="h-12 w-12 text-gray-400 mx-auto" />
                  <p className="mt-4 text-sm text-gray-600">
                    Drag and drop your JSON backup file here, or
                  </p>
                  <label className="mt-2 inline-block">
                    <input
                      type="file"
                      accept=".json"
                      className="hidden"
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file) handleFileSelect(file);
                      }}
                    />
                    <span className="cursor-pointer text-blue-600 hover:underline">
                      browse to select
                    </span>
                  </label>
                </>
              )}
              {previewMutation.isError && (
                <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-600">
                    Failed to parse file. Make sure it's a valid FinTrack JSON export.
                  </p>
                </div>
              )}
            </div>
          )}

          {/* Step 2: Preview */}
          {step === 'preview' && previewData && (
            <div className="space-y-4">
              {/* Preview Summary */}
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="font-medium mb-3">Backup Contents</h3>
                <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 text-sm">
                  <div>
                    <span className="text-gray-500">Profile Type:</span>
                    <span className="ml-2 font-medium">
                      {ProfileTypeLabels[previewData.profileType] ?? 'Unknown'}
                    </span>
                  </div>
                  <div>
                    <span className="text-gray-500">Accounts:</span>
                    <span className="ml-2 font-medium">{previewData.accountCount}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Categories:</span>
                    <span className="ml-2 font-medium">{previewData.categoryCount}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Transactions:</span>
                    <span className="ml-2 font-medium">{previewData.transactionCount.toLocaleString()}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Rules:</span>
                    <span className="ml-2 font-medium">{previewData.ruleCount}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Import Formats:</span>
                    <span className="ml-2 font-medium">{previewData.importFormatCount}</span>
                  </div>
                </div>
              </div>

              {/* Warnings */}
              {previewData.warnings.length > 0 && (
                <div className="p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                  <div className="flex items-start gap-2">
                    <AlertCircle className="h-5 w-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                    <div>
                      <div className="font-medium text-yellow-800 text-sm">Warnings</div>
                      <ul className="mt-1 text-sm text-yellow-700 list-disc list-inside">
                        {previewData.warnings.map((warning, i) => (
                          <li key={i}>{warning}</li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </div>
              )}

              {/* Import Options */}
              <div className="space-y-4">
                <div>
                  <Label htmlFor="newProfileName">New Profile Name</Label>
                  <Input
                    id="newProfileName"
                    value={profileName}
                    onChange={(e) => setProfileName(e.target.value)}
                    placeholder="Enter a name for the imported profile"
                  />
                </div>

                <div className="flex flex-wrap gap-4">
                  {previewData.ruleCount > 0 && (
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={importRules}
                        onChange={(e) => setImportRules(e.target.checked)}
                        className="rounded border-gray-300"
                      />
                      <span className="text-sm">Import Rules ({previewData.ruleCount})</span>
                    </label>
                  )}
                  {previewData.importFormatCount > 0 && (
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={importFormats}
                        onChange={(e) => setImportFormats(e.target.checked)}
                        className="rounded border-gray-300"
                      />
                      <span className="text-sm">Import Formats ({previewData.importFormatCount})</span>
                    </label>
                  )}
                </div>
              </div>

              <div className="flex gap-2">
                <Button variant="outline" onClick={resetImport}>
                  Cancel
                </Button>
                <Button
                  onClick={handleConfirmImport}
                  disabled={!profileName.trim() || confirmMutation.isPending}
                >
                  {confirmMutation.isPending ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Importing...
                    </>
                  ) : (
                    'Import Profile'
                  )}
                </Button>
              </div>

              {confirmMutation.isError && (
                <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-600">
                    Import failed. Please try again.
                  </p>
                </div>
              )}
            </div>
          )}

          {/* Step 3: Complete */}
          {step === 'complete' && importResult && (
            <div className="flex flex-col items-center py-8">
              <CheckCircle className="h-16 w-16 text-green-500" />
              <h3 className="mt-4 text-xl font-medium">Import Complete!</h3>
              <p className="mt-2 text-gray-600 text-center">
                Successfully created new profile with:
              </p>
              <div className="mt-4 grid grid-cols-2 gap-x-8 gap-y-2 text-sm">
                <div className="text-gray-500 text-right">Accounts:</div>
                <div className="font-medium">{importResult.accountsCreated}</div>
                <div className="text-gray-500 text-right">Categories:</div>
                <div className="font-medium">{importResult.categoriesCreated}</div>
                <div className="text-gray-500 text-right">Transactions:</div>
                <div className="font-medium">{importResult.transactionsCreated.toLocaleString()}</div>
                {importResult.rulesCreated > 0 && (
                  <>
                    <div className="text-gray-500 text-right">Rules:</div>
                    <div className="font-medium">{importResult.rulesCreated}</div>
                  </>
                )}
                {importResult.formatsCreated > 0 && (
                  <>
                    <div className="text-gray-500 text-right">Import Formats:</div>
                    <div className="font-medium">{importResult.formatsCreated}</div>
                  </>
                )}
              </div>
              <div className="mt-6 flex gap-2">
                <Button variant="outline" onClick={resetImport}>
                  Import Another
                </Button>
                <a
                  href="/profiles"
                  className="inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium transition-colors bg-blue-600 text-white hover:bg-blue-700 shadow-sm h-10 px-4 py-2"
                >
                  View Profiles
                </a>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
