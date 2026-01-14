# Dashboard Module

> **Phase:** 3 (Dashboard & Visualizations)
> **Status:** Planned

## Overview
Interactive dashboards showing spending patterns, trends, and analytics.

## Dashboard Components

### Spending by Category (Pie/Donut Chart)
- Current month breakdown
- Drill-down into subcategories
- Compare to previous period

### Monthly Trend (Bar Chart)
- Income vs Expenses
- 12-month rolling view
- Stacked by category

### Daily Spending (Area Chart)
- Current month daily spending
- Running balance line
- Highlight unusual days

### Top Merchants (Table)
- Most frequent transaction descriptions
- Total spent per merchant
- Category breakdown

### Account Summary (Cards)
- Current balance per account
- Month-to-date spending
- Pending categorization count

## Aggregation Queries

<!-- TODO: Phase 3 - Implement efficient aggregations -->

```csharp
public record SpendingByCategory(
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Amount,
    int TransactionCount
);

public record MonthlyTrend(
    int Year,
    int Month,
    decimal Income,
    decimal Expenses,
    decimal Net
);
```

## Endpoints

<!-- TODO: Phase 3 -->

### GET /api/profiles/{profileId}/dashboard/summary
Overall dashboard summary.

### GET /api/profiles/{profileId}/dashboard/spending-by-category
Category breakdown for period.

### GET /api/profiles/{profileId}/dashboard/monthly-trend
Monthly income/expense trend.

### GET /api/profiles/{profileId}/dashboard/daily-spending
Daily spending for current month.

## React Components

<!-- TODO: Phase 3 -->

Using Recharts:
- `SpendingPieChart`
- `MonthlyTrendChart`
- `DailySpendingChart`
- `DashboardSummaryCards`

## Caching Strategy

<!-- TODO: Phase 3 -->

Consider caching aggregations:
- Daily recalculation for historical data
- Real-time for current month
- Invalidate on transaction changes
