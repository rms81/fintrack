import { useState, useCallback } from 'react';
import { Upload, CheckCircle, AlertCircle, Loader2, Save, FileText } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useUploadCsv, usePreviewImport, useConfirmImport, useImportFormats, useCreateImportFormat } from '../../hooks/use-import';
import type { UploadResponse, PreviewResponse, ImportFormat } from '../../lib/types';

type ImportStep = 'select' | 'upload' | 'preview' | 'complete';

export function ImportPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? undefined);
  const { data: savedFormats } = useImportFormats(activeProfileId ?? undefined);

  const [step, setStep] = useState<ImportStep>('select');
  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(null);
  const [selectedFormat, setSelectedFormat] = useState<ImportFormat | null>(null);
  const [uploadResult, setUploadResult] = useState<UploadResponse | null>(null);
  const [previewResult, setPreviewResult] = useState<PreviewResponse | null>(null);
  const [importedCount, setImportedCount] = useState(0);
  const [showSaveFormat, setShowSaveFormat] = useState(false);
  const [formatName, setFormatName] = useState('');

  const uploadMutation = useUploadCsv();
  const previewMutation = usePreviewImport();
  const confirmMutation = useConfirmImport();
  const createFormatMutation = useCreateImportFormat();

  const handleFileSelect = useCallback(async (file: File) => {
    if (!selectedAccountId) return;

    try {
      const result = await uploadMutation.mutateAsync({
        accountId: selectedAccountId,
        file,
      });
      setUploadResult(result);

      // Use selected format or detected format for preview
      const formatOverride = selectedFormat?.mapping;
      const preview = await previewMutation.mutateAsync({
        sessionId: result.sessionId,
        formatOverride,
      });
      setPreviewResult(preview);
      setStep('preview');
    } catch (error) {
      console.error('Upload failed:', error);
    }
  }, [selectedAccountId, selectedFormat, uploadMutation, previewMutation]);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    const file = e.dataTransfer.files[0];
    if (file && file.name.endsWith('.csv')) {
      handleFileSelect(file);
    }
  }, [handleFileSelect]);

  const handleConfirmImport = async () => {
    if (!uploadResult) return;

    try {
      const formatOverride = selectedFormat?.mapping;
      const result = await confirmMutation.mutateAsync({
        sessionId: uploadResult.sessionId,
        formatOverride,
        skipDuplicates: true,
      });
      setImportedCount(result.importedCount);
      setStep('complete');
    } catch (error) {
      console.error('Import failed:', error);
    }
  };

  const handleSaveFormat = async () => {
    if (!activeProfileId || !uploadResult || !formatName.trim()) return;

    try {
      await createFormatMutation.mutateAsync({
        profileId: activeProfileId,
        data: {
          name: formatName.trim(),
          bankName: accounts?.find(a => a.id === selectedAccountId)?.bankName,
          mapping: selectedFormat?.mapping ?? uploadResult.detectedFormat,
        },
      });
      setShowSaveFormat(false);
      setFormatName('');
    } catch (error) {
      console.error('Failed to save format:', error);
    }
  };

  const resetImport = () => {
    setStep('select');
    setSelectedAccountId(null);
    setSelectedFormat(null);
    setUploadResult(null);
    setPreviewResult(null);
    setImportedCount(0);
    setShowSaveFormat(false);
    setFormatName('');
  };

  const currentFormat = selectedFormat?.mapping ?? uploadResult?.detectedFormat;

  if (!activeProfileId) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Please select a profile first</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Import Transactions</h1>
        <p className="text-gray-500">Upload CSV files from your bank</p>
      </div>

      {/* Progress Steps */}
      <div className="flex items-center gap-2">
        {['select', 'upload', 'preview', 'complete'].map((s, i) => (
          <div key={s} className="flex items-center">
            <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
              step === s ? 'bg-blue-600 text-white' :
              ['select', 'upload', 'preview', 'complete'].indexOf(step) > i
                ? 'bg-green-600 text-white'
                : 'bg-gray-200 text-gray-600'
            }`}>
              {i + 1}
            </div>
            {i < 3 && <div className="w-8 h-0.5 bg-gray-200" />}
          </div>
        ))}
      </div>

      {/* Step 1: Select Account */}
      {step === 'select' && (
        <Card>
          <CardHeader>
            <CardTitle>Select Account</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {accounts?.map(account => (
                <button
                  key={account.id}
                  onClick={() => {
                    setSelectedAccountId(account.id);
                    setStep('upload');
                  }}
                  className="p-4 border rounded-lg hover:border-blue-500 hover:bg-blue-50 text-left transition-colors"
                >
                  <div className="font-medium">{account.name}</div>
                  {account.bankName && (
                    <div className="text-sm text-gray-500">{account.bankName}</div>
                  )}
                  <div className="text-xs text-gray-400 mt-1">{account.currency}</div>
                </button>
              ))}
              {accounts?.length === 0 && (
                <p className="text-gray-500 col-span-full">
                  No accounts found. Create an account first.
                </p>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 2: Upload CSV */}
      {step === 'upload' && (
        <div className="space-y-6">
          {/* Saved Formats */}
          {savedFormats && savedFormats.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Use Saved Format (Optional)</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2">
                  <button
                    onClick={() => setSelectedFormat(null)}
                    className={`px-3 py-2 text-sm rounded-lg border transition-colors ${
                      selectedFormat === null
                        ? 'border-blue-500 bg-blue-50 text-blue-700'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    Auto-detect
                  </button>
                  {savedFormats.map(format => (
                    <button
                      key={format.id}
                      onClick={() => setSelectedFormat(format)}
                      className={`px-3 py-2 text-sm rounded-lg border transition-colors flex items-center gap-2 ${
                        selectedFormat?.id === format.id
                          ? 'border-blue-500 bg-blue-50 text-blue-700'
                          : 'border-gray-200 hover:border-gray-300'
                      }`}
                    >
                      <FileText className="h-4 w-4" />
                      {format.name}
                      {format.bankName && (
                        <span className="text-xs text-gray-500">({format.bankName})</span>
                      )}
                    </button>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader>
              <CardTitle>Upload CSV File</CardTitle>
            </CardHeader>
            <CardContent>
              <div
                onDragOver={(e) => e.preventDefault()}
                onDrop={handleDrop}
                className="border-2 border-dashed border-gray-300 rounded-lg p-12 text-center hover:border-blue-500 transition-colors"
              >
                {uploadMutation.isPending ? (
                  <div className="flex flex-col items-center">
                    <Loader2 className="h-12 w-12 text-blue-500 animate-spin" />
                    <p className="mt-4 text-sm text-gray-600">
                      {selectedFormat ? 'Processing CSV...' : 'Analyzing CSV...'}
                    </p>
                  </div>
                ) : (
                  <>
                    <Upload className="h-12 w-12 text-gray-400 mx-auto" />
                    <p className="mt-4 text-sm text-gray-600">
                      Drag and drop your CSV file here, or
                    </p>
                    <label className="mt-2 inline-block">
                      <input
                        type="file"
                        accept=".csv"
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
              </div>
              <div className="mt-4 flex gap-2">
                <Button variant="outline" onClick={() => setStep('select')}>
                  Back
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Step 3: Preview */}
      {step === 'preview' && previewResult && uploadResult && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>Preview Import</span>
              <span className="text-sm font-normal text-gray-500">
                {uploadResult.filename} - {uploadResult.rowCount} rows
              </span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            {/* Format Info */}
            <div className="mb-4 p-3 bg-gray-50 rounded-lg text-sm">
              <div className="flex justify-between items-center mb-2">
                <span className="font-medium">
                  {selectedFormat ? `Using: ${selectedFormat.name}` : 'Detected Format:'}
                </span>
                {!showSaveFormat && !selectedFormat && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setShowSaveFormat(true)}
                    className="h-7 text-xs"
                  >
                    <Save className="h-3 w-3 mr-1" />
                    Save Format
                  </Button>
                )}
              </div>
              {currentFormat && (
                <div className="grid grid-cols-2 md:grid-cols-4 gap-2 text-gray-600">
                  <div>Delimiter: <span className="font-mono">{currentFormat.delimiter === '\t' ? 'Tab' : currentFormat.delimiter}</span></div>
                  <div>Date Format: <span className="font-mono">{currentFormat.dateFormat}</span></div>
                  <div>Has Header: {currentFormat.hasHeader ? 'Yes' : 'No'}</div>
                  <div>Amount Type: {currentFormat.amountType}</div>
                </div>
              )}
            </div>

            {/* Save Format Form */}
            {showSaveFormat && (
              <div className="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                <div className="flex items-end gap-2">
                  <div className="flex-1">
                    <Label htmlFor="formatName" className="text-sm">Format Name</Label>
                    <Input
                      id="formatName"
                      value={formatName}
                      onChange={(e) => setFormatName(e.target.value)}
                      placeholder="e.g., My Bank Statement"
                      className="h-9"
                    />
                  </div>
                  <Button
                    size="sm"
                    onClick={handleSaveFormat}
                    disabled={!formatName.trim() || createFormatMutation.isPending}
                  >
                    {createFormatMutation.isPending ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      'Save'
                    )}
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => {
                      setShowSaveFormat(false);
                      setFormatName('');
                    }}
                  >
                    Cancel
                  </Button>
                </div>
              </div>
            )}

            {/* Duplicate Warning */}
            {previewResult.duplicateCount > 0 && (
              <div className="mb-4 p-3 bg-yellow-50 border border-yellow-200 rounded-lg flex items-start gap-2">
                <AlertCircle className="h-5 w-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                <div className="text-sm">
                  <div className="font-medium text-yellow-800">
                    {previewResult.duplicateCount} duplicate transactions found
                  </div>
                  <div className="text-yellow-700">
                    These will be skipped during import to avoid duplicates.
                  </div>
                </div>
              </div>
            )}

            {/* Transaction Preview Table */}
            <div className="border rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-2 text-left font-medium">Date</th>
                    <th className="px-4 py-2 text-left font-medium">Description</th>
                    <th className="px-4 py-2 text-right font-medium">Amount</th>
                    <th className="px-4 py-2 text-center font-medium">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {previewResult.transactions.slice(0, 20).map((tx, i) => (
                    <tr key={i} className={tx.isDuplicate ? 'bg-yellow-50' : ''}>
                      <td className="px-4 py-2">{tx.date}</td>
                      <td className="px-4 py-2 truncate max-w-xs">{tx.description}</td>
                      <td className={`px-4 py-2 text-right font-mono ${
                        tx.amount < 0 ? 'text-red-600' : 'text-green-600'
                      }`}>
                        {tx.amount.toFixed(2)}
                      </td>
                      <td className="px-4 py-2 text-center">
                        {tx.isDuplicate ? (
                          <span className="text-yellow-600 text-xs">Duplicate</span>
                        ) : (
                          <span className="text-green-600 text-xs">New</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {previewResult.transactions.length > 20 && (
                <div className="px-4 py-2 bg-gray-50 text-sm text-gray-500">
                  ... and {previewResult.transactions.length - 20} more transactions
                </div>
              )}
            </div>

            <div className="mt-4 flex gap-2">
              <Button variant="outline" onClick={() => setStep('upload')}>
                Back
              </Button>
              <Button
                onClick={handleConfirmImport}
                disabled={confirmMutation.isPending}
              >
                {confirmMutation.isPending ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Importing...
                  </>
                ) : (
                  <>Import {previewResult.transactions.length - previewResult.duplicateCount} Transactions</>
                )}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 4: Complete */}
      {step === 'complete' && (
        <Card>
          <CardHeader>
            <CardTitle>Import Complete</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center py-8">
              <CheckCircle className="h-16 w-16 text-green-500" />
              <h3 className="mt-4 text-xl font-medium">Success!</h3>
              <p className="mt-2 text-gray-600">
                {importedCount} transactions have been imported.
              </p>
              <div className="mt-6 flex gap-2">
                <Button variant="outline" onClick={resetImport}>
                  Import More
                </Button>
                <a
                  href="/"
                  className="inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium transition-colors bg-blue-600 text-white hover:bg-blue-700 shadow-sm h-10 px-4 py-2"
                >
                  View Dashboard
                </a>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
