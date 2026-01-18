import { useState, useMemo } from 'react';
import { Send, Loader2, Code, AlertCircle, Lightbulb, BarChart3, Table2 } from 'lucide-react';
import {
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useNlqMutation, useNlqSuggestions } from '../../hooks/use-nlq';
import type { NlqResponse } from '../../lib/types';
import { formatCurrency } from '../../lib/utils';

const CHART_COLORS = [
  '#3b82f6', // blue
  '#10b981', // emerald
  '#f59e0b', // amber
  '#ef4444', // red
  '#8b5cf6', // violet
  '#ec4899', // pink
  '#06b6d4', // cyan
  '#84cc16', // lime
];

type NlqHistoryItem = NlqResponse & { id: string };

const DEFAULT_CURRENCY = 'EUR';

export function AskPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? undefined);
  const [question, setQuestion] = useState('');
  const [showSql, setShowSql] = useState<Record<string, boolean>>({});
  const [history, setHistory] = useState<NlqHistoryItem[]>([]);
  const [viewModes, setViewModes] = useState<Record<string, 'table' | 'chart'>>({});

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
        const id = crypto.randomUUID();
        const rows = result.data as Record<string, unknown>[];
        const supportsChart = canShowAsChart(rows);
        const defaultView: 'table' | 'chart' = supportsChart ? 'chart' : 'table';
        setHistory((prev) => [{ ...result, id }, ...prev]);
        setViewModes((prev) => ({ ...prev, [id]: defaultView }));
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
              <Button type="submit" disabled={!question.trim() || isPending}>
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
                    {/* View Toggle for Table/Chart results */}
                    {(result.resultType === 'Table' || result.resultType === 'Chart') && 
                     canShowAsChart(result.data as Record<string, unknown>[]) && (
                      <div className="flex gap-1">
                        <button
                          onClick={() => setViewModes((prev) => ({ ...prev, [result.id]: 'chart' }))}
                          className={`flex items-center gap-1 px-3 py-1.5 text-sm rounded-md transition-colors ${
                            viewModes[result.id] === 'chart'
                              ? 'bg-blue-100 text-blue-700'
                              : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                          }`}
                        >
                          <BarChart3 className="h-4 w-4" />
                          Chart
                        </button>
                        <button
                          onClick={() => setViewModes((prev) => ({ ...prev, [result.id]: 'table' }))}
                          className={`flex items-center gap-1 px-3 py-1.5 text-sm rounded-md transition-colors ${
                            viewModes[result.id] === 'table'
                              ? 'bg-blue-100 text-blue-700'
                              : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                          }`}
                        >
                          <Table2 className="h-4 w-4" />
                          Table
                        </button>
                      </div>
                    )}

                    {/* Result Display */}
                    <NlqResultDisplay 
                      result={result} 
                      currency={currency} 
                      viewMode={viewModes[result.id] || 'table'}
                    />

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

function NlqResultDisplay({ 
  result, 
  currency, 
  viewMode = 'table' 
}: { 
  result: NlqResponse; 
  currency: string; 
  viewMode?: 'table' | 'chart';
}) {
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

  if (result.resultType === 'Table' || result.resultType === 'Chart') {
    const rows = result.data as Record<string, unknown>[];
    if (!rows || rows.length === 0) {
      return <p className="text-gray-500">No results found</p>;
    }

    // Show chart if view mode is chart and data supports it
    if (viewMode === 'chart' && canShowAsChart(rows)) {
      return <NlqChartDisplay rows={rows} chartType={result.chartType} currency={currency} />;
    }

    // Table view
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

  return null;
}

// Check if data can be displayed as a chart (needs a label column and at least one numeric column)
function canShowAsChart(rows: Record<string, unknown>[] | undefined): boolean {
  if (!rows || rows.length === 0) return false;

  const columns = Object.keys(rows[0]);
  if (columns.length < 2) return false;

  // Need at least one numeric column for values
  const hasNumericColumn = columns.some(col =>
    rows.some(row => typeof row[col] === 'number')
  );

  // Chart eligibility is based on structure and data types, not total row count
  return hasNumericColumn;
}

function NlqChartDisplay({ 
  rows, 
  chartType, 
  currency 
}: { 
  rows: Record<string, unknown>[]; 
  chartType: string | null;
  currency: string;
}) {
  const columns = Object.keys(rows[0]);
  
  // Find label column (first non-numeric) and value columns (numeric)
  const labelColumn = columns.find(col => 
    rows.every(row => typeof row[col] !== 'number')
  ) || columns[0];
  
  const valueColumns = columns.filter(col => 
    col !== labelColumn && rows.some(row => typeof row[col] === 'number')
  );

  if (valueColumns.length === 0) {
    return <p className="text-gray-500">Cannot display as chart - no numeric data</p>;
  }

  // Prepare chart data
  const chartData: Array<Record<string, number | string>> = rows.map(row => ({
    name: String(row[labelColumn] ?? 'Unknown'),
    ...valueColumns.reduce((acc, col) => {
      acc[col] = typeof row[col] === 'number' ? row[col] : 0;
      return acc;
    }, {} as Record<string, number>),
  }));

  const hasNegativeValues = valueColumns.some(col =>
    chartData.some(row => Number(row[col] ?? 0) < 0)
  );

  // Use pie chart for single value column with few items, bar chart otherwise
  const usePieChart = !hasNegativeValues && (
    chartType === 'pie' || chartType === 'Pie' ||
    (valueColumns.length === 1 && rows.length <= 8 && !chartType)
  );

  if (usePieChart) {
    const valueKey = valueColumns[0];
    return (
      <div className="h-80">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={({ name, percent }) => `${name} (${((percent ?? 0) * 100).toFixed(0)}%)`}
              outerRadius={100}
              fill="#8884d8"
              dataKey={valueKey}
            >
              {chartData.map((_, index) => (
                <Cell key={`cell-${index}`} fill={CHART_COLORS[index % CHART_COLORS.length]} />
              ))}
            </Pie>
            <Tooltip 
              formatter={(value) => formatCurrency(Number(value ?? 0), currency)}
            />
            <Legend />
          </PieChart>
        </ResponsiveContainer>
      </div>
    );
  }

  // Bar chart for multiple values or larger datasets
  return (
    <div className="h-80">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 60 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis 
            dataKey="name" 
            angle={-45}
            textAnchor="end"
            height={80}
            interval={0}
            tick={{ fontSize: 12 }}
          />
          <YAxis 
            tickFormatter={(value) => formatCurrency(value, currency)}
          />
          <Tooltip 
            formatter={(value) => formatCurrency(Number(value ?? 0), currency)}
          />
          {valueColumns.length > 1 && <Legend />}
          {valueColumns.map((col, index) => (
            <Bar 
              key={col} 
              dataKey={col} 
              fill={CHART_COLORS[index % CHART_COLORS.length]}
              name={col.replace(/_/g, ' ')}
            />
          ))}
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
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
