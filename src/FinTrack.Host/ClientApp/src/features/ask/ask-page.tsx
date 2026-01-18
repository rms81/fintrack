import { useState, useMemo } from 'react';
import { Send, Loader2, Code, AlertCircle, Lightbulb } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useNlqMutation, useNlqSuggestions } from '../../hooks/use-nlq';
import type { NlqResponse } from '../../lib/types';
import { formatCurrency } from '../../lib/utils';

type NlqHistoryItem = NlqResponse & { id: string };

const DEFAULT_CURRENCY = 'EUR';

export function AskPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? undefined);
  const [question, setQuestion] = useState('');
  const [showSql, setShowSql] = useState<Record<string, boolean>>({});
  const [history, setHistory] = useState<NlqHistoryItem[]>([]);

  const { mutate: executeQuery, isPending } = useNlqMutation(activeProfileId ?? undefined);
  const { data: suggestions } = useNlqSuggestions(activeProfileId ?? undefined);

  // Derive currency from accounts: use a single shared currency if all accounts agree, otherwise default to EUR
  const currency = useMemo(() => {
    if (!accounts || accounts.length === 0) {
      return DEFAULT_CURRENCY;
    }

    const uniqueCurrencies = new Set(
      accounts
        .map(account => account.currency)
        .filter((c): c is string => Boolean(c)),
    );

    if (uniqueCurrencies.size === 1) {
      return Array.from(uniqueCurrencies)[0];
    }

    // Multiple different account currencies; fall back to a neutral default
    return DEFAULT_CURRENCY;
  }, [accounts]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!question.trim() || isPending) return;

    executeQuery(question, {
      onSuccess: (result) => {
        setHistory((prev) => [{ ...result, id: crypto.randomUUID() }, ...prev]);
        setQuestion('');
      },
    });
  };

  const handleSuggestionClick = (suggestion: string) => {
    setQuestion(suggestion);
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
        <h1 className="text-2xl font-bold">Ask Your Data</h1>
        <p className="text-gray-500">Ask questions about your finances in plain English</p>
      </div>

      {/* Query Input */}
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="flex gap-2">
              <input
                type="text"
                value={question}
                onChange={(e) => setQuestion(e.target.value)}
                placeholder="e.g., How much did I spend on food last month?"
                className="flex-1 px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={isPending}
              />
              <Button type="submit" disabled={!question.trim() || isPending} aria-label="Send question">
                {isPending ? (
                  <Loader2 className="h-5 w-5 animate-spin" />
                ) : (
                  <Send className="h-5 w-5" />
                )}
              </Button>
            </div>

            {/* Suggestions */}
            {suggestions && suggestions.length > 0 && (
              <div className="flex flex-wrap gap-2">
                <Lightbulb className="h-4 w-4 text-yellow-500 mt-1" />
                {suggestions.map((suggestion) => (
                  <button
                    key={suggestion}
                    type="button"
                    onClick={() => handleSuggestionClick(suggestion)}
                    className="text-sm px-3 py-1 bg-gray-100 hover:bg-gray-200 rounded-full text-gray-700"
                  >
                    {suggestion}
                  </button>
                ))}
              </div>
            )}
          </form>
        </CardContent>
      </Card>

      {/* Results */}
      {history.length > 0 && (
        <div className="space-y-4">
          {history.map((result) => (
            <Card key={result.id}>
              <CardHeader className="pb-2">
                <CardTitle className="text-base font-medium text-gray-700">
                  {result.question}
                </CardTitle>
              </CardHeader>
              <CardContent>
                {result.resultType === 'Error' ? (
                  <div className="flex items-center gap-2 text-red-600">
                    <AlertCircle className="h-5 w-5" />
                    <span>{result.errorMessage}</span>
                  </div>
                ) : (
                  <div className="space-y-4">
                    {/* Result Display */}
                    <NlqResultDisplay result={result} currency={currency} />

                    {/* Explanation */}
                    {result.explanation && (
                      <p className="text-sm text-gray-600">{result.explanation}</p>
                    )}

                    {/* SQL Toggle */}
                    {result.generatedSql && (
                      <div>
                        <button
                          onClick={() => setShowSql((prev) => ({ ...prev, [result.id]: !prev[result.id] }))}
                          className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700"
                        >
                          <Code className="h-4 w-4" />
                          {showSql[result.id] ? 'Hide SQL' : 'Show SQL'}
                        </button>
                        {showSql[result.id] && (
                          <pre className="mt-2 p-3 bg-gray-100 rounded text-xs overflow-x-auto">
                            {result.generatedSql}
                          </pre>
                        )}
                      </div>
                    )}
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Empty State */}
      {history.length === 0 && !isPending && (
        <Card>
          <CardContent className="py-12 text-center text-gray-500">
            <p className="mb-2">Ask a question to get started</p>
            <p className="text-sm">
              Try questions like &quot;How much did I spend last month?&quot; or &quot;What are my top expenses?&quot;
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function NlqResultDisplay({ result, currency }: { result: NlqResponse; currency: string }) {
  if (result.resultType === 'Scalar') {
    const value = result.data as number | string;
    let displayValue: string;

    if (typeof value === 'number') {
      // Heuristic similar to formatCellValue: treat as currency only if it "looks" like an amount
      const looksLikeCurrency = value < 0 || (value !== Math.floor(value) && Math.abs(value) > 1);
      displayValue = looksLikeCurrency ? formatCurrency(value, currency) : value.toLocaleString();
    } else {
      displayValue = String(value);
    }

    return (
      <div className="text-3xl font-bold">
        {displayValue}
      </div>
    );
  }

  if (result.resultType === 'Table') {
    const rows = result.data as Record<string, unknown>[];
    if (!rows || rows.length === 0) {
      return <p className="text-gray-500">No results found</p>;
    }

    const columns = Object.keys(rows[0]);
    return (
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b">
              {columns.map((col) => (
                <th key={col} className="text-left py-2 px-3 font-medium text-gray-600">
                  {col.replace(/_/g, ' ')}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.slice(0, 20).map((row, i) => {
              // Create a stable key by hashing the first few column values + index
              // This is more efficient than JSON.stringify for large objects
              const keyValues = columns.slice(0, 3).map(col => String(row[col] ?? '')).join('|');
              const rowKey = `${keyValues}-${i}`;
              return (
                <tr key={rowKey} className="border-b last:border-0">
                  {columns.map((col) => (
                    <td key={col} className="py-2 px-3">
                      {formatCellValue(row[col], currency)}
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
        {rows.length > 20 && (
          <p className="text-sm text-gray-500 mt-2">
            Showing 20 of {rows.length} results
          </p>
        )}
      </div>
    );
  }

  // Chart type - simplified display as table for now
  if (result.resultType === 'Chart') {
    return <NlqResultDisplay result={{ ...result, resultType: 'Table' }} currency={currency} />;
  }

  return null;
}

function formatCellValue(value: unknown, currency: string): string {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'number') {
    // Check if it looks like currency (has decimals or is negative)
    if (value < 0 || (value !== Math.floor(value) && Math.abs(value) > 1)) {
      return formatCurrency(value, currency);
    }
    return value.toLocaleString();
  }
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  if (value instanceof Date) return value.toLocaleDateString();
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) {
    return new Date(value).toLocaleDateString();
  }
  return String(value);
}
