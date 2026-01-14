import { PieChart } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';

export function ReportsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Reports</h1>
        <p className="text-gray-500">Visualize your spending patterns</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Coming in Phase 3</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <PieChart className="h-12 w-12 text-gray-400" />
            <h3 className="mt-4 text-lg font-medium">Spending Analytics</h3>
            <p className="mt-2 text-sm text-gray-500 max-w-md">
              Interactive charts and reports will be available here to help you
              understand your spending habits. You'll be able to view expenses
              by category, track trends over time, and compare periods.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
