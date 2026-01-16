import { useState, useMemo } from 'react';
import { Link } from 'react-router';
import {
  TrendingUp,
  TrendingDown,
  Wallet,
  Receipt,
  AlertCircle,
  ArrowRight,
  Loader2,
} from 'lucide-react';
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useTransactions } from '../../hooks/use-transactions';
import {
  useDashboardSummary,
  useSpendingByCategory,
  useSpendingOverTime,
  useTopMerchants,
} from '../../hooks/use-dashboard';
import type { DashboardFilter } from '../../lib/types';
import { formatCurrency } from '../../lib/utils';

export function DashboardPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? '');

  // Date filter - default to current month
  const [filter] = useState<DashboardFilter>(() => {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    return {
      fromDate: firstDay.toISOString().split('T')[0],
      toDate: now.toISOString().split('T')[0],
    };
  });

  const { data: summary, isLoading: summaryLoading } = useDashboardSummary(activeProfileId ?? undefined, filter);
  const { data: categorySpending, isLoading: categoryLoading } = useSpendingByCategory(activeProfileId ?? undefined, filter);
  const { data: spendingOverTime, isLoading: timeLoading } = useSpendingOverTime(activeProfileId ?? undefined, filter, 'month');
  const { data: topMerchants, isLoading: merchantsLoading } = useTopMerchants(activeProfileId ?? undefined, filter, 5);
  const { data: recentTransactions } = useTransactions(activeProfileId ?? undefined, { pageSize: 5 });

  // Derive currency from accounts: use a single shared currency if all accounts agree, otherwise default to EUR
  const currency = useMemo(() => {
    if (!accounts || accounts.length === 0) {
      return 'EUR';
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
    return 'EUR';
  }, [accounts]);
  // Use the user's browser locale when available; fall back to environment default
  const userLocale = typeof navigator !== 'undefined' ? navigator.language : undefined;

  // Format chart data for spending over time
  const chartData = useMemo(() => {
    if (!spendingOverTime) return [];
    return spendingOverTime.map(item => ({
      ...item,
      month: new Date(item.date).toLocaleDateString(userLocale, { month: 'short', year: '2-digit' }),
    }));
  }, [spendingOverTime, userLocale]);

  if (!activeProfileId) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Please select a profile first</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Dashboard</h1>
          <p className="text-gray-500">Overview of your finances</p>
        </div>
        <Link
          to="/transactions"
          className="inline-flex items-center gap-2 text-sm text-blue-600 hover:text-blue-700"
        >
          View all transactions
          <ArrowRight className="h-4 w-4" />
        </Link>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-gray-500">Income</CardTitle>
            <TrendingUp className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            {summaryLoading ? (
              <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
            ) : (
              <div className="text-2xl font-bold text-green-600">
                {formatCurrency(summary?.totalIncome ?? 0, currency)}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-gray-500">Expenses</CardTitle>
            <TrendingDown className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            {summaryLoading ? (
              <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
            ) : (
              <>
                <div className="text-2xl font-bold text-red-600">
                  {formatCurrency(summary?.totalExpenses ?? 0, currency)}
                </div>
                {summary?.expenseChangePercentage !== null && summary?.expenseChangePercentage !== undefined && (
                  <p className={`text-xs ${summary.expenseChangePercentage > 0 ? 'text-red-500' : 'text-green-500'}`}>
                    {summary.expenseChangePercentage > 0 ? '+' : ''}{summary.expenseChangePercentage.toFixed(1)}% vs last period
                  </p>
                )}
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-gray-500">Net Balance</CardTitle>
            <Wallet className="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            {summaryLoading ? (
              <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
            ) : (
              <div className={`text-2xl font-bold ${(summary?.netBalance ?? 0) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {formatCurrency(summary?.netBalance ?? 0, currency)}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-gray-500">Transactions</CardTitle>
            <Receipt className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            {summaryLoading ? (
              <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
            ) : (
              <>
                <div className="text-2xl font-bold">{summary?.transactionCount ?? 0}</div>
                {(summary?.uncategorizedCount ?? 0) > 0 && (
                  <p className="text-xs text-yellow-600 flex items-center gap-1">
                    <AlertCircle className="h-3 w-3" />
                    {summary?.uncategorizedCount} uncategorized
                  </p>
                )}
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Charts Row */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Spending by Category */}
        <Card>
          <CardHeader>
            <CardTitle>Spending by Category</CardTitle>
          </CardHeader>
          <CardContent>
            {categoryLoading ? (
              <div className="h-64 flex items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
              </div>
            ) : !categorySpending || categorySpending.length === 0 ? (
              <div className="h-64 flex items-center justify-center text-gray-500">
                No expense data for this period
              </div>
            ) : (
              <div className="h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={categorySpending.map(c => ({ ...c }))}
                      dataKey="amount"
                      nameKey="categoryName"
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={90}
                      paddingAngle={2}
                      label={(props) => {
                        const entry = props.payload as { categoryName: string; percentage: number };
                        return entry.percentage > 5 ? `${entry.categoryName} (${entry.percentage.toFixed(0)}%)` : '';
                      }}
                      labelLine={false}
                    >
                      {categorySpending.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.categoryColor} />
                      ))}
                    </Pie>
                    <Tooltip
                      formatter={(value) => formatCurrency(Number(value), currency)}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Spending Over Time */}
        <Card>
          <CardHeader>
            <CardTitle>Income vs Expenses (Last 12 Months)</CardTitle>
          </CardHeader>
          <CardContent>
            {timeLoading ? (
              <div className="h-64 flex items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
              </div>
            ) : !chartData || chartData.length === 0 ? (
              <div className="h-64 flex items-center justify-center text-gray-500">
                No data available
              </div>
            ) : (
              <div className="h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" fontSize={12} />
                    <YAxis fontSize={12} tickFormatter={(v) => `${v >= 1000 ? (v/1000).toFixed(0) + 'k' : v}`} />
                    <Tooltip formatter={(value) => formatCurrency(Number(value), currency)} />
                    <Legend />
                    <Area
                      type="monotone"
                      dataKey="income"
                      name="Income"
                      stroke="#22c55e"
                      fill="#22c55e"
                      fillOpacity={0.3}
                    />
                    <Area
                      type="monotone"
                      dataKey="expenses"
                      name="Expenses"
                      stroke="#ef4444"
                      fill="#ef4444"
                      fillOpacity={0.3}
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Bottom Row */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Top Merchants */}
        <Card>
          <CardHeader>
            <CardTitle>Top Merchants</CardTitle>
          </CardHeader>
          <CardContent>
            {merchantsLoading ? (
              <div className="h-48 flex items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
              </div>
            ) : !topMerchants || topMerchants.length === 0 ? (
              <div className="h-48 flex items-center justify-center text-gray-500">
                No merchant data for this period
              </div>
            ) : (
              <div className="space-y-3">
                {topMerchants.map((merchant, index) => (
                  <div key={index} className="flex items-center justify-between">
                    <div className="flex-1 min-w-0">
                      <div className="font-medium truncate">{merchant.merchant}</div>
                      <div className="text-xs text-gray-500">
                        {merchant.transactionCount} transaction{merchant.transactionCount !== 1 ? 's' : ''}
                        {merchant.mostCommonCategory && ` | ${merchant.mostCommonCategory}`}
                      </div>
                    </div>
                    <div className="text-right ml-4">
                      <div className="font-mono text-red-600">
                        {formatCurrency(merchant.totalAmount, currency)}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Recent Transactions */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Recent Transactions</CardTitle>
            <Link to="/transactions">
              <Button variant="ghost" size="sm">
                View All
                <ArrowRight className="h-4 w-4 ml-1" />
              </Button>
            </Link>
          </CardHeader>
          <CardContent>
            {!recentTransactions?.items || recentTransactions.items.length === 0 ? (
              <div className="h-48 flex flex-col items-center justify-center text-gray-500">
                <p>No transactions yet</p>
                <Link to="/import" className="mt-2">
                  <Button variant="outline" size="sm">Import Transactions</Button>
                </Link>
              </div>
            ) : (
              <div className="space-y-3">
                {recentTransactions.items.map((tx) => (
                  <div key={tx.id} className="flex items-center justify-between">
                    <div className="flex-1 min-w-0">
                      <div className="font-medium truncate">{tx.description}</div>
                      <div className="text-xs text-gray-500">
                        {new Date(tx.date).toLocaleDateString()}
                        {tx.categoryName && ` | ${tx.categoryName}`}
                      </div>
                    </div>
                    <div className={`font-mono ml-4 ${tx.amount < 0 ? 'text-red-600' : 'text-green-600'}`}>
                      {formatCurrency(tx.amount, currency)}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
