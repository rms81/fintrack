import { useState, useMemo, useEffect, useRef } from 'react';
import { Link } from 'react-router';
import {
  Search,
  Filter,
  ChevronLeft,
  ChevronRight,
  X,
  AlertCircle,
  Pencil,
  Trash2,
  Loader2,
  Tag,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useAccounts } from '../../hooks/use-accounts';
import { useCategories } from '../../hooks/use-categories';
import { useTransactions, useUpdateTransaction, useDeleteTransaction } from '../../hooks/use-transactions';
import type { TransactionFilter, Transaction, Category, Account } from '../../lib/types';
import { formatCurrency } from '../../lib/utils';

const PAGE_SIZE = 20;

export function TransactionsPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: accounts } = useAccounts(activeProfileId ?? undefined);
  const { data: categories } = useCategories(activeProfileId ?? undefined);

  const [filter, setFilter] = useState<TransactionFilter>({
    page: 1,
    pageSize: PAGE_SIZE,
  });
  const [showFilters, setShowFilters] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState<Transaction | null>(null);
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ show: boolean; transactionId: string | null; description: string | null }>({
    show: false,
    transactionId: null,
    description: null,
  });
  const dialogRef = useRef<HTMLDivElement>(null);

  const { data: transactionPage, isLoading, error } = useTransactions(activeProfileId ?? undefined, filter);
  const updateMutation = useUpdateTransaction();
  const deleteMutation = useDeleteTransaction();

  // Focus the dialog when it opens
  useEffect(() => {
    if (deleteConfirm.show && dialogRef.current) {
      dialogRef.current.focus();
    }
  }, [deleteConfirm.show]);

  const categoryMap = useMemo(() => {
    if (!categories) return new Map<string, Category>();
    return new Map(categories.map(c => [c.id, c]));
  }, [categories]);

  const accountMap = useMemo(() => {
    if (!accounts) return new Map<string, Account>();
    return new Map(accounts.map(a => [a.id, a]));
  }, [accounts]);

  const handleFilterChange = (key: keyof TransactionFilter, value: string | number | boolean | undefined) => {
    setFilter(prev => ({
      ...prev,
      [key]: value,
      page: 1, // Reset to first page on filter change
    }));
  };

  const clearFilters = () => {
    setFilter({
      page: 1,
      pageSize: PAGE_SIZE,
    });
  };

  const handlePageChange = (newPage: number) => {
    setFilter(prev => ({ ...prev, page: newPage }));
  };

  const handleUpdateCategory = async () => {
    if (!editingTransaction) return;

    try {
      await updateMutation.mutateAsync({
        id: editingTransaction.id,
        data: { categoryId: selectedCategoryId },
      });
      setEditingTransaction(null);
      setSelectedCategoryId(null);
    } catch (error) {
      console.error('Failed to update transaction:', error);
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirm.transactionId) return;

    try {
      await deleteMutation.mutateAsync(deleteConfirm.transactionId);
      setDeleteConfirm({ show: false, transactionId: null, description: null });
    } catch (error) {
      console.error('Failed to delete transaction:', error);
    }
  };

  const activeFilterCount = Object.entries(filter).filter(
    ([key, value]) => !['page', 'pageSize'].includes(key) && value !== undefined && value !== ''
  ).length;

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
          <h1 className="text-2xl font-bold">Transactions</h1>
          <p className="text-gray-500">View and manage your transactions</p>
        </div>
        <Link
          to="/import"
          className="inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium bg-blue-600 text-white hover:bg-blue-700 shadow-sm h-10 px-4 py-2"
        >
          Import Transactions
        </Link>
      </div>

      {/* Search and Filter Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-4">
            {/* Search */}
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search transactions..."
                value={filter.search ?? ''}
                onChange={(e) => handleFilterChange('search', e.target.value || undefined)}
                className="pl-10"
              />
            </div>

            {/* Filter Toggle */}
            <Button
              variant={showFilters ? 'default' : 'outline'}
              onClick={() => setShowFilters(!showFilters)}
              className="relative"
            >
              <Filter className="h-4 w-4 mr-2" />
              Filters
              {activeFilterCount > 0 && (
                <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-blue-600 text-white text-xs flex items-center justify-center">
                  {activeFilterCount}
                </span>
              )}
            </Button>

            {activeFilterCount > 0 && (
              <Button variant="ghost" onClick={clearFilters}>
                <X className="h-4 w-4 mr-2" />
                Clear
              </Button>
            )}
          </div>

          {/* Expanded Filters */}
          {showFilters && (
            <div className="mt-4 pt-4 border-t grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Account Filter */}
              <div>
                <Label htmlFor="account-filter" className="text-sm">Account</Label>
                <select
                  id="account-filter"
                  className="w-full mt-1 rounded-md border border-gray-200 bg-white px-3 py-2 text-sm"
                  value={filter.accountId ?? ''}
                  onChange={(e) => handleFilterChange('accountId', e.target.value || undefined)}
                >
                  <option value="">All accounts</option>
                  {accounts?.map(account => (
                    <option key={account.id} value={account.id}>
                      {account.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Category Filter */}
              <div>
                <Label htmlFor="category-filter" className="text-sm">Category</Label>
                <select
                  id="category-filter"
                  className="w-full mt-1 rounded-md border border-gray-200 bg-white px-3 py-2 text-sm"
                  value={filter.categoryId ?? ''}
                  onChange={(e) => handleFilterChange('categoryId', e.target.value || undefined)}
                >
                  <option value="">All categories</option>
                  {categories?.filter(c => !c.parentId).map(category => (
                    <option key={category.id} value={category.id}>
                      {category.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Date Range */}
              <div>
                <Label className="text-sm">From Date</Label>
                <Input
                  type="date"
                  value={filter.fromDate ?? ''}
                  onChange={(e) => handleFilterChange('fromDate', e.target.value || undefined)}
                  className="mt-1"
                />
              </div>

              <div>
                <Label className="text-sm">To Date</Label>
                <Input
                  type="date"
                  value={filter.toDate ?? ''}
                  onChange={(e) => handleFilterChange('toDate', e.target.value || undefined)}
                  className="mt-1"
                />
              </div>

              {/* Amount Range */}
              <div>
                <Label className="text-sm">Min Amount</Label>
                <Input
                  type="number"
                  step="0.01"
                  placeholder="0.00"
                  value={filter.minAmount ?? ''}
                  onChange={(e) => handleFilterChange('minAmount', e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="mt-1"
                />
              </div>

              <div>
                <Label className="text-sm">Max Amount</Label>
                <Input
                  type="number"
                  step="0.01"
                  placeholder="0.00"
                  value={filter.maxAmount ?? ''}
                  onChange={(e) => handleFilterChange('maxAmount', e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="mt-1"
                />
              </div>

              {/* Uncategorized Filter */}
              <div className="flex items-end">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={filter.uncategorized ?? false}
                    onChange={(e) => handleFilterChange('uncategorized', e.target.checked || undefined)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="text-sm">Uncategorized only</span>
                </label>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Transactions List */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <span>
              {transactionPage?.totalCount ?? 0} Transaction{transactionPage?.totalCount !== 1 ? 's' : ''}
            </span>
            {transactionPage && transactionPage.totalPages > 1 && (
              <span className="text-sm font-normal text-gray-500">
                Page {transactionPage.page} of {transactionPage.totalPages}
              </span>
            )}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
            </div>
          ) : error ? (
            <div className="flex items-center justify-center py-12 text-red-500">
              <AlertCircle className="h-5 w-5 mr-2" />
              Failed to load transactions
            </div>
          ) : transactionPage?.items.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-500">No transactions found</p>
              {activeFilterCount > 0 && (
                <Button variant="link" onClick={clearFilters} className="mt-2">
                  Clear filters
                </Button>
              )}
            </div>
          ) : (
            <>
              {/* Transaction Table */}
              <div className="border rounded-lg overflow-hidden">
                <table className="w-full text-sm" aria-label="Transactions table">
                  <thead className="bg-gray-50">
                    <tr>
                      <th scope="col" className="px-4 py-3 text-left font-medium">Date</th>
                      <th scope="col" className="px-4 py-3 text-left font-medium">Description</th>
                      <th scope="col" className="px-4 py-3 text-left font-medium">Category</th>
                      <th scope="col" className="px-4 py-3 text-right font-medium">Amount</th>
                      <th
                        scope="col"
                        className="px-4 py-3 text-center font-medium w-24"
                        aria-label="Transaction actions"
                      >
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {transactionPage?.items.map((tx) => (
                      <tr key={tx.id} className="hover:bg-gray-50">
                        <td className="px-4 py-3 whitespace-nowrap">
                          {new Date(tx.date).toLocaleDateString()}
                        </td>
                        <td className="px-4 py-3">
                          <div className="truncate max-w-xs" title={tx.description}>
                            {tx.description}
                          </div>
                          {tx.tags.length > 0 && (
                            <div className="flex gap-1 mt-1">
                              {tx.tags.slice(0, 3).map(tag => (
                                <span
                                  key={tag}
                                  className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-gray-100 text-gray-600"
                                >
                                  <Tag className="h-3 w-3" />
                                  {tag}
                                </span>
                              ))}
                              {tx.tags.length > 3 && (
                                <span className="text-xs text-gray-400">+{tx.tags.length - 3}</span>
                              )}
                            </div>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          {editingTransaction?.id === tx.id ? (
                            <div className="flex items-center gap-2">
                              <select
                                className="rounded-md border border-gray-200 bg-white px-2 py-1 text-sm"
                                value={selectedCategoryId ?? ''}
                                onChange={(e) => setSelectedCategoryId(e.target.value || null)}
                              >
                                <option value="">Uncategorized</option>
                                {categories?.filter(c => !c.parentId).map(cat => (
                                  <option key={cat.id} value={cat.id}>
                                    {cat.name}
                                  </option>
                                ))}
                              </select>
                              <Button
                                size="sm"
                                onClick={handleUpdateCategory}
                                disabled={updateMutation.isPending}
                              >
                                {updateMutation.isPending ? (
                                  <Loader2 className="h-3 w-3 animate-spin" />
                                ) : (
                                  'Save'
                                )}
                              </Button>
                              <Button
                                size="sm"
                                variant="ghost"
                                onClick={() => {
                                  setEditingTransaction(null);
                                  setSelectedCategoryId(null);
                                }}
                              >
                                Cancel
                              </Button>
                            </div>
                          ) : (
                            <button
                              onClick={() => {
                                setEditingTransaction(tx);
                                setSelectedCategoryId(tx.categoryId);
                              }}
                              className="group flex items-center gap-1 text-left hover:text-blue-600 transition-colors"
                            >
                              {tx.categoryId ? (
                                <span
                                  className="inline-flex items-center gap-1 px-2 py-1 rounded text-xs font-medium"
                                  style={{
                                    backgroundColor: `${categoryMap.get(tx.categoryId)?.color}20`,
                                    color: categoryMap.get(tx.categoryId)?.color,
                                  }}
                                >
                                  {tx.categoryName}
                                </span>
                              ) : (
                                <span className="text-gray-400 italic">Uncategorized</span>
                              )}
                              <Pencil className="h-3 w-3 opacity-0 group-hover:opacity-100" />
                            </button>
                          )}
                        </td>
                        <td className={`px-4 py-3 text-right font-mono whitespace-nowrap ${
                          tx.amount < 0 ? 'text-red-600' : 'text-green-600'
                        }`}>
                          {formatCurrency(tx.amount, accountMap.get(tx.accountId)?.currency ?? 'EUR')}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center justify-center gap-1">
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8"
                              onClick={() => {
                                setEditingTransaction(tx);
                                setSelectedCategoryId(tx.categoryId);
                              }}
                              title="Edit category"
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8 text-red-500 hover:text-red-600 hover:bg-red-50"
                              onClick={() => setDeleteConfirm({ show: true, transactionId: tx.id, description: tx.description })}
                              disabled={deleteMutation.isPending}
                              title="Delete"
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              {transactionPage && transactionPage.totalPages > 1 && (
                <div className="flex items-center justify-between mt-4">
                  <div className="text-sm text-gray-500">
                    Showing {((transactionPage.page - 1) * transactionPage.pageSize) + 1} to{' '}
                    {Math.min(transactionPage.page * transactionPage.pageSize, transactionPage.totalCount)} of{' '}
                    {transactionPage.totalCount}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(transactionPage.page - 1)}
                      disabled={transactionPage.page <= 1}
                    >
                      <ChevronLeft className="h-4 w-4 mr-1" />
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(transactionPage.page + 1)}
                      disabled={transactionPage.page >= transactionPage.totalPages}
                    >
                      Next
                      <ChevronRight className="h-4 w-4 ml-1" />
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      {deleteConfirm.show && (
        <>
          <div 
            className="fixed inset-0 z-50 bg-gray-900/50"
            onClick={() => setDeleteConfirm({ show: false, transactionId: null, description: null })}
            role="presentation"
          />
          <div 
            ref={dialogRef}
            className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-lg bg-white p-6 shadow-xl"
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
            aria-describedby="delete-dialog-description"
            onKeyDown={(e) => {
              if (e.key === 'Escape') {
                setDeleteConfirm({ show: false, transactionId: null, description: null });
              }
            }}
            tabIndex={-1}
          >
            <h3 id="delete-dialog-title" className="text-lg font-semibold">Delete Transaction</h3>
            <p id="delete-dialog-description" className="mt-2 text-sm text-gray-500">
              Are you sure you want to delete this transaction{deleteConfirm.description ? ` "${deleteConfirm.description}"` : ''}? This action cannot be undone.
            </p>
            <div className="mt-4 flex justify-end gap-3">
              <Button
                variant="outline"
                onClick={() => setDeleteConfirm({ show: false, transactionId: null, description: null })}
                disabled={deleteMutation.isPending}
              >
                Cancel
              </Button>
              <Button
                variant="destructive"
                onClick={handleDelete}
                disabled={deleteMutation.isPending}
              >
                {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
