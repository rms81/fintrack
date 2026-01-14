import { useState, useCallback } from 'react';
import { Upload, FileText, CheckCircle, AlertCircle, Loader2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useUploadCsv, usePreviewImport, useConfirmImport } from '../../hooks/use-import';
import type { UploadResponse, PreviewResponse, CsvFormatConfig } from '../../lib/types';

type ImportStep = 'select' | 'upload' | 'preview' | 'complete';

export function ImportPage() {
  const [activeProfileId] = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? undefined);

  const [step, setStep] = useState<ImportStep>('select');
  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(null);
  const [uploadResult, setUploadResult] = useState<UploadResponse | null>(null);
  const [previewResult, setPreviewResult] = useState<PreviewResponse | null>(null);
  const [importedCount, setImportedCount] = useState(0);

  const uploadMutation = useUploadCsv();
  const previewMutation = usePreviewImport();
  const confirmMutation = useConfirmImport();

  const handleFileSelect = useCallback(async (file: File) => {
    if (!selectedAccountId) return;

    try {
      const result = await uploadMutation.mutateAsync({
        accountId: selectedAccountId,
        file,
      });
      setUploadResult(result);

      // Automatically preview after upload
      const preview = await previewMutation.mutateAsync({
        sessionId: result.sessionId,
      });
      setPreviewResult(preview);
      setStep('preview');
    } catch (error) {
      console.error('Upload failed:', error);
    }
  }, [selectedAccountId, uploadMutation, previewMutation]);

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
      const result = await confirmMutation.mutateAsync({
        sessionId: uploadResult.sessionId,
        skipDuplicates: true,
      });
      setImportedCount(result.importedCount);
      setStep('complete');
    } catch (error) {
      console.error('Import failed:', error);
    }
  };

  const resetImport = () => {
    setStep('select');
    setSelectedAccountId(null);
    setUploadResult(null);
    setPreviewResult(null);
    setImportedCount(0);
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
                  <p className="mt-4 text-sm text-gray-600">Analyzing CSV...</p>
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
              <div className="font-medium mb-2">Detected Format:</div>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-2 text-gray-600">
                <div>Delimiter: <span className="font-mono">{uploadResult.detectedFormat.delimiter === '\t' ? 'Tab' : uploadResult.detectedFormat.delimiter}</span></div>
                <div>Date Format: <span className="font-mono">{uploadResult.detectedFormat.dateFormat}</span></div>
                <div>Has Header: {uploadResult.detectedFormat.hasHeader ? 'Yes' : 'No'}</div>
                <div>Amount Type: {uploadResult.detectedFormat.amountType}</div>
              </div>
            </div>

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
                <Button asChild>
                  <a href="/">View Dashboard</a>
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
